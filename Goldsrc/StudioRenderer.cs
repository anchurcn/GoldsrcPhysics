using BulletSharp;
using GoldsrcPhysics.LinearMath;
using GoldsrcPhysics.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static GoldsrcPhysics.Goldsrc.ComModel_h;
using static GoldsrcPhysics.Goldsrc.goldsrctype;
using static GoldsrcPhysics.Goldsrc.Studio_h;

namespace GoldsrcPhysics.Goldsrc
{
    public unsafe class StudioTransforms
    {
        private Matrix34f* Transforms;
        public StudioTransforms(void* MatrixArrPtr)
        {
            Transforms = (Matrix34f*)MatrixArrPtr;
        }

        public BulletSharp.Math.Matrix this[int i]
        {
            get=>Transforms[i].ToBullet() * GBConstant.G2BScale;
            set => Transforms[i] = value * GBConstant.B2GScale;
        }
    }
    public unsafe class StudioRenderer
    {
        public static StudioModelRenderer* NativePointer;
        /// <summary>
        /// read or the bone transform that scaled, and write the bone transform that scaled for you
        /// </summary>
        public static StudioTransforms BoneTransform;

        public static studiohdr_t* StudioHeader => NativePointer->m_pStudioHeader;
        public static mstudiobone_t* Bones
        {
            get
            {
                return (mstudiobone_t*)((byte*)StudioHeader + StudioHeader->boneindex);
            }
        }

        public static int BoneCount => StudioHeader->numbones;

        public static int EntityId { get => NativePointer->m_pCurrentEntity->index; }

        public static void Init(IntPtr pointerToStudioRenderer)
        {
            NativePointer = (StudioModelRenderer*)pointerToStudioRenderer;
            BoneTransform = new StudioTransforms(NativePointer->m_pbonetransform);
        }
        public static DebugDraw Drawer;
        public static void DrawCurrentSkeleton()
        {
            for (int i = 0; i < BoneCount; i++)
            {
                    if (Bones[i].parent == -1)
                        continue;
                Drawer.DrawLine(BoneTransform[i].Origin, BoneTransform[Bones[i].parent].Origin, new BulletSharp.Math.Vector3(1, 0, 0));
                
            }
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct StudioModelRenderer
    {
        // Client clock
        public double m_clTime;
        // Old Client clock
        public double m_clOldTime;

        // Do interpolation?
        int m_fDoInterp;
        // Do gait estimation?
        int m_fGaitEstimation;

        // Current render frame #
        int m_nFrameCount;

        // Cvars that studio model code needs to reference
        //
        // Use high quality models?
        cvar_t* m_pCvarHiModels;
        // Developer debug output desired?
        cvar_t* m_pCvarDeveloper;
        // Draw entities bone hit boxes, etc?
        cvar_t* m_pCvarDrawEntities;

        // The entity which we are currently rendering.
        public cl_entity_t* m_pCurrentEntity;

        // The model for the entity being rendered
        /* model_t* */
        void* m_pRenderModel;

        // Player info for current player, if drawing a player
        public player_info_t* m_pPlayerInfo;

        // The index of the player being drawn
        int m_nPlayerIndex;

        // The player's gait movement
        float m_flGaitMovement;

        // Pointer to header block for studio model data
        public studiohdr_t* m_pStudioHeader;

        // Pointers to current body part and submodel
        mstudiobodyparts_t* m_pBodyPart;
        mstudiomodel_t* m_pSubModel;

        // Palette substition for top and bottom of model
        int m_nTopColor;
        int m_nBottomColor;

        //
        // Sprite model used for drawing studio model chrome
        /* model_t* */
        void* m_pChromeSprite;

        // Caching
        // Number of bones in bone cache
        int m_nCachedBones;
        // Names of cached bones
        fixed sbyte m_nCachedBoneNames[MAXSTUDIOBONES * 32];
        // Cached bone & light transformation matrices
        fixed float m_rgCachedBoneTransform[MAXSTUDIOBONES * 3 * 4];
        fixed float m_rgCachedLightTransform[MAXSTUDIOBONES * 3 * 4];

        // Software renderer scale factors
        float m_fSoftwareXScale, m_fSoftwareYScale;

        // Current view vectors and render origin
        public fixed float m_vUp[3];
        public fixed float m_vRight[3];
        public fixed float m_vNormal[3];

        fixed float m_vRenderOrigin[3];

        // Model render counters ( from engine )
        int* m_pStudioModelCount;
        int* m_pModelsDrawn;

        // Matrices
        // Model to world transformation
        Matrix34f* m_protationmatrix;
        // Model to view transformation
        Matrix34f* m_paliastransform;

        // Concatenated bone and light transforms
        public Matrix34f* m_pbonetransform;
        Matrix34f* m_plighttransform;
    }
}
