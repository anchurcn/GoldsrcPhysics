using HLView.Formats.Bsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static GoldsrcPhysics.Goldsrc.Studio_h;

namespace GoldsrcPhysics.Goldsrc
{
	internal struct EngineStudioAPI
	{
		// Allocate number*size bytes and zero it
		internal IntPtr Mem_Calloc;
		// Check to see if pointer is in the cache
		internal IntPtr Cache_Check;
		// Load file into cache ( can be swapped out on demand )
		internal IntPtr LoadCacheFile;
		// Retrieve model pointer for the named model
		internal IntPtr Mod_ForName;
		// Retrieve pointer to studio model data block from a model
		internal IntPtr Mod_Extradata;
		// Retrieve indexed model from client side model precache list
		internal IntPtr GetModelByIndex;
		// Get entity that is set for rendering
		internal IntPtr GetCurrentEntity;
		// Get referenced player_info_t
		internal IntPtr PlayerInfo;
		// Get most recently received player state data from network system
		internal IntPtr GetPlayerState;
		// Get viewentity
		internal IntPtr GetViewEntity;
		// Get current frame count, and last two timestampes on client
		internal IntPtr GetTimes;
		// Get a pointer to a cvar by name
		internal IntPtr GetCvar;
		// Get current render origin and view vectors ( up, right and vpn )
		internal IntPtr GetViewInfo;
		// Get sprite model used for applying chrome effect
		internal IntPtr GetChromeSprite;
		// Get model counters so we can incement instrumentation
		internal IntPtr GetModelCounters;
		// Get software scaling coefficients
		internal IntPtr GetAliasScale;
		
		// Get bone, light, alias, and rotation matrices
		internal IntPtr StudioGetBoneTransform;
		internal IntPtr StudioGetLightTransform;
		internal IntPtr StudioGetAliasTransform;
		internal IntPtr StudioGetRotationMatrix;
		
		// Set up body part, and get submodel pointers
		internal IntPtr StudioSetupModel;
		// Check if entity's bbox is in the view frustum
		internal IntPtr StudioCheckBBox;
		// Apply lighting effects to model
		internal IntPtr StudioDynamicLight;
		internal IntPtr StudioEntityLight;
		internal IntPtr StudioSetupLighting;
		
		// Draw mesh vertices
		internal IntPtr StudioDrawPoints;
		
		// Draw hulls around bones
		internal IntPtr StudioDrawHulls;
		// Draw bbox around studio models
		internal IntPtr StudioDrawAbsBBox;
		// Draws bones
		internal IntPtr StudioDrawBones;
		// Loads in appropriate texture for model
		internal IntPtr StudioSetupSkin;
		// Sets up for remapped colors
		internal IntPtr StudioSetRemapColors;
		// Set's player model and returns model pointer
		internal IntPtr SetupPlayerModel;
		// Fires any events embedded in animation
		internal IntPtr StudioClientEvents;
		// Retrieve/set forced render effects flags
		internal IntPtr GetForceFaceFlags;
		internal IntPtr SetForceFaceFlags;
		// Tell engine the value of the studio model header
		internal IntPtr StudioSetHeader;
		// Tell engine which model_t * is being renderered
		internal IntPtr SetRenderModel;
		
		// Final state setup and restore for rendering
		internal IntPtr SetupRenderer;
		internal IntPtr RestoreRenderer;
		
		// Set render origin for applying chrome effect
		internal IntPtr SetChromeOrigin;
		
		 // True if using D3D/OpenGL
		internal IntPtr IsHardware;
		
		// Only called by hardware interface
		internal IntPtr GL_StudioDrawShadow;
		internal IntPtr GL_SetRenderMode;
		
		internal IntPtr StudioSetRenderamt;  //!!!CZERO added for rendering glass on viewmodels
		internal IntPtr StudioSetCullState;
		internal IntPtr StudioRenderShadow;
	}
	internal struct model_t
	{
		internal unsafe fixed sbyte name[64];
	}
	// Retrieve pointer to studio model data block from a model (model_t* mod)
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal unsafe delegate studiohdr_t* Mod_ExtradataDelegate (model_t* mod );

	internal unsafe delegate model_t* GetModelByIndexDelegate(int index);

	internal unsafe delegate model_t* Mod_ForNameDelegate(sbyte* name, bool crash_if_missing);


	internal static class IEngineStudio
	{
		// Retrieve pointer to studio model data block from a model (model_t* mod)
		internal static Mod_ExtradataDelegate Mod_Extradata;

		// Retrieve indexed model from client side model precache list
		internal static GetModelByIndexDelegate GetModelByIndex;

		// Retrieve model pointer for the named model
		internal static Mod_ForNameDelegate Mod_ForName;

		internal unsafe static void Init(EngineStudioAPI* pEngineStudio)
		{
			Mod_Extradata = Marshal.GetDelegateForFunctionPointer<Mod_ExtradataDelegate>(pEngineStudio->Mod_Extradata);
			GetModelByIndex = Marshal.GetDelegateForFunctionPointer<GetModelByIndexDelegate>(pEngineStudio->GetModelByIndex);
			Mod_ForName = Marshal.GetDelegateForFunctionPointer<Mod_ForNameDelegate>(pEngineStudio->Mod_ForName);
		}
	}
}
