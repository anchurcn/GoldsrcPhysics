using BulletSharp.Math;

namespace GoldsrcPhysics.Goldsrc.Bsp
{
    public struct Plane
    {
        public const int X = 0;
        public const int Y = 1;
        public const int Z = 2;
        public const int AnyX = 3;
        public const int AnyY = 4;
        public const int AnyZ = 5;

        public Vector3 Normal;
        public float Distance;
        public int Type;
    }
}