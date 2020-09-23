using OpenGL;
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
			public sbyte A;
			 [FieldOffset(1)]
			public sbyte R;
			 [FieldOffset(2)]
			public sbyte G;
			 [FieldOffset(3)]
			public sbyte B;

			public static implicit operator int(Colour x)
			{
				return x.Value;
			}
			public static implicit operator Colour(int x)
			{
				var res = new Colour();
				res.Value = x;
				return res;
			}
		}
		public OpenGLDrawContext()
		{
			Environment.SetEnvironmentVariable("OPENGL_NET_GL_STATIC_INIT", "NO");
			Environment.SetEnvironmentVariable("OPENGL_NET_INIT", "NO");
		}
		public override unsafe void DrawLines(PositionColored* buffer, int elementCount)
        {
			//Gl.Disable(EnableCap.DepthTest);
			//Gl.RenderMode(RenderingMode.Render);
			GL.glDisable(2929);
			GL.glDisable(3553);
			GL.glRenderMode(7168);
			GL.glBegin(1);
			//Gl.Begin(PrimitiveType.Lines);
			var pointCount = elementCount * 2;
			for (int i = 0; i < pointCount; i+=2)
			{
				var r = (*(Colour*)(&buffer[i].Color)).R;
				var g= (*((Colour*)(&buffer[i].Color))).G;
				var b= (*((Colour*)(&buffer[i].Color))).B;
				GL.glColor3b((byte)r, (byte)g, (byte)b);
				GL.glVertex3fv((float*)&buffer[i].Position);
				//Gl.Color3(r, g, b);
				//Gl.Vertex3(buffer[i].Position.X,buffer[i].Position.Y,buffer[i].Position.Z);
				r = (*((Colour*)(&buffer[i+1].Color))).R;
				g = (*((Colour*)(&buffer[i+1].Color))).G;
				b = (*((Colour*)(&buffer[i+1].Color))).B;
				GL.glColor3b((byte)r, (byte)g, (byte)b);
				GL.glVertex3fv((float*)&buffer[i + 1].Position);
				//Gl.Color3(r, g, b);
				//Gl.Vertex3(buffer[i + 1].Position.X, buffer[i + 1].Position.Y, buffer[i + 1].Position.Z);
			}
			GL.glEnd();
			GL.glColor3f(1, 1, 1);
			GL.glEnable(2929);
			GL.glEnable(3553);
			//Gl.End();
			//Gl.Enable(EnableCap.DepthTest);

		}
}
}
