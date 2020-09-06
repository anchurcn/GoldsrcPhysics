using BulletSharp;
using BulletSharp.Math;
using GoldsrcPhysics.Goldsrc;
using GoldsrcPhysics.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;

namespace GoldsrcPhysics
{
    /// <summary>
    /// 这个类提供了构造布娃娃的方法，可以通过模型信息或者额外文件构造布娃娃的相关数据（碰撞体，各种变换）
    /// 如果构造出来的对象为null，则说明此模型在此构建配置中不支持布娃娃。
    /// 
    /// 在此维护一个RagdollData缓存，根据算法构造的RagdollData的Key=ModelName+nameof（BuildOption）
    /// </summary>
    class RagdollBuilder
    {
        internal enum BodyPart
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
        internal enum BuildOption
        {
            Default,
            FromFile,
            Bipped,
            Spider
        }
        public static BRagdoll Build(string modelName, BuildOption buildOption, BuildOption fallBackOption = BuildOption.Default)
        {
            BRagdoll result = null;
            result = Build(modelName, buildOption);
            if (result == null)
            {
                Debug.LogLine("Now use fall back option");
                result = Build(modelName, fallBackOption);
            }
            return result;
        }
        public static BRagdoll Build(string modelName, BuildOption buildOption)
        {
            switch (buildOption)
            {
                case BuildOption.Default:
                    return Build(modelName, (BuildOption)Enum.Parse(buildOption.GetType(), PhyConfiguration.GetValue("BuildOption")));
                case BuildOption.FromFile:
                    break;
                case BuildOption.Bipped:
                    {
                        var info = BippedBone.Get(modelName);
                        if (info != null)
                            return BuildBipped(info);
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
        private static BRagdoll BuildBipped(BippedBone info)
        {
            Debug.LogLine("Build Bipped...");
            BoneAccessor accessor = BoneAccessor.GetCurrent();
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
            var ragdoll = new BRagdoll()
            {
                BodyParts = new RigidBody[(int)BodyPart.Count],
                Constraints = new TypedConstraint[9],
                BoneRelativeTransform = new Matrix34f[accessor.BoneCount],
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
            var headRaito = 5.85f / 9;//头宽高比
            var headHeight = headWidth / headRaito;
            var headShape = new CapsuleShape(headWidth / 2, headHeight - headWidth);
            var rigidOrigin = head;
            rigidOrigin.Z += headHeight / 2;
            var headRigid = Matrix.Translation(rigidOrigin).LookAt(in head, in Vector3.UnitY);
            var headBone = accessor.GetWorldTransform(info.Head);
            ragdoll.BodyParts[(int)BodyPart.Head] = BulletHelper.CreateBoneRigidbody(1, ref headBone, ref headRigid, headShape);
            ragdoll.BodyParts[(int)BodyPart.Head].UserIndex = info.Head;

            //lArm
            var lArmBone = accessor.GetWorldTransform(info.LeftArm);
            var lElbow = accessor.Pos(info.LeftElbow);
            ragdoll.BodyParts[(int)BodyPart.LeftArm] = CreateLimb(ref lArmBone, lElbow, headWidth);
            ragdoll.BodyParts[(int)BodyPart.LeftArm].UserIndex = info.LeftArm;
            //rArm
            var rArmBone = accessor.GetWorldTransform(info.LeftArm);
            var rElbow = accessor.Pos(info.RightElbow);
            ragdoll.BodyParts[(int)BodyPart.RightArm] = CreateLimb(ref rArmBone, rElbow, headWidth);
            ragdoll.BodyParts[(int)BodyPart.RightArm].UserIndex = info.RightArm;
            //lElbow
            var lElbowBone = accessor.GetWorldTransform(info.LeftElbow);
            var lHand = accessor.Pos(info.LeftHand);
            ragdoll.BodyParts[(int)BodyPart.LeftElbow] = CreateLimb(ref lElbowBone, lHand, headWidth * 0.6f);
            ragdoll.BodyParts[(int)BodyPart.LeftElbow].UserIndex = info.LeftElbow;
            //rElbow
            var rElbowBone = accessor.GetWorldTransform(info.RightElbow);
            var rHand = accessor.Pos(info.RightHand);
            ragdoll.BodyParts[(int)BodyPart.RightElbow] = CreateLimb(ref rElbowBone, rHand, headWidth * 0.6f);
            ragdoll.BodyParts[(int)BodyPart.RightElbow].UserIndex = info.RightElbow;
            //lHip
            var lHipBone = accessor.GetWorldTransform(info.LeftHip);
            var lKnee = accessor.Pos(info.LeftKnee);
            ragdoll.BodyParts[(int)BodyPart.LeftHip] = CreateLimb(ref lHipBone, lKnee, headWidth);
            ragdoll.BodyParts[(int)BodyPart.LeftHip].UserIndex = info.LeftHip;
            //rHip
            var rHipBone = accessor.GetWorldTransform(info.RightHip);
            var rKnee = accessor.Pos(info.RightKnee);
            ragdoll.BodyParts[(int)BodyPart.RightHip] = CreateLimb(ref rHipBone, rKnee, headWidth);
            ragdoll.BodyParts[(int)BodyPart.RightHip].UserIndex = info.RightHip;
            //lKnee
            var lKneeBone = accessor.GetWorldTransform(info.LeftKnee);
            var lFoot = accessor.Pos(info.LeftFoot);
            ragdoll.BodyParts[(int)BodyPart.LeftKnee] = CreateLimb(ref lKneeBone, lFoot, headWidth * 0.7f);
            ragdoll.BodyParts[(int)BodyPart.LeftKnee].UserIndex = info.LeftKnee;
            //rKnee
            var rKneeBone = accessor.GetWorldTransform(info.RightKnee);
            var rFoot = accessor.Pos(info.RightFoot);
            ragdoll.BodyParts[(int)BodyPart.RightKnee] = CreateLimb(ref rKneeBone, rFoot, headWidth * 0.7f);
            ragdoll.BodyParts[(int)BodyPart.RightKnee].UserIndex = info.RightKnee;
            //body
            var pelvis = accessor.Pos(info.Pelvis);
            var pelvisBone = accessor.GetWorldTransform(info.Pelvis);
            var bodyHeight = (head - pelvis).Length;
            var bodyShape = new CapsuleShape(bodyWidth / 2, bodyHeight - bodyWidth);
            var bodyRigidTrans = BulletMathUtils.CenterOf(pelvis, head).LookAt(in head, in Vector3.UnitY);
            ragdoll.BodyParts[(int)BodyPart.Pelvis] = BulletHelper.CreateBoneRigidbody(3, ref pelvisBone, ref bodyRigidTrans, bodyShape);
            ragdoll.BodyParts[(int)BodyPart.Pelvis].UserIndex = info.Pelvis;

            //==============Constraint=============
            Debug.LogLine("Now setup constraint...");
            var bodys = ragdoll.BodyParts;
            ragdoll.Constraints[0] = CreateJoint(bodys[(int)BodyPart.Head], bodys[(int)BodyPart.Pelvis], head);
            ragdoll.Constraints[1] = CreateJoint(bodys[(int)BodyPart.LeftArm], bodys[(int)BodyPart.Pelvis], lArm);
            ragdoll.Constraints[2] = CreateJoint(bodys[(int)BodyPart.RightArm], bodys[(int)BodyPart.Pelvis], rArm);
            ragdoll.Constraints[3] = CreateJoint(bodys[(int)BodyPart.LeftElbow], bodys[(int)BodyPart.LeftArm], lElbow);
            ragdoll.Constraints[4] = CreateJoint(bodys[(int)BodyPart.RightElbow], bodys[(int)BodyPart.RightArm], rElbow);
            ragdoll.Constraints[5] = CreateJoint(bodys[(int)BodyPart.LeftHip], bodys[(int)BodyPart.Pelvis], accessor.Pos(info.LeftHip));
            ragdoll.Constraints[6] = CreateJoint(bodys[(int)BodyPart.RightHip], bodys[(int)BodyPart.Pelvis], accessor.Pos(info.RightHip));
            ragdoll.Constraints[7] = CreateJoint(bodys[(int)BodyPart.LeftKnee], bodys[(int)BodyPart.LeftHip], lKnee);
            ragdoll.Constraints[8] = CreateJoint(bodys[(int)BodyPart.LeftKnee], bodys[(int)BodyPart.RightHip], rKnee);

            Debug.LogLine("Build Bipped complete.");
            return ragdoll;
        }
        private static TypedConstraint CreateJoint(RigidBody bodyA, RigidBody bodyB, in Vector3 pivot)
        {
            var pivotInA = bodyA.TransformToLocal(in pivot);
            var pivotInB = bodyB.TransformToLocal(in pivot);
            return new Point2PointConstraint(bodyA, bodyB, pivotInA, pivotInB);
        }
        private static RigidBody CreateLimb(ref Matrix bone, Vector3 child, float radius)
        {
            var len = (bone.Origin - child).Length;
            var shape = new CapsuleShape(radius, len);
            var rigidTrans = BulletMathUtils.CenterOf(bone.Origin, child).LookAt(child, Vector3.UnitY);

            var mass = 1.0f;//TODO: auto calc mass via shape volumn
            return BulletHelper.CreateBoneRigidbody(mass, ref bone, ref rigidTrans, shape);
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
