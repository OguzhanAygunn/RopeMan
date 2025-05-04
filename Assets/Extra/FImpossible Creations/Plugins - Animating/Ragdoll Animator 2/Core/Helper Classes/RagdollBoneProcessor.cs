using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    /// <summary>
    /// Class which is processing runtime calculations for the physical bone
    /// </summary>
    public class RagdollBoneProcessor
    {
        public RagdollChainBone BoneSetup { get; private set; }

        private ConfigurableJoint joint;
        private Transform stransform;//{ get { return Settings.SourceBone; } }
        private Transform dtransform
        { get { return BoneSetup.PhysicalDummyBone; } }
        public Rigidbody rigidbody;//=> Settings.GameRigidbody;

        internal Vector3 initLocalPos = Vector3.zero;
        internal Quaternion initLocalRot = Quaternion.identity;
        internal Quaternion calibrationLocalRotation = Quaternion.identity;


        private Quaternion jointAxisConversion;
        private Quaternion initialAxisCorrection;

        /// <summary> Calculated only when using ApplyPositions </summary>
        public Vector3 lastAppliedPosition { get; private set; }

        public Quaternion animatorLocalRotation { get; private set; }
        private Quaternion animatorRotation;
        public Quaternion AnimatorRotation
        { get { return animatorRotation; } }
        private Vector3 animatorPosition;
        public Vector3 AnimatorPosition
        { get { return animatorPosition; } }
        public Vector3 LastMatchingRigidodyOrigin { get; private set; }
        public Vector3 updateLoopRelevantVelocity { get; private set; }
        float lastCaptureTime = -1f;

        public Vector3 PreviousFixedPosition { get; private set; }
        public Vector3 FixedPositionDelta { get; private set; }


        public RagdollAnimator2BoneIndicator IndicatorComponent = null;

        public RagdollBoneProcessor( RagdollChainBone settings ) : this( settings.Joint, settings.SourceBone, settings.GameRigidbody )
        {
            BoneSetup = settings;
        }

        public RagdollBoneProcessor( ConfigurableJoint configurableJoint, Transform sourceTransform, Rigidbody rig )
        {
            joint = configurableJoint;
            stransform = sourceTransform;
            rigidbody = rig;

            initLocalPos = stransform.localPosition;
            initLocalRot = stransform.localRotation;

            calibrationLocalRotation = initLocalRot;

            ResetPoseParameters();

            InitWithJoint();
        }

        public void ResetPoseParameters()
        {
            animatorLocalRotation = stransform.localRotation;
            calibrationLocalRotation = stransform.localRotation;

            animatorPosition = stransform.position;
            animatorRotation = stransform.rotation;

            updateLoopRelevantVelocity = Vector3.zero;
            LastMatchingRigidodyOrigin = stransform.position + stransform.rotation * rigidbody.centerOfMass;

            PreviousFixedPosition = stransform.position;

            FixedPositionDelta = Vector3.zero;
            averageTranslation = Vector3.zero;

            _lastFixedFramePosition = stransform.position;
            lastAppliedPosition = _lastFixedFramePosition;

            averageAngularity = 0f;
            _lastFixedFrameRotation = stransform.rotation;
        }

        /// <summary> Configure joint rotation axis for animation matching coords conversion </summary>
        private void InitWithJoint()
        {
            Vector3 forward = Vector3.Cross( joint.axis, joint.secondaryAxis ).normalized;
            Vector3 up = Vector3.Cross( forward, joint.axis ).normalized;

            if( forward == up )
            {
                jointAxisConversion = Quaternion.identity;
                initialAxisCorrection = initLocalRot;
            }
            else
            {
                Quaternion toJointSpace = Quaternion.LookRotation( forward, up );
                jointAxisConversion = Quaternion.Inverse( toJointSpace );
                initialAxisCorrection = initLocalRot * toJointSpace;
            }
        }


        /// <summary>
        /// Capture current frame animator pose to know what to match with physics
        /// </summary>
        public void CaptureAnimatorPose()
        {
            animatorLocalRotation = stransform.localRotation;

#if UNITY_2022_1_OR_NEWER

            stransform.GetPositionAndRotation( out animatorPosition, out animatorRotation );

#else

            animatorPosition = stransform.position;
            animatorRotation = stransform.rotation;

#endif

            CaptureAnimationVelocity();

        }

        /// <summary> 
        /// Compute animation based velocity.
        /// Capturing animator position still can be choppy with low FPS and VSync on
        /// Update loop delta velocity will never match real fixed velocity
        /// </summary>
        void CaptureAnimationVelocity()
        {
            float elapsed = Time.unscaledTime - lastCaptureTime; lastCaptureTime = Time.unscaledTime; // Capture time for delta time calculation
            if( elapsed <= 0 ) return;

            Vector3 sourcePoseMatchingOrigin = animatorPosition + animatorRotation * rigidbody.centerOfMass;
            updateLoopRelevantVelocity = ( sourcePoseMatchingOrigin - LastMatchingRigidodyOrigin ) / elapsed;
            LastMatchingRigidodyOrigin = sourcePoseMatchingOrigin;
        }

        /// <summary> Prepare bone rotation for procedural animations </summary>
        public void CalibrateRotation()
        {
            stransform.localRotation = calibrationLocalRotation;
        }

        /// <summary> Prepare both rotation and position </summary>
        public void Calibrate()
        {
            CalibrateRotation();
            stransform.localPosition = initLocalPos;
        }

        /// <summary> Store reference pose for no-keyframe animated bones </summary>
        public void StoreCalibrationPose()
        {
            calibrationLocalRotation = animatorLocalRotation;
        }

        /// <summary> Restore default reference pose for no-keyframe animated bones </summary>
        public void RestoreCalibrationPose()
        {
            calibrationLocalRotation = initLocalRot;
        }


        /// <summary> Applied during standing mode - not during fall </summary>
        public void SyncKinematicRigidbodyWithAnimatorPose()
        {
            if( BoneSetup.BypassKinematicControl ) return;

            animatorRotation = animatorRotation.normalized;

//#if UNITY_2022_1_OR_NEWER

//            dtransform.SetPositionAndRotation( animatorPosition, animatorRotation );

//#else

//            dtransform.position = animatorPosition;
//            dtransform.rotation = animatorRotation;

//#endif

            rigidbody.MovePosition( animatorPosition );
            rigidbody.MoveRotation( animatorRotation );

            AverageTranslationDataRequest();
        }

        /// <summary> Position memory for physics loop update </summary>
        public void UpdateFixedPositionDelta()
        {
            if( rigidbody.position == PreviousFixedPosition ) return;

            Vector3 delta = rigidbody.position - PreviousFixedPosition;
            FixedPositionDelta = delta;
            PreviousFixedPosition = rigidbody.position;
        }

        internal void AnimationJointMatchingUpdate( RagdollBonesChain chain )
        {
            if( rigidbody.isKinematic )
            {
                if( chain.ParentHandler.AnimatingMode == RagdollHandler.EAnimatingMode.Standing )
                    SyncKinematicRigidbodyWithAnimatorPose();

                return;
            }

            ApplyJointRotation();
        }

        /// <summary> Calculate alternative bone's rigidbody interia tensor </summary>
        internal void ApplyAlternativeTensor()
        {
            RagdollHandlerUtilities.CalculateInertiaTensor( rigidbody );
        }

        /// <summary> Calculate and apply rotation for the bone's joint </summary>
        public void ApplyJointRotation()
        {
            if( joint == null ) return;
            Quaternion targetRotation = jointAxisConversion * Quaternion.Inverse( animatorLocalRotation ) * initialAxisCorrection;
            joint.targetRotation = targetRotation;
        }


        /// <summary>
        /// Proceed physical coords for the source skeleton bone
        /// </summary>
        internal void ApplyLocalRotationToAnimatorBone( Quaternion localRotation, float blend )
        {
            float targetBlend = BoneSetup.OverrideBlend;
            if( targetBlend == 0f ) targetBlend = blend * BoneSetup.BoneBlendMultiplier;
            ApplyLocalRotationToAnimatorBoneFinal( localRotation, targetBlend );
        }

        /// <summary>
        /// Finally applying just physical rotation for the source skeleton bone in local space
        /// </summary>
        public void ApplyLocalRotationToAnimatorBoneFinal( Quaternion localRotation, float blend )
        {
            if( blend >= 1f )
            {
                stransform.localRotation = localRotation;
            }
            else if( blend <= 0f ) return;
            else
            {
                stransform.localRotation = Quaternion.LerpUnclamped( stransform.localRotation, localRotation, blend );
            }
        }

        internal void ApplyPhysicalRotationToTheBone( float blend )
        {
            Quaternion targetRotation = FEngineering.QToLocal( stransform.parent.rotation, dtransform.rotation );
            ApplyLocalRotationToAnimatorBone( targetRotation, blend );
        }


        internal void ApplyPositionToAnimatorBone( Vector3 localPosition, float blend )
        {
            float targetBlend = BoneSetup.OverrideBlend;
            if( targetBlend == 0f ) targetBlend = blend * BoneSetup.BoneBlendMultiplier;

            if( targetBlend >= 1f )
            {
                stransform.localPosition = localPosition;
            }
            else if( targetBlend <= 0f ) return;
            else
            {
                stransform.localPosition = Vector3.LerpUnclamped( stransform.localPosition, localPosition, targetBlend );
            }

            lastAppliedPosition = stransform.position;
        }

        internal void ApplyPhysicalPositionToTheBone( float blend )
        {
            Vector3 targetPosition = BoneSetup.DetachParent.InverseTransformPoint( dtransform.position );
            ApplyPositionToAnimatorBone( targetPosition, blend );
        }

        public void HardMatchBonePosition( float power )
        {
            Vector3 posDiff = LastMatchingRigidodyOrigin - rigidbody.worldCenterOfMass;
            float accelPower = power;
            accelPower *= 1f / ( ( posDiff.sqrMagnitude * ( 15f + ( 1f - power ) * 65f ) ) + 1f ); // Lower power if far away from target pose
            RagdollHandlerUtilities.AddAccelerationTowardsWorldPositionDiff( rigidbody, posDiff, FixedPositionDelta, accelPower, Time.fixedDeltaTime, power );
        }

        public float storedHardMatch { get; private set; }
        internal void StoreHardMatchFactor( RagdollBonesChain chain, float hardMatchMultiplier = 0f, float overallMultiplier = 1f )
        {
            UpdateFixedPositionDelta(); // For position hard matching
            storedHardMatch = CalculateHardMatchFactor( chain, hardMatchMultiplier, overallMultiplier );
        }

        /// <summary> Calculating hard matching multiplier just once in frame </summary>
        private float CalculateHardMatchFactor( RagdollBonesChain chain, float hardMatchMultiplier = 0f, float overallMultiplier = 1f )
        {
            float hardMatch = BoneSetup.HardMatchOverride;

            if( hardMatch == 0f )
            {
                hardMatch = hardMatchMultiplier * BoneSetup.HardMatchingMultiply * chain.MusclesForce * chain.HardMatchMultiply;
                // Hard matching can't exceed 1
                hardMatch = Mathf.Clamp01( hardMatch );
            }

            if( hardMatch * overallMultiplier == 0f ) return 0f;

            return hardMatch;
        }

        /// <summary> Forcing bones to rotate towards animator pose rotation (standing mode) </summary>
        internal void AnimationRotationHardMatchingStandUpdate( float hardMatch )
        {
            if( rigidbody.isKinematic ) return;
            if( hardMatch <= 0f ) return;

            var targetRotation = animatorRotation; // In animator space
            RagdollHandlerUtilities.AddRigidbodyTorqueToRotateTowards( BoneSetup.GameRigidbody, targetRotation, hardMatch * 1.25f );
        }

        /// <summary> Forcing bones to rotate towards animator pose rotation (falling mode) </summary>
        internal void AnimationRotationHardMatchingFallUpdate( float hardMatch = 0f )
        {
            if( rigidbody.isKinematic ) return;
            if( hardMatch <= 0f ) return;

            var targetRotation = FEngineering.QToWorld( BoneSetup.DetachParent.rotation, animatorLocalRotation ); // In parent physical bone space
            RagdollHandlerUtilities.AddRigidbodyTorqueToRotateTowards( BoneSetup.GameRigidbody, targetRotation, hardMatch * 0.5f );
        }



        // Extra Utils ------------------------------------------------

        /// <summary> Calculating real rigidbody translation in world space </summary>
        private Vector3 averageTranslation;

        private Vector3 _lastFixedFramePosition;
        private float _translationCalculatedAtFixedTime = -2;

        /// <summary> Smoothing average velocity </summary>
        private void UpdateTranslationData()
        {
            averageTranslation = Vector3.Lerp( averageTranslation, ( rigidbody.position - _lastFixedFramePosition ), Time.fixedDeltaTime * 10f );
            _lastFixedFramePosition = rigidbody.position;
        }

        /// <summary> Calculate average velocity </summary>
        public Vector3 AverageTranslationDataRequest()
        {
            float elapsedSinceLastUpdate = Time.fixedTime - _translationCalculatedAtFixedTime;

            if( elapsedSinceLastUpdate < Time.fixedDeltaTime ) return averageTranslation;
            if( elapsedSinceLastUpdate > Time.fixedDeltaTime * 10f )
            {
                // Reset
                averageTranslation = Vector3.zero;
                _lastFixedFramePosition = rigidbody.position;
            }

            _translationCalculatedAtFixedTime = Time.fixedTime;
            UpdateTranslationData();
            return averageTranslation;
        }

        /// <summary> Get value without computing </summary>
        public Vector3 AverageTranslationDataRequestRaw()
        {
            return averageTranslation;
        }


        private float averageAngularity;
        private Quaternion _lastFixedFrameRotation;
        private float _angularCalculatedAtFixedTime = -2;

        /// <summary> Smoothing average angular velocity </summary>
        internal void UpdateAngularData()
        {
            averageAngularity = Mathf.LerpAngle( averageAngularity, Quaternion.Angle( rigidbody.rotation, _lastFixedFrameRotation ), Time.fixedDeltaTime * 10f );
            _lastFixedFrameRotation = rigidbody.rotation;
        }

        /// <summary> Calculate average angular velocity </summary>
        public float AverageAngularityDataRequest()
        {
            float elapsedSinceLastUpdate = Time.fixedTime - _angularCalculatedAtFixedTime;

            if( elapsedSinceLastUpdate < Time.fixedDeltaTime ) return averageAngularity;
            if( elapsedSinceLastUpdate > Time.fixedDeltaTime * 10f )
            {
                // Reset
                averageAngularity = 0f;
                _lastFixedFrameRotation = rigidbody.rotation;
            }

            _angularCalculatedAtFixedTime = Time.fixedTime;
            UpdateAngularData();
            return averageAngularity;
        }

        /// <summary> Get value without computing </summary>
        public float AverageAngularityDataRequestRaw()
        {
            return averageAngularity;
        }

    }
}