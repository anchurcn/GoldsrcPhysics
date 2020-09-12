using BulletSharp;
using GoldsrcPhysics.Utils;
using System;
using System.Runtime.InteropServices;
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
            get 
            {
                var res = Transforms[i].ToBullet();
                res.Origin *= GBConstant.G2BScale;
                return res;   
            }
            set
            {
                var val = value;
                val.Origin *= GBConstant.B2GScale;
                Transforms[i] = val;
            }
        }
    }
    public unsafe class StudioRenderer
    {
        public static StudioModelRenderer* NativePointer;
        /// <summary>
        /// read or the bone transform that scaled, and write the bone transform that scaled for you
        /// </summary>
        public static StudioTransforms ScaledBoneTransform;

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
            void* p = pointerToStudioRenderer.ToPointer();
            NativePointer = (StudioModelRenderer*)p;
            ScaledBoneTransform = new StudioTransforms(((StudioModelRenderer*)p)->m_pbonetransform);
        }
        public static DebugDraw Drawer;
        public static void DrawCurrentSkeleton()
        {
            for (int i = 0; i < BoneCount; i++)
            {
                    if (Bones[i].parent == -1)
                        continue;
                Drawer.DrawLine(ScaledBoneTransform[i].Origin, ScaledBoneTransform[Bones[i].parent].Origin, new BulletSharp.Math.Vector3(0.9f, 0.9f, 0));
                
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
        public int m_fDoInterp;
        // Do gait estimation?
        public int m_fGaitEstimation;

        // Current render frame #
        public int m_nFrameCount;

        // Cvars that studio model code needs to reference
        //
        // Use high quality models?
        public cvar_t* m_pCvarHiModels;
        // Developer debug output desired?
        public cvar_t* m_pCvarDeveloper;
        // Draw entities bone hit boxes, etc?
        public cvar_t* m_pCvarDrawEntities;

        // The entity which we are currently rendering.
        public cl_entity_t* m_pCurrentEntity;

        // The model for the entity being rendered
        /* model_t* */
        public void* m_pRenderModel;

        // Player info for current player, if drawing a player
        public player_info_t* m_pPlayerInfo;

        // The index of the player being drawn
        public int m_nPlayerIndex;

        // The player's gait movement
        public float m_flGaitMovement;

        // Pointer to header block for studio model data
        public studiohdr_t* m_pStudioHeader;

        // Pointers to current body part and submodel
        public mstudiobodyparts_t* m_pBodyPart;
        public mstudiomodel_t* m_pSubModel;

        // Palette substition for top and bottom of model
        public int m_nTopColor;
        public int m_nBottomColor;

        //
        // Sprite model used for drawing studio model chrome
        /* model_t* */
        public void* m_pChromeSprite;

        // Caching
        // Number of bones in bone cache
        public int m_nCachedBones;
        // Names of cached bones
        public fixed sbyte m_nCachedBoneNames[MAXSTUDIOBONES * 32];
        // Cached bone & light transformation matrices
        public fixed float m_rgCachedBoneTransform[MAXSTUDIOBONES * 3 * 4];
        public fixed float m_rgCachedLightTransform[MAXSTUDIOBONES * 3 * 4];

        // Software renderer scale factors
        public float m_fSoftwareXScale, m_fSoftwareYScale;

        // Current view vectors and render origin
        public fixed float m_vUp[3];
        public fixed float m_vRight[3];
        public fixed float m_vNormal[3];

        public fixed float m_vRenderOrigin[3];

        // Model render counters ( from engine )
        int* m_pStudioModelCount;
        int* m_pModelsDrawn;

        // Matrices
        // Model to world transformation
        public Matrix34f* m_protationmatrix;
        // Model to view transformation
        public Matrix34f* m_paliastransform;

        // Concatenated bone and light transforms
        public Matrix34f* m_pbonetransform;
        public Matrix34f* m_plighttransform;
    }
}
