using System;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
        /// <summary> Forcing ragdoll animator to perform T-Pose and wait for unity joints initialization call </summary>
        public void ForceFixedReinitialization()
        {
            ForcingKinematicAnchor = 2;
            fixedInitialized = false;
            fixedFramesElapsed = 0;
        }

        private bool fixedInitialized = false;

        /// <summary> Used by extension methods </summary>
        internal bool disableUpdating = false;

        private bool _wasDisableUpdating = false;
        private EAnimatingMode _lastAnimatingMode = (EAnimatingMode)(-1);

        /// <summary> Delta time lately used by the Ragdoll Animator </summary>
        public float Delta
        { get { return delta; } }

        private float delta = 0.001f;

        private float finalBlend = 1f;

        [Tooltip("Using gravity for the anchor bone during free fall")]
        [NonSerialized] public bool AnchorUseGravity = true;

        private void CheckIfShouldBeUpdated()
        {
            if (RagdollLogic == ERagdollLogic.JustBoneComponents)
            {
                disableUpdating = true;
                _lastAnimatingMode = animatingMode;
                return;
            }

            disableUpdating = false;

            if (LODBlend <= 0f)
            {
                disableUpdating = true;
                _wasDisableUpdating = true;
                _lastAnimatingMode = animatingMode;
                return;
            }

            if (OptimizeOnZeroBlend)
                if (RagdollBlend < 0.000001f)
                {
                    disableUpdating = true;
                    _wasDisableUpdating = true;
                    _lastAnimatingMode = animatingMode;
                    return;
                }

            if (AnimatingMode == EAnimatingMode.Off)
            {
                disableUpdating = true;
                _wasDisableUpdating = true;
                _lastAnimatingMode = animatingMode;
                return;
            }
            else if (AnimatingMode == EAnimatingMode.Sleep)
            {
                SleepModeUpdate();
            }

            if (_wasDisableUpdating != disableUpdating) if (!disableUpdating)
                {
                    // On Enable back
                    if (_wasSleepDisable == false) ResetFadeInBlend();

                    if (IsInFallingMode == false || (_lastAnimatingMode == EAnimatingMode.Off && animatingMode != EAnimatingMode.Off))
                    //if (animatingMode != EAnimatingMode.Sleep)
                    {
                        this.User_ForceMatchPhysicalBonesWithAnimator(true);
                        Caller?.StartCoroutine(_IE_CallForFixedFrames(() => { GetAnchorBoneController.BoneProcessor.ResetPoseParameters(); }, 3));
                        this.User_WarpRefresh();
                    }

                    foreach (var chain in chains)
                        foreach (var bone in chain.BoneSetups)
                            bone.BoneProcessor.ResetPoseParameters();

                    animatingModeChanged = true;
                    _wasSleepDisable = false;
                    _wasDisableUpdating = false;
                }

            _lastAnimatingMode = animatingMode;
        }

        private void CalculateRagdollBlend()
        {
            finalBlend = GetTotalBlend();

            if (User_OverrideMusclesPower != null) targetMusclesPower = User_OverrideMusclesPower.Value;
            else targetMusclesPower = MusclesPower * musclesPowerMultiplier;
        }

        public void OnEnable()
        {
            if (!WasInitialized) return;

            if (RagdollLogic == ERagdollLogic.JustBoneComponents)
            {
                SwitchDummyPhysics(true);
                return;
            }

            if (dummyContainer == null) return;

            ResetFadeInBlend();
            GetAnchorBoneController.BoneProcessor.ResetPoseParameters();
            ForcingKinematicAnchor = 2;
            GetAnchorBoneController.GameRigidbody.isKinematic = true;
            //CallOnAllRagdollBones( ( RagdollChainBone b ) => { b.GameRigidbody.MovePosition(b.SourceBone.position); b.GameRigidbody.MoveRotation(b.SourceBone.rotation); } );
            
            if (dummyContainer.gameObject.activeInHierarchy == false) // In case when object was disabled, it needs full reset
            {
                if (UseReconstruction) ApplyTPoseOnModel(true);

                fixedFramesElapsed = 0;
                fixedInitialized = false;

                dummyContainer.gameObject.SetActive(true);
            }

            if( !disableUpdating) SwitchDummyPhysics(true);

            if (animatingMode != EAnimatingMode.Standing) animatingModeChanged = true;

            CallExtraFeaturesOnEnable();

            this.User_WarpRefresh();
        }

        public void OnDisable()
        {
            if (!WasInitialized) return;

            if (RagdollLogic == ERagdollLogic.JustBoneComponents)
            {
                SwitchDummyPhysics(false);
                return;
            }

            if (dummyContainer == null) return;

            //dummyContainer.gameObject.SetActive(false);
            SwitchDummyPhysics(false);
            CallExtraFeaturesOnDisable();
        }

        public void OnCreatorDestroy()
        {
            if (!WasInitialized) return;
            if (!DummyWasGenerated) return;
            GameObject.Destroy(Dummy_Container.gameObject);
        }

        public void UpdateTick()
        {
            CheckIfShouldBeUpdated();

            _wasDisableUpdating = disableUpdating;

            UpdateAnimatePhysicsVariable();

            CallExtraFeaturesAlwaysUpdateLoops();

            if (disableUpdating) return;

            CallExtraFeaturesUpdateLoops(); // Extra Features -----

            if (animatePhysics) return; // Rest of the calculations are update loop relative

            delta = UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            CalculateRagdollBlend();

            if (!fixedInitialized) return;
            if (Calibrate) PreCalibrate();
        }

        public void LateUpdateTick()
        {
            if (disableUpdating) return;

            #region Animator update physics support

            if (animatePhysics)
            {
                if (scheduledFixedUpdate == false) return;
                scheduledFixedUpdate = false;
            }

            #endregion Animator update physics support

            if (!fixedInitialized) return;

            CallExtraFeaturesPreLateUpdateLoops(); // Extra Features -----

            foreach (var fillBone in skeletonFillExtraBonesList) fillBone.CaptureAnimator(); // Used for reconstruction mode
            foreach (var chain in chains) chain.CaptureAnimator();

            CallExtraFeaturesLateUpdateLoops(); // Extra Features -----

            CalculateFadeIn();

            ApplyAnchorBonePositionAfterAnimationCapture();

            foreach (var chain in chains) chain.ApplyPhysicalRotationsToTheSkeleton(finalBlend);

            if (ApplyPositions)
            {
                // for chain[0] -> i = 1 -> Skip pelvis
                for (int i = 1; i < chains[0].RuntimeBoneProcessors.Count; i++) chains[0].RuntimeBoneProcessors[i].ApplyPhysicalPositionToTheBone(finalBlend);
                // skip chain[0]
                for (int i = 1; i < chains.Count; i++) chains[i].ApplyPhysicalPositionToTheSkeleton(finalBlend);
            }

            UpdateAttachables();

            CallExtraFeaturesPostLateUpdateLoops(); // Extra Features -----

            _motionInfluenceOffset += GetAnchorBoneController.BoneProcessor.AnimatorPosition - _lastFixedPosition;
            _lastFixedPosition = GetAnchorBoneController.BoneProcessor.AnimatorPosition;
        }

        public void FixedUpdateTick()
        {
            if (RagdollLogic == ERagdollLogic.JustBoneComponents) return;

            SwitchDummyPhysics(!disableUpdating);

            if (disableUpdating) return;

            #region Fixed update initialization check

            // Ensuring correct initialization for physical components like unity joints - one tick is not enough for unknown reason -_-

            if (fixedFramesElapsed < 2)
            {
                if (!WaitForInit)
                {
                    fixedFramesElapsed = 3;
                    this.User_ForceMatchPhysicalBonesWithAnimator(true);
                    fixedInitialized = true;
                }
                else
                {
                    ForcingKinematicAnchor = 2;
                    fixedInitialized = false;
                    fixedFramesElapsed += 1;
                    ApplyTPoseOnModel(true);
                    return;
                }
            }
            else
            {
                if (!fixedInitialized)
                {
                    fixedInitialized = true;
                    this.User_ForceMatchPhysicalBonesWithAnimator(true);
                    ForcingKinematicAnchor = 0;
                }
            }

            #endregion


            scheduledFixedUpdate = true; // Animate physics call flag

            if (animatePhysics)
            {
                //foreach( var chain in chains ) chain.CaptureAnimationVelocity();
                if (Calibrate) { PreCalibrate(); }

                delta = UnscaledTime ? Time.fixedUnscaledDeltaTime : Time.fixedDeltaTime;

                CalculateRagdollBlend();
            }

            CallExtraFeaturesFixedUpdateLoops(); // Extra Features -----

            // Main physics tick operations
            FixedUpdateAnchorBone();

            foreach (var fillBone in skeletonFillExtraBonesList) fillBone.SyncWithAnimator();

            UpdatePhysicalAnimationMatching();

            UpdateMotionInfluence();

            FixedUpdateAttachables();
        }

        private void ApplyAnchorBonePositionAfterAnimationCapture()
        {
            float anchorBlend = finalBlend * _playmodeAnchorBone.BoneBlendMultiplier;
            if (anchorBlend <= 0f) return;

            if (_playmodeAnchorBone.GameRigidbody.isKinematic == false)
            {
                Vector3 newPosition = _playmodeAnchorBone.PhysicalDummyBone.position;
                Quaternion newRotation = _playmodeAnchorBone.PhysicalDummyBone.rotation;

                if (anchorBlend >= 1f)
                {
                    // Placing animator anchor bone in physical position and rotation
                    _playmodeAnchorBone.SourceBone.position = newPosition;
                    _playmodeAnchorBone.SourceBone.rotation = newRotation;
                }
                else // or with blending
                {
                    newPosition = Vector3.LerpUnclamped(_playmodeAnchorBone.SourceBone.position, newPosition, anchorBlend);
                    _playmodeAnchorBone.SourceBone.position = newPosition;

                    newRotation = Quaternion.SlerpUnclamped(_playmodeAnchorBone.SourceBone.rotation, newRotation, anchorBlend);
                    _playmodeAnchorBone.SourceBone.rotation = newRotation;
                }
            }
            else
            {
                if (AnimatingMode == EAnimatingMode.Standing)
                {
                    _playmodeAnchorBone.SourceBone.position = _playmodeAnchorBone.PhysicalDummyBone.position;
                    _playmodeAnchorBone.SourceBone.rotation = _playmodeAnchorBone.PhysicalDummyBone.rotation;
                    // -> if kinematic true -> not needed because it redirects bone to its animator position in full precision
                }
                else // In case of using kinematic bone during falling mode
                {
                    Vector3 newPosition = Vector3.LerpUnclamped(_playmodeAnchorBone.SourceBone.position, _playmodeAnchorBone.PhysicalDummyBone.position, anchorBlend);
                    _playmodeAnchorBone.SourceBone.position = newPosition;

                    Quaternion newRotation = Quaternion.SlerpUnclamped(_playmodeAnchorBone.SourceBone.rotation, _playmodeAnchorBone.PhysicalDummyBone.rotation, anchorBlend);
                    _playmodeAnchorBone.SourceBone.rotation = newRotation;
                }
            }
        }

        //EAnimatingMode _lastAnimatingState = (EAnimatingMode)( -1 );
        private EAnimatingMode _lastActionAnimatingState = (EAnimatingMode)(-1);

        /// <summary>
        /// Called during fixed update, before anchor coords apply
        /// </summary>
        private void OnAnimatingModeChange()
        {
            if (AnimatingMode == EAnimatingMode.Standing)
            {
                //RestoreCalibrationPose();
                LastStandingModeAtTime = Time.unscaledTime;
                if (UseReconstruction) GenerateInBetweenBonesPhysics();
                if (PhysicMaterialOnFall && CollidersPhysicMaterial) this.User_ChangeAllCollidersPhysicMaterial(CollidersPhysicMaterial);
            }
            else
            {
                if (UseReconstruction) DiscardInBetweenBonesPhysics();
                if (PhysicMaterialOnFall && CollidersPhysicMaterial) this.User_ChangeAllCollidersPhysicMaterial(PhysicMaterialOnFall);
                if (GetAnchorBoneController.GameRigidbody.useGravity != AnchorUseGravity) GetAnchorBoneController.GameRigidbody.useGravity = AnchorUseGravity;
            }

            ResetSleepMode();

            CallOnFallModeSwitchActions();
            _lastActionAnimatingState = AnimatingMode;

            RefreshAllChainsDynamicParameters();
            User_UpdateJointsPlayParameters(false);

            RefreshAnchorKinematicState();

            animatingModeChanged = false;
        }

        private void UpdatePhysicalAnimationMatching()
        {
            foreach (var chain in chains)
            {
                foreach (var bone in chain.RuntimeBoneProcessors)
                {
                    bone.AnimationJointMatchingUpdate(chain);
                }

                #region Tensors support

                bool applyTensors = false;

                if (chain.AlternativeTensors)
                {
                    applyTensors = true;
                    if (IsInFallingMode) if (chain.AlternativeTensorsOnFall == false) applyTensors = false;
                }

                if (applyTensors)
                {
                    chain.tensorsSwitched = true;
                    foreach (var bone in chain.RuntimeBoneProcessors) bone.ApplyAlternativeTensor();
                }
                else
                {
                    if (chain.tensorsSwitched)
                    {
                        foreach (var bone in chain.RuntimeBoneProcessors) bone.rigidbody.ResetInertiaTensor();
                        chain.tensorsSwitched = false;
                    }
                }

                #endregion
            }

            if (HardMatching <= 0f || disableHardMatching) return; // No hard matching - dont compute

            if (AnimatingMode == EAnimatingMode.Standing)
            {
                foreach (var chain in chains)
                    foreach (var bone in chain.RuntimeBoneProcessors)
                    {
                        bone.StoreHardMatchFactor(chain, HardMatching, 1f);
                        bone.AnimationRotationHardMatchingStandUpdate(bone.storedHardMatch);
                    }

                if (HardMatchPositions)
                {
                    float power = Mathf.InverseLerp(0.2f, 1f, (Time.unscaledTime - LastStandingModeAtTime)) * (0.7f * PositionHardMatchingMultiplier); // Don't apply position matching on get up

                    if (power > 0f)
                        foreach (var chain in chains)
                            foreach (var bone in chain.RuntimeBoneProcessors)
                                bone.HardMatchBonePosition(bone.storedHardMatch * power);
                }
            }
            else
            {
                if (HardMatchingOnFalling <= 0f) return;

                foreach (var chain in chains)
                {
                    foreach (var bone in chain.RuntimeBoneProcessors)
                    {
                        bone.StoreHardMatchFactor(chain, HardMatchingOnFalling, targetMusclesPower);
                        bone.AnimationRotationHardMatchingFallUpdate(bone.storedHardMatch);
                    }
                }

                if (HardMatchPositions && HardMatchPositionsOnFall)
                {
                    foreach (var chain in chains)
                        foreach (var bone in chain.RuntimeBoneProcessors)
                            bone.HardMatchBonePosition(bone.storedHardMatch * PositionHardMatchingMultiplier);
                }
            }
        }

        /// <summary> Updating animation matching joints spring values after manual changes </summary>
        public void User_UpdateJointsPlayParameters(bool reset)
        {
            float damping;
            float spring;

            if (AnimatingMode == EAnimatingMode.Standing)
            {
                damping = DampingValue;
                spring = GetCurrentMainSpringsValue;
            }
            else // Fall mode
            {
                damping = DampingValueOnFall;
                spring = OverrideSpringsValueOnFall == null ? GetCurrentMainSpringsValue : OverrideSpringsValueOnFall.Value;
            }

            float power = targetMusclesPower * targetMusclesPower;

            foreach (var chain in chains)
            {
                foreach (var bone in chain.BoneSetups)
                {
                    if (bone.IsAnchor) continue;
                    bone.SetJointMatchingParameters(power * spring * chain.MusclesForce * bone.ForceMultiplier + bone.MusclesBoost * spring * targetMusclesPower, damping * targetMusclesPower);
                }

                if (WasInitialized)
                    if (chain.AlternativeTensors == false)
                        foreach (var bone in chain.BoneSetups)
                            bone.GameRigidbody.ResetInertiaTensor();
            }
        }

        internal bool UnlimitedRotationOnStandingModeCheck()
        {
            if (AnimationMatchLimits == ERagdollNoLimitAngles.AllLimits) return false;
            if (AnimationMatchLimits == ERagdollNoLimitAngles.NoLimits) return true;
            if (AnimatingMode != EAnimatingMode.Standing) return false;
            if (AnimationMatchLimits == ERagdollNoLimitAngles.NoLimitsOnStandingMode) return true;
            return false;
        }

        internal void OnCollisionEnterEvent(RA2BoneCollisionHandler hitted, Collision coll)
        {
            foreach (var item in OnCollisionEnterActions)
            {
                item.Invoke(hitted, coll);
            }
        }

        internal void OnTriggerEnterEvent(RA2BoneTriggerCollisionHandler hitted, Collider coll)
        {
            foreach (var item in OnTriggerEnterActions)
            {
                item.Invoke(hitted, coll);
            }
        }

        private float _sleepDuration = 0f;
        private float _sleepStableTime = 0f;
        private bool _wasSleepDisable = false;
        private void ResetSleepMode()
        {
            _sleepDuration = 0f;
            _sleepStableTime = 0f;
        }

        private void SleepModeUpdate()
        {
            _sleepDuration += delta;

            if (_sleepDuration < 2f) return; // Minimum 1 second of falling

            float averageTranslation = this.User_GetChainBonesAverageTranslation(ERagdollChainType.Core).magnitude;

            float thresholdUp = 1f + _sleepDuration * 0.003f;

            if (averageTranslation > 0.03f * thresholdUp) { _sleepStableTime = 0f; return; }

            // The velocity of core bones are in move, so not ready for getup
            if (this.User_GetChainAngularVelocity(ERagdollChainType.Core).magnitude > (0.5f * thresholdUp) * this.User_CoreLowTranslationFactor(averageTranslation))
            { _sleepStableTime = 0f; return; }

            _sleepStableTime += delta;
            if (_sleepStableTime < 1f * Mathf.Max(0.0001f, 1f - _sleepDuration * 0.0005f)) return; // Let's be in static pose for a small amount of time

            if (DisableMecanimOnSleep) if (Mecanim) Mecanim.enabled = false;
            AnimatingMode = EAnimatingMode.Off;
            _wasSleepDisable = true;
        }
    }
}