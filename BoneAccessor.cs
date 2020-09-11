using BulletSharp.Math;
using GoldsrcPhysics.Goldsrc;
using GoldsrcPhysics.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics
{
    public unsafe class BoneAccessor
    {
        public static BoneAccessor GetCurrent()
        {
            return new BoneAccessor();
        }

        public static BoneAccessor Get(string name)
        {
            throw new NotImplementedException();
        }
        
        public void SetTPose() { }

        public void SetAnim(int anim) { }
        public void SetFrame(int frame) { }

        public float Scale { get; set; } = 1f;
        public int BoneCount { get => StudioRenderer.BoneCount; }

        public Matrix34f GetLocalTransform(int boneId)
        {
            return (StudioRenderer.BoneTransform[boneId] * ((Matrix)StudioRenderer.BoneTransform[StudioRenderer.Bones[boneId].parent]).GetInverse())*Scale;
        }
        public Matrix34f GetLocalToWorldTransformation(int boneId)
        {
            return StudioRenderer.BoneTransform[boneId]*Scale;
        }
        public Matrix34f GetWorldToLocalTransformation(int boneId)
        {
            return (((Matrix)StudioRenderer.BoneTransform[boneId]).GetInverse())*Scale;
        }
        public Vector3 Pos(int boneId)
        {
            return StudioRenderer.BoneTransform[boneId].Origin*Scale;
        }
        public Matrix GetWorldTransform(int boneId)
        {
            return StudioRenderer.BoneTransform[boneId] * Scale;
        }
    }
}
