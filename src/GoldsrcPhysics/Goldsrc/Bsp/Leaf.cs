namespace GoldsrcPhysics.Goldsrc.Bsp
{
    public struct Leaf
    {
        public const int MaxNumAmbientLevels = 4;

        public Contents Contents;
        public int VisOffset;
        public short[] Mins;
        public short[] Maxs;
        public ushort FirstMarkSurface;
        public ushort NumMarkSurfaces;
        public byte[] AmbientLevels;
    }
}