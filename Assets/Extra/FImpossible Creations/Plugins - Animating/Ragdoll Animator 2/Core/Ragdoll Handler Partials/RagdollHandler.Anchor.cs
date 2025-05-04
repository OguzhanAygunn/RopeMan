using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {

        private void FixedUpdateAnchorBone()
        {
            var anchor = _playmodeAnchorBone;

            UpdateAnchorParent();

            RefreshAnchorKinematicState();

            if (AnimatingMode == EAnimatingMode.Standing)
            {
                if (!anchor.GameRigidbody.isKinematic) // Standing and non kinematic
                {
                    float anchorSpring = AnchorBoneSpring * AnchorBoneSpringMultiplier;

                    // Prevent unstuck teleporting character when starting get up transition
                    if (AutoUnstuck && anchorSpring > 0f && (Time.unscaledTime - LastStandingModeAtTime) > 0.1f)
                    {
                        float refScale = _playmodeAnchorBone.MainBoneCollider.bounds.size.magnitude * 1f;

                        if (Vector3.Distance(anchor.GameRigidbody.position, anchor.BoneProcessor.AnimatorPosition) > refScale)
                            ForcingKinematicAnchor = 2;
                    }

                    if (anchorSpring > 0f) // When want to move anchor towards desired position / rotation
                    {
                        // When hips is far away from target position, the power is lower - needs to pull back body rather than precisely match (avoid wobbling towards target position)
                        float power = Mathf.LerpUnclamped(0f, 1f, anchorSpring);
                        float invPow = 1f - power; invPow *= invPow; invPow = 1f - invPow;
                        float minMultiplier = invPow;

                        anchor.BoneProcessor.UpdateFixedPositionDelta();

                        // ::: Rotate Anchor :::
                        if (LockAnchorRotation)
                        {
                            // Since rigidbody.freezeRotation logic changed in Unity 2023 (without mentioning it in the release notes -_-) rotation needs to be calculated with slerp
                            anchor.GameRigidbody.rotation = Quaternion.Slerp(anchor.GameRigidbody.rotation, anchor.BoneProcessor.AnimatorRotation, Time.fixedDeltaTime * 60f);
                        }
                        else
                        {
                            RagdollHandlerUtilities.AddRigidbodyTorqueToRotateTowards(anchor.GameRigidbody, anchor.BoneProcessor.AnimatorRotation, /*( 0.5f + ( mass * connectedMass ) ) **/ minMultiplier);
                        }

                        // ::: Translate Anchor :::

                        #region Custom provided anchor velocity feature

                        if (_providedAnchorVelocity != null)
                        {
                            if (Vector3.Distance(anchor.PhysicalDummyBone.position, anchor.SourceBone.position) < anchor.BaseColliderSetup.CalculateSize().magnitude * 0.05f)
                            {
                                anchor.GameRigidbody.velocity = _providedAnchorVelocity.Value;
                                _providedAnchorVelocity = null;

                                return; // Don't apply rigidbody forces calculated below
                            }
                            else
                            {
                                _providedAnchorVelocity = null;
                            }
                        }

                        #endregion

                        // Anchor bone spring operation
                        RagdollHandlerUtilities.AddAccelerationTowardsWorldPosition(anchor.GameRigidbody, anchor.BoneProcessor.LastMatchingRigidodyOrigin, anchor.BoneProcessor.FixedPositionDelta, (power * power * power) * anchorBoneSpringPositionMultiplier, UnscaledTime ? Time.fixedUnscaledDeltaTime : Time.fixedDeltaTime);
                        //RagdollHandlerUtilities.AddRigidbodyForceToMoveTowards( anchor.GameRigidbody, anchor.BoneProcessor.LastMatchingRigidodyOrigin, power * anchorBoneSpringPositionMultiplier );

                    }

                    // else // Do not apply any force on zero spring, behave like falling

                }
                else // Standing kinematic
                {
                    anchor.BoneProcessor.AverageTranslationDataRequest(); // Remember velocity for maintaining it on fall
                    ApplyAnchorKinematicPosition();
                }
            }

        }


        /// <summary>
        /// Moving physical dummy immedietely towards source character placement.
        /// Can be used just before enabling back ragdoll dummy, since starting will falling mode will not move dummy object from the old placement.
        /// </summary>
        public void ForceSyncRoot(bool applyUsingAnchor = false)
        {
            if (applyUsingAnchor)
            {
                Vector3 p = GetAnchorBoneController.SourceBone.position;
                Quaternion r = _playmodeAnchorBone.SourceBone.rotation;

                if (UnaffectedMovement)
                {
                    _playmodeAnchorBone.PhysicalDummyBone.position = p;
                    _playmodeAnchorBone.PhysicalDummyBone.rotation = r;
                }
                else
                {
                    _playmodeAnchorBone.GameRigidbody.MovePosition(p);
                    _playmodeAnchorBone.GameRigidbody.MoveRotation(r);
                }
            }
            else
            {
                Vector3 p = GetAnchorBoneController.SourceBone.position - dummyContainer.position;
                Quaternion r = GetAnchorBoneController.SourceBone.rotation * Quaternion.Inverse(dummyContainer.rotation);
                dummyContainer.position += p;
                dummyContainer.rotation *= r;
            }
        }


        private bool afterForcing = false;

        private void RefreshAnchorKinematicState()
        {
            var anchor = _playmodeAnchorBone;
            bool wasKinematic = anchor.GameRigidbody.isKinematic;

            if (ForcingKinematicAnchor > 0)
            {
                ForcingKinematicAnchor -= 1;
                ChangeAnchorKinematicState(true);
                afterForcing = true;
                return;
            }

            if (AnimatingMode == EAnimatingMode.Standing)
            {
                if (AnchorBoneSpring * AnchorBoneSpringMultiplier >= 1.0f)
                {
                    if (MakeAnchorKinematicOnMaxSpring)
                    {
                        ChangeAnchorKinematicState(true);
                    }
                    else
                    {
                        ChangeAnchorKinematicState(false);
                    }
                }
                else
                {
                    ChangeAnchorKinematicState(false);
                }
            }
            else
            {
                if (animatingModeChanged || afterForcing)
                {
                    afterForcing = false;
                    ChangeAnchorKinematicState(false);
                }
            }

            if (animatingModeChanged)
            {
                if (IsFallingOrSleep && anchor.GameRigidbody.isKinematic != wasKinematic && anchor.GameRigidbody.isKinematic == false)
                {
                    // Maintaining kinematic rigidbody velocity
                    Vector3 velo = anchor.BoneProcessor.AverageTranslationDataRequestRaw() / Time.fixedDeltaTime;
                    anchor.GameRigidbody.velocity = velo;
                    if (Caller) Caller.StartCoroutine(_IE_CallForFixedFrames(() => { anchor.GameRigidbody.velocity = velo; }, 3));
                }
            }
        }

        private void ChangeAnchorKinematicState(bool isKinematic)
        {
            if (_playmodeAnchorBone.GameRigidbody.isKinematic == isKinematic) return;

            if (isKinematic) _playmodeAnchorBone.GameRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            _playmodeAnchorBone.GameRigidbody.isKinematic = isKinematic;

            // Check detached chains change parenting
            if (isKinematic)
            {
                foreach (var chain in chains)
                {
                    if (chain.Detach)
                    {
                        foreach (var bone in chain.BoneSetups)
                        {
                            bone.PhysicalDummyBone.SetParent(_playmodeAnchorBone.PhysicalDummyBone, true);
                        }
                    }
                }
            }
            else
            {
                foreach (var chain in chains)
                {
                    if (chain.Detach)
                    {
                        foreach (var bone in chain.BoneSetups)
                        {
                            bone.PhysicalDummyBone.SetParent(dummyContainer, true);
                        }
                    }
                }
            }
        }

        private void ApplyAnchorKinematicPosition()
        {
            Vector3 p = _playmodeAnchorBone.BoneProcessor.AnimatorPosition;
            Quaternion r = _playmodeAnchorBone.BoneProcessor.AnimatorRotation;

            //#if UNITY_2022_3_OR_NEWER
            //            _playmodeAnchorBone.PhysicalDummyBone.SetPositionAndRotation( p, r );
            //#else
            //            _playmodeAnchorBone.PhysicalDummyBone.position = p;
            //            _playmodeAnchorBone.PhysicalDummyBone.rotation = r;
            //#endif

            if (UnaffectedMovement)
            {
                _playmodeAnchorBone.PhysicalDummyBone.position = p;
                _playmodeAnchorBone.PhysicalDummyBone.rotation = r;
            }
            else
            {
                _playmodeAnchorBone.GameRigidbody.MovePosition(p);
                _playmodeAnchorBone.GameRigidbody.MoveRotation(r);
            }
        }

        private void AnchorJointRestoreRotationLock()
        {
            var anchor = _playmodeAnchorBone;

            if (anchor.Joint.angularXMotion != ConfigurableJointMotion.Free)
            {
                anchor.Joint.angularXMotion = ConfigurableJointMotion.Free;
                anchor.Joint.angularYMotion = ConfigurableJointMotion.Free;
                anchor.Joint.angularZMotion = ConfigurableJointMotion.Free;
            }

            if (anchor.Joint.connectedBody == AnchorParent) anchor.Joint.connectedBody = null;
        }

        /// <summary> Generated when using anchor joint limits, used for local rotation limits. </summary>
        public Rigidbody AnchorParent { get; private set; }

        /// <summary>
        /// Generating anchor parent if using pelvis limiting, for local space limits.
        /// Detaching anchor parent dynamically if disabling limits runtime.
        /// </summary>
        private void UpdateAnchorParent()
        {
            var anchor = _playmodeAnchorBone;

            if (IsFallingOrSleep == false)
            {
                if (LockAnchorRotation)
                {
                    if (anchor.GameRigidbody.freezeRotation == false) anchor.GameRigidbody.freezeRotation = true;
                }
                else if (AnchorJointLimits)
                {
                    if (AnchorParent == null) // Generate parent space limit rigidbody
                    {
                        GameObject generatedParent = new GameObject("Generated " + Dummy_Container.name + " Parent");
                        generatedParent.transform.SetParent(Dummy_Container, true);
                        ResetCoords(generatedParent.transform);
                        AnchorParent = generatedParent.AddComponent<Rigidbody>();
                        AnchorParent.isKinematic = true;
                        anchor.Joint.autoConfigureConnectedAnchor = false;
                    }

                    if (AnchorParent.interpolation != RigidbodiesInterpolation) AnchorParent.interpolation = RigidbodiesInterpolation;

                    if (AnchorParent.isKinematic)
                    {
                        AnchorParent.transform.position = anchor.BoneProcessor.AnimatorPosition;
                        AnchorParent.transform.rotation = anchor.BoneProcessor.AnimatorRotation;
                    }
                    else
                    {
#if UNITY_2023_1_OR_NEWER
                        AnchorParent.position = anchor.BoneProcessor.AnimatorPosition;
                        AnchorParent.rotation = anchor.BoneProcessor.AnimatorRotation;
                        AnchorParent.PublishTransform();
#else
                        AnchorParent.position = anchor.BoneProcessor.AnimatorPosition;
                        AnchorParent.rotation = anchor.BoneProcessor.AnimatorRotation;
#endif
                    }

                    if (anchor.Joint.angularXMotion != ConfigurableJointMotion.Limited)
                    {
                        anchor.Joint.angularXMotion = ConfigurableJointMotion.Limited;
                        anchor.Joint.angularYMotion = ConfigurableJointMotion.Limited;
                        anchor.Joint.angularZMotion = ConfigurableJointMotion.Limited;
                    }

                    if (anchor.Joint.connectedBody == null && (AnchorBoneSpring * AnchorBoneSpringMultiplier) >= 1f)
                    {
                        // When changing connected body, the rotation is used by joint as base rotation
                        anchor.PhysicalDummyBone.position = anchor.BoneProcessor.AnimatorPosition;
                        anchor.PhysicalDummyBone.rotation = anchor.BoneProcessor.AnimatorRotation;
                        anchor.Joint.connectedBody = AnchorParent;
                    }
                }

                if (!LockAnchorRotation) if (anchor.GameRigidbody.freezeRotation) anchor.GameRigidbody.freezeRotation = false;
            }
            else // On Falling mode restore anchor parent space limiting
            {
                if (AnchorParent) AnchorJointRestoreRotationLock();
                if (LockAnchorRotation) if (anchor.GameRigidbody.freezeRotation) anchor.GameRigidbody.freezeRotation = false;
            }
        }


        Vector3? _providedAnchorVelocity = null;

        /// <summary> 
        /// Velocity for the anchor bone, which can be used to make movement of character more smooth - dedicated only for character motion like jump
        /// By default, Ragdoll Animator is calculating target anchor velocity, basing on the captured animator position of the anchor bone, but it can desync with physics update loop.
        /// In such case you can provide velocity for the anchor bone, to make it more stable.
        /// </summary>
        public void User_ProvideAnchorVelocity(Vector3 velocity)
        {
            _providedAnchorVelocity = velocity;
        }


        Vector3 _motionInfluenceOffset;
        Vector3 _lastFixedPosition;
        void UpdateMotionInfluence()
        {
            if( IsInStandingMode == false ) // No motion influence on fall mode
            {
                _lastFixedPosition = _playmodeAnchorBone.BoneProcessor.AnimatorPosition;
                _motionInfluenceOffset = Vector3.zero;
                return;
            }

            if (MotionInfluence == 1f) { _motionInfluenceOffset = Vector3.zero; _lastFixedPosition = _playmodeAnchorBone.BoneProcessor.AnimatorPosition; return; }

            Vector3 offset = _motionInfluenceOffset * (1f - MotionInfluence);
            _motionInfluenceOffset = Vector3.zero;

            if (offset.sqrMagnitude < 0.00001f) return; // Optimize

            foreach (var chain in chains)
            {
                foreach (var proc in chain.RuntimeBoneProcessors)
                {
                    proc.rigidbody.transform.position += offset;
                    //proc.rigidbody.MovePosition(proc.rigidbody.position + offset);
                    proc.rigidbody.AddForce(offset, ForceMode.VelocityChange);
                }
            }

        }

    }
}