using BspLib.Bsp;
using BulletSharp;
using BulletSharp.Math;
using DemoFramework;
using GoldsrcPhysics.Goldsrc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoldsrcPhysics
{
    
    public class PhysicsMain//Provide the interface to goldsrc that can access to
    {
        internal static RagdollManager RagdollManager { get; set; }
        private static LocalPlayerBodyPicker LocalPicker { get; set; }

        private static List<RigidBody> Scene { get; } = new List<RigidBody>();

        public static void InitSystem(IntPtr pStudioRenderer)
        {
            //注册金源引擎的各种变量
            //拿到金源引擎的API，使物理引擎可以访问缓存的模型信息、地图信息等
            BWorld.CreateInstance();
            StudioRenderer.Init(pStudioRenderer);
            StudioRenderer.Drawer = BWorld.Instance.DebugDrawer;
            RagdollManager = new RagdollManager();
            LocalPicker = new LocalPlayerBodyPicker();
        }
        /// <summary>
        /// 加载地图
        /// </summary>
        public static void ChangeLevel()
        {
            for (int i = 0; i < Scene.Count; i++)
            {
                BWorld.Instance.RemoveRigidBody(Scene[i]);
            }
            LoadScene("crossfire.bps");
        }
        /// <summary>
        /// 地图不变，内容重置，清理在游戏中动态创建的各种CollisionObjects
        /// cs的每一局结束可以调用
        /// </summary>
        public static void LevelReset()
        {

        }
        public static void Update()
        {
            //handling input
            //player's collider pos, bodypicker's pos
            LocalPicker.Update();

            //physics simulating
            Time.SubStepCount+=BWorld.Instance.StepSimulation(Time.DeltaTime);


            //drawing
            {//not covered draw
                //BWorld.Instance.DebugDrawWorld();
                //just put lines in buffer, then you should draw it on ViewDrawing
            }
            {//normal draw
                StudioRenderer.DrawCurrentSkeleton();
                (BWorld.Instance.DebugDrawer as PhysicsDebugDraw).DrawDebugWorld(BWorld.Instance);
            }
        }

        public static void Pause()
        {

        }

        public static void Resume()
        {

        }

        public static void ShotDown()
        {

        }
        #region BodyPicker
        public static void PickBody()
        {
            LocalPicker.PickBody();
        }

        public static void ReleaseBody()
        {
            LocalPicker.Release();
        }

        #endregion

        #region PrivateMethod
        public static void LoadScene(string levelName)
        {
            var path = PhyConfiguration.GetValue("MapDir");
            LoadBsp(path + levelName);
        }
        static string[] EntityWithInvisableModel =
{
            "func_buyzone",
            "func_bomb_target"
        };
        private static void LoadBsp(string path)
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
                Scene.Add( PhysicsHelper.CreateStaticBody(Matrix.Translation(0, 0, 0), shape, BWorld.Instance));
            }
        }
        #endregion



        //#region Obsolete
        //const string ObsoleteMsg = "this class is deprecated, please use PhysicsMain class instead.";
        //public static Simulation WorldSimulation;

        //public static int AddBspWorld(string bsppath)
        //{
        //    //WorldSimulation.LoadBsp(@"E:\sjz\xash3d_fwgs_win32_0.19.2\valve\maps\crossfire.bsp");
        //    var args = bsppath.Split('|');
        //    GoldsrcRagdoll.StudioPositions = new StudioPositions(Convert.ToInt32(args[0]));
        //    GoldsrcRagdoll.StudioQuaternions = new StudioQuaternions(Convert.ToInt32(args[1]));
        //    GoldsrcRagdoll.StudioBoneMatrices = new StudioBoneMatrices(Convert.ToInt32(args[2]));
        //    return 0;
        //}

        //public static int StartSimulation(string noneed)
        //{
        //    WorldSimulation = new Simulation();
        //    WorldSimulation.Run();
        //    return 0;
        //}

        //public static int AddRagdoll(string arg)
        //{
        //    WorldSimulation.AddRagdoll();
        //    return 0;
        //}
        //public static int UpdateRagdoll(string arg)
        //{
        //    WorldSimulation.UpdateRagdoll(0);
        //    return 0;
        //}
        //public static int Update(string noarg)
        //{
        //    throw new NotImplementedException();
        //    //WorldSimulation.FixedUpdate();
        //    return 0;
        //}

        //public static int AddCube(string ptr)
        //{
        //    if (WorldSimulation == null)
        //        return 1;
        //    WorldSimulation.SpawnBox(Convert.ToInt32(ptr));
        //    return 0;
        //}
        //#endregion

    }
    public class GBConstant
    {
        public const float G2BScale = 0.01905f;
        public const float B2GScale = 1/0.01905f;
        public const int ValuesX = 0;
        public const int ValuesY = 2;
        public const int ValuesZ = 1;
    }
    
}
