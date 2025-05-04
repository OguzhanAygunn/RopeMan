#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_MusclesPowerOnFallMode : RagdollAnimatorFeatureBase
    {
        private FGenerating.FUniversalVariable musclesOnFallMultiplier;
        private FGenerating.FUniversalVariable transitionDuration;

        public override bool OnInit()
        {
            musclesOnFallMultiplier = InitializedWith.RequestVariable( "Multiplier", 1f );
            transitionDuration = InitializedWith.RequestVariable( "Transition Duration:", 1f );
            ParentRagdollHandler.AddToLateUpdateLoop( Update );
            return base.OnInit();
        }

        public override void OnDestroyFeature()
        {
            ParentRagdollHandler.RemoveFromLateUpdateLoop( Update );
        }

        private void Update()
        {
            if( InitializedWith.Enabled == false ) return;

            float preValue = ParentRagdollHandler.musclesPowerMultiplier;

            if( ParentRagdollHandler.AnimatingMode == RagdollHandler.EAnimatingMode.Standing )
            {
                ParentRagdollHandler.musclesPowerMultiplier = Mathf.MoveTowards( ParentRagdollHandler.musclesPowerMultiplier, 1f, ParentRagdollHandler.Delta / transitionDuration.GetFloat() );
            }
            else if( ParentRagdollHandler.IsFallingOrSleep )
            {
                ParentRagdollHandler.musclesPowerMultiplier = Mathf.MoveTowards( ParentRagdollHandler.musclesPowerMultiplier, musclesOnFallMultiplier.GetFloat(), ParentRagdollHandler.Delta / transitionDuration.GetFloat() );
            }

            if( preValue != ParentRagdollHandler.musclesPowerMultiplier )
            {
                ParentRagdollHandler.User_UpdateJointsPlayParameters( false );
            }
        }

#if UNITY_EDITOR

        public override string Editor_FeatureDescription => "Changing muscles power when character is switching to falling state.";

        public override void Editor_InspectorGUI( SerializedProperty handlerProp, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            var multiplierV = helper.RequestVariable( "Multiplier", 1f );

            EditorGUILayout.BeginHorizontal();
            float val = EditorGUILayout.Slider( "Muscles Power On Fall:", Mathf.Round( multiplierV.GetFloat() * 100f ), 0f, 100f );
            EditorGUILayout.LabelField( "%", GUILayout.Width( 16 ) );
            EditorGUILayout.EndHorizontal();
            multiplierV.SetValue( val / 100f );

            var durationV = helper.RequestVariable( "Transition Duration:", 1.5f );
            durationV.SetMinMaxSlider( 0.01f, 5f );
            durationV.Editor_DisplayVariableGUI();
        }

#endif
    }
}