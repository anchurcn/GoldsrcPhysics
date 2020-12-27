using BulletSharp;
using BulletSharp.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics.Utils
{
    public static class BulletHelper
    {
        public static RigidBody CreateBoneRigidbody(float mass, ref Matrix boneTrans, ref Matrix rigidTrans, CollisionShape shape)
        {
            // A dynamic body with zero mass is invalid
            if (mass == 0)
            {
                throw new ArgumentException("{0} can not be zero.", nameof(mass));
            }

            // Using a motion state is recommended,
            // it holds the offset between bone and rigidbody
            var myMotionState = new BoneMotionState(boneTrans, rigidTrans);

            Vector3 localInertia = shape.CalculateLocalInertia(mass);

            RigidBody body;
            using (var rbInfo = new RigidBodyConstructionInfo(mass, myMotionState, shape, localInertia))
            {
                body = new RigidBody(rbInfo);
            }
            return body;
        }
        public static TypedConstraint CreateJoint(RigidBody bodyA, RigidBody bodyB, in Vector3 pivot)
        {
            var pivotInA = bodyA.TransformToLocal(in pivot);
            var pivotInB = bodyB.TransformToLocal(in pivot);
            //should use ConeTwistConstraint here, local frame means local coordinates/局部坐标系
            return new Point2PointConstraint(bodyA, bodyB, pivotInA, pivotInB);
            // disable collisions between bodies connected with the constraint. This will done at World.AddConstraint
        }
        public static RigidBody CreateLimb(ref Matrix bone, Vector3 child, float radius)
        {
            var len = (bone.Origin - child).Length;
            var shape = new CapsuleShape(radius, len);
            var rigidTrans = BulletMathUtils.CenterOf(bone.Origin, child).LookAt(child, Vector3.UnitY);

            var mass = 1;// auto calc mass via shape volumn. (may have better algorithm to do this)
            // Update Tip:because of Simulation becomes unstable when a heavy object is resting on a very light object. 
            // It is best to keep the mass around 1.This means accurate interaction between a tank and a very light object is not realistic. 
            // So we just simply set the mass to 1.
            return CreateBoneRigidbody(mass, ref bone, ref rigidTrans, shape);
        }
        static Random Rand;
        static BulletHelper()
        {
            Rand = new Random();
        }
        public static int RandomInt(int min,int max)
        {
            return Rand.Next(min, max);
        }
        public static RigidBody CreateBody(float mass, Matrix startTransform, CollisionShape shape, DynamicsWorld world)
        {
            // A body with zero mass is considered static
            if (mass == 0)
            {
                return CreateStaticBody(startTransform, shape, world);
            }

            // Using a motion state is recommended,
            // it provides interpolation capabilities and only synchronizes "active" objects
            var myMotionState = new DefaultMotionState(startTransform);

            Vector3 localInertia = shape.CalculateLocalInertia(mass);

            RigidBody body;
            using (var rbInfo = new RigidBodyConstructionInfo(mass, myMotionState, shape, localInertia))
            {
                body = new RigidBody(rbInfo);
            }

            if (world != null)
            {
                world.AddRigidBody(body);
            }

            return body;
        }

        public static RigidBody CreateStaticBody(Matrix startTransform, CollisionShape shape, DynamicsWorld world)
        {
            const float staticMass = 0;

            RigidBody body;
            using (var rbInfo = new RigidBodyConstructionInfo(staticMass, null, shape)
            {
                StartWorldTransform = startTransform
            })
            {
                body = new RigidBody(rbInfo);
            }

            if (world != null)
            {
                world.AddRigidBody(body);
            }

            return body;
        }
    }
}
