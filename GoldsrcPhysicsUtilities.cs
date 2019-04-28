using BulletSharp.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics
{
    public static class GoldsrcPhysicsUtilities
    {
        public static Quaternion DecomQuat(this Matrix matrix)
        {
            matrix.Decompose(out var scale, out var quaternion, out var trans);
            return quaternion;
        }

        public static Matrix GetInverse(this Matrix matrix)
        {
            matrix.Invert();
            return matrix;
        }
    }
}
