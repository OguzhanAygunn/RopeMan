using FIMSpace.AnimationTools;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static partial class RagdollHandlerUtilities
    {
        /// <summary>
        /// Returning reference to the ragdoll dummy setup, using HumanBodyBones unity enum
        /// </summary>
        public static RagdollChainBone User_GetBoneSetupByHumanoidBone( this IRagdollAnimator2HandlerOwner iHandler, HumanBodyBones bone )
        {
            RagdollHandler handler = iHandler.GetRagdollHandler;
            if( handler.Mecanim && handler.Mecanim.isHuman )
            {
                var controller = handler.DictionaryGetBoneSetupBySourceBone( handler.Mecanim.GetBoneTransform( bone ) );
                if( controller == null ) controller = handler.DictionaryGetBoneSetupBySourceBone( handler.Mecanim.GetBoneTransform( bone ).parent );
                if( controller == null ) controller = handler.DictionaryGetBoneSetupBySourceBone( handler.Mecanim.GetBoneTransform( bone ).parent.parent );
                if( controller == null ) controller = handler.DictionaryGetBoneSetupBySourceBone( SkeletonRecognize.GetContinousChildTransform( handler.Mecanim.GetBoneTransform( bone ) ) );
                return controller;
            }
            else // Try get bones using chain types
            {
                UnityEngine.Debug.Log( "[Ragdoll Animator 2] Get controller bone for non humanoid not implemented yet" );
            }

            return null;
        }

        /// <summary>
        /// Returning reference to the ragdoll dummy setup, using ERagdollBoneID enum
        /// </summary>
        public static RagdollChainBone User_GetBoneSetupByBoneID( this IRagdollAnimator2HandlerOwner iHandler, ERagdollBoneID id )
        {
            RagdollHandler handler = iHandler.GetRagdollHandler;
            var controller = handler.DictionaryGetBoneSetupByBoneID( id );
            return controller;
        }

        /// <summary>
        /// Returning reference to the ragdoll dummy setup, using source skeleton transform reference
        /// </summary>
        public static RagdollChainBone User_GetBoneSetupBySourceAnimatorBone( this IRagdollAnimator2HandlerOwner iHandler, Transform skeletonBone )
        {
            return iHandler.GetRagdollHandler.DictionaryGetBoneSetupBySourceBone( skeletonBone );
        }

        /// <summary>
        /// Returning reference to the ragdoll dummy setup, using source skeleton transform name
        /// </summary>
        public static RagdollChainBone User_GetBoneSetupByBoneName( this IRagdollAnimator2HandlerOwner iHandler, string name )
        {
            return iHandler.GetRagdollHandler.DictionaryGetBoneControllerBySourceBoneName( name );
        }

        /// <summary>
        /// Returning reference to the ragdoll dummy setup, using ragdoll dummy bone transform reference
        /// </summary>
        public static RagdollChainBone User_GetBoneSetupByDummyBone( this IRagdollAnimator2HandlerOwner iHandler, Transform ragdollDummyTransform )
        {
            return iHandler.GetRagdollHandler.DictionaryGetBoneControllerByRagdollBone( ragdollDummyTransform );
        }

        /// <summary>
        /// Returning physical dummy bone transform which represents animator bone
        /// </summary>
        public static Transform User_GetPhysicalBoneBySourceBone( this IRagdollAnimator2HandlerOwner iHandler, Transform sourceAnimatorBone )
        {
            RagdollChainBone bone;
            if( iHandler.GetRagdollHandler.animatorTransformBoneDictionary.TryGetValue( sourceAnimatorBone, out bone ) ) return bone.PhysicalDummyBone;
            return null;
        }

        /// <summary>
        /// Returning source animator bone transform basing on the physical dummy transform reference
        /// </summary>
        public static Transform User_GetSourceBoneByPhysicalBone( this IRagdollAnimator2HandlerOwner iHandler, Transform physicalBoneTransform )
        {
            RagdollChainBone bone;
            if( iHandler.GetRagdollHandler.physicalTransformBoneDictionary.TryGetValue( physicalBoneTransform, out bone ) ) return bone.SourceBone;
            return null;
        }

        /// <summary>
        /// Forcing physical bones to be hardly rotated like current animator pose.
        /// </summary>
        public static void User_ForceMatchPhysicalBonesWithAnimator( this IRagdollAnimator2HandlerOwner iHandler, bool syncPositions = false )
        {
            var handler = iHandler.GetRagdollHandler;
            //CallOnAllRagdollBones( ( RagdollChainBone b ) => { b.GameRigidbody.isKinematic = true; } );

            handler.CallOnAllRagdollBones( ( RagdollChainBone b ) => { b.GameRigidbody.rotation = b.SourceBone.rotation; b.GameRigidbody.transform.rotation = b.SourceBone.rotation; } );
            if( syncPositions ) handler.CallOnAllRagdollBones( ( RagdollChainBone b ) => { b.GameRigidbody.position = b.SourceBone.position; b.GameRigidbody.transform.position = b.SourceBone.position; } );
            handler.CallOnAllInBetweenBones( ( RagdollChainBone.InBetweenBone b ) => { b.DummyBone.rotation = b.SourceBone.rotation; } );

            //Caller.StartCoroutine( _IE_CallAfter( 0f, () => { CallOnAllRagdollBones( ( RagdollChainBone b ) => { b.GameRigidbody.isKinematic = false; } ); }, 1 ) );
        }
    }
}