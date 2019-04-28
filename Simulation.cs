using BspLib.Bsp;
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
        List<GoldsrcRagdoll> _goldsrcRagdolls;
        public FPSTimer Timer;

        public Simulation()
        {
            CollisionConfiguration = new DefaultCollisionConfiguration();
            Dispatcher = new CollisionDispatcher(CollisionConfiguration);
            Broadphase = new AxisSweep3(new Vector3(-10000, -10000, -10000), new Vector3(10000, 10000, 10000));
            World = new DiscreteDynamicsWorld(Dispatcher, Broadphase, null, CollisionConfiguration);
            World.Gravity = new Vector3(0, 0, -9.81f);
            _physicsObject = new List<IPhysicsObject>();
            _goldsrcRagdolls = new List<GoldsrcRagdoll>();
            Timer = new FPSTimer();
            Timer.GameUpdate += GetEventHandler();
            //World.DispatchInfo.UseConvexConservativeDistanceUtil = true;
            //World.DispatchInfo.ConvexConservativeDistanceThreshold = 0.01f;
            //CreateGround();
            //SpawnRagdoll(new Vector3(1, 0.5f, 0));
            //SpawnRagdoll(new Vector3(-1, 0.9f, -3));
        }
        ~Simulation()
        {
            Dispose();
        }
        public void SpawnBox(int ptr)
        {
            _physicsObject.Add(new GoldsrcBox(ptr, World));
        }
        GamingEventHandler GetEventHandler()
        {
            return new GamingEventHandler(FixedUpdate);
        }
        void FixedUpdate()
        {
            World.StepSimulation(0.019f * 1000);
            int count = _physicsObject.Count;
            for (int i = 0; i < count; i++)
            {
                _physicsObject[i].FixedUpdate();
            }
        }
        public void UpdateRagdoll(int index)
        {
            _goldsrcRagdolls.Last().UpdateRagdoll();
        }
        public void Run()
        {
            Timer.GameStart(50);
        }
        public void AddRagdoll()
        {
            _goldsrcRagdolls.Add(new GoldsrcRagdoll(World));
        }
        string[] EntityWithInvisableModel =
        {
            "func_buyzone",
            "func_bomb_target"
        };
        public void LoadBsp(string path)
        {
            List<int> invisableModelIndex = new List<int>();
            BspFile bsp = new BspFile();
            BspFile.LoadAllFromFile(bsp, BspFile.LoadFlags.Visuals | BspFile.LoadFlags.Entities, path);
            foreach (var i in bsp.Entities)
            {
                string classname = "";
                if (!i.TryGetValue("classname", out classname))
                    continue;

                for (int j = 0; j < EntityWithInvisableModel.Length; j++)
                {
                    if (classname == EntityWithInvisableModel[j])
                    {
                        invisableModelIndex.Add(Convert.ToInt32(i["model"].Substring(1)));
                        break;
                    }
                }
            }
            List<BvhTriangleMeshShape> shapes = new List<BvhTriangleMeshShape>();
            for (int i = 0; i < bsp.Models.Count; i++)
            {
                if (invisableModelIndex.Contains(i))
                    continue;

                var bspmodel = bsp.Models[i];
                //var gameObject = new GameObject("" + i);
                //gameObject.transform.SetParent(go_all_models.transform);
                //if (i == 0)//如果是静态地图（）
                //{
                //    gameObject.isStatic = true;
                //    gameObject.layer = settings.Model0Layer;
                //}
                //else
                //{
                //    gameObject.layer = settings.OtherModelLayer;
                //}

                // Count triangles
                int submeshCountWithoutSky = 0;
                System.Collections.Generic.KeyValuePair<string, uint[]> kvp_sky = default(KeyValuePair<string, uint[]>);
                foreach (var kvp in bspmodel.Triangles)
                {
                    if (kvp.Key == "sky")
                    {
                        kvp_sky = kvp;
                    }
                    else
                        submeshCountWithoutSky++;
                }

                // Create Mesh
                //var mesh = new Mesh();
                //mesh.name = string.Format("Model {0}", i);
                // Submesh Count
                //mesh.subMeshCount = submeshCountWithoutSky;
                //mesh_renderer.materials = new Material[model.Triangles.Count];

                // Create vertices and uv
                var vertices = new List<Vector3>();
                //var uv = new List<Vector2>();
                for (int ii = 0; ii < bspmodel.Positions.Length; ii++)
                {
                    var p = bspmodel.Positions[ii];
                    vertices.Add(new Vector3(p.X * GBConstant.G2BScale, p.Y * GBConstant.G2BScale, p.Z * GBConstant.G2BScale));

                    //var t = bspmodel.TextureCoordinates[ii];
                    //uv.Add(new Vector2(t.x, -t.y));
                }

                //mesh.SetUVs(0, uv);

                //var materials = new Material[submeshCountWithoutSky];

                int submesh = 0;
                List<int> triangles = new List<int>();
                foreach (var kvp in bspmodel.Triangles)
                {
                    if (kvp.Key == "sky")
                        continue;

                    var indices = new int[kvp.Value.Length];
                    for (int ii = 0; ii < indices.Length; ii += 3)
                    {
                        //indices[ii + 0] = (int)kvp.Value[ii + 0];
                        //indices[ii + 1] = (int)kvp.Value[ii + 1];
                        //indices[ii + 2] = (int)kvp.Value[ii + 2];
                        triangles.Add((int)kvp.Value[ii + 0]);
                        triangles.Add((int)kvp.Value[ii + 1]);
                        triangles.Add((int)kvp.Value[ii + 2]);
                    }
                    //mesh.SetTriangles(indices, submesh: submesh);

                    submesh++;
                }
                var meshShape = new BvhTriangleMeshShape(new TriangleIndexVertexArray(triangles, vertices), true);
                shapes.Add(meshShape);

            }
            foreach (var shape in shapes)
            {
                PhysicsHelper.CreateStaticBody(Matrix.Translation(0, 0, 0), shape, World);
            }
        }
        

        public void Dispose()
        {
            foreach (var obj in _physicsObject)
            {
                obj.Dispose();
            }

            //this.StandardCleanup();
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
