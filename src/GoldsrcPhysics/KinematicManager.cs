using BulletSharp;
using BulletSharp.Math;
using GoldsrcPhysics.Goldsrc;
using GoldsrcPhysics.Utils;
using System.Collections.Generic;
using static GoldsrcPhysics.Goldsrc.goldsrctype;

namespace GoldsrcPhysics
{
    internal struct RigidbodyCache
    {
        internal RigidBody RigidBody;
        // The model used by the rigidbody
        internal int ModelInUse;
    }
    internal unsafe class KinematicsManager
    {
        private TriangleIndexVertexArray[] _bspModels;
        private RigidbodyCache[] _rigidbodyCache;
        private CollisionShape[] _shapesCache;
        // TODO:double linked list is better
        private LinkedList<int> _added;
        private LinkedList<int> _old;

        public KinematicsManager(TriangleIndexVertexArray[] bspModel)
        {
            _bspModels = bspModel;
            _rigidbodyCache = new RigidbodyCache[1024];
            _shapesCache = new CollisionShape[1024];
            _added = new LinkedList<int>();
            _old = new LinkedList<int>();
        }
        internal void AddCollider(cl_entity_t* pEntity)
        {
            var node = _old.Find(pEntity->index);
            if (node != null)
            {
                _old.Remove(node);
                _added.AddFirst(node);
                // if model changed
                if (_rigidbodyCache[node.Value].ModelInUse != pEntity->curstate.modelindex)
                {
                    var shape = GetCollisionShape(pEntity);
                    if (shape == null)
                        return;
                    _rigidbodyCache[node.Value].RigidBody.CollisionShape = shape;
                }
            }
            else
            {
                var shape = GetCollisionShape(pEntity);
                if (shape == null)
                    return;
                RigidBody kinematic = BulletHelper.CreateBodyForEntity(0, pEntity, shape, BWorld.Instance);
                _rigidbodyCache[pEntity->index] = new RigidbodyCache { RigidBody = kinematic, ModelInUse = pEntity->curstate.modelindex };
                BWorld.Instance.AddRigidBody(kinematic);

                _added.AddFirst(pEntity->index);
            }
        }
        internal void BeforeSimulation()
        {
            foreach (var i in _old)
            {
                BWorld.Instance.RemoveRigidBody(_rigidbodyCache[i].RigidBody);
            }
            _old.Clear();
            var temp = _old;
            _old = _added;
            _added = temp;
        }
        internal void AfterSimulation()
        {
        }

        private CollisionShape GetCollisionShape(cl_entity_t* pEntity)
        {
            // directly read from cache.
            CollisionShape shape = _shapesCache[pEntity->curstate.modelindex];
            // cache missing
            if (shape == null)
            {
                if (pEntity->model->type == modtype.mod_brush)
                {
                    int brushIndex = 0;
                    unsafe
                    {
                        // The name looks like "*XX", we ignore '*' and parse to integer.
                        var p = pEntity->model->name + 1;
                        while (*p != 0)
                        {
                            brushIndex = brushIndex * 10 + (*p - 48);
                            p++;
                        }
                    }
                    shape = new BvhTriangleMeshShape(_bspModels[brushIndex], true);
                    _shapesCache[pEntity->curstate.modelindex] = shape;
                }
                else if (pEntity->model->type == modtype.mod_studio)
                {
                    Vector3 aabbSize = (pEntity->curstate.maxs - pEntity->curstate.mins) * GBConstant.G2BScale;
                    shape = new BoxShape(aabbSize / 2f);
                }
                else
                {
                    return null;
                }

            }

            return shape;
        }


        /// <summary>
        /// Call this method to clear kinematic object in physics world. Usually at the end of level.
        /// </summary>
        public void Clear()
        {
            foreach (var i in _old)
            {
                BWorld.Instance.RemoveRigidBody(_rigidbodyCache[i].RigidBody);
            }
            foreach (var i in _added)
            {
                BWorld.Instance.RemoveRigidBody(_rigidbodyCache[i].RigidBody);
            }
        }
    }
}
