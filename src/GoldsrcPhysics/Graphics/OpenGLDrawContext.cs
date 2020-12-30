using BulletSharp.Math;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics.Graphics
{
    public class OpenGLDrawContext:DrawContext
    {
		[StructLayout(LayoutKind.Explicit)]
		struct Colour
		{
			[FieldOffset(0)]
			public int Value;
			[FieldOffset(0)]
			public byte A;
			[FieldOffset(1)]
			public byte R;
			[FieldOffset(2)]
			public byte G;
			[FieldOffset(3)]
			public byte B;
		}
		[StructLayout( LayoutKind.Sequential)]
		internal struct ColoredLine
		{
			internal Vector3 Pos1;
			internal byte A1, R1, G1, B1;
			internal Vector3 Pos2;
			internal byte A2, R2, G2, B2;
		}
		public OpenGLDrawContext()
		{
			Environment.SetEnvironmentVariable("OPENGL_NET_GL_STATIC_INIT", "NO");
			Environment.SetEnvironmentVariable("OPENGL_NET_INIT", "NO");
		}
		public override unsafe void DrawLines(PositionColored* buffer, int elementCount)
        {
			////////////Option1
			DrawLinesSlow(buffer, elementCount);

			/////////////Option2 and option2 may be better
			//DrawLinesFast(buffer, elementCount);			
		}
		private unsafe void DrawLinesSlow(PositionColored* buffer, int elementCount)
		{
			//FIXME: The lines drawn by this function are all black.

			//GL.glDisable(2929);//depth
			//GL.glDisable(2896);//lighting
			GL.glDisable(3553);//texture
			//GL.glRenderMode(7168);
			GL.glBegin(1);//lines
			for (ColoredLine* p = (ColoredLine*)buffer, end = &p[elementCount]; p < end; p++)
			{
				GL.glColor3b(p->R1, p->G1, p->B1);
				GL.glVertex3fv((float*)&p->Pos1);
				GL.glColor3b(p->R2, p->G2, p->B2);
				GL.glVertex3fv((float*)&p->Pos2);
			}
			GL.glEnd();
			GL.glColor3f(1, 1, 1);
			//GL.glEnable(2929);
			//GL.glEnable(2896);
			GL.glEnable(3553);
		}
		/// <summary>
		/// Using glDrawArray could be faster.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="elementCount"></param>
		private unsafe void DrawLinesFast(PositionColored* buffer, int elementCount)
		{
			//TODO:
			//Gl.VertexPointer(3, VertexPointerType.Float, sizeof(int), new IntPtr(buffer));
			//Gl.ColorPointer(4, ColorPointerType.UnsignedByte, sizeof(Vector3), new IntPtr(buffer + sizeof(Vector3)));
			//Gl.DrawArrays(PrimitiveType.Lines, 0, elementCount * 2);
		}
	}
}
