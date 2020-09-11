using GoldsrcPhysics.LinearMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics.Goldsrc
{
	public unsafe class goldsrctype
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct cvar_t
		{
			public byte* name;//const char	*
							  //Technically this should be non-const but that only matters to engine code
			public byte* @string;//const char* 
			public int flags;
			public float value;
			public cvar_t* next;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct position_history_t
		{
			// Time stamp for this movement
			float animtime;

			Vector3 origin;
			Vector3 angles;
		}
		const int HISTORY_MAX = 64;

		[StructLayout(LayoutKind.Sequential)]
		public struct cl_entity_t
		{
			public int index;      // Index into cl_entities ( should match actual slot, but not necessarily )

			public bool player;     // True if this entity is a "player"

			public entity_state_t baseline;   // The original state from which to delta during an uncompressed message
			public entity_state_t prevstate;  // The state information from the penultimate message received from the server
			public entity_state_t curstate;   // The state information from the last message received from server

			public int current_position;  // Last received history update index
								   //position_history_t ph[HISTORY_MAX];   // History of position and angle updates for this player
			fixed float ph[7 * HISTORY_MAX];

			public mouth_t mouth;          // For synchronizing mouth movements.

			latchedvars_t latched;      // Variables used by studio model rendering routines

			// Information based on interplocation, extrapolation, prediction, or just copied from last msg received.
			//
			public float lastmove;

			// Actual render position and angles
			public Vector3 origin;
			public Vector3 angles;

			// Attachment points
			fixed float attachment[3 * 4];//vector3[4]

			// Other entity local information
			int trivial_accept;

			void* model;//model_t* model;         // cl.model_precache[ curstate.modelindes ];  all visible entities have a model
			void* efrag;//efrag_t* efrag;			// linked list of efrags
			void* topnode;//mnode_t * topnode;		// for bmodels, first world node that splits bmodel, or NULL if not split

			float syncbase;     // for client-side animations -- used by obsolete alias animation system, remove?
			int visframe;       // last frame this entity was found in an active leaf
			colorVec cvFloorColor;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct colorVec
		{
			uint r, g, b, a;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct mouth_t
		{
			byte mouthopen;     // 0 = mouth closed, 255 = mouth agape
			byte sndcount;      // counter for running average
			int sndavg;         // running average
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct entity_state_t
		{
			// Fields which are filled in by routines outside of delta compression
			public int entityType;
			 // Index into cl_entities array for this entity.
			public int number;
			public float msg_time;
			 
			 // Message number last time the player/entity state was updated.
			public int messagenum;
			 
			 // Fields which can be transitted and reconstructed over the network stream
			public Vector3 origin;
			public Vector3 angles;
			 
			public int modelindex;
			public int sequence;
			public float frame;
			public int colormap;
			public short skin;
			public short solid;
			public int effects;
			public float scale;
			 
			public byte eflags;
			 
			 // Render information
			public int rendermode;
			public int renderamt;
			public color24 rendercolor;
			public int renderfx;
			 
			public int movetype;
			public float animtime;
			public float framerate;
			public int body;
			public fixed byte controller[4];
			public fixed byte blending[4];
			public Vector3 velocity;
			 
			 // Send bbox down to client for use during prediction.
			public Vector3 mins;
			public Vector3 maxs;
			 
			public int aiment;
			 // If owned by a player, the index of that player ( for projectiles ).
			public int owner;
			 
			 // Friction, for prediction.
			public float friction;
			 // Gravity multiplier
			public float gravity;
			 
			 // PLAYER SPECIFIC
			public int team;
			public int playerclass;
			public int health;
			public bool spectator;
			public int weaponmodel;
			public int gaitsequence;
			 // If standing on conveyor, e.g.
			public Vector3 basevelocity;
			 // Use the crouched hull, or the regular player hull.
			public int usehull;
			 // Latched buttons last time state updated.
			public int oldbuttons;
			 // -1 = in air, else pmove entity number
			public int onground;
			public int iStepLeft;
			 // How fast we are falling
			public float flFallVelocity;
			 
			public float fov;
			public int weaponanim;
			 
			 // Parametric movement overrides
			public Vector3 startpos;
			public Vector3 endpos;
			public float impacttime;
			public float starttime;
			 
			 // For mods
			public int iuser1;
			public int iuser2;
			public int iuser3;
			public int iuser4;
			public float fuser1;
			public float fuser2;
			public float fuser3;
			public float fuser4;
			public Vector3 vuser1;
			public Vector3 vuser2;
			public Vector3 vuser3;
			public Vector3 vuser4;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct color24
		{
			byte r, g, b;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct latchedvars_t
		{
			float prevanimtime;
			float sequencetime;
			fixed byte prevseqblending[2];
			Vector3 prevorigin;
			Vector3 prevangles;

			int prevsequence;
			float prevframe;

			fixed byte prevcontroller[4];
			fixed byte prevblending[2];
		}
		const int MAX_MODEL_NAME = 64;
		#region NouseForNow
		//	[StructLayout(LayoutKind.Sequential)]
		//	public struct model_t
		//	{
		//		fixed byte name[MAX_MODEL_NAME];
		//		qboolean needload;      // bmodels and sprites don't cache normally

		//		modtype_t type;
		//		int numframes;
		//		synctype_t synctype;

		//		int flags;

		//		//
		//		// volume occupied by the model
		//		//		
		//		vec3_t mins, maxs;
		//		float radius;

		//		//
		//		// brush model
		//		//
		//		int firstmodelsurface, nummodelsurfaces;

		//		int numsubmodels;
		//		dmodel_t* submodels;

		//		int numplanes;
		//		mplane_t* planes;

		//		int numleafs;       // number of visible leafs, not counting 0
		//		struct mleaf_s      *leafs;

		//int numvertexes;
		//		mvertex_t* vertexes;

		//		int numedges;
		//		medge_t* edges;

		//		int numnodes;
		//		mnode_t* nodes;

		//		int numtexinfo;
		//		mtexinfo_t* texinfo;

		//		int numsurfaces;
		//		msurface_t* surfaces;

		//		int numsurfedges;
		//		int* surfedges;

		//		int numclipnodes;
		//		dclipnode_t* clipnodes;

		//		int nummarksurfaces;
		//		msurface_t** marksurfaces;

		//		hull_t hulls[MAX_MAP_HULLS];

		//		int numtextures;
		//		texture_t** textures;

		//		byte* visdata;

		//		color24* lightdata;

		//		char* entities;

		//		//
		//		// additional model data
		//		//
		//		cache_user_t cache;     // only access through Mod_Extradata

		//	}
		//	[StructLayout(LayoutKind.Sequential)]
		//	public struct naming
		//	{

		//	}
		//	[StructLayout(LayoutKind.Sequential)]
		//	public struct naming
		//	{

		//	}
		//	[StructLayout(LayoutKind.Sequential)]
		//	public struct naming
		//	{

		//	}
		#endregion

	}
}

