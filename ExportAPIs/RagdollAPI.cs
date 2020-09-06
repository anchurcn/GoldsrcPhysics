using BulletSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics.ExportAPIs
{
    public static class RagdollAPI
    {
        private static RagdollManager Manager { get => PhysicsMain.RagdollManager; }
    }
    public static class PhysicsFileProvider
    {
        //class CheckExist
        //{
        //    public bool Checked;
        //    public bool Existed;
        //}
        static string Postfix = ".phy";
        static string[] LookupDir = {@"valve/models",@"valve/physics" };
        private static Dictionary<string, RagdollData> _RagdollData = new Dictionary<string, RagdollData>();
        //private static Dictionary<string, bool> _HasPhysicsData = new Dictionary<string, bool>();
        //private static Dictionary<string, CheckExist> _PhysicsDataFileInfo = new Dictionary<string, CheckExist>();

        /// <summary>
        /// only check if the physics flie exist
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public static bool RagdollDataExist(string modelName)
        {
            //var result = _PhysicsDataFileInfo[modelName];
            //if (result.Checked)
            //{
            //    return result.Existed;
            //}
            //else
            //{
            //    for (int i = 0; i < LookupDir.Length; i++)
            //    {
            //        result.Existed = File.Exists(LookupDir[i] + modelName + Postfix);
            //        if (result.Existed)
            //        {
            //            result.Checked = true;
            //            return result.Existed;
            //        }
            //    }
            //    Debug.Log("doesn't exist file \"{0}.\"", modelName + Postfix);
            //    result.Checked = true;
            //    return result.Existed;
            //}

            if (_RagdollData.ContainsKey(modelName))//checked
                return _RagdollData[modelName] != null;
            else
            {
                PreCache(modelName);
                return _RagdollData[modelName] != null;
            }
        }
        //public static bool CanPerformPhysics(string modelName)
        //{
        //    bool result = false;
        //    if (!_HasPhysicsData.ContainsKey(modelName))
        //        result = false;
        //    else
        //        result = _HasPhysicsData[modelName];
        //    return result;
        //}

        public static RagdollData GetRagdollData(string modelName)
        {
            RagdollData result = null;

            if (!_RagdollData.TryGetValue(modelName, out result))
            {   //miss hit
                result = LoadFromFile(modelName);
                _RagdollData.Add(modelName, result);
            }
            if (result == null)
                throw new NullReferenceException("ragdoll data can't be null");
            return result;
        }
        public static void PreCache(string modelName)
        {
            if (_RagdollData.ContainsKey(modelName))
                return;

            _RagdollData.Add(modelName, LoadFromFile(modelName));
        }
        private static RagdollData LoadFromFile(string modelName)
        {
            return null;
        }

    }
    public class PhysicsManager
    {
        public static void UpdateEntityMotion(int entityId)
        {
             
        }
    }
}
