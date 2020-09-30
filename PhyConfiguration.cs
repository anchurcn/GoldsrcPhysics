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
        private static Dictionary<string, string> Configuration { get; }
        internal static string GetValue(string key)
        {
            //return Configuration[key];
            string result = null;
            if (Configuration.TryGetValue(key, out result))
                return result;
            else
            {
                Debug.LogLine("Missing Configuration [{0}].", key);
                return null;
            }
        }
        static PhyConfiguration()
        {
            Configuration = new Dictionary<string, string>();
            TextReader reader = new StreamReader(File.OpenRead(@".\gsphysics\physics.cfg"),Encoding.Default);
            string line = reader.ReadLine();
            Debug.LogLine("reading physics.cfg...");
            while (line!=null)
            {
                line.Trim();
                if (line[0] == '#'||line=="")
                {
                    line = reader.ReadLine();
                    continue;
                }

                var pair = line.Split('$');
                Configuration.Add(pair[0], pair[1]);
                Debug.LogLine("{0}\t:{1}", pair[0], pair[1]);
                line = reader.ReadLine();
            }
            Debug.LogLine("configuration reading completed.");
        }
    }
    
}
