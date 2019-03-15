using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics
{
    public class PhysicsMain//Provide the interface to goldsrc that can access to
    {
        public static Simulation WorldSimulation;

        public static int WorldSpawn(string noNeed)
        {
            if (WorldSimulation != null)
                return 0;
            WorldSimulation = new Simulation();
            WorldSimulation.Run();
            return 0;
        }

        public static int WorldDispose(string noNeed)
        {
            WorldSimulation.Dispose();
            WorldSimulation = null;
            return 0;
        }

        public static int AddRagdoll(string ptr)
        {
            if(WorldSimulation==null)
            {
                WorldSpawn("");
            }
            WorldSimulation.SpawnRagdoll(Convert.ToInt32(ptr));
            return 0;
        }

        public static int AddCube(string ptr)
        {
            WorldSimulation.
        }

    }
    
}
