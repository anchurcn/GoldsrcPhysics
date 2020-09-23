using BulletSharp;
using BulletSharp.Math;
using GoldsrcPhysics.Goldsrc;
using GoldsrcPhysics.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using static GoldsrcPhysics.Utils.BulletHelper;
/*
 * for matrix that translation on a column
 * Local = Child * Parent^-1
 * Child = Local * Parent
 * Parent = Local^-1 * Child
 * ( Local means Child's local transform to Parent)
 * 
 * */

namespace GoldsrcPhysics
{
    /// <summary>
    /// 这个类提供了构造布娃娃的方法，可以通过模型信息或者额外文件构造布娃娃的相关数据（碰撞体，各种变换）
    /// 如果构造出来的对象为null，则说明此模型在此构建配置中不支持布娃娃。
    /// 
    /// 在此可以维护一个RagdollData缓存，已经生成过的布娃娃数据可以缓存起来；
    /// 根据算法构造的RagdollData的Key = ModelName + nameof（BuildOption）
    /// </summary>
    public class RagdollBuilder
    {
        public enum BodyPart
        {
            Head,
            //Spine,
            Pelvis,
            LeftArm,
            LeftElbow,
            //LeftHand,
            LeftHip,
            LeftKnee,
            //LeftFoot,
            RightArm,
            RightElbow,
            //RightHand,
            RightHip,
            RightKnee,
            //RightFoot,
            Count
        };
        public enum BuildOption
        {
            /// <summary>
            /// Default option on physics.cfg
            /// </summary>
            Default,
            /// <summary>
            /// build from file that contains colliders and constraints, [.phy file]
            /// </summary>
            FromFile,
            /// <summary>
            /// Build via algorithm that create a bipped ragdoll
            /// </summary>
            Bipped,
            /// <summary>
            /// Build via algorithm that create a bipped ragdoll
            /// </summary>
            Spider
        }
        public static Ragdoll Build(string modelName, BuildOption buildOption, BuildOption fallBackOption = BuildOption.Default)
        {
            Ragdoll result = Build(modelName, buildOption);
            if (result == null)
            {
                Debug.LogLine("Now use fall back option to build ragdoll for {0}.mdl",modelName);
                result = Build(modelName, fallBackOption);
            }
            return result;
        }
        public unsafe static Ragdoll Build(int modelIndex)
        {
            var mod=IEngineStudio.GetModelByIndex(modelIndex);
            string name = Marshal.PtrToStringAnsi((IntPtr)mod->name);
            return BuildBipped(BippedBone.Get(name), BoneAccessor.Get(name));
        }
        public unsafe static Ragdoll Build(Studio_h.studiohdr_t* hdr)
        {
            string name = Marshal.PtrToStringAnsi((IntPtr)hdr->name);
            return BuildBipped(BippedBone.Get(name), BoneAccessor.Get(name));
        }
        public static Ragdoll Build(string modelName, BuildOption buildOption)
        {
            switch (buildOption)
            {
                case BuildOption.Default:
                    var defultOption = PhyConfiguration.GetValue("BuildOption");
                    Debug.LogLine("Default ragdoll build option is [{0}]", defultOption);
                    return Build(modelName, (BuildOption)Enum.Parse(buildOption.GetType(),defultOption));
                case BuildOption.FromFile:
                    break;
                case BuildOption.Bipped:
                    {
                        var info = BippedBone.Get(modelName);
                        if (info != null)
                            return BuildBipped(info,BoneAccessor.GetCurrent());
                        Debug.LogLine("BippedBone missing. Model:{0}", modelName);
                    }
                    break;
                case BuildOption.Spider:
                    break;
                default:
                    break;
            }
            //missing inoformation to build for this model
            return null;
        }

