using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static partial class RagdollHandlerUtilities
    {
        /// <summary>
        /// Checking state for ragdoll get-up possiblity case, basing just on the rotation of the anchor bone.
        /// </summary>
        /// <param name="canBeNone"> If character will lie on the side or be in some undefined state, method will return false </param>
        public static bool User_GetUpByRotationPossible( this IRagdollAnimator2HandlerOwner iHandler, bool canBeNone = false, Vector3? up = null )
        {
            return iHandler.User_CanGetUpByRotation( canBeNone, up ) != ERagdollGetUpType.None;
        }

        ///// <summary> Checking state for ragdoll get-up possiblity case, basing just on the rotation of the anchor bone. </summary>
        //public static  bool User_GetUpByRotationPossible( EGetUpType getup )
        //{
        //    return getup != EGetUpType.None;
        //}

        /// <summary> Basing on the orientation of the anchor bone, defining if character is currently lying on its back </summary>
        public static bool User_IsOnBack( this IRagdollAnimator2HandlerOwner iHandler, bool canBeNone = false, Vector3? up = null )
        {
            return iHandler.User_CanGetUpByRotation( canBeNone, up ) == ERagdollGetUpType.FromBack;
        }

        /// <summary> Calculating multiplier (multiplier which makes value lower 0-1) for average angular velocity, to lower get up angular velocity threshold when body is not moving </summary>
        public static float User_CoreLowTranslationFactor( this IRagdollAnimator2HandlerOwner iHandler, float averageTranslationMagnitude )
        {
            return Mathf.InverseLerp( 0.1f, 0.00004f, averageTranslationMagnitude );
        }

        /// <summary>
        /// Checking state for ragdoll get-up possibility case, basing just on the rotation of the anchor bone.
        /// </summary>
        /// <param name="quadroped"> If it's animal with horizontal instead of vertical spine, it will help detecting facedown / on back </param>
        public static ERagdollGetUpType User_CanGetUpByRotation( this IRagdollAnimator2HandlerOwner iHandler, bool canBeNone = false, Vector3? worldUp = null, bool includeLeftRightSide = false, float tolerance = 0.5f, bool? quadroped = null)
        {
            Vector3 up = worldUp == null ? Vector3.up : worldUp.Value;
            float dot;
            var coreChain = iHandler.GetRagdollHandler.GetChain( ERagdollChainType.Core );

            if( quadroped == null ) quadroped = !iHandler.GetRagdollHandler.IsHumanoid;
            
            if( quadroped == true )
            {
                dot = Vector3.Dot( -iHandler.User_GetAverageDirectionOf( coreChain, RagdollChainBone.ECapsuleDirection.Y ), up );
            }
            else
            {
                dot = Vector3.Dot( iHandler.User_GetAverageDirectionOf( coreChain, RagdollChainBone.ECapsuleDirection.Z ), up );
            }

            if( canBeNone )
            {
                if( dot > tolerance ) return ERagdollGetUpType.FromBack;
                else if( dot < -tolerance ) return ERagdollGetUpType.FromFacedown;
                else
                {
                    if( includeLeftRightSide )
                    {
                        return iHandler.User_LayingOnSide( worldUp );
                    }
                }
            }
            else
            {
                if( dot >= 0f ) return ERagdollGetUpType.FromBack;
                if( dot < 0f ) return ERagdollGetUpType.FromFacedown;
            }

            return ERagdollGetUpType.None;
        }

        /// <summary> Basing on the orientation of the anchor bone, defining if character is currently lying on its side </summary>
        public static ERagdollGetUpType User_LayingOnSide( this IRagdollAnimator2HandlerOwner iHandler, Vector3? worldUp = null, bool canBeNone = true, float tolerance = 0.35f )
        {
            Vector3 up = worldUp == null ? Vector3.up : worldUp.Value;
            float dot = Vector3.Dot( iHandler.User_GetAverageDirectionOf( iHandler.GetRagdollHandler.GetChain( ERagdollChainType.Core ), RagdollChainBone.ECapsuleDirection.X ), up );

            if( canBeNone )
            {
                if( dot > tolerance ) return ERagdollGetUpType.FromLeftSide;
                if( dot < -tolerance ) return ERagdollGetUpType.FromRightSide;
            }
            else
            {
                if( dot >= 0f ) return ERagdollGetUpType.FromLeftSide;
                if( dot < 0f ) return ERagdollGetUpType.FromRightSide;
            }

            return ERagdollGetUpType.None;
        }

        /// <summary>
        /// Checking ground raycast below anchor/hips bone. Same as User_ProbeGroundBelowHips
        /// </summary>
        /// <param name="distance"> If left null, ragdoll animator will compute size of the anchor bone collider and use its average length as raycast distance range </param>
        public static RaycastHit User_ProbeGroundBelowAnchorBone( this IRagdollAnimator2HandlerOwner iHandler, LayerMask groundMask, float? distance = null, Vector3? worldUp = null )
        {
            return iHandler.GetRagdollHandler.ProbeGroundBelowHips( groundMask, distance, worldUp );
        }

        /// <summary> Checking ground raycast below anchor bone. Same as User_ProbeGroundBelowAnchorBone </summary>
        public static RaycastHit User_ProbeGroundBelowHips( this IRagdollAnimator2HandlerOwner iHandler, LayerMask mask, float distance = 10f, Vector3? worldUp = null )
        {
            return iHandler.GetRagdollHandler.ProbeGroundBelowHips( mask, distance, worldUp );
        }

        /// <summary> Checking ground raycast below anchor bone. Same as User_ProbeGroundBelowAnchorBone </summary>
        public static RaycastHit User_ProbeGroundBelow( this IRagdollAnimator2HandlerOwner iHandler, RagdollChainBone bone, LayerMask mask, float distance = 10f, Vector3? worldUp = null )
        {
            return iHandler.GetRagdollHandler.ProbeGroundBelow( bone, mask, distance, worldUp );
        }

        ///// <summary>
        ///// Few operations which are helpful for animating get up animation
        ///// Excluding changing muscles power and hard matching in this overload method
        ///// </summary>
        ///// <param name="transitionDuration"> How much time animating values should take </param>
        ///// <param name="blendToAnimatorFor"> Setting ragdoll animator blend to zero, until progress of transition reach this value (0-1 value). Set zero to not use this feature. </param>
        ///// <param name="animatorTransitionDelay"> Use in case you're crossfading standing animation into lying -> get up animation to prevent mini body hover on the start of get up animation (crossfade swapping standing animation into lying on floor animation). </param>
        ///// <param name="targetAnchorAttachPower"> Target anchor attach value after transitiong end </param>
        ///// <param name="targetMusclesPower"> Target muscles power value after transitiong end </param>
        ///// <param name="targetHardMatch"> Target hard matching value after transitiong end </param>
        ///// <param name="delay"> Delay to start transition </param>
        //public static void User_TransitionToStandingMode( this IRagdollAnimator2HandlerOwner iHandler, float transitionDuration, float blendToAnimatorFor = 0.6f, float animatorTransitionDelay = 0.1f, float targetMusclesPower = 1f, float targetHardMatch = 1f, float delay = 0f )
        //{
        //    RagdollHandler handler = iHandler.GetRagdollHandler;
        //    if( handler.standUpCoroutine != null ) handler.Caller.StopCoroutine( handler.standUpCoroutine );
        //    handler.standUpCoroutine = handler.Caller.StartCoroutine( handler._IE_TransitionToStandingMode( transitionDuration, blendToAnimatorFor, animatorTransitionDelay, delay, targetMusclesPower, targetHardMatch ) );
        //}

        /// <summary>
        /// Few operations which are helpful for animating get up animation
        /// Excluding changing muscles power and hard matching in this overload method
        /// </summary>
        /// <param name="transitionDuration"> How much time animating values should take </param>
        /// <param name="blendToAnimatorFor"> Setting ragdoll animator blend to zero, until progress of transition reach this value (0-1 value). Set zero to not use this feature. </param>
        /// <param name="animatorTransitionDelay"> Use in case you're crossfading standing animation into lying -> get up animation to prevent mini body hover on the start of get up animation (crossfade swapping standing animation into lying on floor animation). </param>
        /// <param name="freezeSourceAnimatedHips"> Freezing position/rotation of source animation anchor bone, to make crossfade to get up animation seamless (no hovering - caused by falling to get up animation crossfade). </param>
        /// <param name="delay"> Delay to start transition </param>
        /// <param name="isOnLegsRestoreCall"> It's dedicated for "Get Up" when character is standing on both legs but during ragdolled state </param>
        public static void User_TransitionToStandingMode( this IRagdollAnimator2HandlerOwner iHandler, float transitionDuration, float blendToAnimatorFor = 0.6f, float animatorTransitionDelay = 0.1f, float freezeSourceAnimatedHips = 0f, float delay = 0f, bool isOnLegsRestoreCall = false )
        {
            RagdollHandler handler = iHandler.GetRagdollHandler;
            if( handler.standUpCoroutine != null ) handler.Caller.StopCoroutine( handler.standUpCoroutine );
            handler.standUpCoroutine = handler.Caller.StartCoroutine( handler._IE_TransitionToStandingMode( transitionDuration, blendToAnimatorFor, animatorTransitionDelay, freezeSourceAnimatedHips, delay, isOnLegsRestoreCall ) );
        }

        ///// <summary> Few operations which are helpful for animating get up animation
        ///// Excluding changing hard matching in this overload method </summary>
        ///// <param name="transitionDuration"> How much time animating values should take </param>
        ///// <param name="blendToAnimatorFor"> Setting ragdoll animator blend to zero, until progress of transition reach this value (0-1 value). Set zero to not use this feature. </param>
        ///// <param name="animatorTransitionDelay"> Use in case you're crossfading standing animation into lying -> get up animation to prevent mini body hover on the start of get up animation (crossfade swapping standing animation into lying on floor animation). </param>
        ///// <param name="targetAnchorAttachPower"> Target anchor attach value after transitiong end </param>
        ///// <param name="targetMusclesPower"> Target muscles power value after transitiong end </param>
        ///// <param name="delay"> Delay to start transition </param>
        //public static void User_TransitionToStandingMode( this IRagdollAnimator2HandlerOwner iHandler, float transitionDuration, float blendToAnimatorFor = 0.6f, float animatorTransitionDelay = 0.1f, float targetMusclesPower = 1f, float delay = 0f )
        //{
        //    RagdollHandler handler = iHandler.GetRagdollHandler;
        //    if( handler.standUpCoroutine != null ) handler.Caller.StopCoroutine( handler.standUpCoroutine );
        //    handler.standUpCoroutine = handler.Caller.StartCoroutine( handler._IE_TransitionToStandingMode( transitionDuration, blendToAnimatorFor, animatorTransitionDelay, delay, targetMusclesPower ) );
        //}

        /// <summary> Few operations which are helpful for animating get up animation
        /// Excluding changing muscles power and hard matching in this overload method </summary>
        /// <param name="transitionDuration"> How much time animating values should take </param>
        /// <param name="targetAnchorAttachPower"> Target anchor attach value after transitiong end </param>
        /// <param name="delay"> Delay to start transition </param>
        public static void User_TransitionToStandingMode( this IRagdollAnimator2HandlerOwner iHandler, float transitionDuration = 0.8f, float delay = 0f )
        {
            RagdollHandler handler = iHandler.GetRagdollHandler;
            if( handler.AnimatingMode == RagdollHandler.EAnimatingMode.Standing ) return;

            if( handler.standUpCoroutine != null ) handler.Caller.StopCoroutine( handler.standUpCoroutine );
            handler.standUpCoroutine = handler.Caller.StartCoroutine( handler._IE_TransitionToStandingMode( transitionDuration, 0f, 0f, delay ) );
        }

        /// <summary> Few operations which are helpful for animating get up animation
        /// Excluding changing muscles power and hard matching in this overload method </summary>
        /// <param name="transitionDuration"> How much time animating values should take </param>
        /// <param name="targetAnchorAttachPower"> Target anchor attach value after transitiong end </param>
        /// <param name="targetMusclesPower"> Target muscles power value after transitiong end </param>
        /// <param name="delay"> Delay to start transition </param>
        //public static void User_TransitionToStandingMode( this IRagdollAnimator2HandlerOwner iHandler, float transitionDuration, float targetMusclesPower = 1f, float delay = 0f )
        //{
        //    RagdollHandler handler = iHandler.GetRagdollHandler;
        //    if( handler.standUpCoroutine != null ) handler.Caller.StopCoroutine( handler.standUpCoroutine );
        //    handler.standUpCoroutine = handler.Caller.StartCoroutine( handler._IE_TransitionToStandingMode( transitionDuration, 0f, 0f, delay, targetMusclesPower ) );
        //}
    }
}