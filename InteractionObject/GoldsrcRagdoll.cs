using BulletSharp;
using BulletSharp.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics
{

    unsafe class GoldsrcRagdoll : IPhysicsObject
    {
        public Ragdoll BRagdoll;
        public static StudioBoneMatrices StudioBoneMatrices;
        public static StudioQuaternions StudioQuaternions;
        public static StudioPositions StudioPositions;
        Matrix[] OriginalRelatedTransformInverse;//子身体部分相对父部分的变换矩阵的逆，用于计算相对变换的变换量（弃用的）
        Matrix[] OriginalInverseTransforms;// bodypart世界变化矩阵的逆，用于计算实时变换分量

        Matrix[] OrigingalStudioRelativedTransformsInverse;//用于计算相对变化偏移量
        Matrix[] OriginalStudioParentTransformsInverse;//studio原始父世界变换的逆，用于计算相对变换
        public GoldsrcRagdoll(DynamicsWorld world)
        {
            BRagdoll = new Ragdoll(world,GBConstant.G2BScale*StudioPositions[127]);
            OriginalRelatedTransformInverse = new Matrix[(int)BodyPart.Count];
            //Matrix inverse;
            //物理端
            //for (int i = 1; i < (int)BodyPart.Count; i++)
            //{

            //    inverse = BRagdoll._bodies[(int)GetParentPart(i)].WorldTransform;
            //    inverse.Invert();
            //    OriginalRelatedTransformInverse[i] = inverse * BRagdoll._bodies[i].WorldTransform;
            //}
            OriginalRelatedTransformInverse[0] = BRagdoll._bodies[0].WorldTransform.GetInverse();

            //TODO 计算每个rigidbody的初始世界变换的逆
            OriginalInverseTransforms = new Matrix[(int)BodyPart.Count];
            for (int i = 0; i < (int)BodyPart.Count; i++)
            {
                OriginalInverseTransforms[(int)IndexBodyPart[i].Item2] = 
                    BRagdoll._bodies[(int)IndexBodyPart[i].Item2].WorldTransform.GetInverse();
            }
            //studio原始相对变化的逆，用于计算相对变换偏移量（q）
            OrigingalStudioRelativedTransformsInverse = new Matrix[(int)BodyPart.Count];
            for (int i = 1; i < (int)BodyPart.Count; i++)
            {
                OriginalStudioParentTransformsInverse[i] =
                    StudioBoneMatrices[GetParentIndex(IndexBodyPart[i].Item1)].GetInverse();
                OrigingalStudioRelativedTransformsInverse[i] =
                    (
                        StudioBoneMatrices[IndexBodyPart[i].Item1] *
                        OriginalStudioParentTransformsInverse[i]
                    ).GetInverse();
            }
        }
        
        
        public void FixedUpdate()
        {
        }
        public void UpdateRagdoll()
        {
            //Matrix inverse;
            //Quaternion rotation;
            //Vector3 scale;
            //Vector3 translation;
            Matrix result;
            for (int i = 1; i < IndexBodyPart.Length; i++)
            {
                //inverse = BRagdoll._bodies[(int)GetParentPart((int)IndexBodyPart[i].Item2)].WorldTransform;
                //inverse.Invert();
                //result = inverse * BRagdoll._bodies[(int)IndexBodyPart[i].Item2].WorldTransform;//relatived transform
                //result = OriginalRelatedTransformInverse[(int)IndexBodyPart[i].Item2] * result;
                //result.Decompose(out scale, out rotation, out translation);
                //StudioQuaternions[IndexBodyPart[i].Item1] = rotation * StudioQuaternions[IndexBodyPart[i].Item1];

                //计算物理引擎世界变换偏移量
                var worldOffset = BRagdoll._bodies[(int)IndexBodyPart[i].Item2].WorldTransform *
                    OriginalInverseTransforms[(int)IndexBodyPart[i].Item2];
                //应用到goldsrc世界变换
                StudioBoneMatrices[IndexBodyPart[i].Item1] =worldOffset* StudioBoneMatrices[IndexBodyPart[i].Item1];
                //计算物理更新后的相对变换
                var relativedTransf = StudioBoneMatrices[IndexBodyPart[i].Item1] * OriginalStudioParentTransformsInverse[i];
                //计算相对变换偏移量，分解出quaternion
                var rotation = (relativedTransf * OrigingalStudioRelativedTransformsInverse[IndexBodyPart[i].Item1]).DecomQuat();
                //赋值
                StudioQuaternions[IndexBodyPart[i].Item1] = rotation * StudioQuaternions[IndexBodyPart[i].Item1];
            }
            result = BRagdoll._bodies[(int)BodyPart.Pelvis].WorldTransform * OriginalRelatedTransformInverse[0];
            StudioQuaternions[1] = result.DecomQuat();
            var origOffset = BRagdoll._bodies[0].WorldTransform.Origin - OriginalRelatedTransformInverse[0].Origin;
            StudioPositions[1] += GBConstant.B2GScale*new Vector3(-origOffset.X,origOffset.Y,origOffset.Z);

        }
        
        public void Dispose()
        {
            BRagdoll.Dispose();
        }
        
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
        Tuple<int, BodyPart>[] IndexBodyPart = new Tuple<int, BodyPart>[]
        {
            new Tuple<int, BodyPart>(1,BodyPart.Pelvis),
            new Tuple<int, BodyPart>(2,BodyPart.LeftUpperLeg),
            new Tuple<int, BodyPart>(3,BodyPart.LeftLowerLeg),
            new Tuple<int, BodyPart>(5,BodyPart.RightUpperLeg),
            new Tuple<int, BodyPart>(6,BodyPart.RightLowerLeg),
            new Tuple<int, BodyPart>(9,BodyPart.Spine),
                                   
            new Tuple<int, BodyPart>(13,BodyPart.Head),//neck 12
            new Tuple<int, BodyPart>(15,BodyPart.LeftUpperArm),//14
            new Tuple<int, BodyPart>(16,BodyPart.LeftLowerArm),
            new Tuple<int, BodyPart>(22,BodyPart.RightUpperArm),//21
            new Tuple<int, BodyPart>(23,BodyPart.RightLowerArm),
        };

        private int GetParentIndex(int i)
        {
            int p = -1;
            switch(i)
            {
                case 1:
                    throw new Exception("Pelvis has no parent");
                case 2:
                    p = 1;
                    break;
                case 3:
                    p = 2;
                    break;
                case 5:
                    p = 1;
                    break;
                case 6:
                    p = 5;
                    break;
                case 9:
                    p = 8;
                    break;

                case 13:
                    p = 12;
                    break;
                case 15:
                    p = 14;
                    break;
                case 16:
                    p = 15;
                    break;
                case 22:
                    p = 21;
                    break;
                case 23:
                    p = 22;
                    break;
            }
            return p;
        }
        BodyPart GetParentPart(int part)
        {
            BodyPart parent;
            switch (part)
            {
                case (int)BodyPart.Spine:
                    parent = BodyPart.Pelvis;
                    break;
                case (int)BodyPart.Head:
                    parent = BodyPart.Spine;
                    break;
                case (int)BodyPart.LeftUpperArm:
                    parent = BodyPart.Spine;
                    break;
                case (int)BodyPart.RightUpperArm:
                    parent = BodyPart.Spine;
                    break;
                case (int)BodyPart.LeftLowerArm:
                    parent = BodyPart.LeftUpperArm;
                    break;
                case (int)BodyPart.RightLowerArm:
                    parent = BodyPart.RightUpperArm;
                    break;
                case (int)BodyPart.LeftUpperLeg:
                    parent = BodyPart.Pelvis;
                    break;
                case (int)BodyPart.RightUpperLeg:
                    parent = BodyPart.Pelvis;
                    break;
                case (int)BodyPart.LeftLowerLeg:
                    parent = BodyPart.LeftUpperLeg;
                    break;
                case (int)BodyPart.RightLowerLeg:
                    parent = BodyPart.RightUpperLeg;
                    break;
                default:
                    throw new Exception("Pelvis has no parent body part.");
            }
            return parent;
        }

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

    public unsafe class StudioBoneMatrices
    {
        float* HeadPointer;

        public float this[int i,int row,int column]
        {
            
            get
            {
                
                return HeadPointer[i * 12 + row * 4 + column];
            }
            set
            {
                HeadPointer[i * 12 + row * 4 + column] = value;
            }
        }

        public Matrix this[int i]
        {
            get
            {
                return new Matrix() {
                    Column1 = new Vector4(this[i, 0, 0], this[i, 0, 1],this[i, 0, 2], this[i, 0, 3]),
                    Column2 = new Vector4(this[i, 1, 0], this[i, 1, 1],this[i, 1, 2], this[i, 1, 3]),
                    Column3=  new Vector4(this[i, 2, 0], this[i, 2, 1],this[i, 2, 2], this[i, 2, 3]),
                    Column4=  new Vector4(0,0,0,1)
                };
            }
            set
            {
                for (int row = 0; row < 3; row++)
                {
                    this[i, row, 0] = value[0,row];
                    this[i, row, 1] = value[1,row];
                    this[i, row, 2] = value[2,row];
                    this[i, row, 3] = value[3,row];
                }
            }
        }
        public void SetRotation(int i,Matrix rotationMatrix)
        {
            for (int row = 0; row < 3; row++)
            {
                this[i, row, 0] = rotationMatrix[0, row];
                this[i, row, 1] = rotationMatrix[1, row];
                this[i, row, 2] = rotationMatrix[2, row];
            }
        }
        public StudioBoneMatrices(int ptr)
        {
            HeadPointer = (float*)ptr;
        }

    }

    public unsafe class StudioQuaternions
    {
        float* q;
        public StudioQuaternions(int ptr)
        {
            q = (float*)ptr;
        }
        public Quaternion this[int i]
        {
            get => new Quaternion(q[i * 4 + 0], q[i * 4 + 1], q[i * 4 + 2], q[i * 4 + 3]);
            set
            {
                q[i * 4 + 0] = value.X;
                q[i * 4 + 1] = value.Y;
                q[i * 4 + 2] = value.Z;
                q[i * 4 + 3] = value.W;
            }
        }
    }

    public unsafe class StudioPositions
    {
        float* pos;
        public StudioPositions(int ptr)
        {
            pos = (float*)ptr;
        }
        public Vector3 this[int i]
        {
            get => new Vector3(pos[i * 3 + 0], pos[i * 3 + 1], pos[i * 3 + 2]);
            set
            {
                pos[i * 3 + 0] = value.X;
                pos[i * 3 + 1] = value.Y;
                pos[i * 3 + 2] = value.Z;
            }
        }


    }
}
