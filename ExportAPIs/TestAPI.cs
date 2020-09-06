using System;

namespace GoldsrcPhysics.ExportAPIs
{
    public static class TestAPI
    {
        static bool Initialized = false;
        static int LastSeq;
        static int Attack = 24;
        static int Pistol = 25;
        static int CurSeq;
        static bool AddPlayer = true;

        /// <summary>
        /// calls on player render
        /// 应该在entity是玩家时调用
        /// 在第三人称模式用自己的模型测试
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static int Test(string modelName)
        {
            //if(!isPlayer)
            //return 0;

            if(!Initialized)
            {
                //precache ragdoll data
                PhysicsFileProvider.PreCache(modelName);
                Initialized = true;
                return 0;
            }
            if(AddPlayer)
            {
                //new ragdoll
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

            if(LastSeq!=Pistol&&CurSeq==Pistol)
            {
                //enable ragdoll
            }
            else if(LastSeq==Pistol&&CurSeq==Pistol)
            {
                //update bone 
            }
            else if(LastSeq==Pistol&&CurSeq!=Pistol)
            {
                //disable ragdoll
            }
            LastSeq = CurSeq;
            return 0;
        }
    }
}
