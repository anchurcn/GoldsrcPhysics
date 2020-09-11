using BulletSharp;
using BulletSharp.Math;
using GoldsrcPhysics.Utils;
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

        public static Vector3 Transform(in Vector3 vector, in Matrix transform)
        {
            Vector3 result;
            Transform(in vector,in transform, out result);
            return result;
        }
        public static void Transform(in Vector3 vector, in Matrix transform, out Vector3 result)
        {
            result = new Vector3(
                (vector.X * transform.M11) + (vector.Y * transform.M21) + (vector.Z * transform.M31) + transform.M41,
                (vector.X * transform.M12) + (vector.Y * transform.M22) + (vector.Z * transform.M32) + transform.M42,
                (vector.X * transform.M13) + (vector.Y * transform.M23) + (vector.Z * transform.M33) + transform.M43);
        }

        /// <summary>
        /// may not work, testing before using
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="result"></param>
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
            Quaternion result = Quaternion.Zero;
            BetweenDirections(ref source, ref target, out result);
            return result;
        }
        public static void MatrixLookAt(ref Matrix transform, in Vector3 worldPoint, in Vector3 forward)
        {
            var originVector = forward;
            var worldToLocalTransform = transform.GetInverse();
            
            //transform the target in world position to object's local position
            var targetVector = Transform(in worldPoint,in worldToLocalTransform);

            var rot = FromToRotaion(originVector, targetVector);
            var rotMatrix = Matrix.RotationQuaternion(rot);
            transform = rotMatrix * transform;
        }
        public static float Angle(Vector3 from, Vector3 to)
        {
            from.Normalize();
            to.Normalize();
            return (float)Math.Acos(MathUtil.Clamp(Vector3.Dot(from,to), (float)-1, (float)1));
        }
        public static Quaternion FromToRotaion(Vector3 fromDirection, Vector3 toDirection)
        {
            fromDirection.Normalize();
            toDirection.Normalize();

            float cosTheta = Vector3.Dot(fromDirection, toDirection);

            if(cosTheta<-1+0.001f) //(Math.Abs(cosTheta)-Math.Abs( -1.0)<1E-6)
            {
                Vector3 rotationAxis =Vector3. Cross(new Vector3(0.0f, 0.0f, 1.0f), fromDirection);
                if (rotationAxis.LengthSquared < 0.01) // bad luck, they were parallel, try again!
                {
                    rotationAxis =Vector3. Cross(new Vector3(0.0f, 0.0f, 1.0f), fromDirection);
                }
                rotationAxis.Normalize();
                return new Quaternion(rotationAxis,(float) Math.PI);
            }
            {
                // Implementation from Stan Melax's Game Programming Gems 1 article
                Vector3 rotationAxis =Vector3. Cross(fromDirection, toDirection);

                float s =(float) Math.Sqrt((1 + cosTheta) * 2);
                float invs = 1 / s;

                return new Quaternion(
                    rotationAxis.X * invs,
                    rotationAxis.Y * invs,
                    rotationAxis.Z * invs,
                    s * 0.5f
                );
            }
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
