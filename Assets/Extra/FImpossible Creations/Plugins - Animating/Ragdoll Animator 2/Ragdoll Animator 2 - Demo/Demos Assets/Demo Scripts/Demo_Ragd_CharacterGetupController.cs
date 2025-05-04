using FIMSpace.FProceduralAnimation;
using System.Collections;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_CharacterGetupController : MonoBehaviour, IRagdollAnimator2Receiver
    {
        public RagdollAnimator2 RagdollAnim;
        public Animator Mecanim;
        public MonoBehaviour MoverToDisable;
        public Rigidbody CharacterRigidbody;
        public Transform debug;

        [Space( 5 )]
        public float ApplyImpactPower = 4f;

        public LayerMask GroundLayer = 1 << 0;
        public int HitsToKnockout = 3;

        private int hitsOccured = 0;
        private float hitCulldown = 0f;

        public void RagdollAnimator2_OnCollisionEnterEvent( RA2BoneCollisionHandler hitted, Collision mainCollision )
        {
            if( Mecanim.GetBool( "Ragdolled" ) ) return; // Already ragdolled, dont calculate special contact actions

            if( Time.time - hitCulldown < 0.1f ) return; // Another hit happened too quick

            if( RagdollAnim.Handler.IsFallingOrSleep ) return; // Already ragdolled - don't proceed push impacts (you can do HeathPoints-- in else)

            if( hitted.LatestEnterCollision.rigidbody == null ) return; // Colliding with non rigidbody object (like floor/walls)

            float minimumMagn = 7f;
            //if( hitted.LatestEnterCollision.rigidbody.isKinematic ) minimumMagn = 3f; // Make static object lower magnite required for impact (it's called when character is running at obstacle and hitting with head by accident
            // this all 'kinematic' options are optional!)

            if( hitted.LatestEnterCollision.relativeVelocity.magnitude < minimumMagn ) return; // Too weak hit

            hitsOccured += 1;
            hitCulldown = Time.time; // Prevent multiple hits in the single frame

            if( hitsOccured >= HitsToKnockout ) // Enough hits for knockout
            {
                Vector3 impactVelo = hitted.LatestEnterCollision.relativeVelocity;
                //if( minimumMagn < 5f ) impactVelo *= 2f; // Make stronger for kinematic hit
                KnockoutImpact( hitted, impactVelo );
            }
        }

        private void KnockoutImpact( RA2BoneCollisionHandler hittedBone, Vector3 relativeVelocity )
        {
            //Mecanim.SetBool( "Ragdolled", true );
            //ragdolledTime = Time.time;
            //isGettingUp = false;
            //hitsOccured = 0; // Reset counter

            //if( MoverToDisable ) MoverToDisable.enabled = false; // Disable mover component, to avoid animation and physics conflicts

            //// Play some fall animation for ragdoll fall pose (not required)
            //Mecanim?.CrossFadeInFixedTime( "Fall", 0.2f );

            //if( hittedBone.LimbID == HumanBodyBones.Hips )
            //{
            //    // Hips impact needs special treatment, because unity is resetting it's velocity when isKinematic is changed
            //    hittedBone.RagdollBone.rigidbody.isKinematic = false;
            //    RagdollAnim.User_SetLimbImpact( hittedBone.RagdollBone.rigidbody, relativeVelocity.normalized * ApplyImpactPower, 0.125f, ForceMode.VelocityChange );
            //}

            //RagdollAnim.Parameters.User_SetAllLimbsVelocity( CharacterRigidbody.velocity );

            //// Free Fall
            //RagdollAnim.User_SwitchFreeFallRagdoll( true );
            //RagdollAnim.User_FadeRagdolledBlend( 1f, 0.1f );

            //// Push whole ragdoll with some force
            //RagdollAnim.User_SetPhysicalImpactAll( relativeVelocity.normalized * ApplyImpactPower, 0.1f, ForceMode.Acceleration );

            //// Empathise hitted limb with impact
            //RagdollAnim.User_SetLimbImpact( hittedBone.RagdollBone.rigidbody, relativeVelocity.normalized * ApplyImpactPower, 0.125f, ForceMode.VelocityChange );

            //// Make character muscles weaker
            //RagdollAnim.User_FadeMuscles( 0.1f, 0.8f, 0.1f );
        }

        //float ragdolledTime = 0f;
        //bool isGettingUp = false;
        private void FixedUpdate()
        {
            // Update for triggering getup animation.
            // When character is not ragdolled - do nothing
            //if( RagdollAnim.Parameters.FreeFallRagdoll == false ) return;

            //// Already animating get up
            //if( isGettingUp ) return;

            //if( Time.time - ragdolledTime < 1.2f )
            //{
            //    return; // Minimum time ragdolled before allowing to getup
            //}

            //// Check getup direction
            //RagdollProcessor.EGetUpType canGetUp = RagdollAnim.Parameters.User_CanGetUp( null, false );
            //if( canGetUp == RagdollProcessor.EGetUpType.None ) return;

            //// Check if libs are still falling
            //var referenceVelocity = RagdollAnim.Parameters.User_GetSpineLimbsVelocity();
            //if( referenceVelocity.magnitude > 0.3f ) return;

            //var hit = RagdollAnim.Parameters.ProbeGroundBelowHips( GroundLayer, 0.5f );

            //if( hit.transform ) GetUp( canGetUp == RagdollProcessor.EGetUpType.FromBack );
        }

        // Call it during fixed update!
        private void GetUp( bool fromBack )
        {
            //// This needs to be called during FixedUpdate()!
            //transform.rotation = RagdollAnim.Parameters.User_GetMappedRotationHipsToHead( Vector3.up );

            //Mecanim.SetBool( "Grounded", true ); // Avoid jump animator transition
            //Mecanim.SetBool( "Ragdolled", false );
            //isGettingUp = true;

            //// Getup
            //RagdollAnim.User_GetUpStackV2( 0f, 0.8f, 0.7f );
            //RagdollAnim.User_ForceRagdollToAnimatorFor( 0.5f, 0.5f ); // (if using blend on collision) Force non-ragdoll for 0.5 sec and restore transition in 0.5 sec
            //Mecanim.Play( fromBack ? "Get Up Back" : "Get Up Face", 0, 0f );

            //StartCoroutine(IERestoreControll());
        }

        private IEnumerator IERestoreControll()
        {
            float elapsed = 0f;

            while( elapsed < 1.35f ) // Just getup animation duration delay
            {
                yield return null;
                elapsed += Time.deltaTime;
                if( Mecanim.GetBool( "Ragdolled" ) ) yield break; // Stop restore culldown since character got ragdolled again
            }

            // Ragdoll Animiator demo mover is calling ResetTargetRotation() and acceleration reset
            // during OnEnable()
            // Similar action may be required in your mover script! But maybe it is already implementing it.
            // Thanks to that mover controller is adapting to new - character rotation after ragdolled getup

            MoverToDisable.enabled = true;
        }
    }
}