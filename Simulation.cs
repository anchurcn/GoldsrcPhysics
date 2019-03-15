using BulletSharp;
using BulletSharp.Math;
using DemoFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics
{
    public interface IPhysicsObject
    {
        void FixedUpdate();
        void Dispose();
    }

    public sealed class Simulation:ISimulation
    {
        public CollisionConfiguration CollisionConfiguration { get; }
        public CollisionDispatcher Dispatcher { get; }
        public BroadphaseInterface Broadphase { get; }
        public DiscreteDynamicsWorld World { get; }
        List<IPhysicsObject> _physicsObject;

        public Simulation()
        {
            CollisionConfiguration = new DefaultCollisionConfiguration();
            Dispatcher = new CollisionDispatcher(CollisionConfiguration);
            Broadphase = new AxisSweep3(new Vector3(-10000, -10000, -10000), new Vector3(10000, 10000, 10000));
            World = new DiscreteDynamicsWorld(Dispatcher, Broadphase, null, CollisionConfiguration);
            _physicsObject = new List<IPhysicsObject>();
            //World.DispatchInfo.UseConvexConservativeDistanceUtil = true;
            //World.DispatchInfo.ConvexConservativeDistanceThreshold = 0.01f;

            //CreateGround();

            //SpawnRagdoll(new Vector3(1, 0.5f, 0));
            //SpawnRagdoll(new Vector3(-1, 0.9f, -3));
        }

        public async void Run()//开始物理循环
        {
            while (true)
            {
                await Task.Delay(20);
                World.StepSimulation(20);
                foreach (var i in _physicsObject)
                {
                    i.FixedUpdate();
                }
            }
        }


        public void Dispose()
        {
            foreach (var ragdoll in _physicsObject)
            {
                ragdoll.Dispose();
            }

            this.StandardCleanup();
        }

        private void CreateGround()
        {
            var groundShape = new BoxShape(100, 10, 100);
            Matrix groundTransform = Matrix.Translation(0, -10, 0);
            RigidBody ground = PhysicsHelper.CreateStaticBody(groundTransform, groundShape, World);
            ground.UserObject = "Ground";
        }

        public void SpawnRagdoll(Vector3 startOffset)
        {
            var ragdoll = new Ragdoll(World, startOffset);
        }
        public void SpawnRagdoll(int ptr)
        {
            GoldsrcRagdoll goldsrcRagdoll = new GoldsrcRagdoll() { Pointer = ptr, BRagdoll = new Ragdoll(World, Vector3.One) };
            _physicsObject.Add(goldsrcRagdoll);
        }



        #region CleanUp
        public static void StandardCleanup(ISimulation simulation)
        {
            CleanupConstraints(simulation.World);
            CleanupBodiesAndShapes(simulation.World);

            var multiBodyWorld = simulation.World as MultiBodyDynamicsWorld;
            if (multiBodyWorld != null)
            {
                CleanupMultiBodyWorld(multiBodyWorld);
            }

            simulation.World.Dispose();
            simulation.Broadphase.Dispose();
            simulation.Dispatcher.Dispose();
            simulation.CollisionConfiguration.Dispose();
        }

        private static void CleanupConstraints(DynamicsWorld world)
        {
            for (int i = world.NumConstraints - 1; i >= 0; i--)
            {
                TypedConstraint constraint = world.GetConstraint(i);
                world.RemoveConstraint(constraint);
                constraint.Dispose();
            }
        }

        private static void CleanupBodiesAndShapes(DynamicsWorld world)
        {
            var shapes = new HashSet<CollisionShape>();

            for (int i = world.NumCollisionObjects - 1; i >= 0; i--)
            {
                CollisionObject obj = world.CollisionObjectArray[i];
                var body = obj as RigidBody;
                if (body != null && body.MotionState != null)
                {
                    body.MotionState.Dispose();
                }
                world.RemoveCollisionObject(obj);
                GetShapeWithChildShapes(obj.CollisionShape, shapes);

                obj.Dispose();
            }

            foreach (var shape in shapes)
            {
                shape.Dispose();
            }
        }

        private static void CleanupMultiBodyWorld(MultiBodyDynamicsWorld world)
        {
            for (int i = world.NumMultiBodyConstraints - 1; i >= 0; i--)
            {
                MultiBodyConstraint multiBodyConstraint = world.GetMultiBodyConstraint(i);
                world.RemoveMultiBodyConstraint(multiBodyConstraint);
                multiBodyConstraint.Dispose();
            }

            for (int i = world.NumMultibodies - 1; i >= 0; i--)
            {
                MultiBody multiBody = world.GetMultiBody(i);
                world.RemoveMultiBody(multiBody);
                multiBody.Dispose();
            }
        }

        private static void GetShapeWithChildShapes(CollisionShape shape, HashSet<CollisionShape> shapes)
        {
            shapes.Add(shape);

            var convex2DShape = shape as Convex2DShape;
            if (convex2DShape != null)
            {
                GetShapeWithChildShapes(convex2DShape.ChildShape, shapes);
                return;
            }

            var compoundShape = shape as CompoundShape;
            if (compoundShape != null)
            {
                foreach (var childShape in compoundShape.ChildList)
                {
                    GetShapeWithChildShapes(childShape.ChildShape, shapes);
                }
                return;
            }

            var scaledTriangleMeshShape = shape as ScaledBvhTriangleMeshShape;
            if (scaledTriangleMeshShape != null)
            {
                GetShapeWithChildShapes(scaledTriangleMeshShape.ChildShape, shapes);
                return;
            }

            var uniformScalingShape = shape as UniformScalingShape;
            if (uniformScalingShape != null)
            {
                GetShapeWithChildShapes(uniformScalingShape.ChildShape, shapes);
                return;
            }
        }
        #endregion
    }
}
