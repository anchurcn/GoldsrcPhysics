namespace GoldsrcPhysics.Goldsrc.Bsp
{
    public struct Lump
    {
        public const int Entities = 0;
        public const int Planes = 1;
        public const int Textures = 2;
        public const int Vertices = 3;
        public const int Visibility = 4;
        public const int Nodes = 5;
        public const int Texinfo = 6;
        public const int Faces = 7;
        public const int Lighting = 8;
        public const int Clipnodes = 9;
        public const int Leaves = 10;
        public const int Marksurfaces = 11;
        public const int Edges = 12;
        public const int Surfedges = 13;
        public const int Models = 14;

        public const int NumLumps = 15;

        public int Offset;
        public int Length;
    }
}