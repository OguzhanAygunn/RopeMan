using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static partial class RagdollHandlerUtilities
    {
        /// <summary>
        /// Switching ragdoll mode to fall / standing mode
        /// </summary>
        public static void User_SwitchFallState( this IRagdollAnimator2HandlerOwner iHandler, RagdollHandler.EAnimatingMode state )
        {
            iHandler.GetRagdollHandler.AnimatingMode = state;
        }

        /// <summary>
        /// Switching ragdoll mode to fall / standing mode
        /// </summary>
        public static void User_SwitchFallState( this IRagdollAnimator2HandlerOwner iHandler, bool standing = false )
        {
            iHandler.GetRagdollHandler.AnimatingMode = standing ? RagdollHandler.EAnimatingMode.Standing : RagdollHandler.EAnimatingMode.Falling;
        }

        /// <summary>
        /// Adding physical push impact to single bone's rigidbody
        /// </summary>
        /// <param name="velocity"> World space direction velocity </param>
        /// <param name="duration"> Time in seconds, set zero to impact just once </param>
        public static void User_AddBoneImpact( this IRagdollAnimator2HandlerOwner iHandler, RagdollChainBone bone, Vector3 velocity, float duration, ForceMode forceMode = ForceMode.Impulse, float delay = 0f, int waitFixedFrames = 0 )
        {
            if( bone.GameRigidbody == null ) return;
            User_AddRigidbodyImpact( iHandler, bone.GameRigidbody, velocity, duration, forceMode, delay, waitFixedFrames );
        }

        /// <summary>
        /// Adding physical push impact to the single provided rigidbody object
        /// </summary>
        /// <param name="velocity"> World space direction velocity </param>
        /// <param name="duration"> Time in seconds, set zero to impact just once </param>
        public static void User_AddRigidbodyImpact( this IRagdollAnimator2HandlerOwner iHandler, Rigidbody rigb, Vector3 velocity, float duration, ForceMode forceMode = ForceMode.Impulse, float delay = 0f, int waitFixedFrames = 0 )
        {
            var handler = iHandler.GetRagdollHandler;

            if( handler.Caller == null && ( delay > 0f || duration > 0f ) ) { Debug.Log( "[Ragdoll Animator 2] No Caller Behaviour Assigned, can't run Coroutine!" ); return; }

            if( duration <= 0f ) // No impact duration - add impulse once
            {
                if( delay > 0f || waitFixedFrames > 0 )
                {
                    handler.Caller.StartCoroutine( handler._IE_CallAfter( delay, () => { ApplyLimbImpact( rigb, velocity, forceMode ); }, waitFixedFrames ) );
                    return;
                }
                else
                {
                    ApplyLimbImpact( rigb, velocity, forceMode );
                }

                return;
            }

            handler.Caller.StartCoroutine( handler._IE_SetPhysicalImpact( rigb, velocity, duration, forceMode, delay, waitFixedFrames ) );
        }

        /// <summary>
        /// Adding physical push impact to whole chain bones rigidbodies
        /// </summary>
        /// <param name="velocity"> World space direction velocity </param>
        /// <param name="duration"> Time in seconds, set zero to impact just once </param>
        public static void User_AddChainImpact( this IRagdollAnimator2HandlerOwner iHandler, RagdollBonesChain chain, Vector3 velocity, float duration, ForceMode forceMode = ForceMode.Impulse )
        {
            if( chain == null ) return;

            if( duration <= 0f ) // No impact duration - add impulse once
            {
                foreach( var bone in chain.BoneSetups ) User_AddRigidbodyImpact( iHandler, bone.GameRigidbody, velocity, duration, forceMode );
                return;
            }

            var handler = iHandler.GetRagdollHandler;

            if( handler.Caller == null ) { Debug.Log( "[Ragdoll Animator 2] No Caller Behaviour Assigned, can't run Coroutine!" ); return; }

            handler.Caller.StartCoroutine( handler._IE_SetChainPhysicalImpact( chain, velocity, duration, forceMode ) );
        }

        /// <summary>
        /// Adding physical push impact to whole chain bones rigidbodies
        /// </summary>
        /// <param name="velocity"> World space direction velocity </param>
        /// <param name="duration"> Time in seconds, set zero to impact just once </param>
        public static void User_AddChainImpact( this IRagdollAnimator2HandlerOwner iHandler, ERagdollChainType chain, Vector3 velocity, float duration, ForceMode forceMode = ForceMode.Impulse )
        {
            User_AddChainImpact( iHandler, iHandler.GetRagdollHandler.GetChain( chain ), velocity, duration, forceMode );
        }

        /// <summary>
        /// Applying physical push to all rigidbodies of the ragdoll dummy
        /// </summary>
        /// <param name="velocity"> World space direction velocity </param>
        public static void User_AddAllBonesImpact( this IRagdollAnimator2HandlerOwner iHandler, Vector3 velocity, float impactDuration = 0f, ForceMode mode = ForceMode.Impulse, float delay = 0f, int waitExtraFixedSteps = 0 )
        {
            var handler = iHandler.GetRagdollHandler;

            if( delay > 0f )
            {
                handler.Caller.StartCoroutine( handler._IE_CallAfter( delay, () => { handler.User_AddAllBonesImpact( velocity, impactDuration, mode, 0, 0 ); }, waitExtraFixedSteps ) );
                return;
            }

            handler.User_AddAllImpact( velocity, impactDuration, mode );
        }

        /// <summary>
        /// Assigning velocity to all rigidbodies of the ragdoll dummy
        /// </summary>
        /// <param name="velocity"> World space direction velocity </param>
        public static void User_SetAllBonesVelocity( this IRagdollAnimator2HandlerOwner iHandler, Vector3 velocity, float delay = 0f, int waitExtraFixedSteps = 0 )
        {
            var handler = iHandler.GetRagdollHandler;

            if( delay > 0f )
            {
                handler.Caller.StartCoroutine( handler._IE_CallAfter( delay, () => { handler.User_SetAllVelocity( velocity ); }, waitExtraFixedSteps ) );
                return;
            }

            handler.User_SetAllVelocity( velocity );
        }

        /// <summary>
        /// Setting all ragdoll bones rigidbodies kinematic or non kinematic
        /// </summary>
        public static void User_SetAllKinematic( this IRagdollAnimator2HandlerOwner iHandler, bool kinematic = true )
        {
            iHandler.GetRagdollHandler.CallOnAllRagdollBones( ( RagdollChainBone bone ) => bone.GameRigidbody.isKinematic = kinematic );
            iHandler.GetRagdollHandler.CallOnAllInBetweenBones( ( RagdollChainBone.InBetweenBone bone ) => { if( bone.rigidbody ) bone.rigidbody.isKinematic = kinematic; } );
        }

        /// <summary>
        /// Setting all ragdoll bones rigidbodies Use Gravity switch
        /// </summary>
        public static void User_SwitchAllBonesUseGravity( this IRagdollAnimator2HandlerOwner iHandler, bool useGravity = true )
        {
            iHandler.GetRagdollHandler.CallOnAllRagdollBones( ( RagdollChainBone bone ) => bone.GameRigidbody.useGravity = useGravity );
        }

        /// <summary>
        /// Switch all dummy bones rigidbodies parameter
        /// </summary>
        public static void User_SwitchAllBonesMaxVelocity( this IRagdollAnimator2HandlerOwner iHandler, float MaxVelocity = 0f )
        {
#if UNITY_2022_1_OR_NEWER
            iHandler.GetRagdollHandler.CallOnAllRagdollBones( ( RagdollChainBone bone ) => bone.GameRigidbody.maxLinearVelocity = MaxVelocity );
#endif
        }

        /// <summary>
        /// Switch all dummy bones rigidbodies parameter
        /// </summary>
        public static void User_ChangeAllRigidbodiesDrag( this IRagdollAnimator2HandlerOwner iHandler, float drag = 0f )
        {
            iHandler.GetRagdollHandler.CallOnAllRagdollBones( ( RagdollChainBone bone ) => bone.GameRigidbody.drag = drag );
        }

        /// <summary>
        /// Switch all bones rigidbodies parameter
        /// </summary>
        public static void User_ChangeAllRigidbodiesAngularDrag( this IRagdollAnimator2HandlerOwner iHandler, float drag = 0f )
        {
            iHandler.GetRagdollHandler.CallOnAllRagdollBones( ( RagdollChainBone bone ) => bone.GameRigidbody.angularDrag = drag );
        }

        /// <summary>
        /// Adding physical force to all ragdoll rigidbodies
        /// </summary>
        public static void User_AddAllImpact( this IRagdollAnimator2HandlerOwner iHandler, Vector3 force, float duration, ForceMode mode )
        {
            var handler = iHandler.GetRagdollHandler;
            handler.CallOnAllRagdollBones( ( RagdollChainBone bone ) => handler.User_AddRigidbodyImpact( bone.GameRigidbody, force, duration, mode ) );
        }

        /// <summary>
        /// Setting all ragdoll bones rigidbodies velocity
        /// </summary>
        public static void User_SetAllVelocity( this IRagdollAnimator2HandlerOwner iHandler, Vector3 worldVelocity )
        {
            iHandler.GetRagdollHandler.CallOnAllRagdollBones( ( RagdollChainBone bone ) => bone.GameRigidbody.velocity = worldVelocity );
        }

        /// <summary>
        /// Resetting all ragdoll bones angular velocity
        /// </summary>
        public static void User_ResetAngularVelocityForAllBones( this IRagdollAnimator2HandlerOwner iHandler )
        {
            iHandler.GetRagdollHandler.CallOnAllRagdollBones( ( RagdollChainBone bone ) => bone.GameRigidbody.angularVelocity = Vector3.zero );
        }

        /// <summary>
        /// Setting all ragdoll bones rigidbodies angular speed limit (by default unity restricts it very tightly)
        /// You can set it using ragdoll handler max angular velocity in the inspector view or changing it through code and calling UpdateAllAfterManualChanges
        /// </summary>
        public static void User_SetAllAngularSpeedLimit( this IRagdollAnimator2HandlerOwner iHandler, float angularSpeedLimit )
        {
            iHandler.GetRagdollHandler.CallOnAllRagdollBones( ( RagdollChainBone bone ) => bone.GameRigidbody.maxAngularVelocity = angularSpeedLimit );
        }

        /// <summary>
        /// Setting all ragdoll bones rigidbodies interpolation mode
        /// You can set it using ragdoll handler interpolate mode in the inspector view or changing it through code and calling UpdateAllAfterManualChanges
        /// </summary>
        public static void User_SetAllIterpolation( this IRagdollAnimator2HandlerOwner iHandler, RigidbodyInterpolation interpolation )
        {
            iHandler.GetRagdollHandler.CallOnAllRagdollBones( ( RagdollChainBone bone ) => bone.GameRigidbody.interpolation = interpolation );
        }

        /// <summary> Default impulse impact on the rigidbody (AddForce) </summary>
        internal static void ApplyLimbImpact( Rigidbody rigidbody, Vector3 powerDirection, ForceMode forceMode = ForceMode.Impulse )
        {
            rigidbody.AddForce( powerDirection, forceMode );
        }

        /// <summary>
        /// Adding physical torque impact to the selected bone
        /// </summary>
        /// <param name="rotationPower"> Rotation angles torque power </param>
        /// <param name="duration"> Time in seconds </param>
        public static void User_SetPhysicalTorqueOnRigidbody( this IRagdollAnimator2HandlerOwner iHandler, Rigidbody limb, Vector3 rotationPower, float duration, bool relativeSpace = false, ForceMode forceMode = ForceMode.Impulse, bool deltaScale = false )
        {
            if( deltaScale )
            {
                if( Time.fixedDeltaTime > 0f ) rotationPower /= Time.fixedDeltaTime;
            }

            if( duration <= 0f )
            {
                if( relativeSpace )
                    limb.AddRelativeTorque( rotationPower, forceMode );
                else
                    limb.AddTorque( rotationPower, forceMode );
                return;
            }

            iHandler.GetRagdollHandler.Caller.StartCoroutine( iHandler.GetRagdollHandler._IE_SetPhysicalTorque( limb, rotationPower, duration, relativeSpace, forceMode ) );
        }

        /// <summary>
        /// Adding physical torque impact to the all bones
        /// Use localOf to apply torque axis by for example baseTransform orientation
        /// </summary>
        public static void User_SetAllPhysicalTorque( this IRagdollAnimator2HandlerOwner iHandler, Vector3 localEuler, float duration, bool relativeSpace = false, Transform localOf = null, Vector3? power = null, ForceMode force = ForceMode.Impulse )
        {
            Quaternion rot = Quaternion.Euler( localEuler );
            if( localOf != null ) rot = FEngineering.QToWorld( localOf.rotation, rot );

            Vector3 angles = FEngineering.WrapVector( rot.eulerAngles );

            if( power != null ) angles = Vector3.Scale( angles, power.Value );

            iHandler.GetRagdollHandler.Caller.StartCoroutine( iHandler.GetRagdollHandler._IE_SetPhysicalTorque( angles, duration, relativeSpace, force ) );
        }

        /// <summary>
        /// Adding physical torque impact to the bone
        /// Use localOf to apply torque axis by for example baseTransform orientation
        /// </summary>
        public static void User_SetPhysicalTorque( this IRagdollAnimator2HandlerOwner iHandler, Rigidbody rigidbody, Vector3 localEuler, float duration, bool relativeSpace = false, Transform localOf = null, Vector3? power = null, ForceMode force = ForceMode.Impulse )
        {
            Quaternion rot = Quaternion.Euler( localEuler );
            if( localOf ) rot = FEngineering.QToWorld( localOf.rotation, rot );

            Vector3 angles = FEngineering.WrapVector( rot.eulerAngles );

            if( power != null ) angles = Vector3.Scale( angles, power.Value );

            iHandler.GetRagdollHandler.Caller.StartCoroutine( iHandler.GetRagdollHandler._IE_SetPhysicalTorque( rigidbody, angles, duration, relativeSpace, force ) );
        }

        /// <summary>
        /// Returning velocity of the bone with greatest velocity magnitude
        /// </summary>
        public static Vector3 User_GetAllBonesMaxVelocity( this IRagdollAnimator2HandlerOwner iHandler )
        {
            Vector3 velo = Vector3.zero;

            foreach( var chain in iHandler.GetRagdollHandler.Chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    if( bone.GameRigidbody.velocity.sqrMagnitude > velo.sqrMagnitude ) velo = bone.GameRigidbody.velocity;
                }
            }

            return velo;
        }

        /// <summary>
        /// Computing average translation of ragdoll chain bones
        /// Average translation is calculated only when calling this method! - To avoid calculating it all the time.
        /// </summary>
        public static Vector3 User_GetChainBonesAverageTranslation( this IRagdollAnimator2HandlerOwner iHandler, ERagdollChainType chainType )
        {
            var chain = iHandler.GetRagdollHandler.GetChain( chainType );
            if( chain == null ) return Vector3.zero;

            Vector3 velo = Vector3.zero;
            foreach( var bone in chain.BoneSetups ) velo += bone.BoneProcessor.AverageTranslationDataRequest();

            return velo / (float)chain.BoneSetups.Count;
        }

        /// <summary>
        /// Computing average translation of ragdoll chain bones
        /// Average angular velocity is calculated only when calling this method! - To avoid calculating it all the time.
        /// </summary>
        public static float User_GetChainBonesAverageAngularVelocity( this IRagdollAnimator2HandlerOwner iHandler, ERagdollChainType chainType )
        {
            var chain = iHandler.GetRagdollHandler.GetChain( chainType );
            if( chain == null ) return 0f;

            float velo = 0f;
            foreach( var bone in chain.BoneSetups ) velo += bone.BoneProcessor.AverageAngularityDataRequest();

            return velo / (float)chain.BoneSetups.Count;
        }

        /// <summary>
        /// Computing velocity of ragdoll chain bones
        /// </summary>
        public static Vector3 User_GetChainBonesVelocity( this IRagdollAnimator2HandlerOwner iHandler, ERagdollChainType chainType, bool average = true )
        {
            var chain = iHandler.GetRagdollHandler.GetChain( chainType );
            if( chain == null ) return Vector3.zero;

            Vector3 velo = Vector3.zero;
            foreach( var bone in chain.BoneSetups ) velo += bone.GameRigidbody.velocity;

            return average ? velo / (float)chain.BoneSetups.Count : velo;
        }

        /// <summary>
        /// Computing average angular velocity of ragdoll chain bones
        /// </summary>
        public static Vector3 User_GetChainAngularVelocity( this IRagdollAnimator2HandlerOwner iHandler, ERagdollChainType chainType, bool average = true )
        {
            var chain = iHandler.GetRagdollHandler.GetChain( chainType );
            if( chain == null ) return Vector3.zero;

            Vector3 velo = Vector3.zero;
            foreach( var bone in chain.BoneSetups ) velo += bone.GameRigidbody.angularVelocity;

            return average ? velo / (float)chain.BoneSetups.Count : velo;
        }

        /// <summary>
        /// Switching ragdoll mode to fall, applying strong impact to the selected bone and weaker impact to all bones
        /// </summary>
        public static void User_FallImpact( this IRagdollAnimator2HandlerOwner iHandler, Vector3 impactDirection, float power, float impactDuration = 0.15f, float bodyPushPower = 1f, Rigidbody hittedBone = null )
        {
            User_SwitchFallState( iHandler );

            // Push whole ragdoll with some force
            User_AddAllBonesImpact( iHandler, impactDirection * bodyPushPower, impactDuration, ForceMode.Acceleration );

            // Empathise hitted limb with impact
            if( hittedBone ) User_AddRigidbodyImpact( iHandler, hittedBone, impactDirection * power, impactDuration, ForceMode.VelocityChange );
        }
    }
}