        //				 6 ------------- 5
        //			   /  			   /  
        //			 /	 |			 /	 |
        //		   /	 |		   /	 | height/height
        //		 2 ------------- 1		 |
        //		  		 |		  		 |
        //		 |		  		 |		  
        //		 |		 7 ------|------ 4
        //		 |	   /		 |	   /
        //		 |	 /			 |	 / depth/width
        //		   /			   /
        //		 3 ------------- 0
        //           width/length
        private static Ragdoll BuildBipped(BippedBone info,BoneAccessor accessor)
        {
            Debug.LogLine("Bipped build begin...");
            var listKeybone = new List<int>()
            {
            info.Head,
            //info.Spine,
            info.Pelvis,
            info.LeftArm,
            info.LeftElbow,
            //info.LeftHand,
            info.LeftHip,
            info.LeftKnee,
            //info.LeftFoot,
            info.RightArm,
            info.RightElbow,
            //info.RightHand,
            info.RightHip,
            info.RightKnee,
            //info.RightFoot,
            };

            var listNonKeybone = new List<int>();
            for (int i = info.Pelvis; i < accessor.BoneCount; i++)
            {
                if (!listKeybone.Contains(i))
                    listNonKeybone.Add(i);
            }
            var ragdoll = new Ragdoll()
            {
                RigidBodies = new RigidBody[(int)BodyPart.Count],
                Constraints = new TypedConstraint[9],
                BoneRelativeTransform = new Matrix[accessor.BoneCount],
                EntityId = StudioRenderer.EntityId,
                RagdollData = new RagdollData()
                {
                    KeyBoneIndeces = listKeybone,
                    NonKeyBoneIndeces = listNonKeybone,
                }
            };
            //===========Rigidbody=================
            Debug.LogLine("Now setup rigidbody...");
            var lArm = accessor.Pos(info.LeftArm);
            var rArm = accessor.Pos(info.RightArm);

            var bodyWidth = (lArm - rArm).Length;
            Debug.LogLine("body width:{0}", bodyWidth);

            //head
            var head = accessor.Pos(info.Head);
            var headWidth = bodyWidth / 2.5f;//躯干是头的2.5倍
            var headRaito = 5.85f / 8.1f;//头宽高比
            var headHeight = headWidth / headRaito;
            var headShape = new CapsuleShape(headWidth / 2, headHeight - headWidth);
            var rigidOrigin = head;
            rigidOrigin.Z += headHeight / 2;
            rigidOrigin.X += headWidth / 4.6f;
            var headLookAt = head;
            headLookAt.X += headWidth / 4.6f;
            var headRigid = Matrix.Translation(rigidOrigin).LookAt(in headLookAt, in Vector3.UnitY);
            var headBone = accessor.GetWorldTransform(info.Head);
            ragdoll.RigidBodies[(int)BodyPart.Head] = BulletHelper.CreateBoneRigidbody(1, ref headBone, ref headRigid, headShape);
            ragdoll.RigidBodies[(int)BodyPart.Head].UserIndex = info.Head;

            //lArm
            var lArmBone = accessor.GetWorldTransform(info.LeftArm);
            var lElbow = accessor.Pos(info.LeftElbow);
            ragdoll.RigidBodies[(int)BodyPart.LeftArm] = CreateLimb(ref lArmBone, lElbow, headWidth*0.55f);
            ragdoll.RigidBodies[(int)BodyPart.LeftArm].UserIndex = info.LeftArm;
            //rArm
            var rArmBone = accessor.GetWorldTransform(info.RightArm);
            var rElbow = accessor.Pos(info.RightElbow);
            ragdoll.RigidBodies[(int)BodyPart.RightArm] = CreateLimb(ref rArmBone, rElbow, headWidth*0.55f);
            ragdoll.RigidBodies[(int)BodyPart.RightArm].UserIndex = info.RightArm;
            //lElbow
            var lElbowBone = accessor.GetWorldTransform(info.LeftElbow);
            var lHand = accessor.Pos(info.LeftHand);
            ragdoll.RigidBodies[(int)BodyPart.LeftElbow] = CreateLimb(ref lElbowBone, lHand, headWidth * 0.5f);
            ragdoll.RigidBodies[(int)BodyPart.LeftElbow].UserIndex = info.LeftElbow;
            //rElbow
            var rElbowBone = accessor.GetWorldTransform(info.RightElbow);
            var rHand = accessor.Pos(info.RightHand);
            ragdoll.RigidBodies[(int)BodyPart.RightElbow] = CreateLimb(ref rElbowBone, rHand, headWidth * 0.5f);
            ragdoll.RigidBodies[(int)BodyPart.RightElbow].UserIndex = info.RightElbow;
            //lHip
            var lHipBone = accessor.GetWorldTransform(info.LeftHip);
            var lKnee = accessor.Pos(info.LeftKnee);
            ragdoll.RigidBodies[(int)BodyPart.LeftHip] = CreateLimb(ref lHipBone, lKnee, headWidth*0.55f);
            ragdoll.RigidBodies[(int)BodyPart.LeftHip].UserIndex = info.LeftHip;
            //rHip
            var rHipBone = accessor.GetWorldTransform(info.RightHip);
            var rKnee = accessor.Pos(info.RightKnee);
            ragdoll.RigidBodies[(int)BodyPart.RightHip] = CreateLimb(ref rHipBone, rKnee, headWidth*0.55f);
            ragdoll.RigidBodies[(int)BodyPart.RightHip].UserIndex = info.RightHip;
            //lKnee
            var lKneeBone = accessor.GetWorldTransform(info.LeftKnee);
            var lFoot = accessor.Pos(info.LeftFoot);
            ragdoll.RigidBodies[(int)BodyPart.LeftKnee] = CreateLimb(ref lKneeBone, lFoot, headWidth * 0.5f);
            ragdoll.RigidBodies[(int)BodyPart.LeftKnee].UserIndex = info.LeftKnee;
            //rKnee
            var rKneeBone = accessor.GetWorldTransform(info.RightKnee);
            var rFoot = accessor.Pos(info.RightFoot);
            ragdoll.RigidBodies[(int)BodyPart.RightKnee] = CreateLimb(ref rKneeBone, rFoot, headWidth * 0.5f);
            ragdoll.RigidBodies[(int)BodyPart.RightKnee].UserIndex = info.RightKnee;
            //body
            var pelvis = accessor.Pos(info.Pelvis);
            var pelvisBone = accessor.GetWorldTransform(info.Pelvis);
            var bodyHeight = (head - pelvis).Length;
            var bodyShape = new CapsuleShape(bodyWidth / 2, bodyHeight - bodyWidth);
            var bodyRigidTrans = BulletMathUtils.CenterOf(pelvis, head).LookAt(in head, in Vector3.UnitY);
            ragdoll.RigidBodies[(int)BodyPart.Pelvis] = BulletHelper.CreateBoneRigidbody(3, ref pelvisBone, ref bodyRigidTrans, bodyShape);
            ragdoll.RigidBodies[(int)BodyPart.Pelvis].UserIndex = info.Pelvis;

            //==============Constraint=============
            Debug.LogLine("Now setup constraint...");
            var bodys = ragdoll.RigidBodies;
            //ragdoll.Constraints[0] = CreateJoint(bodys[(int)BodyPart.Head], bodys[(int)BodyPart.Pelvis], head);
            var bodyHead = bodys[(int)BodyPart.Head];
            var bodyPelvis = bodys[(int)BodyPart.Pelvis];
            var jointTrans = Matrix.Translation(head).LookAt(rigidOrigin, Vector3.UnitX);
            var localHead = jointTrans * bodyHead.WorldTransform.GetInverse();
            var localPelvis = jointTrans * bodyPelvis.WorldTransform.GetInverse();
            ragdoll.Constraints[0] = new ConeTwistConstraint(bodyHead, bodyPelvis, localHead, localPelvis);
            (ragdoll.Constraints[0] as ConeTwistConstraint).SetLimit((float)Math.PI / 7, (float)Math.PI / 7, (float)Math.PI / 4);
            ragdoll.Constraints[1] = CreateJoint(bodys[(int)BodyPart.LeftArm], bodys[(int)BodyPart.Pelvis], lArm);
            ragdoll.Constraints[2] = CreateJoint(bodys[(int)BodyPart.RightArm], bodys[(int)BodyPart.Pelvis], rArm);
            ragdoll.Constraints[3] = CreateJoint(bodys[(int)BodyPart.LeftElbow], bodys[(int)BodyPart.LeftArm], lElbow);
            ragdoll.Constraints[4] = CreateJoint(bodys[(int)BodyPart.RightElbow], bodys[(int)BodyPart.RightArm], rElbow);
            ragdoll.Constraints[5] = CreateJoint(bodys[(int)BodyPart.LeftHip], bodys[(int)BodyPart.Pelvis], accessor.Pos(info.LeftHip));
            ragdoll.Constraints[6] = CreateJoint(bodys[(int)BodyPart.RightHip], bodys[(int)BodyPart.Pelvis], accessor.Pos(info.RightHip));
            ragdoll.Constraints[7] = CreateJoint(bodys[(int)BodyPart.LeftKnee], bodys[(int)BodyPart.LeftHip], lKnee);
            ragdoll.Constraints[8] = CreateJoint(bodys[(int)BodyPart.RightKnee], bodys[(int)BodyPart.RightHip], rKnee);

            foreach (var i in ragdoll.RigidBodies)
            {
                i.SetDamping(0.05f, 0.85f);
                i.DeactivationTime = 0.8f;
                i.SetSleepingThresholds(1.6f, 2.5f);
                i.Friction = 2;
            }
            // damping, friction and restitution document forum
            //https://pybullet.org/Bullet/phpBB3/viewtopic.php?t=8100
            // What is the unit of Damping in Generic Spring Constraint? 
            // https://github.com/bulletphysics/bullet3/issues/345
            foreach (var i in ragdoll.Constraints)
            {
                i.DebugDrawSize = 5;
            }
            Debug.LogLine("Bipped build complete.");
            return ragdoll;
        }
    }
    public class BippedBone
    {
        static Dictionary<string, BippedBone> KeyValues;
        public static BippedBone Get(string modelName)
        {
            return KeyValues[modelName];
        }
        static BippedBone()
        {
            KeyValues = new Dictionary<string, BippedBone>();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(KeyValues.GetType());
            using (var stream = File.OpenRead("RagdollBone.json"))
            {
                KeyValues =(Dictionary<string,BippedBone>)serializer.ReadObject(stream);
            }
        }
        public int Head;
        public int Spine;
        public int Pelvis;
        public int LeftArm;
        public int LeftElbow;
        public int LeftHand;
        public int LeftHip;
        public int LeftKnee;
        public int LeftFoot;
        public int RightArm;
        public int RightElbow;
        public int RightHand;
        public int RightHip;
        public int RightKnee;
        public int RightFoot;
    }
}
