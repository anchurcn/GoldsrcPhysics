using BulletSharp;
using BulletSharp.Math;
using GoldsrcPhysics.Goldsrc;
using GoldsrcPhysics.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics
{
    class BWorld
    {
        public static DynamicsWorld Instance { get; set; }//studio cl_time cl_oldtime can be used for simulating

        public static void CreateInstance()
        {
            var CollisionConfiguration = new DefaultCollisionConfiguration();
            var Dispatcher = new CollisionDispatcher(CollisionConfiguration);
            var Broadphase = new AxisSweep3(new Vector3(-10000, -10000, -10000), new Vector3(10000, 10000, 10000));
            var World = new DiscreteDynamicsWorld(Dispatcher, Broadphase, null, CollisionConfiguration);
            World.Gravity = new Vector3(0, 0, -9.8f);

            //set debug draw
            World.DebugDrawer = new PhysicsDebugDraw(new GoldsrcDefaultDrawContext());
            World.DebugDrawer.DebugMode = DebugDrawModes.All;
            Debug.LogLine("Debug Draw Mode: {0}", World.DebugDrawer.DebugMode.ToString());

            Instance = World;
        }
    }
}
