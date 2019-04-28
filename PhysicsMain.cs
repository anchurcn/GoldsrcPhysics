using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoldsrcPhysics
{
    public class PhysicsMain//Provide the interface to goldsrc that can access to
    {
        public static Simulation WorldSimulation;
        
        public static int AddBspWorld(string bsppath)
        {
            WorldSimulation.LoadBsp(@"E:\sjz\xash3d_fwgs_win32_0.19.2\valve\maps\crossfire.bsp");
            var args = bsppath.Split('|');
            GoldsrcRagdoll.StudioPositions = new StudioPositions(Convert.ToInt32(args[0]));
            GoldsrcRagdoll.StudioQuaternions = new StudioQuaternions(Convert.ToInt32(args[1]));
            GoldsrcRagdoll.StudioBoneMatrices = new StudioBoneMatrices(Convert.ToInt32(args[2]));
            return 0;
        }

        public static int StartSimulation(string noneed)
        {
            WorldSimulation = new Simulation();
            WorldSimulation.Run();
            return 0;
        }

        public static int AddRagdoll(string arg)
        {
            WorldSimulation.AddRagdoll();
            return 0;
        }
        public static int UpdateRagdoll(string arg)
        {
            WorldSimulation.UpdateRagdoll(0);
            return 0;
        }
        

        public static int AddCube(string ptr)
        {
            if (WorldSimulation == null)
                return 1;
            WorldSimulation.SpawnBox(Convert.ToInt32(ptr));
            return 0;
        }

    }
    public class GBConstant
    {
        public const float G2BScale = 0.01905f;
        public const float B2GScale = 1/0.01905f;
        public const int ValuesX = 0;
        public const int ValuesY = 2;
        public const int ValuesZ = 1;
    }
    
}
