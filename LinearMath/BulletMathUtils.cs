using BulletSharp.Math;
using System;

namespace GoldsrcPhysics
{
    public static class BulletMathUtils
    {
        //public static Quaternion DecomQuat(this Matrix matrix)
        //{
        //    matrix.Decompose(out var scale, out var quaternion, out var trans);
        //    return quaternion;
        //}

        public static Matrix GetInverse(this Matrix matrix)
        {
            matrix.Invert();
            return matrix;
        }
        public static void Transform(ref Vector3 vector, ref Matrix transform, out Vector3 result)
        {
            result = new Vector3(
                (vector.X * transform.M11) + (vector.Y * transform.M21) + (vector.Z * transform.M31) + transform.M41,
                (vector.X * transform.M12) + (vector.Y * transform.M22) + (vector.Z * transform.M32) + transform.M42,
                (vector.X * transform.M13) + (vector.Y * transform.M23) + (vector.Z * transform.M33) + transform.M43);
        }
        public static void BetweenDirections(ref Vector3 source, ref Vector3 target, out Quaternion result)
        {
            var norms = (float)Math.Sqrt(source.LengthSquared * target.LengthSquared);
            var real = norms + Vector3.Dot(source, target);
            if (real < LinearMath.MathUtil.ZeroTolerance * norms)
            {
                // If source and target are exactly opposite, rotate 180 degrees around an arbitrary orthogonal axis.
                // Axis normalisation can happen later, when we normalise the quaternion.
                result = Math.Abs(source.X) > Math.Abs(source.Z)
                    ? new Quaternion(-source.Y, source.X, 0.0f, 0.0f)
                    : new Quaternion(0.0f, -source.Z, source.Y, 0.0f);
            }
            else
            {
                // Otherwise, build quaternion the standard way.
                var axis = Vector3.Cross(source, target);
                result = new Quaternion(axis, real);
            }
            result.Normalize();
        }
        public static Quaternion BetweenDirections(Vector3 source, Vector3 target)
        {
            Quaternion result;
            BetweenDirections(ref source, ref target, out result);
            return result;
        }
        public static void MatrixLookAt(ref Matrix transform, in Vector3 worldPoint, in Vector3 forward)
        {

            var originVector = forward;
            var targetVector = new Vector3();

            var targetInWorldPos = worldPoint - transform.Origin;
            var worldToLocalTransform = transform.GetInverse();
            
            Transform(ref targetInWorldPos, ref worldToLocalTransform, out targetVector);//transform the target in world position to object's local position
            var rot = BetweenDirections(originVector, targetVector);
            var rotMatrix = Matrix.RotationQuaternion(rot);
            transform = transform * rotMatrix;
        }

        public static Vector3 CenterOf(ref Vector3 one, ref Vector3 two)
        {
            return Vector3.Add(one, two) /2;
        }

        public static Matrix CenterOf(Vector3 one, Vector3 two)
        {
            Vector3 center = CenterOf(ref one, ref two);
            return Matrix.Translation(center);
        }
    }
}
