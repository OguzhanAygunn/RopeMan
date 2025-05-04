using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static partial class RagdollHandlerUtilities
    {
        /// <summary>
        /// Returning nearest dummy bone collider, towards provided world point
        /// </summary>
        /// <param name="pos"> Word position to search nearest bone for </param>
        /// <param name="fast"> Fast mode is not checking bones colliders but only bone origin position as distance reference </param>
        /// <param name="justCoreChain"> If it's not neccesary to calculate distance of all limbs, you can call search just on one bones chain. Null means checking all chains. </param>
        public static Collider User_GetNearestRagdollColliderToPosition( this IRagdollAnimator2HandlerOwner iHandler, Vector3 pos, bool fast = true, ERagdollChainType? justChain = null )
        {
            return User_GetNearestRagdollBoneControllerToPosition( iHandler, pos, fast, justChain ).MainBoneCollider;
        }

        /// <summary>
        /// Returning nearest ragdoll dummy bone rigidbody, towards provided world point
        /// </summary>
        /// <param name="pos"> Word position to search nearest bone for </param>
        /// <param name="fast"> Fast mode is not checking bones colliders but only bone origin position as distance reference </param>
        /// <param name="justCoreChain"> If it's not neccesary to calculate distance of all limbs, you can call search just on one bones chain. Null means checking all chains. </param>
        public static Rigidbody User_GetNearestRagdollRigidbodyToPosition( this IRagdollAnimator2HandlerOwner iHandler, Vector3 pos, bool fast = true, ERagdollChainType? justChain = null )
        {
            return User_GetNearestRagdollBoneControllerToPosition( iHandler, pos, fast, justChain ).GameRigidbody;
        }

        /// <summary>
        /// Returning nearest source bone transform, towards the provided world point
        /// </summary>
        /// <param name="pos"> Word position to search nearest bone for </param>
        /// <param name="fast"> Fast mode is not checking bones colliders but only bone origin position as distance reference </param>
        /// <param name="justCoreChain"> If it's not neccesary to calculate distance of all limbs, you can call search just on one bones chain. Null means checking all chains. </param>
        public static Transform User_GetNearestAnimatorTransformBoneToPosition( this IRagdollAnimator2HandlerOwner iHandler, Vector3 pos, bool fast = true, ERagdollChainType? justChain = null )
        {
            return User_GetNearestRagdollBoneControllerToPosition( iHandler, pos, fast, justChain ).SourceBone;
        }

        /// <summary>
        /// Returning nearest dummy bone transform, towards the provided world point
        /// </summary>
        /// <param name="pos"> Word position to search nearest bone for </param>
        /// <param name="fast"> Fast mode is not checking bones colliders but only bone origin position as distance reference </param>
        /// <param name="justCoreChain"> If it's not neccesary to calculate distance of all limbs, you can call search just on one bones chain. Null means checking all chains. </param>
        public static Transform User_GetNearestPhysicalTransformBoneToPosition( this IRagdollAnimator2HandlerOwner iHandler, Vector3 pos, bool fast = true, ERagdollChainType? justChain = null )
        {
            return User_GetNearestRagdollBoneControllerToPosition( iHandler, pos, fast, justChain ).PhysicalDummyBone;
        }

        /// <summary>
        /// Returning nearest bone controller, towards the provided world point
        /// </summary>
        /// <param name="pos"> Word position to search nearest bone for </param>
        /// <param name="fast"> Fast mode is not checking bones colliders but only bone origin position as distance reference </param>
        /// <param name="justCoreChain"> If it's not neccesary to calculate distance of all limbs, you can call search just on one bones chain. Null means checking all chains. </param>
        public static RagdollChainBone User_GetNearestRagdollBoneControllerToPosition( this IRagdollAnimator2HandlerOwner iHandler, Vector3 pos, bool fast = true, ERagdollChainType? justChain = null )
        {
            RagdollHandler handler = iHandler.GetRagdollHandler;

            if( fast )
            {
                RagdollChainBone nearestB = null;
                float nearestDist = float.MaxValue;

                if( justChain != null )
                {
                    var coreChain = handler.GetChain( justChain.Value );

                    foreach( var bone in coreChain.BoneSetups )
                    {
                        float dist = ( pos - bone.GameRigidbody.worldCenterOfMass ).sqrMagnitude;
                        if( dist < nearestDist ) { nearestDist = dist; nearestB = bone; }
                    }
                }
                else
                {
                    handler.CallOnAllRagdollBones( ( RagdollChainBone bone ) =>
                    {
                        float dist = ( pos - bone.GameRigidbody.worldCenterOfMass ).sqrMagnitude;
                        if( dist < nearestDist ) { nearestDist = dist; nearestB = bone; }
                    } );
                }

                return nearestB;
            }
            else
            {
                RagdollChainBone nearestB = null;
                float nearestDist = float.MaxValue;

                if( justChain != null )
                {
                    var coreChain = handler.GetChain( justChain.Value );

                    foreach( var bone in coreChain.BoneSetups )
                    {
                        // TODO: foreach collider?
                        float dist = ( pos - nearestB.MainBoneCollider.ClosestPoint( pos ) ).sqrMagnitude;
                        if( dist < nearestDist ) { nearestDist = dist; nearestB = bone; }
                    }
                }
                else
                {
                    handler.CallOnAllRagdollBones( ( RagdollChainBone bone ) =>
                    {
                        float dist = ( pos - nearestB.MainBoneCollider.ClosestPoint( pos ) ).sqrMagnitude;
                        if( dist < nearestDist ) { nearestDist = dist; nearestB = bone; }
                    } );
                }

                return nearestB;
            }
        }

        /// <summary>
        /// Setting target physical material to all dummy bones (will be overwritten if using physical materials in the handler setup)
        /// </summary>
        public static void User_ChangeAllCollidersPhysicMaterial( this IRagdollAnimator2HandlerOwner iHandler, PhysicMaterial targetMaterial )
        {
            RagdollHandler handler = iHandler.GetRagdollHandler;

            if( handler.DummyWasGenerated == false ) return;

            foreach( var chain in handler.Chains )
                foreach( var bone in chain.BoneSetups )
                    bone.ApplyPhysicMaterial( targetMaterial );
        }
    }
}