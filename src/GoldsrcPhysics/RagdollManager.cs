using BulletSharp.Math;
using GoldsrcPhysics.ExportAPIs;
using GoldsrcPhysics.Goldsrc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GoldsrcPhysics.Goldsrc.Studio_h;

namespace GoldsrcPhysics
{

    public interface IGoldsrcBehaviour
    {
        void Awake();//：当一个脚本被实例化时，Awake 被调用。我们大多在这个类中完成成员变量的初始化。
        void Start();//：仅在 Update 函数第一次被调用前调用。因为它是在 Awake 之后被调用的，我们可以把一些需要依赖 Awake 的变量放在Start里面初始化。 同时我们还大多在这个类中执行 StartCoroutine 进行一些协程的触发。要注意在用C#写脚本时，必须使用 StartCoroutine 开始一个协程，但是如果使用的是 JavaScript，则不需要这么做。
        void Update();//：当开始播放游戏帧时（此时，GameObject 已实例化完毕），其 Update 在 每一帧 被调用。
        void LateUpdate();//：LateUpdate 是在所有 Update 函数调用后被调用。
        void FixedUpdate();    //：当 MonoBehaviour 启用时，其 FixedUpdate 在每一固定帧被调用。
        void OnEnable();//：当对象变为可用或激活状态时此函数被调用。
        void OnDisable();//：当对象变为不可用或非激活状态时此函数被调用。
        void OnDestroy();//：当 MonoBehaviour 将被销毁时，这个函数被调用。
        void Dispose();
    }
    public class RagdollManager
    {
        #region Useless
        List<IGoldsrcBehaviour> PhysicsServicesList { get; }


        public void Configure()
        {
        }

        public void Update()
        {
            for (int i = 0; i < PhysicsServicesList.Count; i++)
            {
                PhysicsServicesList[i].Update();
            }
        }
        public void ImpulseBone(int entityId, int boneId, Vector3 force)
        {

        }
        public void HeadShootRagdoll(int entityId, Vector3 force)
        {

        }
        #endregion
        //access in O(1)
        private readonly Ragdoll[] _ragdolls = new Ragdoll[4096];//entityId->BRagdoll

        //not null ragdoll index
        private List<int> _registered = new List<int>();

        public void SetVelocity(int entityId,Vector3 v)
        {
            _ragdolls[entityId].SetVelocity(v);
        }


        public RagdollManager()
        {
            PhysicsServicesList = new List<IGoldsrcBehaviour>();
        }

        /// <summary>
        /// precache model physics data
        /// </summary>
        /// <param name="modelName"></param>
        public void PrecacheRagdoll(string modelName)
        {
            PhysicsFileProvider.PreCache(modelName);
        }
        /// <summary>
        /// Create a ragdoll instance for specified entity
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="modelName"></param>
        public void CreateRagdollController(int entityId, string modelName)
        {
            _ragdolls[entityId] = RagdollBuilder.Build(modelName);
            if(_ragdolls[entityId]!=null)
            {
                _ragdolls[entityId].EntityId = entityId;
                _registered.Add(entityId);
            }
        }
        public void CreateRagdollController(int entityId,int index)
        {
            _ragdolls[entityId] = RagdollBuilder.Build(index);
            if (_ragdolls[entityId] != null)
            {
                _ragdolls[entityId].EntityId = entityId;
                _registered.Add(entityId);
            }
        }
        public unsafe void CreateRagdollController(int entityId,model_t* model)
        {
            _ragdolls[entityId] = RagdollBuilder.Build(model);
            if (_ragdolls[entityId] != null)
            {
                _ragdolls[entityId].EntityId = entityId;
                _registered.Add(entityId);
            }
        }
        /// <summary>
        /// change owner when the corpse changed from player entity to just a model entity
        /// </summary>
        /// <param name="oldEntity"></param>
        /// <param name="newEntity"></param>
        public void ChangeOwner(int oldEntity, int newEntity)
        {
            if (_ragdolls[oldEntity] == null)
                return;
            _ragdolls[newEntity] = _ragdolls[oldEntity];
            _ragdolls[newEntity].EntityId = newEntity;
            _ragdolls[oldEntity] = null;
            _registered.Remove(oldEntity);
            _registered.Add(newEntity);
        }
        /// <summary>
        /// enable ragdoll for specified entity
        /// </summary>
        /// <param name="entityId"></param>
        public void StartRagdoll(int entityId)
        {
            _ragdolls[entityId]?.Enable();
        }
        public void StopRagdoll(int entityId)
        {
            _ragdolls[entityId]?.Disable();
        }

        public unsafe void SetPose(int entityId, float* pBoneWorldTransform)
        {
            _ragdolls[entityId]?.SetPose(pBoneWorldTransform);
        }
        public void ClearRagdoll()
        {
            foreach (var i in _registered)
            {
                _ragdolls[i].Disable();
                _ragdolls[i].Dispose();
                _ragdolls[i] = null;
            }
            _registered.Clear();
        }
        public void DisposeRagdollController(int entityId)
        {
            var ragdoll = _ragdolls[entityId];
            if (ragdoll == null)
                return;
            ragdoll.Disable();
            ragdoll.Dispose();
            _ragdolls[entityId] = null;
            _registered.Remove(entityId);
        }
       
        public void SetupBonesPhysically(int entityId)
        {
            _ragdolls[entityId]?.SetupBones();
        }
    }
    public static class Time
    {
        public static float DeltaTime;

        public static int SubStepCount;
    }

}
