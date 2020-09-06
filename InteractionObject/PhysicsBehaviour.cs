using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics.InteractionObject
{
    public class PhysicsBehaviour
    {
        /// <summary>
        /// Calls by stutio renderer when it needs the physics state to setup bones
        /// this function will use StudioHelper setup bones transform
        /// </summary>
        public virtual void OnPhysicsSetupBones()
        {
            //
        }

        public void PhysicsUpdate()
        {
            
        }
    }
}
