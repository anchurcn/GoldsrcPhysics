using BulletSharp.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics
{

    unsafe class GoldsrcRagdoll : IPhysicsObject
    {
        public Ragdoll BRagdoll;

        Matrix[] LastTransforms;

        public GoldsrcRagdoll(Ragdoll ragdoll,int ptrToValues)
        {
            BRagdoll = ragdoll;
            values = (float**)ptrToValues;
            LastTransforms = new Matrix[(int)BodyPart.Count];
            for (int i = 0; i < (int)BodyPart.Count; i++)
            {
                LastTransforms[i] = BRagdoll._bodies[indexToBodypart[i]].WorldTransform;
            }
        }
        
        
        public void FixedUpdate()
        {
            for (int j = 0; j < (int)BodyPart.Count; j++)
            {
                Vector3 rotation = GetPartRotation(indexToBodypart[j]);
                BRagdoll._bodies[0].WorldTransform.
                values[j][3] += rotation.X;
                values[j][4] += rotation.Y;
                values[j][5] += rotation.Z;
            }
        }
        
        public void Dispose()
        {
            BRagdoll.Dispose();
        }

        float** values;
        int[] indexToBodypart = new int[]{//0 ref bodypart 2 head
            (int)BodyPart.Head,
            (int)BodyPart.LeftLowerArm,
            (int)BodyPart.LeftUpperArm,
            (int)BodyPart.RightUpperArm,
            (int)BodyPart.RightLowerArm,
            (int)BodyPart.Spine,
            (int)BodyPart.Pelvis,
            (int)BodyPart.LeftUpperLeg,
            (int)BodyPart.RightUpperLeg,
            (int)BodyPart.LeftLowerLeg,
            (int)BodyPart.RightLowerLeg,
        };
        private Vector3 GetPartRotation(BodyPart bodyPart)
        {
            //return BRagdoll._bodies[(int)bodyPart].WorldTransform
            Matrix localMatrix = new Matrix();
            Vector3 eulerRotation = new Vector3(
                (float)Math.Atan2(localMatrix.M32, localMatrix.M33), 
                (float)Math.Atan2(-localMatrix.M31, Math.Sqrt(localMatrix.M32 * localMatrix.M32 + localMatrix.M33 * localMatrix.M33)),
                (float)Math.Atan2(localMatrix.M21,localMatrix.M11));
            return eulerRotation;
        }
        private Vector3 GetPartRotation(int bodyPart)
        {
            Matrix localMatrix = new Matrix();
            Vector3 eulerRotation = new Vector3(
                (float)Math.Atan2(localMatrix.M32, localMatrix.M33),
                (float)Math.Atan2(-localMatrix.M31, Math.Sqrt(localMatrix.M32 * localMatrix.M32 + localMatrix.M33 * localMatrix.M33)),
                (float)Math.Atan2(localMatrix.M21, localMatrix.M11));
            return eulerRotation;
        }

    }
}
