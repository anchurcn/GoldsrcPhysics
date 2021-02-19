using BulletSharp;
using BulletSharp.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoldsrcPhysics
{
    public class LocalPlayerBodyPicker : IGoldsrcBehaviour
    {
        private BodyPicker BodyPicker { get; }
        public LocalPlayerBodyPicker()
        {
            BodyPicker = new BodyPicker();
        }
        public void Awake()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void FixedUpdate()
        {
            throw new NotImplementedException();
        }

        public void LateUpdate()
        {
            throw new NotImplementedException();
        }

        public void OnDestroy()
        {
            throw new NotImplementedException();
        }

        public void OnDisable()
        {
            throw new NotImplementedException();
        }

        public void OnEnable()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            //TODO:using current view from model renderer to pick
        }

        public void PickBody()
        {
            //TODO:using current view from model renderer to pick
        }
        public void Release()
        {
            BodyPicker.RemovePickingConstraint();
        }
    }

    public sealed class BodyPicker
    {
        public bool IsFixedConstraint { get; set; } = true;
        public float OldPickingDist { get; set; }
        public Vector3 Eye { get; set; }
        public Vector3 TargetPoint { get; set; }


        private RigidBody _pickedBody;
        private TypedConstraint _rigidBodyPickConstraint;
        private MultiBodyPoint2Point _multiBodyPickConstraint;
        private bool _prevCanSleep;
        private DynamicsWorld World { get => BWorld.Instance; }//TODO
        

        

        //public void Update()
        //{
        //    var input = _demo.Input;

        //    if ((input.MousePressed & MouseButtons.Right) != 0)
        //    {
        //        PickBody();
        //    }
        //    else if (input.MouseReleased == MouseButtons.Right)
        //    {
        //        RemovePickingConstraint();
        //    }

        //    if (input.MouseDown == MouseButtons.Right)
        //    {
        //        MovePickedBody();
        //    }
        //}
        public void PickBody(Vector3 eye,Vector3 targetPoint)
        {
            Eye = eye;
            TargetPoint = targetPoint;
            PickBody();
        }

        public void MovePickedBody(Vector3 eye, Vector3 targetPoint,float distance=float.NaN)
        {
            Eye = eye;
            TargetPoint = targetPoint;
            if(distance!=float.NaN)
                OldPickingDist = distance;
            MovePickedBody();
        }

        public void RemovePickingConstraint()
        {
            if (_rigidBodyPickConstraint != null)
            {
                World.RemoveConstraint(_rigidBodyPickConstraint);
                _rigidBodyPickConstraint.Dispose();
                _rigidBodyPickConstraint = null;
                _pickedBody.ForceActivationState(ActivationState.ActiveTag);
                _pickedBody.DeactivationTime = 0;
                _pickedBody = null;
            }

            if (_multiBodyPickConstraint != null)
            {
                _multiBodyPickConstraint.MultiBodyA.CanSleep = _prevCanSleep;
                (World as MultiBodyDynamicsWorld).RemoveMultiBodyConstraint(_multiBodyPickConstraint);
                _multiBodyPickConstraint.Dispose();
                _multiBodyPickConstraint = null;
            }
        }

        public void PickBody()
        {
            Vector3 rayFrom = Eye;
            Vector3 rayTo = TargetPoint;

            DynamicsWorld world = World;

            using (var rayCallback = new ClosestRayResultCallback(ref rayFrom, ref rayTo))
            {
                world.RayTestRef(ref rayFrom, ref rayTo, rayCallback);
                if (rayCallback.HasHit)
                {
                    Vector3 pickPosition = rayCallback.HitPointWorld;
                    var body = rayCallback.CollisionObject as RigidBody;
                    if (body != null)
                    {
                        PickRigidBody(body, ref pickPosition);
                    }
                    else
                    {
                        var collider = rayCallback.CollisionObject as MultiBodyLinkCollider;
                        if (collider != null)
                        {
                            PickMultiBody(collider, ref pickPosition);
                        }
                    }
                    OldPickingDist = (pickPosition - rayFrom).Length;
                }
            }
        }

        public void MovePickedBody()
        {
            if (_rigidBodyPickConstraint != null)
            {
                Vector3 rayFrom = Eye;
                Vector3 newRayTo = TargetPoint;

                //keep it at the same picking distance
                Vector3 direction = newRayTo - rayFrom;
                direction.Normalize();
                direction *= OldPickingDist;

                if (_rigidBodyPickConstraint.ConstraintType == TypedConstraintType.D6)
                {
                    var dof6 = _rigidBodyPickConstraint as Generic6DofConstraint;

                    //keep it at the same picking distance
                    Matrix tempFrameOffsetA = dof6.FrameOffsetA;
                    tempFrameOffsetA.Origin = rayFrom + direction;
                    dof6.SetFrames(tempFrameOffsetA, dof6.FrameOffsetB);
                }
                else
                {
                    var p2p = _rigidBodyPickConstraint as Point2PointConstraint;

                    //keep it at the same picking distance
                    p2p.PivotInB = rayFrom + direction;
                }
            }
            else if (_multiBodyPickConstraint != null)
            {
                Vector3 rayFrom = Eye;
                Vector3 newRayTo = TargetPoint;

                Vector3 dir = (newRayTo - rayFrom);
                dir.Normalize();
                dir *= OldPickingDist;
                _multiBodyPickConstraint.PivotInB = rayFrom + dir;
            }
        }

        private void PickRigidBody(RigidBody body, ref Vector3 pickPosition)
        {
            if (body.IsStaticObject || body.IsKinematicObject)
            {
                return;
            }

            _pickedBody = body;
            _pickedBody.ActivationState = ActivationState.DisableDeactivation;

            DynamicsWorld world = World;

            Vector3 localPivot = Vector3.TransformCoordinate(pickPosition, Matrix.Invert(body.CenterOfMassTransform));

            if (IsFixedConstraint)
            {
                var dof6 = new Generic6DofConstraint(body, Matrix.Translation(localPivot), false)
                {
                    LinearLowerLimit = Vector3.Zero,
                    LinearUpperLimit = Vector3.Zero,
                    AngularLowerLimit = Vector3.Zero,
                    AngularUpperLimit = Vector3.Zero
                };

                world.AddConstraint(dof6);
                _rigidBodyPickConstraint = dof6;

                dof6.SetParam(ConstraintParam.StopCfm, 0.8f, 0);
                dof6.SetParam(ConstraintParam.StopCfm, 0.8f, 1);
                dof6.SetParam(ConstraintParam.StopCfm, 0.8f, 2);
                dof6.SetParam(ConstraintParam.StopCfm, 0.8f, 3);
                dof6.SetParam(ConstraintParam.StopCfm, 0.8f, 4);
                dof6.SetParam(ConstraintParam.StopCfm, 0.8f, 5);

                dof6.SetParam(ConstraintParam.StopErp, 0.1f, 0);
                dof6.SetParam(ConstraintParam.StopErp, 0.1f, 1);
                dof6.SetParam(ConstraintParam.StopErp, 0.1f, 2);
                dof6.SetParam(ConstraintParam.StopErp, 0.1f, 3);
                dof6.SetParam(ConstraintParam.StopErp, 0.1f, 4);
                dof6.SetParam(ConstraintParam.StopErp, 0.1f, 5);
            }
            else
            {
                var p2p = new Point2PointConstraint(body, localPivot);
                world.AddConstraint(p2p);
                _rigidBodyPickConstraint = p2p;
                p2p.Setting.ImpulseClamp = 30;
                //very weak constraint for picking
                p2p.Setting.Tau = 0.001f;
                /*
                p2p.SetParam(ConstraintParams.Cfm, 0.8f, 0);
                p2p.SetParam(ConstraintParams.Cfm, 0.8f, 1);
                p2p.SetParam(ConstraintParams.Cfm, 0.8f, 2);
                p2p.SetParam(ConstraintParams.Erp, 0.1f, 0);
                p2p.SetParam(ConstraintParams.Erp, 0.1f, 1);
                p2p.SetParam(ConstraintParams.Erp, 0.1f, 2);
                */
            }
        }

        private void PickMultiBody(MultiBodyLinkCollider collider, ref Vector3 pickPosition)
        {
            MultiBody multiBody = collider.MultiBody;
            if (multiBody == null)
            {
                return;
            }

            _prevCanSleep = multiBody.CanSleep;
            multiBody.CanSleep = false;
            Vector3 pivotInA = multiBody.WorldPosToLocal(collider.Link, pickPosition);

            var p2p = new MultiBodyPoint2Point(multiBody, collider.Link, null, pivotInA, pickPosition)
            {
                MaxAppliedImpulse = 2
            };

            var world = World as MultiBodyDynamicsWorld;
            world.AddMultiBodyConstraint(p2p);
            _multiBodyPickConstraint = p2p;
        }


    }
}
