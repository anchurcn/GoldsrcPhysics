using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics
{
    public static class PhyConfiguration
    {
        //public static dynamic Configuration { get; }
        private static Dictionary<string, string> Configuration { get; }
        public static string GetValue(string key)
        {
            //return Configuration[key];
            return Configuration[key];
        }
        static PhyConfiguration()
        {
            Configuration = new Dictionary<string, string>();
            TextReader reader = new StreamReader(File.OpenRead("physics.cfg"),Encoding.Default);
            string line = reader.ReadLine();
            Debug.LogLine("reading physics.cfg...");
            while (line!=null)
            {
                line.Trim();
                if (line[0] == '#')
                    continue;

                var pair = line.Split(':');
                Configuration.Add(pair[0], pair[1]);
                Debug.LogLine("{0}\t:{1}", pair[0], pair[1]);
                line = reader.ReadLine();
            }
            Debug.LogLine("configuration reading completed.");
        }
    }
    
}
