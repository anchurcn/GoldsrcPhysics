using BulletSharp;
using BulletSharp.Math;
using GoldsrcPhysics.Goldsrc;
using GoldsrcPhysics.Utils;
using System;
using System.Collections.Generic;
using static GoldsrcPhysics.Goldsrc.Studio_h;

namespace GoldsrcPhysics
{
    /*
      布娃娃的三个阶段
      1.预缓存碰撞形状（box，capsule，凸包）、约束信息、刚体骨骼关系信息，一个模型一组，多个布娃娃实例共用
      2.玩家加入游戏时构造布娃娃对象（刚体，约束），但此时并不添加进物理世界
      3.玩家死亡时将之前构造的布娃娃对象激活（Enable），下一帧布娃娃开始接管骨骼控制
     */
     
    
    public class RagdollData
    {
        public struct ConstraintInfo
        {
            public int RigidA;
            public int RigidB;
            public Vector3 PivotInA;
            public Vector3 PivotInB;
            public TypedConstraintType Type;
        }
        public struct BodyPartInfo
        {
            public int CollisionShape;
            public int BoneIndex;
            /// <summary>
            /// the rigidbody local transform relatived to attached bone.
            /// RigidOffset = RigidWorldTransform * BoneWorldTransform^-1.
            /// Rigid = Offset * Bone.
            /// Bone = Offset^-1 * Rigid.
            /// </summary>
            public Matrix RigidOffset;
        }
        public string Version;
        public List<byte> Geometry;
        public List<byte> Material;
        public List<CollisionShape> CollisionShapes;
        public List<BodyPartInfo> BodyPartInfos;
        public List<ConstraintInfo> ConstraintInfos;
        public List<int> KeyBoneIndeces;
        public List<int> NonKeyBoneIndeces;

    }
    /// <summary>
    /// provide methods to control goldsrc animations
    /// only used when current model being rendered is your target being controlled
    /// </summary>
    public unsafe class Ragdoll
    {
        
        /// <summary>
        /// The entity id that attached to.
        /// </summary>
        public int EntityId { get; set; }

        internal Matrix[] BoneRelativeTransform;
        internal RagdollData RagdollData;
        internal RigidBody[] RigidBodies;
        internal TypedConstraint[] Constraints;

        private readonly studiohdr_t* _pStudioHeader;

        private bool _enabled = false;

        internal DynamicsWorld World = BWorld.Instance;

        /// <summary>
        /// Instantiate a ragdoll for given model.
        /// </summary>
        /// <param name="pStudioHeader"></param>
        public Ragdoll(studiohdr_t* pStudioHeader)
        {
            _pStudioHeader = pStudioHeader;
        }

        /// <summary>
        /// Set pose for this ragdoll.
        /// </summary>
        /// <param name="pBoneWorldTransform"></param>
        /// <param name="pStudioHeader"></param>
        public void SetPose(float* pBoneWorldTransform)
        {
            mstudiobone_t* bones = (mstudiobone_t*)((byte*)_pStudioHeader + _pStudioHeader->boneindex);
            StudioTransforms scaledBoneTransform = new StudioTransforms(pBoneWorldTransform);

            //骨骼变换快照
            for (int i = 0; i < _pStudioHeader->numbones; i++)
            {
                var parent = bones[i].parent;
                if (parent == -1)
                {
                    BoneRelativeTransform[i] = scaledBoneTransform[i];
                }
                else
                {
                    BoneRelativeTransform[i] = scaledBoneTransform[i] *
                        scaledBoneTransform[parent].GetInverse();
                }
            }
            //初始化刚体变换
            for (int i = 0; i < RigidBodies.Length; i++)
            {
                //先将骨骼变换赋给motionstate
                (RigidBodies[i].MotionState as BoneMotionState).BoneTransform = scaledBoneTransform[RigidBodies[i].UserIndex];
                //刚体变换
                RigidBodies[i].WorldTransform = RigidBodies[i].MotionState.WorldTransform;
            }
        }

        /// <summary>
        /// The ragdoll start to control the entity that attached to.
        /// </summary>
        public void Enable()
        {
            if (_enabled)
                return;
            _enabled = true;

            {//reset body states
                foreach (var i in RigidBodies)
                {
                    i.ClearForces();
                    i.LinearVelocity = Vector3.Zero;
                    i.AngularVelocity = Vector3.Zero;
                    i.Activate();
                }
            }
            AddToWorld();//将布娃娃添加进物理世界

            Debug.LogLine("ragdoll start to control entity {0}.", EntityId);
        }

        /// <summary>
        /// 覆写骨骼变换
        /// </summary>
        public void SetupBones()
        {
            if (_enabled)
                WritePoseToRenderer();
        }

        internal void Dispose()
        {
            for (int i = 0; i < RigidBodies.Length; i++)
            {
                RigidBodies[i].Dispose();
            }
            for (int i = 0; i < Constraints.Length; i++)
            {
                Constraints[i].Dispose();
            }
        }

