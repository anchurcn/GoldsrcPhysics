using BulletSharp;
using BulletSharp.Math;
using GoldsrcPhysics.Goldsrc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics.Utils
{
    public static class MathExtensions
    {
        public static Matrix ToBullet(this Matrix34f self)
        {
            return self;
        }

        public static Matrix34f ToGoldsrc(this Matrix self)
        {
            return self;
        }

        public static Matrix LookAt(this Matrix self,in Vector3 worldPoint, in Vector3 forward)
        {
            BulletMathUtils.MatrixLookAt(ref self, in worldPoint, in forward);
            return self;
        }
        public static Matrix GetInverse(this Matrix matrix)
        {
            matrix.Invert();
            return matrix;
        }
        public static Vector3 TransformToLocal(this RigidBody body,in Vector3 worldPoint)
        {
            Vector3 result = Vector3.Zero;
            var transform = body.WorldTransform.GetInverse();
            Transform(worldPoint, transform, out result);
            return result;
        }
        
        
        public static void Transform(in Vector3 vector, in Matrix transform, out Vector3 result)
        {
            result = new Vector3(
                (vector.X * transform.M11) + (vector.Y * transform.M21) + (vector.Z * transform.M31) + transform.M41,
                (vector.X * transform.M12) + (vector.Y * transform.M22) + (vector.Z * transform.M32) + transform.M42,
                (vector.X * transform.M13) + (vector.Y * transform.M23) + (vector.Z * transform.M33) + transform.M43);
        }

    }
}
