using BulletSharp.Math;
using GoldsrcPhysics.Goldsrc;
using System;

namespace GoldsrcPhysics.ExportAPIs
{
    public unsafe static class TestAPI
    {
        static bool Initialized = false;
        static bool AddPlayer = true;
        static int LastSeq;
        static int CurSeq=>StudioRenderer.NativePointer->m_pCurrentEntity->curstate.sequence;
        //25 拿着撬棍
        static int Attack = 26;//撬棍攻击
        static int Pistol = 33;//拿着手枪

        /// <summary>
        /// calls on player render
        /// 应该在entity是玩家时调用
        /// 在第三人称模式用自己的模型测试
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static void Test()
        {
            //if(!isPlayer)
            //return 0;

            if(!Initialized)
            {
                //precache ragdoll data
                //PhysicsFileProvider.PreCache(modelName);
                PhysicsMain.ChangeLevel("crossfire");
                Initialized = true;
                return;
            }
            if(AddPlayer)
            {
                //new ragdoll
                RagdollAPI.CreateRagdollController(StudioRenderer.EntityId, "gordon");
                AddPlayer = false;
            }

            if(LastSeq!= Attack && CurSeq==Attack)
            {
                //pickup
            }
            else if(LastSeq==Attack && CurSeq==Attack)
            {
                //move picked body
            }
            else if(LastSeq==Attack&&CurSeq!=Attack)
            {
                //remove
            }
            StudioRenderer.DrawCurrentSkeleton();
            var origin=StudioRenderer.NativePointer->m_pCurrentEntity->curstate.origin;
            var from = Vector3.Zero;
            var color = new Vector3(0.9f, 0.9f, 0);
            StudioRenderer.Drawer.DrawLine(ref from,ref origin,ref color);
            if(LastSeq!=Pistol&&CurSeq==Pistol)
            {
                //enable ragdoll
                RagdollAPI.StartRagdoll(StudioRenderer.EntityId);
            }
            else if(LastSeq==Pistol&&CurSeq==Pistol)
            {
                //update bone 
                RagdollAPI.SetupBonesPhysically(StudioRenderer.EntityId);
            }
            else if(LastSeq==Pistol&&CurSeq!=Pistol)
            {
                //disable ragdoll
                RagdollAPI.StopRagdoll(StudioRenderer.EntityId);
            }
            LastSeq = CurSeq;
        }
    }
}
