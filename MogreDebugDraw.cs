//using BulletSharp;
//using Mogre;

//namespace YourNamespace
//{
//	public class BulletDebugDrawer : IDebugDraw, IDisposable
//	{
//		SceneManager sceneMgr;
//		ManualObject lines;
//		ManualObject triangles;
//		public DebugDrawModes DebugMode { get; set; }

//		bool begin = false;

//		public BulletDebugDrawer()
//		{
//			sceneMgr = LKernel.Get<SceneManager>();

//			lines = new ManualObject("physics lines");
//			triangles = new ManualObject("physics triangles");
//			lines.Dynamic = true;
//			triangles.Dynamic = true;

//			sceneMgr.RootSceneNode.AttachObject(lines);
//			sceneMgr.RootSceneNode.AttachObject(triangles);

//			string matName = "OgreBulletCollisionsDebugDefault";
//			MaterialPtr mtl = MaterialManager.Singleton.GetDefaultSettings().Clone(matName);
//			mtl.ReceiveShadows = false;
//			mtl.SetSceneBlending(SceneBlendType.SBT_TRANSPARENT_ALPHA);
//			mtl.SetDepthBias(0.1f, 0);

//			TextureUnitState tu = mtl.GetTechnique(0).GetPass(0).CreateTextureUnitState();
//			tu.SetColourOperationEx(LayerBlendOperationEx.LBX_SOURCE1, LayerBlendSource.LBS_DIFFUSE);
//			mtl.GetTechnique(0).SetLightingEnabled(false);

//			lines.Begin(matName, RenderOperation.OperationTypes.OT_LINE_LIST);
//			begin = true;
//			lines.Position(Vector3.ZERO);
//			lines.Colour(ColourValue.Blue);
//			lines.Position(Vector3.ZERO);
//			lines.Colour(ColourValue.Blue);
//			lines.End();

//			triangles.Begin(matName, RenderOperation.OperationTypes.OT_TRIANGLE_LIST);
//			triangles.Position(Vector3.ZERO);
//			triangles.Colour(ColourValue.Blue);
//			triangles.Position(Vector3.ZERO);
//			triangles.Colour(ColourValue.Blue);
//			triangles.Position(Vector3.ZERO);
//			triangles.Colour(ColourValue.Blue);
//			triangles.End();
//			begin = false;

//			DebugMode = DebugDrawModes.DrawWireframe | DebugDrawModes.DrawAabb | DebugDrawModes.DrawContactPoints;

//			// I have two events, one that fires right before we call bullet's simulate method, and one that runs right afterwards. That's what these are. You could probably replace these with FrameStarted and FrameEnded if you wanted to
//			LKernel.Get<PhysicsMain>().PreSimulate += PreSimulate;
//			LKernel.Get<PhysicsMain>().PostSimulate += PostSimulate;

//			// tell the physics world to use this as the debug drawer
//			LKernel.Get<Physics.PhysicsMain>().World.DebugDrawer = this;
//		}

//		void PostSimulate(DiscreteDynamicsWorld world, FrameEvent evt)
//		{
//			// this "begin" variable is to make sure that we do things in the right order
//			if (!begin)
//			{
//				lines.BeginUpdate(0);
//				triangles.BeginUpdate(0);
//				begin = true;
//			}
//		}

//		void PreSimulate(DiscreteDynamicsWorld world, FrameEvent evt)
//		{
//			if (begin)
//			{
//				lines.End();
//				triangles.End();
//				begin = false;
//			}
//		}

//		public void Dispose()
//		{
//			// unhook from these events
//			LKernel.Get<PhysicsMain>().PreSimulate -= PreSimulate;
//			LKernel.Get<PhysicsMain>().PostSimulate -= PostSimulate;
//			lines.Dispose();
//			triangles.Dispose();
//		}



//		/// <summary>
//		/// How many "steps" when we draw circles
//		/// </summary>
//		private const int numIter = 12;
//		/// <summary>
//		/// radian amount to increase the angle by when drawing circles
//		/// </summary>
//		private const float increaseAmount = (360f / numIter) * (Math.TWO_PI / 360f);
//		/// <summary>
//		/// maximum angle to draw circles with
//		/// </summary>
//		private const float limit = Math.TWO_PI + increaseAmount;





