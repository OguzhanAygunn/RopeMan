using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static partial class RagdollHandlerUtilities
    {
        /// <summary>
        /// Refreshing Ragdoll Animator object after teleporting or teleporting and refreshing rigidbodies of ragdoll animator
        /// </summary>
        public static void User_Teleport( this IRagdollAnimator2HandlerOwner iHandler, Vector3? worldPosition = null, Quaternion? worldRotation = null )
        {
            var handler = iHandler.GetRagdollHandler;
            var anchor = handler.GetAnchorBoneController;

            if( worldPosition == null && worldRotation == null )
            {
                handler.User_SetAllKinematic( true );
                handler.Caller.StartCoroutine( handler._IE_CallAfter( 0f, () => { handler.User_SetAllKinematic( false ); handler.User_UpdateAllBonesParametersAfterManualChanges(); }, 1 ) );
            }
            else
            {
                handler.User_SetAllKinematic( true );
                if( worldPosition != null )
                {
                    handler.GetBaseTransform().position = worldPosition.Value;
                    anchor.GameRigidbody.position = worldPosition.Value - anchor.SourceBone.TransformVector( handler.anchorToRootLocal );
                }

                if( worldRotation != null )
                {
                    handler.GetBaseTransform().rotation = worldRotation.Value;
                }

                handler.Caller.StartCoroutine( handler._IE_CallAfter( 0f, () => { handler.User_SetAllKinematic( false ); handler.User_UpdateAllBonesParametersAfterManualChanges(); }, 1 ) );
                handler.Caller.StartCoroutine( handler._IE_CallForFixedFrames( () => { handler.User_SetAllVelocity(Vector3.zero); handler.User_ResetAngularVelocityForAllBones(); }, 2 ) );
            }
        }

        /// <summary>
        /// Refreshing Ragdoll Animator object after warping ragdoll body to new place (doing few fixed update ticks forcing rigidbodies to match animated pose)
        /// </summary>
        public static void User_WarpRefresh( this IRagdollAnimator2HandlerOwner iHandler, int frames = 3)
        {
            var handler = iHandler.GetRagdollHandler;
            handler.Caller.StartCoroutine( handler._IE_RefreshBonesAfterTeleport(frames) );
            handler.Caller.StartCoroutine( handler._IE_RefreshBonesAfterTeleportFixed(frames) );
        }

        /// <summary>
        /// Defined offset from anchor (hips) towards character standing position (by standards - origin in feet middle)
        /// </summary>
        public static Vector3 User_GetStoredAnchorRootOffset( this IRagdollAnimator2HandlerOwner iHandler )
        {
            var handler = iHandler.GetRagdollHandler;
            if( handler.anchorToRootLocal == Vector3.zero ) return handler.BaseTransform.position;
            return handler.GetAnchorBoneController.PhysicalDummyBone.TransformPoint( handler.anchorToRootLocal );
        }

        /// <summary>
        /// Defined offset from anchor (hips) towards character standing rotation
        /// </summary>
        public static Quaternion User_GetStoredAnchorRootOffsetRot( this IRagdollAnimator2HandlerOwner iHandler )
        {
            var handler = iHandler.GetRagdollHandler;
            if( handler.anchorToRootLocalRot == Quaternion.identity ) return handler.BaseTransform.rotation;
            return FEngineering.QToWorld( handler.GetAnchorBoneController.PhysicalDummyBone.rotation, handler.anchorToRootLocalRot );
        }

        /// <summary>
        /// Computing target bone forward pointing direction in world space
        /// </summary>
        public static Vector3 User_BoneWorldForward( this IRagdollAnimator2HandlerOwner iHandler, RagdollChainBone bone )
        {
            return iHandler.GetRagdollHandler.GetAnchorBoneController.GameRigidbody.rotation * bone.LocalForward;
        }

        /// <summary>
        /// Computing target bone up pointing direction in world space
        /// </summary>
        public static Vector3 User_BoneWorldUp( this IRagdollAnimator2HandlerOwner iHandler, RagdollChainBone bone )
        {
            return iHandler.GetRagdollHandler.GetAnchorBoneController.GameRigidbody.rotation * bone.LocalUp;
        }

        /// <summary>
        /// Computing target bone right pointing direction in world space
        /// </summary>
        public static Vector3 User_BoneWorldRight( this IRagdollAnimator2HandlerOwner iHandler, RagdollChainBone bone )
        {
            return iHandler.GetRagdollHandler.GetAnchorBoneController.GameRigidbody.rotation * bone.LocalRight;
        }

        /// <summary>
        /// Using collider bounding boxes volumes to define bounding box of current ragdoll pose.
        /// </summary>
        public static Bounds User_GetRagdollBonesStateBounds( this IRagdollAnimator2HandlerOwner iHandler, bool fast = true )
        {
            Bounds b = new Bounds( iHandler.GetRagdollHandler.GetAnchorBoneController.PhysicalDummyBone.position, new Vector3( 0f, 0f, 0f ) );

            foreach( var chain in iHandler.GetRagdollHandler.Chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    foreach( var collS in bone.Colliders )
                    {
                        b.Encapsulate( collS.GameCollider.bounds );
                    }
                }
            }

            return b;
        }

        /// <summary>
        /// Calculating position based on the current state of physical bones.
        /// Using collider bounding boxes volumes.
        /// </summary>
        public static Vector3 User_GetPosition_BottomCenter( this IRagdollAnimator2HandlerOwner iHandler )
        {
            Bounds b = iHandler.User_GetRagdollBonesStateBounds();
            Vector3 bott = b.center;
            bott.y = b.min.y;
            return bott;
        }

        /// <summary>
        /// Calculating position based on the current state of physical bones.
        /// Using collider bounding boxes volumes.
        /// </summary>
        public static Vector3 User_GetPosition_Center( this IRagdollAnimator2HandlerOwner iHandler )
        {
            return iHandler.User_GetRagdollBonesStateBounds().center;
        }

        /// <summary>
        /// Calculating position based on the current state of physical anchor bone collider.
        /// </summary>
        public static Vector3 User_GetPosition_AnchorBottom( this IRagdollAnimator2HandlerOwner iHandler )
        {
            Bounds anchorBounds = iHandler.GetRagdollHandler.GetAnchorBoneController.MainBoneCollider.bounds;
            Vector3 pos = anchorBounds.center;
            pos.y = anchorBounds.min.y;
            return pos;
        }

        /// <summary>
        /// Calculating position based on the current state of physical anchor bone collider.
        /// </summary>
        public static Vector3 User_GetPosition_AnchorCenter( this IRagdollAnimator2HandlerOwner iHandler )
        {
            Bounds anchorBounds = iHandler.GetRagdollHandler.GetAnchorBoneController.MainBoneCollider.bounds;
            return anchorBounds.center;
        }

        /// <summary>
        /// Calculating position basing on the current state of physical bones.
        /// Trying to define character object stand position with origin in feet.
        /// Using collider bounding boxes volumes.
        /// </summary>
        public static Vector3 User_GetPosition_HipsToFoot( this IRagdollAnimator2HandlerOwner iHandler )
        {
            Bounds b = iHandler.User_GetRagdollBonesStateBounds();
            Vector3 hps = iHandler.User_GetStoredAnchorRootOffset();
            hps.y = b.min.y;
            return hps;
        }

        /// <summary>
        /// Calculating position basing on the leg chains last bones middle position.
        /// </summary>
        public static Vector3 User_GetPosition_FeetMiddle( this IRagdollAnimator2HandlerOwner iHandler )
        {
            Vector3 midPos = iHandler.User_GetStoredAnchorRootOffset();

            foreach( var chain in iHandler.GetRagdollHandler.Chains )
            {
                if( chain.ChainType.IsLeg() )
                {
                    midPos = Vector3.LerpUnclamped( midPos, chain.BoneSetups[chain.BoneSetups.Count - 1].PhysicalDummyBone.position, 0.5f );
                }
            }

            return midPos;
        }

        /// <summary>
        /// Getting hips direction rotation, prioritized with up vector, dedicated for get up orientation,
        /// defining direction using middle feet position and anchor bone (hips) position
        /// </summary>
        public static Quaternion User_GetMappedRotationHipsToLegsMiddle( this IRagdollAnimator2HandlerOwner iHandler, Vector3? up = null, bool checkIfOnBack = true )
        {
            var anchor = iHandler.GetRagdollHandler.GetAnchorBoneController;
            Vector3 upVector = Vector3.up;
            if( up != null ) upVector = up.Value;

            if( checkIfOnBack )
            {
                if( iHandler.User_IsOnBack( false, upVector ) )
                    return Quaternion.LookRotation( Vector3.ProjectOnPlane( -( anchor.PhysicalDummyBone.position - iHandler.User_GetPosition_FeetMiddle() ), upVector ), upVector );
                else
                    return Quaternion.LookRotation( Vector3.ProjectOnPlane( anchor.PhysicalDummyBone.position - iHandler.User_GetPosition_FeetMiddle(), upVector ), upVector );
            }
            else
                return Quaternion.LookRotation( Vector3.ProjectOnPlane( anchor.PhysicalDummyBone.position - iHandler.User_GetPosition_FeetMiddle(), upVector ), upVector );
        }

        /// <summary>
        /// Getting hips direction rotation, prioritized with up vector, dedicated for get up orientation,
        /// Hips to head can be helpful for calculating target rotation for animal get up action.
        /// </summary>
        public static Quaternion User_GetMappedRotationHipsToHead( this IRagdollAnimator2HandlerOwner iHandler, Vector3? up = null, bool checkIfOnBack = true )
        {
            var anchor = iHandler.GetRagdollHandler.GetAnchorBoneController;
            Vector3 upVector = Vector3.up;
            if( up != null ) upVector = up.Value;

            var head = iHandler.GetRagdollHandler.GetChain( ERagdollChainType.Core ).GetBone( 1000 ); // Get last core bone, treat as head

            if( checkIfOnBack )
            {
                if( iHandler.User_IsOnBack( false, upVector ) )
                    return Quaternion.LookRotation( Vector3.ProjectOnPlane( -( head.PhysicalDummyBone.position - anchor.PhysicalDummyBone.position ), upVector ), upVector );
                else
                    return Quaternion.LookRotation( Vector3.ProjectOnPlane( head.PhysicalDummyBone.position - anchor.PhysicalDummyBone.position, upVector ), upVector );
            }
            else
                return Quaternion.LookRotation( Vector3.ProjectOnPlane( head.PhysicalDummyBone.position - anchor.PhysicalDummyBone.position, upVector ), upVector );
        }

        /// <summary>
        /// Getting hips direction rotation, prioritized with up vector
        /// </summary>
        public static Quaternion User_GetRotation_Mapped( this IRagdollAnimator2HandlerOwner iHandler, Vector3 up )
        {
            var anchor = iHandler.GetRagdollHandler.GetAnchorBoneController;
            Vector3 anchorForward = iHandler.User_BoneWorldForward( anchor );
            float dot = Vector3.Dot( anchorForward, up );

            if( dot > 0.6f ) return iHandler.User_GetMappedRotationHipsToLegsMiddle( up );
            else if( dot < -0.6f ) return iHandler.User_GetMappedRotationHipsToLegsMiddle( up );

            return Quaternion.LookRotation( Vector3.ProjectOnPlane( anchor.PhysicalDummyBone.transform.rotation * anchorForward, up ), up );
        }

        /// <summary>
        /// Getting hips direction rotation, prioritized with up vector, prepared for certain get up type
        /// </summary>
        public static Quaternion User_GetRotation_MappedFor( this IRagdollAnimator2HandlerOwner iHandler, ERagdollGetUpType getupType, Vector3 up )
        {
            var anchor = iHandler.GetRagdollHandler.GetAnchorBoneController;
            Vector3 anchorUp = iHandler.User_BoneWorldUp( anchor );
            return Quaternion.LookRotation( Vector3.ProjectOnPlane( anchor.PhysicalDummyBone.rotation * ( getupType == ERagdollGetUpType.FromBack ? -anchorUp : anchorUp ), up ), up );
        }

        /// <summary>
        /// Summing and normalizing all chain bones target world directions
        /// </summary>
        public static Vector3 User_GetAverageDirectionOf( this IRagdollAnimator2HandlerOwner iHandler, RagdollBonesChain chain, RagdollChainBone.ECapsuleDirection axis )
        {
            Vector3 dir = Vector3.zero;

            if( axis == RagdollChainBone.ECapsuleDirection.X )
            {
                foreach( var bone in chain.BoneSetups ) dir += iHandler.User_BoneWorldRight( bone );
            }
            else if( axis == RagdollChainBone.ECapsuleDirection.Y )
            {
                foreach( var bone in chain.BoneSetups ) dir += iHandler.User_BoneWorldUp( bone );
            }
            else if( axis == RagdollChainBone.ECapsuleDirection.Z )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    dir += iHandler.User_BoneWorldForward( bone );
                }
            }

            return ( dir / (float)chain.BoneSetups.Count ).normalized;
        }
    }
}