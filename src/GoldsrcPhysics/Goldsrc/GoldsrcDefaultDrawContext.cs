using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics.Goldsrc
{
    /// <summary>
    /// using method export by client.dll implement in TriAPI.
    /// </summary>
    public unsafe class GoldsrcDefaultDrawContext : DrawContext
    {
        public enum PrimitiveType
        {
            TriangleFan = 6,
            TriangleStrip = 5,
            TriangleList = 4,
            LineStrip = 3,
            LineList = 2,
            PointList = 1
        }
        /// <summary>
        /// provides by client sdk. Draw buffered data.
        /// </summary>
        /// <param name="buffer"></param>
        [DllImport("client.dll")]
        private static extern void DrawBufferedLines(void* buffer, int elementCount);
        //put these things to int HUD_Redraw(float time, int intermission)
        //maybe game loop use this function
        private static void Exsample()
        {
            // if you want to draw something that can't be covered, just use [glDepthMask(GL_FALSE);] disable depth detection
            //

            //glBegin(Lines)
            //glColor()//use color for next vertex
            //glVertex()
            //glColor()
            //glVertex
            //glEnd
        }

        [DllImport("client.dll")]
        private static extern void DrawUserPrimitives(PrimitiveType primitiveType,void* buffer,int elementCount);


        public override unsafe void DrawLines(PositionColored* buffer, int elementCount)
        {
            DrawBufferedLines(buffer, elementCount);
        }
    }
}
