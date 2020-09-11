using BulletSharp.Math;
using GoldsrcPhysics.ExportAPIs;
using GoldsrcPhysics.Goldsrc;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        List<IGoldsrcBehaviour> PhysicsServicesList { get; }
        //access in O(1)
        Ragdoll[] Ragdolls = new Ragdoll[4096];//entityId->BRagdoll
        //not null ragdoll index
        List<int> Register = new List<int>();

        public RagdollManager()
        {
            PhysicsServicesList = new List<IGoldsrcBehaviour>();
        }

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
            Ragdolls[entityId] = RagdollBuilder.Build(modelName, RagdollBuilder.BuildOption.Bipped);
            if(Ragdolls[entityId]!=null)
                Register.Add(entityId);
        }
        /// <summary>
        /// change owner when the corpse changed from player entity to just a model entity
        /// </summary>
        /// <param name="oldEntity"></param>
        /// <param name="newEntity"></param>
        public void ChangeOwner(int oldEntity, int newEntity)
        {
            if (Ragdolls[oldEntity] == null)
                return;
            Ragdolls[newEntity] = Ragdolls[oldEntity];
            Ragdolls[oldEntity] = null;
            Register.Remove(oldEntity);
            Register.Add(newEntity);
        }
        /// <summary>
        /// enable ragdoll for specified entity
        /// </summary>
        /// <param name="entityId"></param>
        public void StartRagdoll(int entityId)
        {
            Ragdolls[entityId]?.EnableRagdoll();
        }
        public void StopRagdoll(int entityId)
        {
            Ragdolls[entityId]?.DisableRagdoll();
        }
        public void ClearRagdoll()
        {
            foreach (var i in Register)
            {
                Ragdolls[i].DisableRagdoll();
                Ragdolls[i].Dispose();
                Ragdolls[i] = null;
            }
            Register.Clear();
        }
        public void DisposeRagdoll(int entityId)
        {
            var ragdoll = Ragdolls[entityId];
            if (ragdoll == null)
                return;
            ragdoll.DisableRagdoll();
            ragdoll.Dispose();
            Ragdolls[entityId] = null;
            Register.Remove(entityId);
        }
        public void ImpulseBone(int entityId, int boneId, Vector3 force)
        {

        }
        public void HeadShootRagdoll(int entityId, Vector3 force)
        {

        }
        public void SetupBonesPhysically(int entityId)
        {
            Ragdolls[entityId]?.SetupBones();
        }
    }
    public static class Time
    {
        public unsafe static float DeltaTime { get =>(float)( StudioRenderer.NativePointer->m_clTime - StudioRenderer.NativePointer->m_clOldTime); }

        public static int SubStepCount;
    }

}