//		public void Draw3dText(ref Vector3 location, string textString) { }

//		/// <summary>
//		/// Draws an axis-aligned bounding box
//		/// </summary>
//		/// <param name="colour">I override this and make it white with 30% opacity</param>
//		public void DrawAabb(ref Vector3 from, ref Vector3 to, ColourValue colour)
//		{
//			if (!begin)
//				return;

//			colour = new ColourValue(1, 1, 1, 0.3f);

//			// I'm sure there's a better way of doing this
//			Vector3 loo = new Vector3(to.x, from.y, from.z);
//			Vector3 olo = new Vector3(from.x, to.y, from.z);
//			Vector3 ool = new Vector3(from.x, from.y, to.z);
//			Vector3 llo = new Vector3(to.x, to.y, from.z);
//			Vector3 lol = new Vector3(to.x, from.y, to.z);
//			Vector3 oll = new Vector3(from.x, to.y, to.z);

//			lines.Position(from); lines.Colour(colour);
//			lines.Position(loo); lines.Colour(colour);
//			lines.Position(from); lines.Colour(colour);
//			lines.Position(olo); lines.Colour(colour);
//			lines.Position(from); lines.Colour(colour);
//			lines.Position(ool); lines.Colour(colour);

//			lines.Position(to); lines.Colour(colour);
//			lines.Position(llo); lines.Colour(colour);
//			lines.Position(to); lines.Colour(colour);
//			lines.Position(lol); lines.Colour(colour);
//			lines.Position(to); lines.Colour(colour);
//			lines.Position(oll); lines.Colour(colour);

//			lines.Position(loo); lines.Colour(colour);
//			lines.Position(lol); lines.Colour(colour);
//			lines.Position(lol); lines.Colour(colour);
//			lines.Position(ool); lines.Colour(colour);
//			lines.Position(ool); lines.Colour(colour);
//			lines.Position(oll); lines.Colour(colour);
//			lines.Position(oll); lines.Colour(colour);
//			lines.Position(olo); lines.Colour(colour);
//			lines.Position(olo); lines.Colour(colour);
//			lines.Position(llo); lines.Colour(colour);
//			lines.Position(llo); lines.Colour(colour);
//			lines.Position(loo); lines.Colour(colour);
//		}

//		public void DrawArc(ref Vector3 center, ref Vector3 normal, ref Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, ColourValue colour, bool drawSect, float stepDegrees)
//		{

//		}

//		public void DrawArc(ref Vector3 center, ref Vector3 normal, ref Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, ColourValue colour, bool drawSect)
//		{

//		}

//		public void DrawBox(ref Vector3 bbMin, ref Vector3 bbMax, ref Matrix4 trans, ColourValue colour)
//		{

//		}

//		public void DrawBox(ref Vector3 bbMin, ref Vector3 bbMax, ColourValue colour)
//		{

//		}

//		public void DrawCapsule(float radius, float halfHeight, int upAxis, ref Matrix4 transform, ColourValue colour)
//		{
//			DrawCylinder(radius, halfHeight, upAxis, ref transform, colour);

//			Vector3 previousXYPos = transform * new Vector3(radius, halfHeight, 0);
//			Vector3 previousYZPos = transform * new Vector3(0, halfHeight, radius);
//			Vector3 previousNXYPos = transform * new Vector3(radius, -halfHeight, 0);
//			Vector3 previousNYZPos = transform * new Vector3(0, -halfHeight, radius);

//			float capsuleLimit = limit * 0.5f;

//			// y-x circle
//			for (float a = 0; a <= capsuleLimit; a += increaseAmount)
//			{
//				float y = Math.Sin(a) * radius;
//				float x = Math.Cos(a) * radius;

//				// xy
//				Vector3 xyPos = transform * new Vector3(x, y + halfHeight, 0);
//				lines.Position(previousXYPos);
//				lines.Colour(colour);
//				lines.Position(xyPos);
//				lines.Colour(colour);
//				previousXYPos = xyPos;

//				// yz
//				float z = Math.Cos(a) * radius;

//				Vector3 yzPos = transform * new Vector3(0, y + halfHeight, z);
//				lines.Position(previousYZPos);
//				lines.Colour(colour);
//				lines.Position(yzPos);
//				lines.Colour(colour);
//				previousYZPos = yzPos;

