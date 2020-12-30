namespace GoldsrcPhysics.Goldsrc.Bsp
{
    public struct Face
    {
        public const int MaxLightmaps = 4;

        public short Plane;
        public short Side;
        public int FirstEdge;
        public short NumEdges;
        public short TextureInfo;
        public byte[] Styles;
        public int LightmapOffset;
    }
}