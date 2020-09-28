using BulletSharp.Math;
using GoldsrcPhysics.Goldsrc;
using System;
using System.Runtime.InteropServices;
using System.Text;

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
        static Vector3 LastOrigin;
        public static float k=1.35f;
        /// <summary>
        /// calls on player render
        /// 应该在entity是玩家时调用
        /// 在第三人称模式用自己的模型测试
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static void Test()
        {
            var curent = StudioRenderer.NativePointer->m_pCurrentEntity;
            //if(!isPlayer)
            //return 0;

            if (!Initialized)
            {
                //precache ragdoll data
                //PhysicsFileProvider.PreCache(modelName);
                string map = "crossfire";
                sbyte* mapName = (sbyte*)Marshal.StringToHGlobalAnsi(map);
                PhysicsMain.ChangeLevel(mapName);
                Initialized = true;
                PhysicsMain.ShowConfigForm();
                return;
            }
            if(AddPlayer)
            {
                //new ragdoll
                PhysicsMain.CreateRagdollControllerHeader(StudioRenderer.EntityId,StudioRenderer.StudioHeader );
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
                PhysicsMain.StartRagdoll(StudioRenderer.EntityId);
                var v = (curent->origin - LastOrigin) * k;
                PhysicsMain.SetVelocity(StudioRenderer.EntityId,&v );
            }
            else if(LastSeq==Pistol&&CurSeq==Pistol)
            {
                //update bone 
                PhysicsMain.SetupBonesPhysically(StudioRenderer.EntityId);
            }
            else if(LastSeq==Pistol&&CurSeq!=Pistol)
            {
                //disable ragdoll
                PhysicsMain.StopRagdoll(StudioRenderer.EntityId);
            }
            LastSeq = CurSeq;
            
            var e = 0.00001f;
            //if (
            //    StudioRenderer.NativePointer->m_pCurrentEntity->curstate.velocity.Length > e||
            //    StudioRenderer.NativePointer->m_pCurrentEntity->baseline.velocity.Length>e||
            //     (curent->prevstate.origin-curent->origin).Length>e||
            //     curent->prevstate.basevelocity.Length>e||
            //     curent->prevstate.velocity.Length>e
            //    )
            //    throw new Exception();
            LastOrigin = curent->origin;
        }
    }
}
