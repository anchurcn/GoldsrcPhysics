using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics.Goldsrc
{
    public unsafe class Custom_h
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
        // Customization.h


        const int MAX_QPATH = 64;    // Must match value in quakedefs.h

        /////////////////
        // Customization
        // passed to pfnPlayerCustomization
        // For automatic downloading.
        enum resourcetype_t
        {
            t_sound = 0,
            t_skin,
            t_model,
            t_decal,
            t_generic,
            t_eventscript,
            t_world,        // Fake type for world, is really t_model
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct _resourceinfo_t
        {
            int size;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct resourceinfo_t
        {
            _resourceinfo_t info0;
            _resourceinfo_t info1;
            _resourceinfo_t info2;
            _resourceinfo_t info3;
            _resourceinfo_t info4;
            _resourceinfo_t info5;
            _resourceinfo_t info6;
            _resourceinfo_t info7;
        };

        const int RES_FATALIFMISSING = (1 << 0); // Disconnect if we can't get this file.
        const int RES_WASMISSING = (1 << 1); // Do we have the file locally, did we get it ok?
        const int RES_CUSTOM = (1 << 2); // Is this resource one that corresponds to another player's customization
                                         // or is it a server startup resource.
        const int RES_REQUESTED = (1 << 3);// Already requested a download of this one
        const int RES_PRECACHED = (1 << 4);// Already precached
        const int RES_ALWAYS = (1 << 5);        // download always even if available on client	
        const int RES_CHECKFILE = (1 << 7);// check file on client

        [StructLayout(LayoutKind.Sequential)]
        public struct resource_t
        {
            fixed sbyte szFileName[MAX_QPATH]; // File name to download/precache.
            resourcetype_t type;                // t_sound, t_skin, t_model, t_decal.
            int nIndex;              // For t_decals
            int nDownloadSize;       // Size in Bytes if this must be downloaded.
            byte ucFlags;

            // For handling client to client resource propagation
            fixed byte rgucMD5_hash[16];    // To determine if we already have it.
            byte playernum;           // Which player index this resource is associated with, if it's a custom resource.

            fixed byte rguc_reserved[32]; // For future expansion
            resource_t* pNext;              // Next in chain.
            resource_t* pPrev;
        };

        [StructLayout(LayoutKind.Sequential)]

        public struct customization_t
        {
            bool bInUse;     // Is this customization in use;
            resource_t resource; // The resource_t for this customization
            bool bTranslated; // Has the raw data been translated into a useable format?  
                              //  (e.g., raw decal .wad make into texture_t *)
            int nUserData1; // Customization specific data
            int nUserData2; // Customization specific data
            void* pInfo;          // Buffer that holds the data structure that references the data (e.g., the cachewad_t)
            void* pBuffer;       // Buffer that holds the data for the customization (the raw .wad data)
            customization_t* pNext; // Next in chain
        };

        const int FCUST_FROMHPAK = (1 << 0);
        const int FCUST_WIPEDATA = (1 << 1);
        const int FCUST_IGNOREINIT = (1 << 2);

        //	void COM_ClearCustomizationList( struct customization_s * pHead, qboolean bCleanDecals);
        //qboolean COM_CreateCustomization( struct customization_s * pListHead, struct resource_s * pResource, int playernumber, int flags,
        //			struct customization_s ** pCustomization, int* nLumps ); 
        //int COM_SizeofResourceList( struct resource_s * pList, struct resourceinfo_s * ri );


    }
}
