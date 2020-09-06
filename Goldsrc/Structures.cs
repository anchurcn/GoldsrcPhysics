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

			entity_state_t baseline;   // The original state from which to delta during an uncompressed message
			entity_state_t prevstate;  // The state information from the penultimate message received from the server
			entity_state_t curstate;   // The state information from the last message received from server

			int current_position;  // Last received history update index
								   //position_history_t ph[HISTORY_MAX];   // History of position and angle updates for this player
			fixed float ph[7 * HISTORY_MAX];

			mouth_t mouth;          // For synchronizing mouth movements.

			latchedvars_t latched;      // Variables used by studio model rendering routines

			// Information based on interplocation, extrapolation, prediction, or just copied from last msg received.
			//
			float lastmove;

			// Actual render position and angles
			Vector3 origin;
			Vector3 angles;

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
			int entityType;
			// Index into cl_entities array for this entity.
			int number;
			float msg_time;

			// Message number last time the player/entity state was updated.
			int messagenum;

			// Fields which can be transitted and reconstructed over the network stream
			Vector3 origin;
			Vector3 angles;

			int modelindex;
			int sequence;
			float frame;
			int colormap;
			short skin;
			short solid;
			int effects;
			float scale;

			byte eflags;

			// Render information
			int rendermode;
			int renderamt;
			color24 rendercolor;
			int renderfx;

			int movetype;
			float animtime;
			float framerate;
			int body;
			fixed byte controller[4];
			fixed byte blending[4];
			Vector3 velocity;

			// Send bbox down to client for use during prediction.
			Vector3 mins;
			Vector3 maxs;

			int aiment;
			// If owned by a player, the index of that player ( for projectiles ).
			int owner;

			// Friction, for prediction.
			float friction;
			// Gravity multiplier
			float gravity;

			// PLAYER SPECIFIC
			int team;
			int playerclass;
			int health;
			bool spectator;
			int weaponmodel;
			int gaitsequence;
			// If standing on conveyor, e.g.
			Vector3 basevelocity;
			// Use the crouched hull, or the regular player hull.
			int usehull;
			// Latched buttons last time state updated.
			int oldbuttons;
			// -1 = in air, else pmove entity number
			int onground;
			int iStepLeft;
			// How fast we are falling
			float flFallVelocity;

			float fov;
			int weaponanim;

			// Parametric movement overrides
			Vector3 startpos;
			Vector3 endpos;
			float impacttime;
			float starttime;

			// For mods
			int iuser1;
			int iuser2;
			int iuser3;
			int iuser4;
			float fuser1;
			float fuser2;
			float fuser3;
			float fuser4;
			Vector3 vuser1;
			Vector3 vuser2;
			Vector3 vuser3;
			Vector3 vuser4;
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

