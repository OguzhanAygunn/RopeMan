using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static partial class RagdollHandlerUtilities
    {
        /// <summary>
        /// Call it if your character died and you want to keep it in the current lying pose, without possibility to make it ragdolled again.
        /// This method is disabling unity's Animator! - It's required to avoid playing standing animations, it would be resurrected.
        /// </summary>
        public static void User_FreezeAndDestroyRagdollDummy( this IRagdollAnimator2HandlerOwner iHandler, bool disableAnimator = true )
        {
            RagdollHandler handler = iHandler.GetRagdollHandler;
            if( disableAnimator ) if( handler.Mecanim ) handler.Mecanim.enabled = false;

            handler.CallOnAllRagdollBones( ( RagdollChainBone bone ) =>
            {
                bone.SourceBone.SetPositionAndRotation( bone.PhysicalDummyBone.position, bone.PhysicalDummyBone.rotation );
            } );

            handler.disableUpdating = true;
            handler.AnimatingMode = RagdollHandler.EAnimatingMode.Off;
            handler.OnDisable();
            GameObject.Destroy( handler.Dummy_Container.gameObject );
        }

        /// <summary>
        /// Generates new list of Rigidbodies belonging to the physical ragdoll dummy
        /// </summary>
        public static List<Rigidbody> User_GetAllRigidbodies( this IRagdollAnimator2HandlerOwner iHandler )
        {
            List<Rigidbody> rigs = new List<Rigidbody>();
            iHandler.GetRagdollHandler.CallOnAllRagdollBones( ( RagdollChainBone bone ) => { rigs.Add( bone.GameRigidbody ); } );
            return rigs;
        }

        /// <summary>
        /// Generates new list of Ragdoll Bone Setups of this Ragdoll
        /// </summary>
        public static List<RagdollChainBone> User_GetAllRagdollDummyBoneSetups( this IRagdollAnimator2HandlerOwner iHandler )
        {
            List<RagdollChainBone> bones = new List<RagdollChainBone>();
            iHandler.GetRagdollHandler.CallOnAllRagdollBones( ( RagdollChainBone bone ) => { bones.Add( bone ); } );
            return bones;
        }

        /// <summary> If you changed some ragdoll handler variables through code, you need to trigger settings refresh for the ragdoll dummy components </summary>
        public static void User_UpdateRigidbodyParametersForAllBones( this IRagdollAnimator2HandlerOwner iHandler )
        {
            RagdollHandler handler = iHandler.GetRagdollHandler;
            if( handler.DummyWasGenerated == false ) return;

            foreach( var chain in handler.Chains )
                foreach( var bone in chain.BoneSetups )
                    bone.RefreshRigidbody( handler, chain, false );
        }

        /// <summary> If you changed some ragdoll handler variables through code, you need to trigger settings refresh for the ragdoll dummy components </summary>
        public static void User_UpdateColliderParametersForAllBones( this IRagdollAnimator2HandlerOwner iHandler )
        {
            RagdollHandler handler = iHandler.GetRagdollHandler;
            if( handler.DummyWasGenerated == false ) return;

            bool fall = handler.IsFallingOrSleep;

            foreach( var chain in handler.Chains )
                foreach( var bone in chain.BoneSetups )
                    bone.RefreshCollider( chain, fall, false );

            if( handler.WasInitialized )
            {
                handler.EnsureCollisionsIgnoreSetup();
            }
        }

        /// <summary> If you changed some ragdoll handler variables through code, you need to trigger settings refresh for the ragdoll dummy components </summary>
        public static void User_UpdatePhysicsParametersForAllBones( this IRagdollAnimator2HandlerOwner iHandler )
        {
            if( iHandler.GetRagdollHandler.DummyWasGenerated == false ) return;

            bool fall = iHandler.GetRagdollHandler.IsFallingOrSleep;

            foreach( var chain in iHandler.GetRagdollHandler.Chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    // In case of using rotation correction //if( bone.IsAnchor ) continue;
                    bone.RefreshJoint( chain, fall, false, true );
                }
            }
        }

        /// <summary>
        /// Call after changing layer settings in ragdoll animator (can't be executed in OnValidate for some reason, unity gives warning about that)
        /// </summary>
        public static void User_UpdateLayersAfterManualChanges( this IRagdollAnimator2HandlerOwner iHandler )
        {
            RagdollHandler handler = iHandler.GetRagdollHandler;
            handler.Dummy_Container.gameObject.layer = handler.RagdollDummyLayer;

            foreach( var chain in handler.Chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    bone.GameRigidbody.gameObject.layer = handler.RagdollDummyLayer;

                    foreach( var collS in bone.Colliders )
                        collS.GameCollider.gameObject.layer = handler.RagdollDummyLayer;
                }

                if( chain.ParentConnectionBones != null )
                    foreach( var item in chain.ParentConnectionBones )
                    {
                        item.DummyBone.gameObject.layer = handler.RagdollDummyLayer;
                    }
            }

            if( handler.skeletonFillExtraBonesList != null )
            {
                foreach( var item in handler.skeletonFillExtraBonesList )
                {
                    item.DummyBone.gameObject.layer = handler.RagdollDummyLayer;
                }
            }
        }

        /// <summary> If you changed some ragdoll handler variables through code, you need to trigger settings refresh for the ragdoll dummy components </summary>
        public static void User_UpdateAllBonesParametersAfterManualChanges( this IRagdollAnimator2HandlerOwner iHandler )
        {
            RagdollHandler handler = iHandler.GetRagdollHandler;
            handler.User_UpdateColliderParametersForAllBones();
            handler.User_UpdateRigidbodyParametersForAllBones();
            handler.User_UpdatePhysicsParametersForAllBones();

            if( handler.WasInitialized == false ) return;
            handler.User_UpdateJointsPlayParameters( true );
        }
    }
}