using BulletSharp;
using BulletSharp.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics
{
    public unsafe class BRigidbodyBone
    {
        private RigidBody RigidBody;

        private Matrix* _Transform;

        /// <summary>
        /// goldsrc object transform
        /// maybe a bone transform or entity tranform
        /// if _Transform point to bone transform, ONLY set it on render
        /// </summary>
        public Matrix Transform
        {
            get
            {
                return (*_Transform)*Matrix.Identity;//apply offset
            }
            set
            {
                *_Transform = value*Matrix.Identity;
            }
        }
    }

    public unsafe class BRigidbodyEntity
    {
        private float* _EntityPos;//vector3
        private float* _EntityRot;//vector3 angles

        RigidBody RigidBody;

        public void Update()
        {
            Vector3* pos = (Vector3*) _EntityPos;
            *pos = RigidBody.WorldTransform.Origin;//TODO:to goldsrc coordinate. Reference bulletsharpUnity
        }
    }
}
