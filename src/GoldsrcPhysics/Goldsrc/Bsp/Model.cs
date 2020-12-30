using BulletSharp.Math;

namespace GoldsrcPhysics.Goldsrc.Bsp
{
    public struct Model
    {
        public Vector3 Mins;
        public Vector3 Maxs;
        public Vector3 Origin;
        public int[] HeadNodes;
        public int VisLeaves;
        public int FirstFace;
        public int NumFaces;
    }
}