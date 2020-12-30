using BulletSharp;
using BulletSharp.Math;
using GoldsrcPhysics.Utils;
using GoldsrcPhysics.Goldsrc.Bsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Face = GoldsrcPhysics.Goldsrc.Bsp.Face;

namespace GoldsrcPhysics
{
    public class ModelFaces
    {
        /// <summary>
        /// Bsp model index. Static bsp geometry is model[0]
        /// </summary>
        public int Index { get; set; }

        public List<Face> Faces { get; set; }

        public Vector3 Origin { get; set; }

        public string ClassName { get; set; }

        public ModelFaces()
        {
            Faces = new List<Face>();
        }

        public ModelFaces(int index, List<Face> faces, Vector3 origin, string className)
        {
            Index = index;
            Faces = faces;
            Origin = origin;
            ClassName = className;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BspLoader
    {
        public List<TriangleIndexVertexArray> Models { get; } = new List<TriangleIndexVertexArray>();

        /// <summary>
        /// Model[0] + static entity model
        /// NOTE: Some entity brush models don't do anything just like static geometry (model[0]). We merge them with static geometry.
        /// </summary>
        public TriangleIndexVertexArray StaticGeometry { get; private set; }

        private List<ModelFaces> ModelFaces { get; } = new List<ModelFaces>();

        private List<(Face, Vector3)> _Faces { get; } = new List<(Face, Vector3)>();

        private BspFile _Bsp { get; }

        private readonly float Scale = GBConstant.G2BScale;

        public BspLoader(string bspPath)
        {
            _Bsp = new BspFile(File.OpenRead(bspPath));
            LoadStaticGeometryFaces();
            LoadModelFaces();
            MergeStaticModels();
        }

        /// <summary>
        /// Select static models from Models, then generate StaticGeometry
        /// </summary>
        private void MergeStaticModels()
        {
            IEnumerable<ModelFaces> staticModels = ModelFaces.Where(x => !EntityWithInvisableModel.Contains(x.ClassName))
                .Select(x => x);

            // Lazy coding, use a vertex class so I don't have to worry about pass-by-value
            var verts = new List<TempVertex>();
            var indices = new List<int>();

            // Loop through staticModels turn to vertices and indeces
            foreach (ModelFaces model in staticModels)
            {
                foreach (var face in model.Faces)
                {
                    var faceVerts = new List<Vector3>();
                    for (var i = 0; i < face.NumEdges; i++)
                    {
                        var ei = _Bsp.SurfaceEdges[face.FirstEdge + i];
                        var edge = _Bsp.Edges[Math.Abs(ei)];
                        var vtx = _Bsp.Vertices[ei > 0 ? edge.Start : edge.End];
                        faceVerts.Add(vtx);
                    }

                    var start = verts.Count;

                    // Triangulate the face
                    for (int i = 1; i < faceVerts.Count - 1; i++)
                    {
                        indices.Add(start);
                        indices.Add(start + i);
                        indices.Add(start + i + 1);
                    }

                    // Translate entity local to world coordinate system
                    foreach (var point in faceVerts)
                    {
                        verts.Add(new TempVertex
                        {
                            Position = point + model.Origin,
                        });
                    }

                }
            }
            var vertices = verts.Select(x => new Vector3(x.Position.X * Scale, x.Position.Y * Scale, x.Position.Z * Scale)).ToArray();

            StaticGeometry = new TriangleIndexVertexArray(indices, vertices);
        }

        /// <summary>
        /// Load bsp model[0] to ModelFaces[0]
        /// </summary>
        private void LoadStaticGeometryFaces()
        {
            // BSP model[0]
            // Collect the static faces in the BSP (no need for special entity treatment)

            // The faces contains different texture group, but for collider, it should be merged.
            var staticFaces = new List<Face>();
            var nodes = new Queue<Node>(_Bsp.Nodes.Take(1));
            while (nodes.Any())
            {
                var node = nodes.Dequeue();
                foreach (var child in node.Children)
                {
                    if (child >= 0)
                    {
                        nodes.Enqueue(_Bsp.Nodes[child]);
                    }
                    else
                    {
                        var leaf = _Bsp.Leaves[-1 - child];
                        if (leaf.Contents == Contents.Sky)
                        {
                            continue;
                        }
                        for (var ms = 0; ms < leaf.NumMarkSurfaces; ms++)
                        {
                            var faceidx = _Bsp.MarkSurfaces[ms + leaf.FirstMarkSurface];
                            var face = _Bsp.Faces[faceidx];
                            if (face.Styles[0] != byte.MaxValue) staticFaces.Add(face);
                        }

                    }
                }
            }

            ModelFaces.Add(new ModelFaces(0, staticFaces, Vector3.Zero, "worldspawn"));
        }
        // Treat entity model as static geometry, except these
        string[] EntityWithInvisableModel =
        {
            "func_buyzone",
            "func_bomb_target"
        };
        /// <summary>
        /// Translate brush model to world coordinate system.
        /// </summary>
        [Obsolete]
        private void ModelFacesToModels()
        {
            ModelFaces.ForEach(x => Models.Add(ModelFacesToMeshShape(x)));
        }
        private void LoadModelFaces()
        {
            IEnumerable<(Model, EntityData x)> models = _Bsp.Entities
                .Where(x => x.Model > 0)
                .Select(x => (_Bsp.Models[x.Model], x));

            // get static model faces, append these faces to static geometry faces

            foreach (var i in models)
            {
                var _model = i.Item1;
                var entity = i.x;
                var origin = _model.Origin + entity.GetVector3("origin", Vector3.Zero);

                var entityFaces = new List<Face>();
                var nodes = new Queue<Node>();
                nodes.Enqueue(_Bsp.Nodes[_model.HeadNodes[0]]);
                while (nodes.Any())
                {
                    var node = nodes.Dequeue();
                    foreach (var child in node.Children)
                    {
                        if (child >= 0)
                        {
                            nodes.Enqueue(_Bsp.Nodes[child]);
                        }
                        else
                        {
                            var leaf = _Bsp.Leaves[-1 - child];
                            if (leaf.Contents == Contents.Sky)
                            {
                                continue;
                            }
                            for (var ms = 0; ms < leaf.NumMarkSurfaces; ms++)
                            {
                                var faceidx = _Bsp.MarkSurfaces[ms + leaf.FirstMarkSurface];
                                var face = _Bsp.Faces[faceidx];
                                if (face.Styles[0] != byte.MaxValue) entityFaces.Add(face);
                            }

                        }
                    }
                }

                ModelFaces.Add(new ModelFaces(entity.Model, entityFaces, origin, entity.ClassName));
            }


        }

        public void LoadBrushModel()
        {

        }
        private class TempVertex
        {
            public Vector3 Position;
        }
        private TriangleIndexVertexArray ModelFacesToMeshShape(ModelFaces modelFaces)
        {
            // Lazy coding, use a vertex class so I don't have to worry about pass-by-value
            var verts = new List<TempVertex>();
            var indices = new List<int>();

            foreach (var face in modelFaces.Faces)
            {

                var faceVerts = new List<Vector3>();
                for (var i = 0; i < face.NumEdges; i++)
                {
                    var ei = _Bsp.SurfaceEdges[face.FirstEdge + i];
                    var edge = _Bsp.Edges[Math.Abs(ei)];
                    var vtx = _Bsp.Vertices[ei > 0 ? edge.Start : edge.End];
                    faceVerts.Add(vtx);
                }

                var start = verts.Count;

                // Triangulate the face
                for (int i = 1; i < faceVerts.Count - 1; i++)
                {
                    indices.Add(start);
                    indices.Add(start + i);
                    indices.Add(start + i + 1);
                }

                // Translate entity local to world coordinate system
                foreach (var point in faceVerts)
                {
                    verts.Add(new TempVertex
                    {
                        Position = point + modelFaces.Origin,
                    });
                }

            }
            var vertices = verts.Select(x => new Vector3(x.Position.X * Scale, x.Position.Y * Scale, x.Position.Z * Scale)).ToArray();

            return new TriangleIndexVertexArray(indices, vertices);
        }
        private BvhTriangleMeshShape GetMapStaticCollider()
        {
            // Lazy coding, use a vertex class so I don't have to worry about pass-by-value
            var verts = new List<TempVertex>();
            var indices = new List<int>();

            foreach (var tuple in _Faces)
            {
                var face = tuple.Item1;

                var faceVerts = new List<Vector3>();
                for (var i = 0; i < face.NumEdges; i++)
                {
                    var ei = _Bsp.SurfaceEdges[face.FirstEdge + i];
                    var edge = _Bsp.Edges[Math.Abs(ei)];
                    var vtx = _Bsp.Vertices[ei > 0 ? edge.Start : edge.End];
                    faceVerts.Add(vtx);
                }

                var start = verts.Count;

                // Triangulate the face
                for (int i = 1; i < faceVerts.Count - 1; i++)
                {
                    indices.Add(start);
                    indices.Add(start + i);
                    indices.Add(start + i + 1);
                }

                // Translate entity local to world coordinate system
                foreach (var point in faceVerts)
                {
                    verts.Add(new TempVertex
                    {
                        Position = point + tuple.Item2,
                    });
                }

            }
            var vertices = verts.Select(x => new Vector3(x.Position.X * Scale, x.Position.Y * Scale, x.Position.Z * Scale)).ToArray();

            return new BvhTriangleMeshShape(new TriangleIndexVertexArray(indices, vertices), true);
        }
    }
}
