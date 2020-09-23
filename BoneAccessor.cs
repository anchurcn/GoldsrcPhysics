using BulletSharp.Math;
using GoldsrcPhysics.Goldsrc;
using GoldsrcPhysics.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static GoldsrcPhysics.Goldsrc.Studio_h;

namespace GoldsrcPhysics
{
    public unsafe class BoneAccessor
    {
        public static BoneAccessor GetCurrent()
        {
            return new TPoseBoneAccessor(StudioRenderer.StudioHeader);
        }

        public static BoneAccessor Get(string name)
        {
            sbyte* n = (sbyte*)Marshal.StringToHGlobalAnsi(name);
            var mod = IEngineStudio.Mod_ForName(n,true);
            Marshal.FreeHGlobal((IntPtr)n);
            return new TPoseBoneAccessor(IEngineStudio.Mod_Extradata(mod));
        }
        public static BoneAccessor Get(int index)
        {
            var mod = IEngineStudio.GetModelByIndex(index);
            return new TPoseBoneAccessor(IEngineStudio.Mod_Extradata(mod));
        }
        
        public void SetTPose() { }

        public void SetAnim(int anim) { }
        public void SetFrame(int frame) { }

        public float Scale { get; set; } = 1f;
        public int BoneCount { get => StudioRenderer.BoneCount; }

        public virtual Matrix34f GetLocalTransform(int boneId)
        {
            return (StudioRenderer.ScaledBoneTransform[boneId] * ((Matrix)StudioRenderer.ScaledBoneTransform[StudioRenderer.Bones[boneId].parent]).GetInverse())*Scale;
        }
        public virtual Matrix34f GetLocalToWorldTransformation(int boneId)
        {
            return StudioRenderer.ScaledBoneTransform[boneId]*Scale;
        }
        public virtual Matrix34f GetWorldToLocalTransformation(int boneId)
        {
            return (((Matrix)StudioRenderer.ScaledBoneTransform[boneId]).GetInverse())*Scale;
        }
        public virtual Vector3 Pos(int boneId)
        {
            return StudioRenderer.ScaledBoneTransform[boneId].Origin*Scale;
        }
        public virtual Matrix GetWorldTransform(int boneId)
        {
            return StudioRenderer.ScaledBoneTransform[boneId] * Scale;
        }
    }
    public unsafe class TPoseBoneAccessor:BoneAccessor
    {
        private Matrix34f[] BoneTransform;
        public TPoseBoneAccessor(studiohdr_t* studioHeader)
        {
            var bones = (mstudiobone_t*)((byte*)studioHeader + studioHeader->boneindex);
            BoneTransform = new Matrix34f[StudioRenderer.BoneCount];

            Matrix34f matrix = new Matrix34f();
            Quaternion q = new Quaternion();
            Matrix34f.AngleQuaternion(&bones[0].value[3], out q);
            Matrix34f.QuaternionMatrix(q, out matrix);
            matrix.Origin = new Vector3(bones[0].value[0], bones[0].value[1], bones[0].value[2]);
            Matrix34f rebaseTransform = Matrix34f.Zero;
            rebaseTransform.M[1] = -1;
            rebaseTransform.M[4] = 1;
            rebaseTransform.M[10] = 1;
            Matrix34f.ConcatTransforms(rebaseTransform, matrix, out BoneTransform[0]);

            for (int i = 1; i < StudioRenderer.BoneCount; i++)
            {
                Matrix34f.AngleQuaternion(&bones[i].value[3], out q);
                Matrix34f.QuaternionMatrix(q, out matrix);
                matrix.Origin = new Vector3(bones[i].value[0], bones[i].value[1], bones[i].value[2]);
                Matrix34f.ConcatTransforms(BoneTransform[bones[i].parent], matrix, out BoneTransform[i]);
            }
            Scale = GBConstant.G2BScale;
        }
        public override Vector3 Pos(int boneId)
        {
            return BoneTransform[boneId].Origin*Scale;
        }
        public override Matrix GetWorldTransform(int boneId)
        {
            var matrix = BoneTransform[boneId];
            matrix.Origin *= Scale;
            return matrix;
        }
    }
}
