using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
        /// <summary> Getting all generated ragdoll dummy colliders </summary>
        public List<Collider> User_GetAllDummyColliders()
        {
            List<Collider> colliders = new List<Collider>();

            foreach( var chain in chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    foreach( var coll in bone.Colliders )
                    {
                        if( coll.GameCollider == null ) continue;
                        if( !colliders.Contains( coll.GameCollider ) ) colliders.Add( coll.GameCollider );
                    }
                }
            }

            return colliders;
        }

        /// <summary> Including just defined dummy bones rigidbodies </summary>
        public List<Rigidbody> User_GetDummyRigidbodies()
        {
            List<Rigidbody> rigids = new List<Rigidbody>();
            if( !WasInitialized ) return rigids;

            foreach( var chain in chains )
                foreach( var bone in chain.BoneSetups )
                {
                    Rigidbody rig = bone.PhysicalDummyBone.GetComponent<Rigidbody>();
                    if( rig ) rigids.Add( rig );
                }

            return rigids;
        }

        /// <summary> Using Physics.Ignore to trigger colliders ignore for all colliders within provided transform and ragdoll dummy colliders </summary>
        public void User_FindAllCollidersInsideAndIgnoreTheirCollisionWithDummyColliders( Transform root, bool ignore = true )
        {
            List<Collider> dummyColliders = User_GetAllDummyColliders();
            List<Collider> found = new List<Collider>();

            foreach( Transform t in root.GetComponentsInChildren<Transform>() )
            {
                t.GetComponents<Collider>( found );

                foreach( var coll in found )
                {
                    foreach( var dummyColl in dummyColliders )
                    {
                        Physics.IgnoreCollision( coll, dummyColl, ignore );
                    }
                }
            }
        }

        private bool wasDummyDisabled = false;

        /// <summary> Setting ragdoll dummy rigidbodies kinematic, disabling their collision detection and disabling dummy colliders </summary>
        public void SwitchDummyPhysics( bool enable )
        {
            #region Just bone components physics switch

            if( RagdollLogic == ERagdollLogic.JustBoneComponents )
            {
                foreach( var chain in Chains )
                {
                    foreach( var bone in chain.BoneSetups )
                    {
                        Rigidbody rig = bone.SourceBone.GetComponent<Rigidbody>();
                        rig.detectCollisions = enable;
                        rig.isKinematic = !enable;
                        Collider coll = bone.SourceBone.GetComponent<Collider>();
                        ConfigurableJoint joint = bone.SourceBone.GetComponent<ConfigurableJoint>();

                        if( joint )
                        {
                            var drive = joint.slerpDrive;
                            drive.positionSpring = GetCurrentMainSpringsValue;
                            joint.slerpDrive = drive;
                        }

                        if( coll == null ) coll = bone.SourceBone.GetComponentInChildren<Collider>();
                        if( coll ) coll.enabled = enable;
                    }
                }

                return;
            }

            #endregion Just bone components physics switch

            if( wasDummyDisabled && enable == false ) return;
            if( !wasDummyDisabled && enable ) return;

            wasDummyDisabled = !enable;

            foreach( var chain in chains )
            {
                chain.SwitchPhysics( enable );
            }

            if( enable ) RefreshAnchorKinematicState();
        }

        /// Checking ground raycast below anchor/hips bone
        /// </summary>
        /// <param name="distance"> If left null, ragdoll animator will compute size of the anchor bone collider and use its average length as raycast distance range </param>
        public RaycastHit ProbeGroundBelowHips( LayerMask mask, float? distance = null, Vector3? worldUp = null )
        {
            return ProbeGroundBelow( GetAnchorBoneController, mask, distance, worldUp );
        }

        /// Checking ground raycast below anchor/hips bone
        /// </summary>
        /// <param name="distance"> If left null, ragdoll animator will compute size of the anchor bone collider and use its average length as raycast distance range </param>
        public RaycastHit ProbeGroundBelow( RagdollChainBone bone, LayerMask mask, float? distance = null, Vector3? worldUp = null )
        {
            Vector3 up = worldUp == null ? Vector3.up : worldUp.Value;
            RaycastHit result;
            if( distance == null ) distance = bone.MainBoneCollider.bounds.size.magnitude + 0.01f;
            Physics.Raycast( new Ray( bone.PhysicalDummyBone.position, -up ), out result, distance.Value, mask, QueryTriggerInteraction.Ignore );
            return result;
        }

        /// <summary> For custom use - overriding muscles power value no matter what </summary>
        public float? User_OverrideMusclesPower
        {
            get { return user_overrideMusclesPower; }
            set { user_overrideMusclesPower = value; CalculateRagdollBlend(); User_UpdateJointsPlayParameters( false ); }
        }

        private float? user_overrideMusclesPower = null;
    }
}