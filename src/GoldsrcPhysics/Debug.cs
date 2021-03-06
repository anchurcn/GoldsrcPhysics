﻿using BulletSharp;
using BulletSharp.Math;
using GoldsrcPhysics.Utils;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GoldsrcPhysics
{
    public static class Debug
    {
        static TextWriter FileWriter { get; }
        private static bool _logOn;
        public static void LogLine()
        {
            if (!_logOn)
                return;
            FileWriter.WriteLine();
            FileWriter.Flush();
        }
        public static void LogLine(string format,params object[] args)
        {
            if (!_logOn)
                return;
            var log = string.Format(format, args);
            Console.WriteLine(log);
            FileWriter.WriteLine(log);
            FileWriter.Flush();
        }
        private static async void LogToFileAsync(string log)
        {
            await FileWriter.WriteLineAsync(log);
            await FileWriter.FlushAsync();
        }
        static Debug()
        {
            var args=Environment.GetCommandLineArgs();
            if (args.Any(x => x == "-phylog"))
            {
                _logOn = true;
                var filepath = string.Format(@"gsphysics\logs\{0}.txt", DateTime.Now.ToFileTime());
                var stream = File.Create(filepath);
                FileWriter = new StreamWriter(stream);
            }
            _logOn = false; 
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionColored
    {
        public const int Stride = Vector3.SizeInBytes + sizeof(int);

        public Vector3 Position;
        public int Color;

        public PositionColored(Vector3 pos, int col)
        {
            Position = pos;
            Color = col;
        }

        public PositionColored(ref Vector3 pos, int col)
        {
            Position = pos;
            Color = col;
        }
    }

    public abstract class BufferedDebugDraw : DebugDraw
    {
        PositionColored[] _lines = new PositionColored[3000];
        protected PositionColored[] Lines
        {
            get { return _lines; }
        }

        protected int LineIndex { get; set; }

        public override DebugDrawModes DebugMode { get; set; }

        int ColorToInt(ref Vector3 c)
        {
            return ((int)(c.X * 255.0f)) + ((int)(c.Y * 255.0f) << 8) + ((int)(c.Z * 255.0f) << 16)+ (255 << 24);
        }

        public override void Draw3DText(ref Vector3 location, string textString)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="color">float[4] RGB</param>
        public override void DrawLine(ref Vector3 from, ref Vector3 to, ref Vector3 color)
        {
            int intColor = ColorToInt(ref color);

            int line2Index = LineIndex + 1;
            if (line2Index >= Lines.Length)
            {
                Array.Resize(ref _lines, line2Index + 255);
            }
            var scaledFrom = from * GBConstant.B2GScale;
            var scaledTo = to * GBConstant.B2GScale;
            _lines[LineIndex].Position = scaledFrom;
            _lines[LineIndex].Color = intColor;
            _lines[line2Index].Position = scaledTo;
            _lines[line2Index].Color = intColor;

            LineIndex = line2Index + 1;
        }

        public override void ReportErrorWarning(string warningString)
        {
            //System.Windows.Forms.MessageBox.Show(warningString);
            Debug.LogLine(warningString);
        }
    }
    /// <summary>
    /// only support drawlines now
    /// </summary>
    public unsafe class PhysicsDebugDraw : BufferedDebugDraw
    {
        private DrawContext Device;
        public PhysicsDebugDraw(DrawContext device)
        {
            Device = device;
        }
        public void DrawDebugWorld(DynamicsWorld world)
        {
            world.DebugDrawWorld();
            if (LineIndex == 0)
                return;

            fixed(PositionColored* p=Lines)
            {
                Device.DrawLines(p,LineIndex/2);
            }
            LineIndex = 0;
        }
    }
}
