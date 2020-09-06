using BulletSharp;
using BulletSharp.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics.Utils
{
    public class BulletHelper
    {
        public static RigidBody CreateBoneRigidbody(float mass,ref Matrix boneTrans,ref Matrix rigidTrans, CollisionShape shape)
        {
            // A dynamic body with zero mass is invalid
            if (mass == 0)
            {
                throw new ArgumentException("{0} can not be zero.", nameof(mass));
            }

            // Using a motion state is recommended,
            // it holds the offset between bone and rigidbody
            var myMotionState = new BoneMotionState(boneTrans,rigidTrans);

            Vector3 localInertia = shape.CalculateLocalInertia(mass);

            RigidBody body;
            using (var rbInfo = new RigidBodyConstructionInfo(mass, myMotionState, shape, localInertia))
            {
                body = new RigidBody(rbInfo);
            }
            return body;
        }
    }
}
