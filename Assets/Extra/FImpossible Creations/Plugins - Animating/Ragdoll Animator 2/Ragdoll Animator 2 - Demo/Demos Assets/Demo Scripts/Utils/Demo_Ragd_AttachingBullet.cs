using FIMSpace.FProceduralAnimation;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_AttachingBullet : Demo_Ragd_Bullet
    {
        private List<Attached> attached;
        public bool KinematicAttach = true;
        public bool AllowAttachMultipleEnemies = false;
        public bool AllowAttachAlreadyRagdolled = false;

        protected override void Start()
        {
            base.Start();
            attached = new List<Attached>();
        }

        private struct Attached
        {
            public RagdollAnimator2BoneIndicator indicator;
            public Vector3 localOffset;
        }

        private bool AlreadyAttached( RagdollAnimator2BoneIndicator indic )
        {
            foreach( var attach in attached )
                if( attach.indicator.ParentHandler == indic.ParentHandler )
                    return true;
            return false;
        }

        private RaycastHit[] hitAlloc = new RaycastHit[8];
        private int hits = 0;

        protected override bool DoRaycast( Vector3 newPosition )
        {
            bool hitted = false;

            Vector3 diff = newPosition - transform.position;
            hits = Physics.RaycastNonAlloc( new Ray( transform.position, diff.normalized ), hitAlloc, diff.magnitude, ProjectiletHitMask, QueryTriggerInteraction.Ignore );

            if( hits > 0 )
            {
                for( int i = 0; i < hits; i++ )
                {
                    RagdollAnimator2BoneIndicator indic = hitAlloc[i].collider.GetComponent<RagdollAnimator2BoneIndicator>();
                    if( indic != null ) if( AlreadyAttached( indic ) ) continue;

                    bulletHit = hitAlloc[i];
                    break;
                }

                hitted = true;
            }
            else
            {
                bulletHit = new RaycastHit();
            }

            return hitted;
        }

        private bool doUpdate = true;

        protected override void Update()
        {
            if( doUpdate ) base.Update(); // Flight Update - dont update when hit wall
        }

        private void LateUpdate()
        {
            if( KinematicAttach )
            {
                // Unfortunately, with low fps, kinematic manipulation can cause jitter during bullet flight
                foreach( var attach in attached )
                {
                    Vector3 targetPosition = transform.TransformPoint( attach.localOffset );
                    RagdollHandlerUtilities.SwitchKinematicAndProjection( attach.indicator.DummyBoneRigidbody, attach.indicator.ParentHandler );
                    attach.indicator.DummyBoneRigidbody.position = targetPosition;
                }
            }
            else
            {
                foreach( var attach in attached )
                {
                    Vector3 targetPosition = transform.TransformPoint( attach.localOffset );
                    attach.indicator.DummyBoneRigidbody.mass = attach.indicator.ParentHandler.ReferenceMass * 1000; // When using Drag Rigidbody we need to use higher mass to drag rest of the body with attached bone
                    RagdollHandlerUtilities.DragRigidbodyTowards( attach.indicator.DummyBoneRigidbody, targetPosition, 1.4f );
                }
            }
        }

        void OnAttachIndicator( Attached attach )
        {
            var handler = attach.indicator.ParentHandler;
            handler.RigidbodyDragValue = 0f; // Prevent air pressure
            handler.User_UpdateRigidbodyParametersForAllBones();
        }

        protected override void OnHitEnd()
        {
            if( bulletHit.transform == null ) return;

            RagdollAnimator2BoneIndicator indicator = bulletHit.collider.GetComponent<RagdollAnimator2BoneIndicator>();

            if( indicator == null )
            {
                transform.position = bulletHit.point - transform.forward * 0.15f;
                doUpdate = false;
                KinematicAttach = true; // Force attach at the end
            }
            else
            {
                if( AllowAttachMultipleEnemies == false && attached.Count > 1 ) return;

                if( AlreadyAttached( indicator ) == false )
                {
                    if( AllowAttachAlreadyRagdolled == false )
                    {
                        if( indicator.ParentHandler.Dummy_Container.GetComponent<AttachementMarker>() )
                            return; // Dont allow attach more limbs
                        else
                            indicator.ParentHandler.Dummy_Container.gameObject.AddComponent<AttachementMarker>();
                    }

                    indicator.ParentHandler.User_SwitchFallState();

                    Attached attach = new Attached();
                    attach.indicator = indicator;
                    transform.position = bulletHit.point;
                    attach.localOffset = transform.InverseTransformPoint( indicator.transform.position - transform.forward * 0.2f );
                    //RagdollHandlerUtilities.SetKinematicSwitches( attach.indicator.DummyBoneRigidbody, attach.indicator.ParentHandler );
                    attached.Add( attach );

                    if ( KinematicAttach == false) OnAttachIndicator( attach );

                    //ProjectiletHitMask &= ~( 1 << bulletHit.collider.gameObject.layer );
                }
            }

        }

        // To prevent attaching already attached body
        public class AttachementMarker : MonoBehaviour
        { }

    }
}