        public void Disable()
        {
            _enabled = false;
            RemoveFromWorld();
            Debug.LogLine("ragdoll stop to control entity {0}.", EntityId);
        }
        public void SetVelocity(Vector3 v)
        {
            foreach (var i in RigidBodies)
            {
                i.LinearVelocity = v;
            }
        }

        public void WritePoseToRenderer()
        {
            //set up keybone, so that the non-key bone can set up using BoneRelativeTransform
            for (int i = 0; i < RigidBodies.Length; i++)
            {
                var body = RigidBodies[i];
                StudioRenderer.ScaledBoneTransform[body.UserIndex] = (body.MotionState as BoneMotionState).BoneTransform;
            }
            //set up non-key bone
            for (int index = 0; index < RagdollData.NonKeyBoneIndeces.Count; index++)
            {
                var i = RagdollData.NonKeyBoneIndeces[index];
                if (i == -1)// ignore the root
                    continue;

                //Matrix34f.ConcatTransforms(in StudioRenderer.NativePointer->m_pbonetransform[StudioRenderer.Bones[i].parent],
                //    in BoneRelativeTransform[i],
                //    out StudioRenderer.NativePointer->m_pbonetransform[i]);
                StudioRenderer.ScaledBoneTransform[i] = BoneRelativeTransform[i] * StudioRenderer.ScaledBoneTransform[StudioRenderer.Bones[i].parent];
            }
        }

        public void ReadPoseFromRenderer()
        {
            //骨骼变换快照
            for (int i = 0; i < StudioRenderer.BoneCount; i++)
            {
                var parent = StudioRenderer.Bones[i].parent;
                if (parent == -1)
                {
                    BoneRelativeTransform[i] = StudioRenderer.ScaledBoneTransform[i];
                }
                else
                {
                    BoneRelativeTransform[i] = StudioRenderer.ScaledBoneTransform[i] *
                        StudioRenderer.ScaledBoneTransform[parent].GetInverse();
                }
            }
            //初始化刚体变换
            for (int i = 0; i < RigidBodies.Length; i++)
            {
                //先将骨骼变换赋给motionstate
                (RigidBodies[i].MotionState as BoneMotionState).BoneTransform = StudioRenderer.ScaledBoneTransform[RigidBodies[i].UserIndex];
                //刚体变换
                RigidBodies[i].WorldTransform = RigidBodies[i].MotionState.WorldTransform;
            }
        }

        /// <summary>
        /// add to default world
        /// </summary>
        private void AddToWorld()
        {
            for (int i = 0; i < RigidBodies.Length; i++)
            {
                World.AddRigidBody(RigidBodies[i]);
            }
            for (int i = 0; i < Constraints.Length; i++)
            {
                World.AddConstraint(Constraints[i], true);
            }
        }
        private void RemoveFromWorld()
        {
            for (int i = 0; i < RigidBodies.Length; i++)
            {
                World.RemoveRigidBody(RigidBodies[i]);
            }
            for (int i = 0; i < Constraints.Length; i++)
            {
                World.RemoveConstraint(Constraints[i]);
            }
        }

    }
    /// <summary>
    /// Holds the offset between bone's world transform and rigidbody's world transform
    /// Offset = Rigid * Bone^-1;
    /// Rigid = Offset * Bone;
    /// Bone = Offset^1 * Rigid
    /// </summary>
    public class BoneMotionState : MotionState
    {
        /// <summary>
        /// to get bone transform for rendering
        /// </summary>
        public Matrix BoneTransform { get; set; }
        Matrix Offset;
        /// <summary>
        /// as you wish
        /// </summary>
        /// <param name="boneTrans"></param>
        /// <param name="rigidTrans"></param>
        public BoneMotionState(Matrix boneTrans, Matrix rigidTrans)
        {
            BoneTransform = boneTrans;
            Offset = rigidTrans * boneTrans.GetInverse();
        }
        /// <summary>
        /// bone's transform equals rigidbody's transform
        /// </summary>
        /// <param name="matrix"></param>
        public BoneMotionState(Matrix transform)
        {
            BoneTransform = transform;
            Offset = Matrix.Identity;
        }
        public BoneMotionState(Matrix boneTrans,ref Matrix rigidOffset)
        {
            BoneTransform = boneTrans;
            Offset = rigidOffset;
        }
        public override void GetWorldTransform(out Matrix worldTrans)
        {
            worldTrans = Offset * BoneTransform;
        }
        /// <summary>
        /// bullet use this function to set BoneTrans every frame
        /// </summary>
        /// <param name="worldTrans"></param>
        public override void SetWorldTransform(ref Matrix worldTrans)
        {
            BoneTransform = Offset.GetInverse() * worldTrans;
        }

    }
}
