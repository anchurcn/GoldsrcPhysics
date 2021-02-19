using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics
{
    internal static class PhyConfiguration
    {
        //public static dynamic Configuration { get; }
        private static readonly Dictionary<string, string> _configuration =
            _configuration = new Dictionary<string, string>();
        internal static void SetValue(string key,string value)
        {
            if (_configuration.ContainsKey(key))
                _configuration[key] = value;
            else
                _configuration.Add(key, value);
        }
        internal static string GetValue(string key)
        {
            //return Configuration[key];
            string result = null;
            if (_configuration.TryGetValue(key, out result))
                return result;
            else
            {
                Debug.LogLine("Missing Configuration [{0}].", key);
                return null;
            }
        }
        internal static void Init(string modFolder)
        {
            LoadFromFile();
            SetValue("ModDir", modFolder);
        }
        private static void LoadFromFile()
        {
            TextReader reader = new StreamReader(File.OpenRead(@".\gsphysics\physics.cfg"), Encoding.Default);
            string line = reader.ReadLine();
            Debug.LogLine("reading physics.cfg...");
            while (line != null)
            {
                line.Trim();
                if (line == "" || line[0] == '#')
                {
                    line = reader.ReadLine();
                    continue;
                }

                var pair = line.Split('$');
                _configuration.Add(pair[0], pair[1]);
                Debug.LogLine("{0}\t:{1}", pair[0], pair[1]);
                line = reader.ReadLine();
            }
            Debug.LogLine("configuration reading completed.");
        }
    }
    
}
