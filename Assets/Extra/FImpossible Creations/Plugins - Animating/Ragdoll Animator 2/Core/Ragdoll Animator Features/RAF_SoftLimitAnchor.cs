#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_SoftLimitAnchor : RagdollAnimatorFeatureUpdate
    {
        public override bool UseFixedUpdate => true;

        FGenerating.FUniversalVariable softLimit;
        FGenerating.FUniversalVariable resetRange;
        FGenerating.FUniversalVariable fallRange;

        /// <summary> To Prevent fall in unwanted time </summary>
        float standingDuration = 0f;

        public override bool OnInit()
        {
            softLimit = InitializedWith.RequestVariable( "Soft Limit Range:", 0.5f );
            resetRange = InitializedWith.RequestVariable( "Reset On Range:", 0f );
            fallRange = InitializedWith.RequestVariable( "Fall On Factor:", 0f );

            ParentRagdollHandler.AddToOnFallModeSwitchActions( RefreshFactor );

            return base.OnInit();
        }

        float lastFactor = 0f;
        public override void FixedUpdate()
        {
            if( Helper.Enabled == false ) return;

            if( softLimit.GetFloat() <= 0f )
            {
                ParentRagdollHandler.anchorBoneSpringPositionMultiplier = 1f;
                return;
            }

            var anchor = ParentRagdollHandler.GetAnchorBoneController;
            float poseDiff = ( anchor.BoneProcessor.LastMatchingRigidodyOrigin - anchor.GameRigidbody.worldCenterOfMass ).sqrMagnitude;

            float factor = ( poseDiff * softLimit.GetFloat() * 25f ) + 1f;
            ParentRagdollHandler.anchorBoneSpringPositionMultiplier = 1f / factor;

            lastFactor = factor;

            if( ParentRagdollHandler.AnimatingMode == RagdollHandler.EAnimatingMode.Standing )
            {
                standingDuration += Time.fixedDeltaTime;
                if( standingDuration < 0.5f ) return; // Not allow to compute actions below for 0.5 sec on switch to standing
            }
            else
            {
                standingDuration = 0f;
                return;
            }

            if( resetRange.GetFloat() > 1f ) if( factor > resetRange.GetFloat() ) ParentRagdollHandler.anchorBoneSpringPositionMultiplier = 1f;
            if( fallRange.GetFloat() > 2f ) if( factor > fallRange.GetFloat() ) ParentRagdollHandler.User_SwitchFallState();
        }

        public override void OnEnabledSwitch()
        {
            lastFactor = 0f;
        }

        void RefreshFactor()
        {
            lastFactor = 0f;
        }

        public override void OnDestroyFeature()
        {
            base.OnDestroyFeature();
            ParentRagdollHandler.RemoveFromOnFallModeSwitchActions( RefreshFactor );
        }

#if UNITY_EDITOR

        public override void Editor_InspectorGUI( SerializedProperty handlerProp, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            FGenerating.FUniversalVariable softLimitV = helper.RequestVariable( "Soft Limit Range:", 0.5f );
            softLimitV.SetMinMaxSlider( 0f, 1f );
            softLimitV.AssignTooltip( "When bigger, then anchor bone is blocked more easily" );
            softLimitV.Editor_DisplayVariableGUI();

            GUILayout.Space( 2 );
            FGenerating.FUniversalVariable resetRangeV = helper.RequestVariable( "Reset On Range:", 1f );
            resetRangeV.AssignTooltip( "If distance factor is too big, you can reset position of the anchor bone" );

            EditorGUILayout.BeginHorizontal();
            resetRangeV.Editor_DisplayVariableGUI();
            if( resetRangeV.GetFloat() <= 1f ) EditorGUILayout.LabelField( "Not Using", EditorStyles.centeredGreyMiniLabel, GUILayout.Width( 60 ) );
            EditorGUILayout.EndHorizontal();

            if( resetRangeV.GetFloat() < 1f ) resetRangeV.SetValue( 1f );

            GUILayout.Space( 2 );
            FGenerating.FUniversalVariable fallRangeV = helper.RequestVariable( "Fall On Factor:", 2f );
            fallRangeV.AssignTooltip( "Set 2 to not use. If distance factor is big enough, triggering ragdoll fall state." );

            EditorGUILayout.BeginHorizontal();
            fallRangeV.Editor_DisplayVariableGUI();
            if( fallRangeV.GetFloat() <= 2f ) EditorGUILayout.LabelField( "Not Using", EditorStyles.centeredGreyMiniLabel, GUILayout.Width( 60 ) );
            EditorGUILayout.EndHorizontal();

            if( fallRangeV.GetFloat() < 2f ) fallRangeV.SetValue( 2f );

            if( ragdollHandler.WasInitialized )
            {
                GUILayout.Space( 4 );
                EditorGUILayout.HelpBox( "Last Soft Factor: " + lastFactor, UnityEditor.MessageType.None );
            }
        }

        public override string Editor_FeatureDescription => "Applying soft limit to the anchor bone position spring, so it applies less power when anchor is pushed away from the target body position.";

#endif
    }
}