//				// -xy
//				Vector3 nxyPos = transform * new Vector3(x, -y - halfHeight, 0);
//				lines.Position(previousNXYPos);
//				lines.Colour(colour);
//				lines.Position(nxyPos);
//				lines.Colour(colour);
//				previousNXYPos = nxyPos;

//				// -yz
//				Vector3 nyzPos = transform * new Vector3(0, -y - halfHeight, z);
//				lines.Position(previousNYZPos);
//				lines.Colour(colour);
//				lines.Position(nyzPos);
//				lines.Colour(colour);
//				previousNYZPos = nyzPos;
//			}
//		}

//		/// <summary>
//		/// Draws a cone
//		/// </summary>
//		/// <param name="upAxis">ignored for now</param>
//		public void DrawCone(float radius, float height, int upAxis, ref Matrix4 transform, ColourValue colour)
//		{
//			float halfHeight = height / 2f;
//			Vector3 previousPos = transform * new Vector3(0, -halfHeight, radius);
//			Vector3 tip = transform * new Vector3(0, halfHeight, 0);

//			for (float a = 0; a <= limit; a += increaseAmount)
//			{
//				float z = Math.Cos(a) * radius;
//				float x = Math.Sin(a) * radius;

//				// the circle
//				Vector3 pos = transform * new Vector3(x, -halfHeight, z);
//				lines.Position(previousPos);
//				lines.Colour(colour);
//				lines.Position(pos);
//				lines.Colour(colour);
//				previousPos = pos;

//				// the sides
//				lines.Position(pos);
//				lines.Colour(colour);
//				lines.Position(tip);
//				lines.Colour(colour);
//			}
//		}

//		public void DrawContactPoint(ref Vector3 pointOnB, ref Vector3 normalOnB, float distance, int lifeTime, ColourValue colour)
//		{

//			lines.Position(pointOnB);
//			lines.Colour(colour);
//			lines.Position(pointOnB + normalOnB * distance);
//			lines.Colour(colour);
//		}

//		/// <summary>
//		/// Draws a cylinder
//		/// </summary>
//		/// <param name="upAxis">ignored for now</param>
//		public void DrawCylinder(float radius, float halfHeight, int upAxis, ref Matrix4 transform, ColourValue colour)
//		{
//			Vector3 previousPos = transform * new Vector3(0, halfHeight, radius);
//			Vector3 previousNPos = transform * new Vector3(0, -halfHeight, radius);

//			for (float a = 0; a <= limit; a += increaseAmount)
//			{
//				float z = Math.Cos(a) * radius;
//				float x = Math.Sin(a) * radius;

//				// positive
//				Vector3 pos = transform * new Vector3(x, halfHeight, z);
//				lines.Position(previousPos);
//				lines.Colour(colour);
//				lines.Position(pos);
//				lines.Colour(colour);
//				previousPos = pos;

//				// negative
//				Vector3 npos = transform * new Vector3(x, -halfHeight, z);
//				lines.Position(previousNPos);
//				lines.Colour(colour);
//				lines.Position(npos);
//				lines.Colour(colour);
//				previousNPos = npos;

//				// the sides
//				lines.Position(pos);
//				lines.Colour(colour);
//				lines.Position(npos);
//				lines.Colour(colour);
//			}
//		}

//		public void DrawLine(ref Vector3 from, ref Vector3 to, ColourValue colour)
//		{
//			lines.Position(from);
//			lines.Colour(colour);
//			lines.Position(to);
//			lines.Colour(colour);
//		}

//		public void DrawLine(ref Vector3 from, ref Vector3 to, ColourValue fromcolour, ColourValue tocolour)
//		{
//			lines.Position(from);
//			lines.Colour(fromcolour);
//			lines.Position(to);
//			lines.Colour(tocolour);
//		}

//		public void DrawPlane(ref Vector3 planeNormal, float planeConst, ref Matrix4 transform, ColourValue colour)
//		{

//		}

//		/// <summary>
//		/// Draws a sphere that doesn't rotate
//		/// </summary>
//		public void DrawSphere(ref Vector3 p, float radius, ColourValue colour)
//		{
//			Vector3 previousXYPos = p + new Vector3(0, radius, 0);
//			Vector3 previousYZPos = p + new Vector3(0, radius, 0);
//			Vector3 previousXZPos = p + new Vector3(0, 0, radius);

