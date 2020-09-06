//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;
//using BulletSharp;
//using BulletSharp.Math;
//using DemoFramework;


//namespace GoldsrcPhysics
//{
    
//    public unsafe class GoldsrcBox : IGoldsrcBehaviour
//    {
//        [StructLayout(LayoutKind.Sequential)]
//        public struct BoxArgs
//        {
//            public float x, y, z;
//            public Quaternion rotation;
//            public float halfExtent;
//        }
//        private RigidBody BoxBody;
//        DynamicsWorld World;

//        /// <summary>
//        /// ponint to the head of float[8]
//        /// </summary>
//        float* values;
//        BoxArgs *BoxValue;

//        public GoldsrcBox(int valuesptr,DynamicsWorld world)
//        {
//            World = world;
//            values = (float*)valuesptr;
//            BoxValue = (BoxArgs*)valuesptr;
            
//            var shape = new BoxShape(values[7] * GBConstant.G2BScale);
//            float[] floatArr = new float[8] { values[0], values[1], values[2], values[3], values[4], values[5], values[6],values[7] };
//            BoxBody = PhysicsHelper.CreateBody(50, Matrix.Translation(values[0]*GBConstant.G2BScale, values[1] * GBConstant.G2BScale, values[2] * GBConstant.G2BScale), shape, world);
//        }
//        public void Dispose()
//        {
//            World.RemoveRigidBody(BoxBody);
//            BoxBody.Dispose();
//        }

//        public void FixedUpdate()
//        {
//            if (BoxBody.ActivationState == ActivationState.IslandSleeping)
//                return;
            
//            values[0] = BoxBody.WorldTransform.Origin.X*GBConstant.B2GScale;
//            values[1] = BoxBody.WorldTransform.Origin.Y * GBConstant.B2GScale;
//            values[2] = BoxBody.WorldTransform.Origin.Z * GBConstant.B2GScale;
//            Quaternion rotation;
//            Vector3 scale;
//            Vector3 translation;

//            BoxBody.WorldTransform.Decompose(out scale, out rotation, out translation);
//            values[3] = rotation.X;
//            values[4] = rotation.Y;
//            values[5] = rotation.Z;
//            values[6] = rotation.W;
//        }
//    }
//}
