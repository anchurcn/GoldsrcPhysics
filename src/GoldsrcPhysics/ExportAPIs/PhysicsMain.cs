using BulletSharp;
using BulletSharp.Math;
using GoldsrcPhysics.Forms;
using GoldsrcPhysics.Goldsrc;
using GoldsrcPhysics.Goldsrc.Bsp;
using GoldsrcPhysics.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GoldsrcPhysics.ExportAPIs
{
    

    /// <summary>
    /// Provide the interface for goldsrc that can access to.
    /// </summary>
    public static unsafe class PhysicsMain
    {
        #region Static Properties

        private readonly static RagdollManager _ragdollManager = new RagdollManager();

        //private readonly static LocalPlayerBodyPicker _localBodyPicker = new LocalPlayerBodyPicker();

        private readonly static List<RigidBody> _sceneStaticObjects = new List<RigidBody>();

        private readonly static PickerManager _pickerManager = new PickerManager();

        private static bool _isPaused;

        #endregion

        #region Config
        public static void Set([In]sbyte* key,[In]sbyte* value)
        {
            PhyConfiguration.SetValue(
                Marshal.PtrToStringAnsi((IntPtr)key),
                Marshal.PtrToStringAnsi((IntPtr)value)
                );
        }
        #endregion

        #region Test


        public static void Test()
        {
            TestAPI.Test();
        }


        #endregion

        #region PhysicsSystem
        /// <summary>
        /// 
        /// </summary>
        private static bool _initialized = false;
        /// <summary>
        
        /// Init physics system
        /// if the struct layout is different from default layout, that will throw a fatal error.
        /// </summary>
        /// <param name="pStudioRenderer">the address of StudioModelRenderer's first field. (m_clTime)</param>
        /// <param name="lastFieldAddress">>the address of StudioModelRenderer's last field. (m_plighttransform)</param>
        /// <param name="engineStudioAPI">pIEngineStudio</param>
        public static unsafe void InitSystem([In]sbyte* modFolder,void* pEngineStudioAPI)
        {
            InitGlobal();
            PhyConfiguration.Init(Marshal.PtrToStringAnsi((IntPtr)modFolder));
            SetEngineStudioAPI(pEngineStudioAPI);
        }

        /// <summary>
        /// Can be only called once at a process
        /// </summary>
        private static void InitGlobal()
        {
            if (_initialized)
                return;

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((sender, e) =>
            {
                var exception = e.ExceptionObject as Exception;
                if (exception != null)
                {
#if DEBUG
                    StackTrace stackTrace = new StackTrace(exception, true);
#else
                    StackTrace stackTrace = new StackTrace(exception);
#endif
                    MessageBox.Show($"{exception.GetType().FullName}:\"{exception.Message}\"\n\n{exception.StackTrace}", "Unhandled Exception From GoldsrcPhysics", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                }
            });

            //Set native lib search path
            if (IntPtr.Size == 8)
            {
                Environment.SetEnvironmentVariable("PATH", Path.Combine(Directory.GetCurrentDirectory(), @"gsphysics\bin\x64"));
            }
            else if (IntPtr.Size == 4)
            {
                Environment.SetEnvironmentVariable("PATH", Path.Combine(Directory.GetCurrentDirectory(), @"gsphysics\bin\x86"));
            }
            // set managed lib search path
            //Environment.SetEnvironmentVariable("PATH", Path.Combine(Directory.GetCurrentDirectory(), @"gsphysics\bin"));

            _initialized = true;
        }

        private static unsafe void SetEngineStudioAPI(void* pEngineStudioAPI)
        {
            IEngineStudio.Init((EngineStudioAPI*)pEngineStudioAPI);
            //register goldsrc global variables
            //拿到金源引擎的API，使物理引擎可以访问缓存的模型信息、地图信息等
            BWorld.CreateInstance();
            StudioRenderer.Init();
            StudioRenderer.Drawer = BWorld.Instance.DebugDrawer;
            //Validation
            //if ((void*)(&StudioRenderer.NativePointer->m_plighttransform) != lastFieldAddress)
            //    throw new Exception("Studio model renderer is invalid.");

        }

        /// <summary>
        /// Load map geomitry collider. 
        /// </summary>
        /// <param name="mapName"></param>
        public static void ChangeLevel([In]sbyte* mapName)
        {
            for (int i = 0; i < _sceneStaticObjects.Count; i++)
            {
                BWorld.Instance.RemoveRigidBody(_sceneStaticObjects[i]);
                _sceneStaticObjects[i].Dispose();
            }
            _sceneStaticObjects.Clear();
            LoadScene(Marshal.PtrToStringAnsi((IntPtr)mapName));
        }
        /// <summary>
        /// 地图不变，内容重置，清理在游戏中动态创建的各种CollisionObjects
        /// cs的每一局结束可以调用
        /// </summary>
        public static void LevelReset()
        {
            _bodyPicker.RemovePickingConstraint();
            ClearRagdoll();
        }

        /// <summary>
        /// Physics world update
        /// </summary>
        /// <param name="delta"></param>
        public static void Update(float delta)
        {
            if (_isPaused)
                return;
            //handling input
            //player's collider pos, bodypicker's pos


            //physics simulating
            if (delta < 0)
                return;
            
                Time.SubStepCount += BWorld.Instance.StepSimulation(delta);

            Time.DeltaTime = delta;

            //drawing
            {//buffered draw
                //BWorld.Instance.DebugDrawWorld();
                //just put lines in buffer, then you should draw it later.
            }
            {//normal draw
                //(BWorld.Instance.DebugDrawer as PhysicsDebugDraw).DrawDebugWorld(BWorld.Instance);
            }
        }

        public static void Pause()
        {
            _isPaused = true;
        }

        public static void Resume()
        {
            _isPaused = false;
        }

        /// <summary>
        /// Close physics system and release physics resources.
        /// </summary>
        public static void ShotDown()
        {

        }
        /// <summary>
        /// Show configration form.
        /// Using cvar to call this is recommended.
        /// </summary>
        public static void ShowConfigForm()
        {
            new Form1().Show();
        }
        #endregion

        #region RagdollAPI
        public static void CreateRagdollController(int entityId, sbyte* modelName)
        {
            string name = Marshal.PtrToStringAnsi((IntPtr)modelName);
            _ragdollManager.CreateRagdollController(entityId, name);
        }
        public static void CreateRagdollControllerIndex(int entityId, int index)
        {
            _ragdollManager.CreateRagdollController(entityId, index);
        }
        public static void CreateRagdollControllerModel(int entityId,model_t* model)
        {
            _ragdollManager.CreateRagdollController(entityId, model);
        }

        public static void StartRagdoll(int entityId)
        {
            _ragdollManager.StartRagdoll(entityId);
        }
        public static void StopRagdoll(int entityId)
        {
            _ragdollManager.StopRagdoll(entityId);
        }
        public static void SetupBonesPhysically(int entityId)
        {
            _ragdollManager.SetupBonesPhysically(entityId);
        }

        public static void ChangeOwner(int oldEntity, int newEntity)
        {
            _ragdollManager.ChangeOwner(oldEntity, newEntity);
        }

        public static void SetVelocity(int entityId, Vector3* v)
        {
            _ragdollManager.SetVelocity(entityId, *v);
        }
        public static void DisposeRagdollController(int entityId)
        {
            _ragdollManager.DisposeRagdollController(entityId);
        }
        public static void ImpulseBone(int entityId, int boneId, Vector3* force)
        {
            _ragdollManager.ImpulseBone(entityId, boneId, *force);
        }
        public static void ClearRagdoll()
        {
            _ragdollManager.ClearRagdoll();
        }
        public static void HeadShootRagdoll(int entityId, Vector3* force)
        {
            _ragdollManager.HeadShootRagdoll(entityId, *force);
        }

        /// <summary>
        /// Set ragdoll pose for given entity.
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="pBoneWorldTransform"></param>
        public static void SetPose(int entityId, float* pBoneWorldTransform)
        {
            _ragdollManager.SetPose(entityId, pBoneWorldTransform);
        }
        #endregion

        #region Interaction

        /// <summary>
        /// Set an explosion on the specified position.
        /// The impact range is calculated automatically via intensity.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="intensity"></param>
        public static void Explosion(Vector3* pos, float intensity)
        {
            float r = 30;
            var world = BWorld.Instance;
            for (int i = 0; i < world.NumCollisionObjects; i++)
            {
                var obj = world.CollisionObjectArray[i];
                var point = obj.WorldTransform.Origin;
                var dir = (point - (*pos)*GBConstant.G2BScale);
                var distsqared = dir.LengthSquared;
                if (distsqared < r * r)
                {
                    var force = intensity / distsqared;
                    dir.Normalize();
                    (obj as RigidBody)?.ApplyCentralImpulse(dir * force);
                }
            }
        }

        /// <summary>
        /// Shoot an invisable bullet to apply impulse to the rigidbody it hits.
        /// </summary>
        /// <param name="from">eye pos.</param>
        /// <param name="force">contains direction and intensity.</param>
        public static void Shoot(Vector3* from, Vector3* force)
        {
            var gunPos = *from * GBConstant.G2BScale;
            var to = gunPos + *force*GBConstant.G2BScale;
            var world = BWorld.Instance;
            using (var rayCallback = new ClosestRayResultCallback(ref gunPos, ref to))
            {
                world.RayTestRef(ref *from, ref to, rayCallback);
                if (rayCallback.HasHit)
                {
                    Vector3 pickPosition = rayCallback.HitPointWorld;
                    var body = rayCallback.CollisionObject as RigidBody;
                    if (body != null)
                    {
                        body.ApplyCentralImpulseRef(ref *force);
                    }
                    //else
                    //{
                    //    var collider = rayCallback.CollisionObject as MultiBodyLinkCollider;
                    //    if (collider != null)
                    //    {
                    //        (collider as RigidBody).ApplyCentralImpulseRef(ref *force);
                    //    }
                    //}
                }
            }
        }
        #endregion

        #region BodyPicker
        
        //public static void PickBody()
        //{
        //    _localBodyPicker.PickBody();
        //}

        //public static void ReleaseBody()
        //{
        //    _localBodyPicker.Release();
        //}

        private static BodyPicker _bodyPicker = new BodyPicker();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from">Eye pos or camera origin</param>
        /// <param name="to">Camera origin + direction</param>
        public static void PickBodyLocal(Vector3 from,Vector3 to)
        {
            _bodyPicker.PickBody(from, to);
        }
        /// <summary>
        /// Release picked body
        /// </summary>
        public static void ReleaseBodyLocal()
        {
            _bodyPicker.RemovePickingConstraint();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from">Eye pos or camera origin</param>
        /// <param name="to">Camera origin + direction</param>
        public static void MoveBodyLocal(Vector3 from, Vector3 to)
        {
            _bodyPicker.MovePickedBody(from, to);
        }
        #endregion

        private static void LoadScene(string levelName)
        {
            if (string.IsNullOrEmpty(levelName))
            {
                Debug.LogLine("Level name is null ot empty.");
                return;
            }
            var path = PhyConfiguration.GetValue("ModDir");
            var filePath = Path.Combine(path,levelName);
            Debug.LogLine("Load map {0}", filePath);

            var bspLoader = new BspLoader(filePath);
            _sceneStaticObjects.Add(BulletHelper.CreateStaticBody(Matrix.Translation(0, 0, 0),
                new BvhTriangleMeshShape(bspLoader.StaticGeometry, true),
                BWorld.Instance));
        }
    }
}
