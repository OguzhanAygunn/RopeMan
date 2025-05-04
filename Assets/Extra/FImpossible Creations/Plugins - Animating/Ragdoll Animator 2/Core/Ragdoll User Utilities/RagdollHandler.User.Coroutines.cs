using System;
using System.Collections;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
        internal Coroutine _Coro_FadeMuscles = null;
        internal Coroutine _Coro_FadeMusclesMul = null;

        /// <summary>
        /// Adding physical push impact to single rigidbody limb
        /// </summary>
        /// <param name="limb"> Access 'Parameters' for ragdoll limb </param>
        /// <param name="powerDirection"> World space direction vector </param>
        /// <param name="duration"> Time in seconds </param>
        internal IEnumerator _IE_SetPhysicalImpact( Rigidbody limb, Vector3 powerDirection, float duration, ForceMode forceMode = ForceMode.Impulse, float delay = 0f, int waitFixedFrames = 0 )
        {
            float elapsed = -0.0001f;
            WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();

            if( waitFixedFrames > 0 )
            {
                int f = 0;
                while( f < waitFixedFrames )
                {
                    f += 1;
                    yield return fixedWait;
                }
            }

            if( delay > 0f ) yield return new WaitForSeconds( delay );

            powerDirection *= GetFixedDeltaMultiplicator(); // Unify impact when using different timestep

            while( elapsed < duration )
            {
                RagdollHandlerUtilities.ApplyLimbImpact( limb, powerDirection, forceMode );
                elapsed += Time.fixedDeltaTime;
                yield return fixedWait;
            }

            yield break;
        }

        /// <summary>
        /// Adding physical push impact to chain bones
        /// </summary>
        /// <param name="limb"> Access 'Parameters' for ragdoll limb </param>
        /// <param name="powerDirection"> World space direction vector </param>
        /// <param name="duration"> Time in seconds </param>
        internal IEnumerator _IE_SetChainPhysicalImpact( RagdollBonesChain chain, Vector3 powerDirection, float duration, ForceMode forceMode = ForceMode.Impulse )
        {
            float elapsed = -0.0001f;
            WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();

            powerDirection *= GetFixedDeltaMultiplicator(); // Unify impact when using different timestep

            while( elapsed < duration )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    bone.GameRigidbody.AddForce( powerDirection, forceMode );
                }

                elapsed += Time.fixedDeltaTime;

                yield return fixedWait;
            }

            yield break;
        }

        /// <summary>
        /// Lowering impact power is fixed timestep is set very low, increasing if exceeded default value
        /// </summary>
        public static float GetFixedDeltaMultiplicator()
        {
            return Time.fixedDeltaTime / 0.02f;
        }

        /// <summary>
        /// Adding physical push impact to all limbs of the ragdoll
        /// </summary>
        /// <param name="powerDirection"> World space direction vector </param>
        /// <param name="duration"> Time in seconds </param>
        internal IEnumerator _IE_SetPhysicalImpactAll( Vector3 powerDirection, float duration, ForceMode forceMode = ForceMode.Impulse )
        {
            float elapsed = -0.0001f;
            WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();

            powerDirection *= GetFixedDeltaMultiplicator(); // Unify impact when using different timestep

            while( elapsed < duration )
            {
                foreach( var chain in chains )
                {
                    foreach( var bone in chain.BoneSetups )
                    {
                        bone.GameRigidbody.AddForce( powerDirection, forceMode );
                    }
                }

                elapsed += Time.fixedDeltaTime;

                yield return fixedWait;
            }

            yield break;
        }

        /// <summary>
        /// Adding physical torque impact to the core limbs
        /// </summary>
        /// <param name="rotationPower"> Rotation angles torque power </param>
        /// <param name="duration"> Time in seconds </param>
        internal IEnumerator _IE_SetPhysicalTorque( Vector3 rotationPower, float duration, bool relativeSpace = true, ForceMode forceMode = ForceMode.Impulse )
        {
            float elapsed = -0.0001f;
            WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();

            rotationPower *= GetFixedDeltaMultiplicator(); // Unify impact when using different timestep

            while( elapsed < duration )
            {
                foreach( var chain in chains )
                {
                    foreach( var bone in chain.BoneSetups )
                    {
                        if( relativeSpace ) bone.GameRigidbody.AddRelativeTorque( rotationPower, forceMode );
                        else bone.GameRigidbody.AddTorque( rotationPower, forceMode );
                    }
                }

                elapsed += Time.fixedDeltaTime;

                yield return fixedWait;
            }

            yield break;
        }

        internal IEnumerator _IE_SetPhysicalTorque( Rigidbody limb, Vector3 rotationPower, float duration, bool relativeSpace = true, ForceMode forceMode = ForceMode.Impulse )
        {
            float elapsed = -0.0001f;
            WaitForFixedUpdate fixedWait = new WaitForFixedUpdate();

            rotationPower *= GetFixedDeltaMultiplicator(); // Unify impact when using different timestep

            while( elapsed < duration )
            {
                if( relativeSpace )
                    limb.AddRelativeTorque( rotationPower, forceMode );
                else
                    limb.AddTorque( rotationPower, forceMode );

                elapsed += Time.fixedDeltaTime;
                yield return fixedWait;
            }

            yield break;
        }

        internal IEnumerator _IE_FadeMusclesPower( float targetMusclesForce = 0f, float duration = 0.75f, float delay = 0f, bool disableMecanimAtEnd = false )
        {
            if( delay > 0f ) yield return new WaitForSeconds( delay );

            float startMusclesForce = MusclesPower;
            float elapsed = -0.0001f;

            while( elapsed < duration )
            {
                elapsed += delta;
                if( elapsed > duration ) elapsed = duration;
                MusclesPower = Mathf.LerpUnclamped( startMusclesForce, targetMusclesForce, elapsed / duration );
                User_UpdateJointsPlayParameters( false );
                yield return null;
            }

            MusclesPower = targetMusclesForce;

            if( disableMecanimAtEnd ) if( Mecanim ) Mecanim.enabled = false;

            yield break;
        }

        internal IEnumerator _IE_FadeMusclesPowerMultiplicator( float targetMusclesForce = 0f, float duration = 0.75f, float delay = 0f )
        {
            if( delay > 0f ) yield return new WaitForSeconds( delay );

            float startMusclesForce = musclesPowerMultiplier;
            float elapsed = -0.0001f;

            while( elapsed < duration )
            {
                elapsed += delta;
                if( elapsed > duration ) elapsed = duration;
                musclesPowerMultiplier = Mathf.LerpUnclamped( startMusclesForce, targetMusclesForce, elapsed / duration );
                User_UpdateJointsPlayParameters( false );
                yield return null;
            }

            musclesPowerMultiplier = targetMusclesForce;

            yield break;
        }


        internal Coroutine standUpCoroutine = null;
        public bool IsStandUpCoroutineRunning { get; private set; } = false;

        /// <summary> Helper flag, is true when calling Get Up from ragdolled but standing on both legs pose </summary>
        public bool GetUpCall_StandingRestore { get; private set; } = false;

        internal IEnumerator _IE_TransitionToStandingMode( float duration, float animatorFadeOffFor = 0.6f, float animatorTransitionDelay = 0.1f, float freezeSourceAnimatedHips = 0f, float delay = 0f, bool isOnLegsRestoreCall = false, float? targetMusclesPower = null, float? targetHardMatching = null )
        {
            IsStandUpCoroutineRunning = true;
            GetUpCall_StandingRestore = isOnLegsRestoreCall;

            if( delay > 0f )
            {
                yield return new WaitForSeconds( delay );
                //if( AnimatingMode != EAnimatingMode.Standing ) { yield break; } // Not standing anymore after delay
            }

            if( duration < 0f ) duration = 0f;

            AnchorBoneSpringMultiplier = 0f;
            AnimatingMode = EAnimatingMode.Standing;
            LastStandingModeAtTime = Time.unscaledTime;
            StandUpTransitionBlend = 1f;

            if( freezeSourceAnimatedHips > 0f && Caller != null )
            {
                Caller.StartCoroutine( IEFreezeAnchor( freezeSourceAnimatedHips ) );
            }

            yield return null;
            yield return new WaitForFixedUpdate();

            float startMusclesForce = MusclesPower;
            float startHardMatching = HardMatchingOnFalling;
            float elapsed = -0.0001f;

            while( elapsed < duration )
            {
                if( AnimatingMode != EAnimatingMode.Standing ) { StandUpTransitionBlend = 1f; IsStandUpCoroutineRunning = false; yield break; }

                elapsed += delta;
                float progress = elapsed / duration;
                if( progress > 1f ) break;

                if( animatorFadeOffFor > 0f )
                {
                    if( progress > animatorTransitionDelay ) // Start after a while to prevent body rotation towards animator state in stand pose (in case of animation transitioning)
                    {
                        if( progress < animatorFadeOffFor )
                            StandUpTransitionBlend = Mathf.MoveTowards( StandUpTransitionBlend, 0f, delta * ( 1f / ( duration * animatorFadeOffFor ) ) * 1.5f );
                        else
                            StandUpTransitionBlend = Mathf.MoveTowards( StandUpTransitionBlend, 1f, delta * ( 1f / ( duration * animatorFadeOffFor ) ) * 2f );
                    }
                }

                AnchorBoneSpringMultiplier = Mathf.LerpUnclamped( 0f, 1f, progress * progress );
                if( targetMusclesPower != null ) MusclesPower = Mathf.LerpUnclamped( startMusclesForce, targetMusclesPower.Value, progress );
                if( targetHardMatching != null ) HardMatching = Mathf.LerpUnclamped( startHardMatching, targetHardMatching.Value, progress );

                User_UpdateJointsPlayParameters( false );

                yield return null;
            }

            AnchorBoneSpringMultiplier = 1f;
            if( targetMusclesPower != null ) MusclesPower = targetMusclesPower.Value;
            if( targetHardMatching != null ) HardMatching = targetHardMatching.Value;

            if( animatorFadeOffFor > 0f )
            {
                while( StandUpTransitionBlend < 1f )
                {
                    StandUpTransitionBlend = Mathf.MoveTowards( StandUpTransitionBlend, 1f, delta * ( 1f / ( duration * animatorFadeOffFor ) ) * 2f );
                    yield return null;
                }
            }

            IsStandUpCoroutineRunning = false;
            yield break;
        }


        private Vector3 _hipsFreezeUpdatePosition;
        private Vector3 _hipsFreezeActivePosition;
        private Quaternion _hipsFreezeUpdateRotation;
        private Quaternion _hipsFreezeActiveRotation;
        private IEnumerator IEFreezeAnchor( float duration )
        {
            yield return null;

            var anchor = GetAnchorBoneController;
            _hipsFreezeUpdatePosition = anchor.SourceBone.position;
            _hipsFreezeUpdateRotation = anchor.SourceBone.rotation;

            Vector3 initFreezePos = _hipsFreezeUpdatePosition;
            Quaternion initFreezeRot = _hipsFreezeUpdateRotation;

            AddToPreLateUpdateLoop( HipsFreezeUpdate );

            float elapsed = 0f;
            while( elapsed < duration )
            {
                elapsed += Time.deltaTime;

                float progress = elapsed / duration;
                if( progress > 0.5f ) // smooth blend back
                {
                    progress = ( progress - 0.5f ) * 2f;

                    _hipsFreezeUpdatePosition = Vector3.Lerp( initFreezePos, _hipsFreezeActivePosition, progress );
                    _hipsFreezeUpdateRotation = Quaternion.Slerp( initFreezeRot, _hipsFreezeActiveRotation, progress );
                }

                yield return null;
            }

            RemoveFromPreLateUpdateLoop( HipsFreezeUpdate );

            yield break;
        }

        private void HipsFreezeUpdate()
        {
            var anchor = GetAnchorBoneController;

            _hipsFreezeActivePosition = anchor.SourceBone.position;
            _hipsFreezeActiveRotation = anchor.SourceBone.rotation;

#if UNITY_2022_3_OR_NEWER
            anchor.SourceBone.SetPositionAndRotation( _hipsFreezeUpdatePosition, _hipsFreezeUpdateRotation );
#else
            anchor.SourceBone.position = _hipsFreezeUpdatePosition;
            anchor.SourceBone.rotation = _hipsFreezeUpdateRotation;
#endif
        }

        /// <summary> Can be used to force blending legs on beginning of the stand up animations when using blend on collision feature </summary>
        [NonSerialized] public bool LegsBlendInRequest = false;

        internal Coroutine _coro_legsBlendRequest = null;

        public void RequestLegsBlendFor( float duration )
        {
            if( _coro_legsBlendRequest != null ) Caller.StopCoroutine( _coro_legsBlendRequest );
            _coro_legsBlendRequest = Caller.StartCoroutine( IERequestLegsBlend( duration ) );
        }

        private IEnumerator IERequestLegsBlend( float duration )
        {
            LegsBlendInRequest = true;

            float elapsed = 0f;
            while( elapsed < duration )
            {
                elapsed += delta;
                yield return null;
            }

            LegsBlendInRequest = false;

            yield break;
        }

        // Utils ------------------------------------------------------------------------------------------

        internal IEnumerator _IE_CallAfter( float delay, System.Action act, int waitExtraFixedSteps = 0 )
        {
            if( waitExtraFixedSteps > 0 ) for( int i = 0; i < waitExtraFixedSteps; i++ ) yield return new WaitForFixedUpdate();
            if( act == null ) yield break;
            if( delay > 0 ) yield return new WaitForSeconds( delay );
            act.Invoke();
            yield break;
        }

        internal IEnumerator _IE_CallForFixedFrames( System.Action act, int framesToCall = 0 )
        {
            if( act == null ) yield break;

            for( int i = 0; i < framesToCall; i++ )
            {
                act.Invoke();
                yield return new WaitForFixedUpdate();
            }

            yield break;
        }

        internal IEnumerator _IE_RefreshBonesAfterTeleport(int frames)
        {
            int c = 0;

            while( c < frames )
            {
                GetAnchorBoneController.BoneProcessor.ResetPoseParameters();
                this.User_ForceMatchPhysicalBonesWithAnimator( true ); // Restore for teleport body parts

                CallOnAllRagdollBones( ( RagdollChainBone b ) => 
                {
                    if( b.GameRigidbody.isKinematic == false )
                    {
                        b.GameRigidbody.velocity = Vector3.zero;
                        b.GameRigidbody.angularVelocity = Vector3.zero;
                    }
                } );

                c += 1;
                yield return null;
            }
        }

        internal IEnumerator _IE_RefreshBonesAfterTeleportFixed( int frames )
        {
            int c = 0;

            while( c < frames )
            {
                GetAnchorBoneController.BoneProcessor.ResetPoseParameters();
                this.User_ForceMatchPhysicalBonesWithAnimator( true ); // Restore for teleport body parts
                CallOnAllRagdollBones( ( RagdollChainBone b ) =>
                {
                    if( b.GameRigidbody.isKinematic == false )
                    {
                        b.GameRigidbody.velocity = Vector3.zero;
                        b.GameRigidbody.angularVelocity = Vector3.zero;
                    }
                } );

                c += 1;
                yield return new WaitForFixedUpdate();
            }
        }

    }
}