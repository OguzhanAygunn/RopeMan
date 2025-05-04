using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_BlendOnSourceCollision : RAF_BlendOnCollisions
    {

#if UNITY_EDITOR

        public override string Editor_FeatureDescription => "Second approach to the 'Blend On Collisions' feature. It is more expensive and involves generating trigger colliders on the source skeleton, but can provide more relevant bones blending than just dummy bones blend on collision feature.";

#endif

        protected override void InitIndicators()
        {
            ParentRagdollHandler.PrepareSourceBonesCollisionIndicators( true, true, false );

            // Ignore all collision between ragdoll dummy and source bones
            foreach( var chain in ParentRagdollHandler.Chains )
                foreach( var bone in chain.BoneSetups )
                    foreach( var coll in bone.Colliders )
                        ParentRagdollHandler.IgnoreCollisionWith( coll.GameColliderOnSource );

            ParentRagdollHandler.EnsureRelatedCollidersIgnore();

            foreach( var chain in ParentRagdollHandler.Chains )
            {
                foreach( var bone in chain.BoneSetups )
                    foreach( var coll in bone.Colliders )
                    {
                        var rigid = bone.SourceBone.gameObject.AddComponent<Rigidbody>();
                        if( rigid ) { rigid.isKinematic = true; }
                        if( coll.GameColliderOnSource ) coll.GameColliderOnSource.isTrigger = true;
                    }
            }
        }

        protected override RA2BoneCollisionHandlerBase GetCollisionHandler( RagdollChainBone bone )
        {
            return bone.SourceBone.GetComponent<RA2BoneTriggerCollisionHandler>();
        }
    }
}