using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static partial class RagdollHandlerUtilities
    {
        /// <summary>
        /// Transitioning all rigidbody muscles power to the target value (RagdollHandler.MusclesPower)
        /// </summary>
        /// <param name="targetMusclesForce"> Target muscle power </param>
        /// <param name="duration"> Transition duration </param>
        /// <param name="delay"> Delay to start transition </param>
        public static void User_FadeMusclesPower( this IRagdollAnimator2HandlerOwner iHandler, float targetMusclesForce = 0f, float duration = 0.75f, float delay = 0f, bool disableMecanimAtEnd = false )
        {
            RagdollHandler handler = iHandler.GetRagdollHandler;

            if( handler.Caller == null ) { Debug.Log( "[Ragdoll Animator 2] No Caller Behaviour Assigned, can't run Coroutine!" ); return; }

            if( handler._Coro_FadeMuscles != null ) handler.Caller.StopCoroutine( handler._Coro_FadeMuscles );

            handler._Coro_FadeMuscles = handler.Caller.StartCoroutine( handler._IE_FadeMusclesPower( targetMusclesForce, duration, delay, disableMecanimAtEnd ) );
        }

        /// <summary>
        /// Transitioning all rigidbody internal muscles power multiplier to the target value (RagdollHandler.musclesPowerMultiplier)
        /// Should be used to transition to zero or one value.
        /// </summary>
        /// <param name="targetMusclesMultiply"> Target muscle multiplayer value </param>
        /// <param name="duration"> Transition duration </param>
        /// <param name="delay"> Delay to start transition </param>
        public static void User_FadeMusclesPowerMultiplicator( this IRagdollAnimator2HandlerOwner iHandler, float targetMusclesMultiply = 0f, float duration = 0.75f, float delay = 0f )
        {
            RagdollHandler handler = iHandler.GetRagdollHandler;

            if( handler.Caller == null ) { Debug.Log( "[Ragdoll Animator 2] No Caller Behaviour Assigned, can't run Coroutine!" ); return; }

            if( handler._Coro_FadeMusclesMul != null ) handler.Caller.StopCoroutine( handler._Coro_FadeMusclesMul );

            handler._Coro_FadeMusclesMul = handler.Caller.StartCoroutine( handler._IE_FadeMusclesPowerMultiplicator( targetMusclesMultiply, duration, delay ) );
        }

        /// <summary>
        /// Disabling mecanim after some time, before disabling, storing pose as calibration default pose to hold it when mecanim is inactive, useful for death behaviour
        /// </summary>
        /// <param name="delay"> Delay to disable mecanim after </param>
        public static void User_DisableMecanimAfter( this IRagdollAnimator2HandlerOwner iHandler, float delay )
        {
            RagdollHandler handler = iHandler.GetRagdollHandler;
            if( handler.Mecanim == null ) return;
            if( handler.Caller == null ) { Debug.Log( "[Ragdoll Animator 2] No Caller Behaviour Assigned, can't run Coroutine!" ); return; }

            handler._Coro_FadeMuscles = handler.Caller.StartCoroutine( handler._IE_CallAfter( delay, () => 
            { 
                iHandler.GetRagdollHandler.Calibrate = true; 
                iHandler.GetRagdollHandler.StoreCalibrationPose(); 
                iHandler.GetRagdollHandler.Mecanim.enabled = false; 
            } ) );
        }

        /// <summary>
        /// Changing internal Muscles Power Multiplier value using Mathf.MoveTowards method.
        /// If you're not using extra feature controlling muscles power, you can use this value for custom muscles power control.
        /// </summary>
        /// <param name="to"> Change muscles power multiplier to this value (value 1 is default - unchanged muscles power value) </param>
        /// <param name="delta"> Time.deltaTime speed you can multiply to make transition faster </param>
        public static void User_TransitionMusclesPowerMultiplier( this IRagdollAnimator2HandlerOwner iHandler, float to, float delta )
        {
            iHandler.GetRagdollHandler.musclesPowerMultiplier = Mathf.MoveTowards( iHandler.GetRagdollHandler.musclesPowerMultiplier, to, delta );
        }
    }
}