//			for (float a = 0; a <= limit; a += increaseAmount)
//			{
//				float y = Math.Cos(a) * radius;
//				float x = Math.Sin(a) * radius;

//				// xy
//				Vector3 xyPos = p + new Vector3(x, y, 0);
//				lines.Position(previousXYPos);
//				lines.Colour(colour);
//				lines.Position(xyPos);
//				lines.Colour(colour);
//				previousXYPos = xyPos;

//				// yz
//				float z = Math.Sin(a) * radius;

//				Vector3 yzPos = p + new Vector3(0, y, z);
//				lines.Position(previousYZPos);
//				lines.Colour(colour);
//				lines.Position(yzPos);
//				lines.Colour(colour);
//				previousYZPos = yzPos;

//				// xz
//				z = Math.Cos(a) * radius;

//				Vector3 xzPos = p + new Vector3(x, 0, z);
//				lines.Position(previousXZPos);
//				lines.Colour(colour);
//				lines.Position(xzPos);
//				lines.Colour(colour);
//				previousXZPos = xzPos;
//			}

//		}

//		/// <summary>
//		/// Draws a sphere that does rotate
//		/// </summary>
//		public void DrawSphere(float radius, ref Matrix4 transform, ColourValue colour)
//		{

//			Vector3 previousXYPos = transform * new Vector3(0, radius, 0);
//			Vector3 previousYZPos = transform * new Vector3(0, radius, 0);
//			Vector3 previousXZPos = transform * new Vector3(0, 0, radius);

//			for (float a = 0; a <= limit; a += increaseAmount)
//			{
//				float y = Math.Cos(a) * radius;
//				float x = Math.Sin(a) * radius;

//				// xy
//				Vector3 xyPos = transform * new Vector3(x, y, 0);
//				lines.Position(previousXYPos);
//				lines.Colour(colour);
//				lines.Position(xyPos);
//				lines.Colour(colour);
//				previousXYPos = xyPos;

//				// yz
//				float z = Math.Sin(a) * radius;

//				Vector3 yzPos = transform * new Vector3(0, y, z);
//				lines.Position(previousYZPos);
//				lines.Colour(colour);
//				lines.Position(yzPos);
//				lines.Colour(colour);
//				previousYZPos = yzPos;

//				// xz
//				z = Math.Cos(a) * radius;

//				Vector3 xzPos = transform * new Vector3(x, 0, z);
//				lines.Position(previousXZPos);
//				lines.Colour(colour);
//				lines.Position(xzPos);
//				lines.Colour(colour);
//				previousXZPos = xzPos;
//			}
//		}

//		public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, ColourValue colour, float stepDegrees, bool drawSphere)
//		{

//		}

//		public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, ColourValue colour, float stepDegrees)
//		{

//		}

//		public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, ColourValue colour)
//		{

//		}

//		public void DrawTransform(ref Matrix4 transform, float orthoLen)
//		{

//		}

//		/// <param name="__unnamed004">alpha?</param>
//		public void DrawTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, ColourValue colour, float __unnamed004)
//		{
//			triangles.Position(v0);
//			triangles.Colour(colour);
//			triangles.Position(v1);
//			triangles.Colour(colour);
//			triangles.Position(v2);
//			triangles.Colour(colour);
//		}

//		/// <param name="__unnamed003">no idea</param>
//		/// <param name="__unnamed004">no idea</param>
//		/// <param name="__unnamed005">no idea</param>
//		public void DrawTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, ref Vector3 __unnamed003, ref Vector3 __unnamed004, ref Vector3 __unnamed005, ColourValue colour, float alpha)
//		{
//			DrawTriangle(ref v0, ref v1, ref v2, colour, alpha);
//		}

//		public void ReportErrorWarning(string warningString)
//		{
//			// replace this with your own logging thing
//			Launch.Log("[WARNING] (BulletDebugManager): " + warningString);
//		}
//	}
//}
///*
// * Singleton ： Manager.Get<Component>();
// * GameObject： Which can Update/PostUpdate/......
// * 
// */