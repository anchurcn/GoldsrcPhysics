using BulletSharp;
using BulletSharp.Math;
using GoldsrcPhysics.Goldsrc;
using GoldsrcPhysics.Goldsrc.Bsp;
using GoldsrcPhysics.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Json;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static GoldsrcPhysics.Goldsrc.Studio_h;
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
    public partial class RagdollBuilder
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
        public static Ragdoll Build(string modelName)
        {
            return BuildInternal(modelName);
        }
        public unsafe static Ragdoll Build(model_t* model)
        {

            return BuildBipped(IEngineStudio.Mod_Extradata(model), BippedBoneManager.GetBippedBone(model), BoneAccessor.Get(IEngineStudio.Mod_Extradata(model)));
        }

        /// <summary>
        /// Build ragdoll to the best of its ability.
        /// </summary>
        /// <returns></returns>
        private static unsafe Ragdoll BuildInternal(string modelName)
        {
            // try build from file

            // otherwise using build options to auto calc ragdoll.
            IntPtr pName = Marshal.StringToHGlobalAnsi(modelName);
            var model = IEngineStudio.Mod_ForName((sbyte*)pName, true);
            Marshal.FreeHGlobal(pName);
            var hdr = IEngineStudio.Mod_Extradata(model);
            throw new NotImplementedException();
        }
        //public static Ragdoll Build(string modelName, BuildOption buildOption)
        //{
        //    switch (buildOption)
        //    {
        //        case BuildOption.Default:
        //            var defultOption = PhyConfiguration.GetValue("BuildOption");
        //            Debug.LogLine("Default ragdoll build option is [{0}]", defultOption);
        //            return Build(modelName, (BuildOption)Enum.Parse(buildOption.GetType(),defultOption));
        //        case BuildOption.FromFile:
        //            break;
        //        case BuildOption.Bipped:
        //            {
        //                var info = BippedBone.Get(modelName);
        //                if (info != null)
        //                    return BuildBipped(IEngineStudio.Mod_ForName(modelName,true),info,BoneAccessor.GetCurrent());
        //                Debug.LogLine("BippedBone missing. Model:{0}", modelName);
        //            }
        //            break;
        //        case BuildOption.Spider:
        //            break;
        //        default:
        //            break;
        //    }
        //    //missing inoformation to build for this model
        //    return null;
        //}

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
        private static unsafe Ragdoll BuildBipped(studiohdr_t* pStudioHeader, BippedBone info, BoneAccessor accessor)
        {
            Debug.LogLine();
            Debug.LogLine("=========================================");
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
            var ragdoll = new Ragdoll(pStudioHeader)
            {
                RigidBodies = new RigidBody[(int)BodyPart.Count],
                Constraints = new TypedConstraint[9],
                BoneRelativeTransform = new Matrix[accessor.BoneCount],
                EntityId = 0,
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
            var headShape = new SphereShape(headHeight / 2);
            var rigidOrigin = head;
            rigidOrigin.Z += headHeight / 2;
            rigidOrigin.X += headWidth / 4.6f;
            var headLookAt = head;
            headLookAt.X += headWidth / 4.6f;
            var headRigid = Matrix.Translation(rigidOrigin);
            var headBone = accessor.GetWorldTransform(info.Head);
            ragdoll.RigidBodies[(int)BodyPart.Head] = BulletHelper.CreateBoneRigidbody(1, ref headBone, ref headRigid, headShape);
            ragdoll.RigidBodies[(int)BodyPart.Head].UserIndex = info.Head;

            //lArm
            var lArmBone = accessor.GetWorldTransform(info.LeftArm);
            var lElbow = accessor.Pos(info.LeftElbow);
            ragdoll.RigidBodies[(int)BodyPart.LeftArm] = CreateLimb(ref lArmBone, lElbow, headWidth * 0.55f);
            ragdoll.RigidBodies[(int)BodyPart.LeftArm].UserIndex = info.LeftArm;
            //rArm
            var rArmBone = accessor.GetWorldTransform(info.RightArm);
            var rElbow = accessor.Pos(info.RightElbow);
            ragdoll.RigidBodies[(int)BodyPart.RightArm] = CreateLimb(ref rArmBone, rElbow, headWidth * 0.55f);
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
            ragdoll.RigidBodies[(int)BodyPart.LeftHip] = CreateLimb(ref lHipBone, lKnee, headWidth * 0.55f);
            ragdoll.RigidBodies[(int)BodyPart.LeftHip].UserIndex = info.LeftHip;
            //rHip
            var rHipBone = accessor.GetWorldTransform(info.RightHip);
            var rKnee = accessor.Pos(info.RightKnee);
            ragdoll.RigidBodies[(int)BodyPart.RightHip] = CreateLimb(ref rHipBone, rKnee, headWidth * 0.55f);
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
            // head constraint
            {
                //ragdoll.Constraints[0] = CreateJoint(bodys[(int)BodyPart.Head], bodys[(int)BodyPart.Pelvis], head);
                var bodyHead = bodys[(int)BodyPart.Head];
                var bodyPelvis = bodys[(int)BodyPart.Pelvis];
                var jointTrans = Matrix.Translation(head).LookAt(rigidOrigin, Vector3.UnitX);
                var localHead = jointTrans * bodyHead.WorldTransform.GetInverse();
                var localPelvis = jointTrans * bodyPelvis.WorldTransform.GetInverse();
                ragdoll.Constraints[0] = new ConeTwistConstraint(bodyHead, bodyPelvis, localHead, localPelvis);
                (ragdoll.Constraints[0] as ConeTwistConstraint).SetLimit((float)Math.PI / 6.5f, (float)Math.PI / 6.5f, (float)Math.PI * 0.3333333f);
            }
            // left arm constraint
            {
                //ragdoll.Constraints[1] = CreateJoint(bodys[(int)BodyPart.LeftArm], bodys[(int)BodyPart.Pelvis], lArm);
                var bodyLeftArm = bodys[(int)BodyPart.LeftArm];
                var bodyPelvis = bodys[(int)BodyPart.Pelvis];

                var jointOrigin = accessor.Pos(info.LeftArm);
                var jointTransform = Matrix.Translation(jointOrigin).LookAt(jointOrigin + new Vector3(0, -1, 0), Vector3.UnitX);

                var localInLeftArm = jointTransform * bodyLeftArm.WorldTransform.GetInverse();
                var localPelvis = jointTransform * bodyPelvis.WorldTransform.GetInverse();

                var joint = new ConeTwistConstraint(bodyLeftArm, bodyPelvis, localInLeftArm, localPelvis);
                joint.SetLimit(3.1415926f * 0.6f, 3.1415926f * 0.6f, 0);
                ragdoll.Constraints[1] = joint;
            }
            // right arm constraint
            {
                //ragdoll.Constraints[2] = CreateJoint(bodys[(int)BodyPart.RightArm], bodys[(int)BodyPart.Pelvis], rArm);
                var bodyRightArm = bodys[(int)BodyPart.RightArm];
                var bodyPelvis = bodys[(int)BodyPart.Pelvis];

                var jointOrigin = accessor.Pos(info.RightArm);
                var jointTransform = Matrix.Translation(jointOrigin).LookAt(jointOrigin + new Vector3(0, 1, 0), Vector3.UnitX);

                var localInRightArm = jointTransform * bodyRightArm.WorldTransform.GetInverse();
                var localPelvis = jointTransform * bodyPelvis.WorldTransform.GetInverse();

                var joint = new ConeTwistConstraint(bodyRightArm, bodyPelvis, localInRightArm, localPelvis);
                joint.SetLimit(3.1415926f * 0.6f, 3.1415926f * 0.6f, 0);
                ragdoll.Constraints[2] = joint;
            }
            {// Left elbow
                //ragdoll.Constraints[3] = CreateJoint(bodys[(int)BodyPart.LeftElbow], bodys[(int)BodyPart.LeftArm], lElbow);

                var bodyArm = bodys[(int)BodyPart.LeftArm];
                var bodyElbow = bodys[(int)BodyPart.LeftElbow];


                var jointTrans = Matrix.Translation(lElbow).LookAt(lElbow + new Vector3(0, 0, -1), Vector3.UnitZ);
                // joint's local transform
                var localInArm = jointTrans * bodyArm.WorldTransform.GetInverse();
                var localInElbow = jointTrans * bodyElbow.WorldTransform.GetInverse();
                var joint = new HingeConstraint(bodyArm, bodyElbow, localInArm, localInElbow);
                joint.SetAxis(Vector3.UnitZ);
                joint.SetLimit(-(float)Math.PI * 0.7f, 1);
                ragdoll.Constraints[3] = joint;
            }
            {// right elbow
                //ragdoll.Constraints[4] = CreateJoint(bodys[(int)BodyPart.RightElbow], bodys[(int)BodyPart.RightArm], rElbow);

                var bodyArm = bodys[(int)BodyPart.RightArm];
                var bodyElbow = bodys[(int)BodyPart.RightElbow];


                var jointTrans = Matrix.Translation(rElbow).LookAt(rElbow + new Vector3(0, 0, -1), Vector3.UnitZ);
                // joint's local transform
                var localInArm = jointTrans * bodyArm.WorldTransform.GetInverse();
                var localInElbow = jointTrans * bodyElbow.WorldTransform.GetInverse();
                var joint = new HingeConstraint(bodyArm, bodyElbow, localInArm, localInElbow);
                joint.SetAxis(Vector3.UnitZ);
                joint.SetLimit(-(float)Math.PI * 0.7f, 1);
                ragdoll.Constraints[4] = joint;
            }
            {
                //ragdoll.Constraints[5] = CreateJoint(bodys[(int)BodyPart.LeftHip], bodys[(int)BodyPart.Pelvis], accessor.Pos(info.LeftHip));
                var bodyHipL = bodys[(int)BodyPart.LeftHip];
                var bodyPelvis = bodys[(int)BodyPart.Pelvis];
                var leglen = (accessor.Pos(info.LeftHip) - lKnee).Length;
                var x = (float)Math.Cos(Math.PI / 6) * leglen;
                var z = (float)Math.Sin(Math.PI / 6) * leglen;
                var posA = accessor.Pos(info.LeftHip);
                posA.Z += z;
                posA.X += x;
                var posB = lKnee;
                var posCenter = (posA + posB) / 2;

                var jointTrans = Matrix.Translation(accessor.Pos(info.LeftHip)).LookAt(posCenter, Vector3.UnitX);
                var localPelvis = jointTrans * bodyPelvis.WorldTransform.GetInverse();
                var localHipL = jointTrans * bodyHipL.WorldTransform.GetInverse();

                ragdoll.Constraints[5] = new ConeTwistConstraint(bodyHipL, bodyPelvis, localHipL, localPelvis);
                (ragdoll.Constraints[5] as ConeTwistConstraint).SetLimit((float)Math.PI / 4, (float)Math.PI / 4, (float)Math.PI / 12);
            }
            {
                //ragdoll.Constraints[6] = CreateJoint(bodys[(int)BodyPart.RightHip], bodys[(int)BodyPart.Pelvis], accessor.Pos(info.RightHip));
                var bodyHipR = bodys[(int)BodyPart.RightHip];
                var bodyPelvis = bodys[(int)BodyPart.Pelvis];
                var leglen = (accessor.Pos(info.RightHip) - rKnee).Length;
                var x = (float)Math.Cos(Math.PI / 6) * leglen;
                var z = (float)Math.Sin(Math.PI / 6) * leglen;
                var posA = accessor.Pos(info.RightHip);
                posA.Z += z;
                posA.X += x;
                var posB = rKnee;
                var posCenter = (posA + posB) / 2;

                var jointTrans = Matrix.Translation(accessor.Pos(info.RightHip)).LookAt(posCenter, Vector3.UnitX);
                var localPelvis = jointTrans * bodyPelvis.WorldTransform.GetInverse();
                var localHipR = jointTrans * bodyHipR.WorldTransform.GetInverse();

                ragdoll.Constraints[6] = new ConeTwistConstraint(bodyHipR, bodyPelvis, localHipR, localPelvis);
                (ragdoll.Constraints[6] as ConeTwistConstraint).SetLimit((float)Math.PI / 4, (float)Math.PI / 4, (float)Math.PI / 12);
            }
            {// left knee joint
                {// p2p joint
                    //ragdoll.Constraints[7] = CreateJoint(bodys[(int)BodyPart.LeftKnee], bodys[(int)BodyPart.LeftHip], lKnee);
                }
                {// hinge joint 1
                    var bodyHipL = bodys[(int)BodyPart.LeftHip];
                    var bodyKneeL = bodys[(int)BodyPart.LeftKnee];


                    var jointTrans = Matrix.Translation(lKnee).LookAt(lKnee + new Vector3(0, 1, 0), Vector3.UnitZ);
                    // joint's local transform in left hip
                    var localInHipL = jointTrans * bodyHipL.WorldTransform.GetInverse();
                    var localInKneeL = jointTrans * bodyKneeL.WorldTransform.GetInverse();
                    ragdoll.Constraints[7] = new HingeConstraint(bodyHipL, bodyKneeL, localInHipL, localInKneeL);
                    (ragdoll.Constraints[7] as HingeConstraint).SetLimit(-(float)Math.PI * 0.7f, 0);
                }
                {// hinge joint 2
                    //var bodyHipL = bodys[(int)BodyPart.LeftHip];
                    //var bodyKneeL = bodys[(int)BodyPart.LeftKnee];

                    //Vector3 axisInA = new Vector3(1, 0, 0);

                    //Matrix transform = Matrix.Invert(bodyB.WorldTransform) * bodyA.WorldTransform;
                    //Vector3 pivotInB = Vector3.TransformCoordinate(pivotInA, transform);

                    //transform = Matrix.Invert(bodyB.WorldTransform) * bodyB.WorldTransform;
                    //Vector3 axisInB = Vector3.TransformCoordinate(axisInA, transform);
                    //var jointTrans = Matrix.Translation(lKnee).LookAt(lKnee + new Vector3(0, 1, 0), Vector3.UnitZ);
                    //// joint's local transform in left hip
                    //var localInHipL = jointTrans * bodyHipL.WorldTransform.GetInverse();
                    //var localInKneeL = jointTrans * bodyKneeL.WorldTransform.GetInverse();
                    //ragdoll.Constraints[7] = new HingeConstraint(bodyHipL, bodyKneeL, localInHipL, localInKneeL);
                    //(ragdoll.Constraints[7] as HingeConstraint).SetLimit(-(float)Math.PI / 6, (float)Math.PI / 6);
                }

            }
            {
                //ragdoll.Constraints[8] = CreateJoint(bodys[(int)BodyPart.RightKnee], bodys[(int)BodyPart.RightHip], rKnee);

                {// hinge joint 1
                    var bodyHipR = bodys[(int)BodyPart.RightHip];
                    var bodyKneeR = bodys[(int)BodyPart.RightKnee];


                    var jointTrans = Matrix.Translation(rKnee).LookAt(rKnee + new Vector3(0, 1, 0), Vector3.UnitZ);
                    // joint's local transform in left hip
                    var localInHipR = jointTrans * bodyHipR.WorldTransform.GetInverse();
                    var localInKneeR = jointTrans * bodyKneeR.WorldTransform.GetInverse();
                    ragdoll.Constraints[8] = new HingeConstraint(bodyHipR, bodyKneeR, localInHipR, localInKneeR);
                    (ragdoll.Constraints[8] as HingeConstraint).SetLimit(-(float)Math.PI * 0.7f, 0);
                }
            }
            foreach (var i in ragdoll.RigidBodies)
            {
                i.SetDamping(0.05f, 0.85f);
                i.DeactivationTime = 0.8f;
                i.SetSleepingThresholds(1.6f, 2.5f);
                i.Friction = 2.2f;//v1.0 is 2
            }
            // damping, friction and restitution document forum
            //https://pybullet.org/Bullet/phpBB3/viewtopic.php?t=8100
            // What is the unit of Damping in Generic Spring Constraint? 
            // https://github.com/bulletphysics/bullet3/issues/345
            foreach (var i in ragdoll.Constraints)
            {
                i.DebugDrawSize = 3;
            }
            Debug.LogLine("Bipped build complete.");
            Debug.LogLine("=========================================");
            return ragdoll;
        }
    }
    public class BippedBone
    {
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

    internal unsafe static class BippedBoneManager
    {
        private static Dictionary<long, BippedBone> _cache;

        /// <summary>
        /// RagdollBoneNaming
        /// </summary>
        private static List<BippedBoneNaming> _boneNamings;
        /// <summary>
        /// RagdollBone
        /// </summary>
        private static Dictionary<string, BippedBone> _dicBippedBone;
        private static FieldInfo[] _namingFields;
        private static Type _bippedBoneType;

        internal static void Init()
        {
            _cache = new Dictionary<long, BippedBone>(512);
            _boneNamings = new List<BippedBoneNaming>();
            _namingFields = typeof(BippedBoneNaming).GetFields();
            _bippedBoneType = typeof(BippedBone);
            LoadRagdollBoneNaming();
            LoadRagdollBone();
        }

        /// <summary>
        /// Get BippedBone for specified model.
        /// </summary>
        /// <param name="pModel"></param>
        /// <returns></returns>
        internal static BippedBone GetBippedBone(model_t* pModel)
        {
            BippedBone bippedBone = null;
            long key = (long)pModel;
            // cache missing
            if (!_cache.TryGetValue(key, out bippedBone))
            {
                // calc via naming convension
                bippedBone = GenerateBippedBone(pModel);
                // special treatment
                if (bippedBone == null)
                {
                    string name = Marshal.PtrToStringAnsi((IntPtr)pModel->name);
                    name = Path.GetFileNameWithoutExtension(name);
                    // null or not null
                    bippedBone = GetBippedBone(name);
                }
                _cache.Add(key, bippedBone);
            }
            return bippedBone;
        }
        private static void LoadRagdollBoneNaming()
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(_boneNamings.GetType());
            using (var stream = File.OpenRead(@"gsphysics\RagdollBoneNaming.json"))
            {
                _boneNamings = (List<BippedBoneNaming>)serializer.ReadObject(stream);
            }
        }
        private static void LoadRagdollBone()
        {
            _dicBippedBone = new Dictionary<string, BippedBone>();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(_dicBippedBone.GetType());
            using (var stream = File.OpenRead(Path.Combine(PhyConfiguration.GetValue("ModDir"), @"phydata\RagdollBone.json")))
            {
                _dicBippedBone = (Dictionary<string, BippedBone>)serializer.ReadObject(stream);
            }
        }

        private static BippedBone GenerateBippedBone(model_t* pModel)
        {
            var pStudioModel = IEngineStudio.Mod_Extradata(pModel);
            BippedBone result = new BippedBone();

            // If there exist any naming convension to find all index great than 0.
            if (_boneNamings.Any(
                x => _namingFields.All(
                    field =>
                    {
                        var index = GetBoneIndex((string)field.GetValue(x), pStudioModel);
                        _bippedBoneType.GetField(field.Name).SetValue(result, index);
                        return index > 0;
                    })
                ))
                return result;
            else
                return null;


        }
        /// <summary>
        /// Find a bone named given "boneName" in a model and reutrn its index.
        /// </summary>
        /// <param name="boneName"></param>
        /// <param name="pStudioModel"></param>
        /// <returns>Bone index for given name. -1 represents not found.</returns>
        private static int GetBoneIndex(string boneName, studiohdr_t* pStudioModel)
        {
            var bones = (mstudiobone_t*)((byte*)pStudioModel + pStudioModel->boneindex);
            for (int i = 0; i < pStudioModel->numbones; i++)
            {
                var name = Marshal.PtrToStringAnsi((IntPtr)bones[i].name);
                if (boneName == name)
                {
                    return i;
                }
            }
            return -1;
        }

        private static BippedBone GetBippedBone(string modelName)
        {
            BippedBone result = null;
            if (!_dicBippedBone.TryGetValue(modelName, out result))
                result = null;
            return result;
        }
        
    }

    /// <summary>
    /// Key bone naming conventions.
    /// </summary>
    public unsafe class BippedBoneNaming
    {
        public string Head;
        public string Spine;
        public string Pelvis;
        public string LeftArm;
        public string LeftElbow;
        public string LeftHand;
        public string LeftHip;
        public string LeftKnee;
        public string LeftFoot;
        public string RightArm;
        public string RightElbow;
        public string RightHand;
        public string RightHip;
        public string RightKnee;
        public string RightFoot;
    }
}
