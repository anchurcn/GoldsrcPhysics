using GoldsrcPhysics.LinearMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics.Goldsrc
{
    public unsafe class Studio_h
    {
        /***
*
*	Copyright (c) 1996-2002, Valve LLC. All rights reserved.
*	
*	This product contains software technology licensed from Id 
*	Software, Inc. ("Id Technology").  Id Technology (c) 1996 Id Software, Inc. 
*	All Rights Reserved.
*
*   Use, distribution, and modification of this source code and/or resulting
*   object code is restricted to non-commercial enhancements to products from
*   Valve LLC.  All other use, distribution, or modification is prohibited
*   without written permission from Valve LLC.
*
****/


        /*
		==============================================================================

		STUDIO MODELS

		Studio models are position independent, so the cache manager can move them.
		==============================================================================
		*/


        public const int MAXSTUDIOTRIANGLES = 20000;// TODO: tune this
        public const int MAXSTUDIOVERTS = 2048;// TODO: tune this
        public const int MAXSTUDIOSEQUENCES = 2048;// total animation sequences -- KSH incremented
        public const int MAXSTUDIOSKINS = 100; // total textures
        public const int MAXSTUDIOSRCBONES = 512;  // bones allowed at source movement
        public const int MAXSTUDIOBONES = 128; // total bones actually used
        public const int MAXSTUDIOMODELS = 32;// sub-models per model
        public const int MAXSTUDIOBODYPARTS = 32;
        public const int MAXSTUDIOGROUPS = 16;
        public const int MAXSTUDIOANIMATIONS = 2048;
        public const int MAXSTUDIOMESHES = 256;
        public const int MAXSTUDIOEVENTS = 1024;
        public const int MAXSTUDIOPIVOTS = 256;
        public const int MAXSTUDIOCONTROLLERS = 8;

        public struct studiohdr_t
        {
            public int id;
            public int version;
             
            public fixed sbyte name[64];
            public int length;
             
            public Vector3 eyeposition; // ideal eye position
            public Vector3 min;         // ideal movement hull size
            public Vector3 max;
             
            public Vector3 bbmin;           // clipping bounding box
            public Vector3 bbmax;
             
            public int flags;
             
            public int numbones;           // bones
            public int boneindex;
             
            public int numbonecontrollers;     // bone controllers
            public int bonecontrollerindex;
             
            public int numhitboxes;            // complex bounding boxes
            public int hitboxindex;
             
            public int numseq;             // animation sequences
            public int seqindex;
             
            public int numseqgroups;       // demand loaded sequences
            public int seqgroupindex;
             
            public int numtextures;        // raw textures
            public int textureindex;
            public int texturedataindex;
             
            public int numskinref;         // replaceable textures
            public int numskinfamilies;
            public int skinindex;
             
            public int numbodyparts;
            public int bodypartindex;
             
            public int numattachments;     // queryable attachable points
            public int attachmentindex;
             
            public int soundtable;
            public int soundindex;
            public int soundgroups;
            public int soundgroupindex;
             
            public int numtransitions;     // animation node to animation node transition graph
            public int transitionindex;
        };

        // header for demand loaded sequence group data
        public struct studioseqhdr_t
        {
            int id;
            int version;

            fixed sbyte name[64];
            int length;
        };

        // bones
        public struct mstudiobone_t
        {
            public fixed sbyte name[32];  // bone name for symbolic links
            public int parent;     // parent bone
            public int flags;      // ??
            public fixed int bonecontroller[6];  // bone controller index, -1 == none
            public fixed float value[6]; // default DoF values
            public fixed float scale[6];   // scale for delta DoF values
        };


        // bone controllers
        public struct mstudiobonecontroller_t
        {
            int bone;   // -1 == 0
            int type;   // X, Y, Z, XR, YR, ZR, M
            float start;
            float end;
            int rest;   // byte index value at rest
            int index;  // 0-3 user set controller, 4 mouth
        };

        // intersection boxes
        public struct mstudiobbox_t
        {
            int bone;
            int group;          // intersection group
            Vector3 bbmin;       // bounding box
            Vector3 bbmax;
        };

        public struct cache_user_t
        {
            void* data;
        };


        //
        // demand loaded sequence groups
        //
        public struct mstudioseqgroup_t
        {
            fixed sbyte label[32]; // textual name
            fixed sbyte name[64];  // file name
            int unused1;    // was "cache"  - index pointer
            int unused2;    // was "data" -  hack for group 0
        };

        // sequence descriptions
        public struct mstudioseqdesc_t
        {
            fixed sbyte label[32]; // sequence label

            float fps;      // frames per second	
            int flags;      // looping/non-looping flags

            int activity;
            int actweight;

            int numevents;
            int eventindex;

            int numframes;  // number of frames per sequence

            int numpivots;  // number of foot pivots
            int pivotindex;

            int motiontype;
            int motionbone;
            Vector3 linearmovement;
            int automoveposindex;
            int automoveangleindex;

            Vector3 bbmin;       // per sequence bounding box
            Vector3 bbmax;

            int numblends;
            int animindex;      // mstudioanim_t pointer relative to start of sequence group data
                                // [blend][bone][X, Y, Z, XR, YR, ZR]

            fixed int blendtype[2];   // X, Y, Z, XR, YR, ZR
            fixed float blendstart[2];    // starting value
            fixed float blendend[2];  // ending value
            int blendparent;

            int seqgroup;       // sequence group for demand loading

            int entrynode;      // transition node at entry
            int exitnode;       // transition node at exit
            int nodeflags;      // transition rules

            int nextseq;        // auto advancing sequences
        };

        // events
        //#include "studio_event.h"
        /*
        typedef struct 
        {
            int 				frame;
            int					event;
            int					type;
            char				options[64];
        } mstudioevent_t;
        */

        // pivots
        public struct mstudiopivot_t
        {
            Vector3 org; // pivot point
            int start;
            int end;
        };

        // attachment
        public struct mstudioattachment_t
        {
            fixed sbyte name[32];
            int type;
            int bone;
            Vector3 org; // attachment point
            fixed float vectors[3 * 3];
        };

        public struct mstudioanim_t
        {
            fixed ushort offset[6];
        };

        // animation frames
        [StructLayout(LayoutKind.Explicit)]
        public struct mstudioanimvalue_t
        {
            [FieldOffset(0)]
            byte valid;
            [FieldOffset(1)]
            byte total;
            [FieldOffset(0)]
            short value;
        };



        // body part index
        public struct mstudiobodyparts_t
        {
            fixed sbyte name[64];
            int nummodels;
            int @base;
            int modelindex; // index into models array
        };



        // skin info
        public struct mstudiotexture_t
        {
            fixed sbyte name[64];
            int flags;
            int width;
            int height;
            int index;
        };


        // skin families
        // short	index[skinfamilies][skinref]

        // studio models
        public struct mstudiomodel_t
        {
            fixed sbyte name[64];

            int type;

            float boundingradius;

            int nummesh;
            int meshindex;

            int numverts;       // number of unique vertices
            int vertinfoindex;  // vertex bone info
            int vertindex;      // vertex vec3_t
            int numnorms;       // number of unique surface normals
            int norminfoindex;  // normal bone info
            int normindex;      // normal vec3_t

            int numgroups;      // deformation groups
            int groupindex;
        };


        // vec3_t	boundingbox[model][bone][2];	// complex intersection info


        // meshes
        public struct mstudiomesh_t
        {
            int numtris;
            int triindex;
            int skinref;
            int numnorms;       // per mesh normals
            int normindex;      // normal vec3_t
        };

        // triangles
#if false
typedef struct 
{
	short				vertindex;		// index into vertex array
	short				normindex;		// index into normal array
	short				s,t;			// s,t position on skin
} mstudiotrivert_t;
#endif

        // lighting options
        const int STUDIO_NF_FLATSHADE = 0x0001;
        const int STUDIO_NF_CHROME = 0x0002;
        const int STUDIO_NF_FULLBRIGHT = 0x0004;
        const int STUDIO_NF_NOMIPS = 0x0008;
        const int STUDIO_NF_ALPHA = 0x0010;
        const int STUDIO_NF_ADDITIVE = 0x0020;
        const int STUDIO_NF_MASKED = 0x0040;

        // motion flags
        const int STUDIO_X = 0x0001;
        const int STUDIO_Y = 0x0002;
        const int STUDIO_Z = 0x0004;
        const int STUDIO_XR = 0x0008;
        const int STUDIO_YR = 0x0010;
        const int STUDIO_ZR = 0x0020;
        const int STUDIO_LX = 0x0040;
        const int STUDIO_LY = 0x0080;
        const int STUDIO_LZ = 0x0100;
        const int STUDIO_AX = 0x0200;
        const int STUDIO_AY = 0x0400;
        const int STUDIO_AZ = 0x0800;
        const int STUDIO_AXR = 0x1000;
        const int STUDIO_AYR = 0x2000;
        const int STUDIO_AZR = 0x4000;
        const int STUDIO_TYPES = 0x7FFF;
        const int STUDIO_RLOOP = 0x8000;    // controller that wraps shortest distance

        // sequence flags
        const int STUDIO_LOOPING = 0x0001;

        const float M_PI = (float)Math.PI;
        // bone flags
        const int intSTUDIO_HAS_NORMALS = 0x0001;
        const int intSTUDIO_HAS_VERTICES = 0x0002;
        const int intSTUDIO_HAS_BBOX = 0x0004;
        const int intSTUDIO_HAS_CHROME = 0x0008;    // if any of the textures have chrome on them

        const float intRAD_TO_STUDIO = (32768.0f / M_PI);
        const float intSTUDIO_TO_RAD = (M_PI / 32768.0f);



    }
}
