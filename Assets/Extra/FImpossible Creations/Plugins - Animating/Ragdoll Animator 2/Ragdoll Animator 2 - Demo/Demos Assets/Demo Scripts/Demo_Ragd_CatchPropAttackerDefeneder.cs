using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    [DefaultExecutionOrder( 100 )]
    public class Demo_Ragd_CatchPropAttackerDefeneder : FimpossibleComponent
    {
        public bool IsDefender = false;
        public RagdollAnimator2 Self;
        public RagdollAnimator2 Defender;
        public RA2AttachableObject AttachableWeapon;
        public RA2MagnetPoint MagnetDefenderHandHelper;

        private GameObject catchObject;
        private RagdollChainBone defenderHand;
        private ConfigurableJoint catchJoint;

        private bool catched = false;
        private bool triggerHit = false;
        private Vector3 catchLocalAttackerHandPos;

        /// <summary>
        /// Call hit calculations when whole ragdoll pose is completed
        /// </summary>
        private void LateUpdate()
        {
            // Triggering catch event in the right execution order - after all body calculations
            if( triggerHit )
            {
                triggerHit = false;
                catched = true;

                Physics.SyncTransforms();

                Self.User_SwitchFallState();
                var attackerHand = Self.User_GetBoneSetupByHumanoidBone( HumanBodyBones.LeftHand );
                defenderHand = Defender.User_GetBoneSetupByHumanoidBone( HumanBodyBones.RightHand );

                // Generate additional object with rigidbody as child of attacker hand
                catchObject = new GameObject( "Generated Sword Catch Body" );
                catchObject.transform.position = attackerHand.GameRigidbody.position;
                catchObject.transform.rotation = attackerHand.GameRigidbody.rotation;
                catchObject.transform.parent = attackerHand.GameRigidbody.transform;

                var catchRig = catchObject.AddComponent<Rigidbody>();

                // Add joint to attacker hand, lock it, set anchor to be in sword catch position + set above rigidbody as connection body
                catchJoint = attackerHand.GameRigidbody.transform.gameObject.AddComponent<ConfigurableJoint>();
                RagdollHandler.SetConfigurableJointMotionLock( catchJoint, ConfigurableJointMotion.Locked );
                RagdollHandler.SetConfigurableJointAngularMotionLock( catchJoint, ConfigurableJointMotion.Locked );
                catchJoint.connectedBody = catchRig;
                catchJoint.autoConfigureConnectedAnchor = false;

                // Use magnet to control position of the above rigidbody - magnet is child of defender hand to follow its position and rotation
                MagnetDefenderHandHelper.ToMove = catchRig.transform;
                MagnetDefenderHandHelper.transform.rotation = attackerHand.BoneProcessor.AnimatorRotation;
                MagnetDefenderHandHelper.transform.position = defenderHand.BoneProcessor.AnimatorPosition;

                // Detect visible sword collider closest point to the defender's hand
                bool preEn = AttachableWeapon.AttachableColliders[0].enabled;
                AttachableWeapon.AttachableColliders[0].enabled = true;
                Vector3 catchPos = AttachableWeapon.AttachableColliders[0].ClosestPoint( defenderHand.BoneProcessor.AnimatorPosition );
                AttachableWeapon.AttachableColliders[0].enabled = preEn;

                // Apply according position offsets to make hand catch sword blade precisely
                catchLocalAttackerHandPos = attackerHand.SourceBone.InverseTransformPoint( catchPos );
                if ( MagnetDefenderHandHelper.OriginOffset == Vector3.zero ) MagnetDefenderHandHelper.OriginOffset = -catchLocalAttackerHandPos;
                MagnetDefenderHandHelper.transform.position = defenderHand.BoneProcessor.AnimatorPosition;
                MagnetDefenderHandHelper.transform.rotation = defenderHand.BoneProcessor.AnimatorRotation;

                // Prevent sword collision with defender body
                Defender.Handler.IgnoreCollisionWith( AttachableWeapon.GeneratedPhysicsColliders );

                MagnetDefenderHandHelper.enabled = true;
            }

            if( !catched ) return;
            // Update magnet with defender hand
            MagnetDefenderHandHelper.transform.position = defenderHand.SourceBone.position;
            MagnetDefenderHandHelper.transform.rotation = defenderHand.SourceBone.rotation;
        }

        public void Hit()
        {
            // Schedule catch action in LateUpdate
            triggerHit = true;
        }

        public void Throw()
        {
            // If script is attached to the Defender, then just forward message to the attacker script
            if( IsDefender )
            {
                Self.GetComponent<Demo_Ragd_CatchPropAttackerDefeneder>().Throw();
                return;
            }

            // Destroy attachement joints like breaking joint
            Destroy( catchJoint );
            Destroy( catchObject );

            // Disable unneccesary script
            MagnetDefenderHandHelper.enabled = false;

            // Push attacker away with the throw motion
            Self.User_AddAllBonesImpact( ( Defender.GetBaseTransform.forward + Vector3.up * 0.15f ) * 1f, 0.1f );

            // restore collision
            Defender.Handler.IgnoreCollisionWith( AttachableWeapon.GeneratedPhysicsColliders, false );
        }
    }
}