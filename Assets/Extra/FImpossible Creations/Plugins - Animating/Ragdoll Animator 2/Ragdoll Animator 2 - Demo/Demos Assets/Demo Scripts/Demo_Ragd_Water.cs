using FIMSpace.FProceduralAnimation;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_Water : FimpossibleComponent
    {
        private List<RagdollAnimator2> WaterModeRagdolls = new List<RagdollAnimator2>();
        public Collider WaterCollider;
        public GameObject WaterParticle;
        public GameObject WaterParticleLight;

        [Space( 6 )]
        public float StopPushOn = 0.05f;

        public float StrongerPushBelow = 1.5f;
        public float AveragePushPower = 1f;
        public float DeepPushMultiplier = 5f;

        private void OnTriggerEnter( Collider other )
        {
            RagdollAnimator2BoneIndicator indicator = other.gameObject.GetComponent<RagdollAnimator2BoneIndicator>();

            if( indicator == null ) return;
            if( indicator.BoneSettings == null ) return;

            if( indicator.BoneSettings.IsAnchor == false ) return; // Apply "In Water" state only for anchor bones entering water trigger

            if( WaterModeRagdolls.Contains( indicator.ParentRagdollAnimator ) ) return; // Already contained

            WaterModeRagdolls.Add( indicator.ParentRagdollAnimator );

            indicator.ParentRagdollAnimator.User_SwitchAllBonesUseGravity( false );
            indicator.ParentRagdollAnimator.User_SwitchAllBonesMaxVelocity( 5f );
            indicator.ParentRagdollAnimator.User_ChangeAllRigidbodiesDrag( 1f );
            indicator.ParentRagdollAnimator.User_ChangeAllRigidbodiesAngularDrag( 1f );

            float hitVelocity = indicator.DummyBoneRigidbody.velocity.magnitude;

            if( hitVelocity > 4f )
            {
                SpawnSplashParticle( WaterParticle, indicator.DummyBoneRigidbody.position );
            }
            else if( hitVelocity > 2f )
            {
                SpawnSplashParticle( WaterParticleLight, indicator.DummyBoneRigidbody.position );
            }
        }

        private void SpawnSplashParticle( GameObject particle, Vector3 wPos )
        {
            Ray surfaceCheckRay = new Ray( wPos + Vector3.up * 1000f, Vector3.down );
            RaycastHit surfaceHit;

            float surfaceY = WaterCollider.bounds.max.y;

            if( WaterCollider.Raycast( surfaceCheckRay, out surfaceHit, 10000f ) )
            {
                surfaceY = surfaceHit.point.y;
            }

            wPos.y = surfaceY;
            Instantiate( particle, wPos, Quaternion.Euler( -90f, 0f, 0f ) );
        }

        private void OnTriggerExit( Collider other )
        {
            RagdollAnimator2BoneIndicator indicator = other.gameObject.GetComponent<RagdollAnimator2BoneIndicator>();

            if( indicator == null ) return;
            if( indicator.BoneSettings == null ) return;

            if( indicator.BoneSettings.IsAnchor == false ) return; // Change "In Water" state only for anchor bones entering water trigger

            if( WaterModeRagdolls.Contains( indicator.ParentRagdollAnimator ) == false ) return; // Already exited

            WaterModeRagdolls.Remove( indicator.ParentRagdollAnimator );

            indicator.ParentRagdollAnimator.User_SwitchAllBonesUseGravity( true );
            indicator.ParentRagdollAnimator.User_SwitchAllBonesMaxVelocity( 10000f );
            indicator.ParentRagdollAnimator.User_ChangeAllRigidbodiesDrag( indicator.ParentRagdollAnimator.Handler.RigidbodyDragValue );
            indicator.ParentRagdollAnimator.User_ChangeAllRigidbodiesAngularDrag( indicator.ParentRagdollAnimator.Handler.RigidbodyAngularDragValue );
        }

        private void FixedUpdate()
        {
            for( int i = 0; i < WaterModeRagdolls.Count; i++ )
            {
                var ragdoll = WaterModeRagdolls[i];

                var anchor = ragdoll.Handler.GetAnchorBoneController;

                Ray surfaceCheckRay = new Ray( anchor.PhysicalDummyBone.position + Vector3.up * 1000f, Vector3.down );
                RaycastHit surfaceHit;

                float underWaterFactor = 0.5f;
                float surfaceY = WaterCollider.bounds.max.y;

                if( WaterCollider.Raycast( surfaceCheckRay, out surfaceHit, 10000f ) )
                {
                    surfaceY = surfaceHit.point.y;

                    float yDiff = Mathf.Abs( surfaceHit.point.y - anchor.PhysicalDummyBone.position.y );
                    if( yDiff < StrongerPushBelow ) underWaterFactor = 1f;
                    else
                    {
                        underWaterFactor = Mathf.InverseLerp( 0f, StrongerPushBelow, yDiff );
                    }
                }

                surfaceY -= StopPushOn;

                // Stronger push up if body is lower, below water surface

                foreach( var chain in ragdoll.Handler.Chains )
                {
                    for( int b = 0; b < chain.BoneSetups.Count; b++ )
                    {
                        if( chain.BoneSetups[b].MainBoneCollider.bounds.center.y > surfaceY ) continue; // Over the surface - skip

                        chain.BoneSetups[b].GameRigidbody.AddForce( Vector3.up * ( 1f + underWaterFactor * DeepPushMultiplier ) * AveragePushPower, ForceMode.Acceleration );
                    }
                }
            }
        }
    }
}