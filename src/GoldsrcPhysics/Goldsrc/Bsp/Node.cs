namespace GoldsrcPhysics.Goldsrc.Bsp
{
    public struct Node
    {
        public uint Plane;
        public short[] Children;
        public short[] Mins;
        public short[] Maxs;
        public ushort FirstFace;
        public ushort NumFaces;
    }
}