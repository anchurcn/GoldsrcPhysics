namespace GoldsrcPhysics
{
    /// <summary>
    /// drawing method implement in games or any metahook like program.
    /// </summary>
    public abstract class DrawContext
    {
        public abstract unsafe void DrawLines(PositionColored* buffer, int elementCount);
    }
}