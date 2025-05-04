using FIMSpace.FProceduralAnimation;

#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_AutoGetup : FimpossibleComponent
    {
        [Tooltip( "Script reads mecanim animator reference from the ragdoll animator" )]
        public RagdollAnimator2 ragdoll;

        public Rigidbody controllerRigibody;
        public MonoBehaviour controller;

        [Space( 6 )]
        [Range( 0f, 1f )] public float MinimumWaitTime = 0.3f;

        public LayerMask GroundMask = 0 << 0;
        public float TransitionDuration = 0.85f;

        private float fallingDuration = 0f;
        private ERagdollGetUpType getUpType;
        private RaycastHit getupHit;

        private void Update()
        {
            if( ragdoll.Handler.IsFallingOrSleep == false ) { fallingDuration = 0f; return; }

            getUpType = ERagdollGetUpType.None;

            CalculateGetUpType();

            if( getUpType != ERagdollGetUpType.None ) TriggerGetUp();
        }

        private float stableTime = 0f;

        private void CalculateGetUpType()
        {
            fallingDuration += Time.deltaTime;
            // Falling for too short duration
            if( fallingDuration < MinimumWaitTime ) return;

            ERagdollGetUpType canGetUp = ragdoll.User_CanGetUpByRotation( false );

            // Checking how hips are rotated in current pose to define target getup
            //if( canGetUp == ERagdollGetUpType.None ) return;

            // The velocity of core bones are in move, so not ready for getup
            float averageTranslation = ragdoll.User_GetChainBonesAverageTranslation( ERagdollChainType.Core ).magnitude;
            if( averageTranslation > 0.075f ) { stableTime = 0f; return; }

            // The velocity of core bones are in move, so not ready for getup
            if( ragdoll.User_GetChainAngularVelocity( ERagdollChainType.Core ).magnitude > 1f * ragdoll.Settings.User_CoreLowTranslationFactor( averageTranslation ) )
            { stableTime = 0f; return; }

            stableTime += Time.deltaTime;
            if( stableTime < 0.15f ) return; // Let's be in static pose for a small amount of time

            var hit = ragdoll.Handler.ProbeGroundBelowHips( GroundMask, ragdoll.Settings.GetAnchorBoneController.MainBoneCollider.bounds.size.magnitude + 0.01f );
            if( hit.transform == null ) return; // No Ground below

            getupHit = hit;
            getUpType = canGetUp;
        }

        private void TriggerGetUp()
        {
            transform.position = getupHit.point;
            transform.rotation = ragdoll.User_GetMappedRotationHipsToLegsMiddle();

            if( controllerRigibody )
            {
                controllerRigibody.position = transform.position;
                controllerRigibody.rotation = transform.rotation;
            }

            if( controller ) controller.enabled = true;

            string getupAnim;
            if( getUpType == ERagdollGetUpType.FromFacedown ) getupAnim = "Get Up Face";
            else getupAnim = "Get Up Back";

            ragdoll.Handler.Mecanim.CrossFadeInFixedTime( getupAnim, 0.175f ); // Beware, it's crossfading standing animation into lying animation
            ragdoll.User_TransitionToStandingMode( TransitionDuration, 0.6f, 0.1f, 0.125f );
            ragdoll.User_FadeMusclesPowerMultiplicator( 1f, TransitionDuration );
        }


        #region Editor Class

#if UNITY_EDITOR

        public override string HeaderInfo => "This is just example script, you should use Extra Modules for get up, but still you can take look in this script, to see how to trigger get up using code.";

        [UnityEditor.CanEditMultipleObjects]
        [UnityEditor.CustomEditor( typeof( Demo_Ragd_AutoGetup ), true )]
        public class Demo_Ragd_AutoGetupEditor : FimpossibleComponentEditor
        {
            public Demo_Ragd_AutoGetup Get
            { get { if( _get == null ) _get = (Demo_Ragd_AutoGetup)target; return _get; } }
            private Demo_Ragd_AutoGetup _get;

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if( Application.isPlaying == false ) return;

                GUILayout.Space( 7f );
                var canGetUp = Get.ragdoll.Settings.User_CanGetUpByRotation();
                EditorGUILayout.EnumPopup( "Calculated Getup Type:", canGetUp );

                GUILayout.Space( 3f );
                EditorGUILayout.LabelField( "Values helpful for detecting get up moment:" );
                EditorGUILayout.LabelField( "Core Chains Translation: " + Get.ragdoll.Settings.User_GetChainBonesAverageTranslation( ERagdollChainType.Core ).magnitude );
                EditorGUILayout.LabelField( "Core Chains Velocity: " + Get.ragdoll.Settings.User_GetChainBonesVelocity( ERagdollChainType.Core ).magnitude );
                EditorGUILayout.LabelField( "Core Chains Angularity: " + Get.ragdoll.Settings.User_GetChainBonesAverageAngularVelocity( ERagdollChainType.Core ) );
                EditorGUILayout.LabelField( "Core Chains Angular Velocity: " + Get.ragdoll.Settings.User_GetChainAngularVelocity( ERagdollChainType.Core ).magnitude );
            }
        }

#endif

        #endregion Editor Class
    }
}