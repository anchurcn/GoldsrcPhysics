using BulletSharp.Math;
using System;
using System.Runtime.InteropServices;

namespace GoldsrcPhysics.Goldsrc
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Matrix34f
    {
        [FieldOffset(0)]
        public BulletSharp.Math.Vector4 Row1;
        [FieldOffset(16)]
        public BulletSharp.Math.Vector4 Row2;
        [FieldOffset(32)]
        public BulletSharp.Math.Vector4 Row3;
        [FieldOffset(0)]
        public fixed float M[3 * 4];

        public BulletSharp.Math.Vector3 Origin { 
            get => new Vector3(M[3], M[7], M[11]);
            set 
            {
                M[3] = value.X;
                M[7] = value.Y;
                M[11] = value.Z;
            } 
        }

        public static Matrix34f Identity
        {
            get
            {
                var result = new Matrix34f();
                result.M[0] = 1;
                result.M[1 * 4 + 1] = 1;
                result.M[2 * 4 + 2] = 1;
                return result;
            }

        }
        public float this[int i] => M[i];

        public static Matrix34f Zero => new Matrix34f();

        //public Vector3 IHat { get=>new Vector3(M[0*0],M[ }
        //public Vector3 JHat { get; }
        //public Vector3 KHat { get; }
        //public Vector3 Origin;


        public static implicit operator BulletSharp.Math.Matrix(Matrix34f matrix)
        {
            return new BulletSharp.Math.Matrix(
                matrix[0], matrix[4], matrix[8], 0,
                matrix[1], matrix[5], matrix[9], 0,
                matrix[2], matrix[6], matrix[10], 0,
                matrix[3], matrix[7], matrix[11], 1
                );
        }
        public static implicit operator LinearMath.Matrix(Matrix34f matrix)
        {
            return new LinearMath.Matrix(
                matrix[0], matrix[4], matrix[8], 0,
                matrix[1], matrix[5], matrix[9], 0,
                matrix[2], matrix[6], matrix[10], 0,
                matrix[3], matrix[7], matrix[11], 1
                );
        }
        public static implicit operator Matrix34f(BulletSharp.Math.Matrix m)
        {
            return new Matrix34f()
            {
                Row1 = m.Column1,
                Row2 = m.Column2,
                Row3 = m.Column3
            };
        }
        public static unsafe implicit operator Matrix34f(LinearMath.Matrix m)
        {
            var col1 = m.Column1;
            var col2 = m.Column2;
            var col3 = m.Column3;
            return new Matrix34f()
            {

                Row1 = *((Vector4*)&col1),
                Row2 = *((Vector4*)&col2),
                Row3 = *((Vector4*)&col3)
            };
        }

        /// <summary>
        /// 将儿子的局部变换转化为世界变换，用的是矩阵乘法
        /// 如果translation在同一列，则
        /// lhs is parent world transform
        /// rhs is child's local transform
        /// result is child's world transform
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <param name="res"></param>
        public static void ConcatTransforms(in Matrix34f lhs, in Matrix34f rhs, out Matrix34f result)
        {
            Matrix34f res = new Matrix34f();
            res.M[0] = lhs[0 * 4 + 0] * rhs[0 * 4 + 0] + lhs[0 * 4 + 1] * rhs[1 * 4 + 0] +
                lhs[0 * 4 + 2] * rhs[2 * 4 + 0];
            res.M[1] = lhs[0 * 4 + 0] * rhs[0 * 4 + 1] + lhs[0 * 4 + 1] * rhs[1 * 4 + 1] +
                lhs[0 * 4 + 2] * rhs[2 * 4 + 1];
            res.M[2] = lhs[0 * 4 + 0] * rhs[0 * 4 + 2] + lhs[0 * 4 + 1] * rhs[1 * 4 + 2] +
                lhs[0 * 4 + 2] * rhs[2 * 4 + 2];
            res.M[3] = lhs[0 * 4 + 0] * rhs[0 * 4 + 3] + lhs[0 * 4 + 1] * rhs[1 * 4 + 3] +
                lhs[0 * 4 + 2] * rhs[2 * 4 + 3] + lhs[0 * 4 + 3];
            res.M[4] = lhs[1 * 4 + 0] * rhs[0 * 4 + 0] + lhs[1 * 4 + 1] * rhs[1 * 4 + 0] +
                lhs[1 * 4 + 2] * rhs[2 * 4 + 0];
            res.M[5] = lhs[1 * 4 + 0] * rhs[0 * 4 + 1] + lhs[1 * 4 + 1] * rhs[1 * 4 + 1] +
                lhs[1 * 4 + 2] * rhs[2 * 4 + 1];
            res.M[6] = lhs[1 * 4 + 0] * rhs[0 * 4 + 2] + lhs[1 * 4 + 1] * rhs[1 * 4 + 2] +
                lhs[1 * 4 + 2] * rhs[2 * 4 + 2];
            res.M[7] = lhs[1 * 4 + 0] * rhs[0 * 4 + 3] + lhs[1 * 4 + 1] * rhs[1 * 4 + 3] +
                lhs[1 * 4 + 2] * rhs[2 * 4 + 3] + lhs[1 * 4 + 3];
            res.M[8] = lhs[2 * 4 + 0] * rhs[0 * 4 + 0] + lhs[2 * 4 + 1] * rhs[1 * 4 + 0] +
                lhs[2 * 4 + 2] * rhs[2 * 4 + 0];
            res.M[9] = lhs[2 * 4 + 0] * rhs[0 * 4 + 1] + lhs[2 * 4 + 1] * rhs[1 * 4 + 1] +
                lhs[2 * 4 + 2] * rhs[2 * 4 + 1];
            res.M[10] = lhs[2 * 4 + 0] * rhs[0 * 4 + 2] + lhs[2 * 4 + 1] * rhs[1 * 4 + 2] +
                lhs[2 * 4 + 2] * rhs[2 * 4 + 2];
            res.M[11] = lhs[2 * 4 + 0] * rhs[0 * 4 + 3] + lhs[2 * 4 + 1] * rhs[1 * 4 + 3] +
                lhs[2 * 4 + 2] * rhs[2 * 4 + 3] + lhs[2 * 4 + 3];
            result = res;
        }
        public static void AngleQuaternion(float* angles, out Quaternion quaternion)
        {
            quaternion = new Quaternion();
            float angle;
            float sr, sp, sy, cr, cp, cy;

            // FIXME: rescale the inputs to 1/2 angle
            angle = angles[2] * 0.5f;
            sy = (float)Math.Sin(angle);
            cy = (float)Math.Cos(angle);
            angle = angles[1] * 0.5f;
            sp = (float)Math.Sin(angle);
            cp = (float)Math.Cos(angle);
            angle = angles[0] * 0.5f;
            sr = (float)Math.Sin(angle);
            cr = (float)Math.Cos(angle);

            quaternion[0] = sr * cp * cy - cr * sp * sy; // X
            quaternion[1] = cr * sp * cy + sr * cp * sy; // Y
            quaternion[2] = cr * cp * sy - sr * sp * cy; // Z
            quaternion[3] = cr * cp * cy + sr * sp * sy; // W
        }
        public static void QuaternionMatrix(Quaternion quaternion, out Matrix34f  result )
        {
            Matrix34f matrix = new Matrix34f();
            matrix.M[0*4+0] =(float)(1.0 - 2.0 * quaternion[1] * quaternion[1] - 2.0 * quaternion[2] * quaternion[2]);
            matrix.M[1*4+0] =(float)(2.0 * quaternion[0] * quaternion[1] + 2.0 * quaternion[3] * quaternion[2]);
            matrix.M[2*4+0] =(float)(2.0 * quaternion[0] * quaternion[2] - 2.0 * quaternion[3] * quaternion[1]);
                           
            matrix.M[0*4+1] =(float)(2.0 * quaternion[0] * quaternion[1] - 2.0 * quaternion[3] * quaternion[2]);
            matrix.M[1*4+1] =(float)(1.0 - 2.0 * quaternion[0] * quaternion[0] - 2.0 * quaternion[2] * quaternion[2]);
            matrix.M[2*4+1] =(float)(2.0 * quaternion[1] * quaternion[2] + 2.0 * quaternion[3] * quaternion[0]);
                           
            matrix.M[0*4+2] =(float)(2.0 * quaternion[0] * quaternion[2] + 2.0 * quaternion[3] * quaternion[1]);
            matrix.M[1*4+2] =(float)(2.0 * quaternion[1] * quaternion[2] - 2.0 * quaternion[3] * quaternion[0]);
            matrix.M[2*4+2] =(float)(1.0 - 2.0 * quaternion[0] * quaternion[0] - 2.0 * quaternion[1] * quaternion[1]);
            result = matrix;
        }
    }

}
