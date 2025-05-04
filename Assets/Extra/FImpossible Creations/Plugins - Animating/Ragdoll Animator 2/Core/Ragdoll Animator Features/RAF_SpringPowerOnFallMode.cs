#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_SpringPowerOnFallMode : RagdollAnimatorFeatureBase
    {
        private FGenerating.FUniversalVariable springsOnFallPower;
        private FGenerating.FUniversalVariable transitionDuration;

        private float _sd = 0f;

        public override bool OnInit()
        {
            springsOnFallPower = InitializedWith.RequestVariable( "Power", 1f );
            transitionDuration = InitializedWith.RequestVariable( "Transition Duration:", 0.15f );
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

            float? preValue = ParentRagdollHandler.OverrideSpringsValueOnFall;

            if( ParentRagdollHandler.AnimatingMode == RagdollHandler.EAnimatingMode.Standing )
            {
                if( ParentRagdollHandler.OverrideSpringsValueOnFall != null )
                {
                    SmoothChange( ParentRagdollHandler.SpringsValue, transitionDuration.GetFloat() );

                    //ParentRagdollHandler.SpringsValueOnFall = Mathf.MoveTowards( ParentRagdollHandler.SpringsValueOnFall.Value, ParentRagdollHandler.SpringsValue, ParentRagdollHandler.Delta * transitionDuration.GetFloat() );
                    if( ParentRagdollHandler.OverrideSpringsValueOnFall == ParentRagdollHandler.SpringsValue ) ParentRagdollHandler.OverrideSpringsValueOnFall = null;
                }
            }
            else if( ParentRagdollHandler.IsFallingOrSleep ) // Falling mode
            {
                if( ParentRagdollHandler.OverrideSpringsValueOnFall == null ) ParentRagdollHandler.OverrideSpringsValueOnFall = ParentRagdollHandler.GetCurrentMainSpringsValue;
                SmoothChange( springsOnFallPower.GetFloat(), transitionDuration.GetFloat() );
                //ParentRagdollHandler.SpringsValueOnFall = Mathf.MoveTowards( ParentRagdollHandler.SpringsValueOnFall.Value, springsOnFallPower.GetFloat(), ParentRagdollHandler.Delta * transitionDuration.GetFloat() );
            }

            if( preValue != ParentRagdollHandler.OverrideSpringsValueOnFall )
            {
                ParentRagdollHandler.User_UpdateJointsPlayParameters( false );
            }
        }

        private void SmoothChange( float to, float duration )
        {
            ParentRagdollHandler.OverrideSpringsValueOnFall = Mathf.SmoothDamp( ParentRagdollHandler.OverrideSpringsValueOnFall.Value,
                to, ref _sd, duration, 10000000f, ParentRagdollHandler.Delta );

            if( Mathf.Abs( ParentRagdollHandler.OverrideSpringsValueOnFall.Value - to ) < 0.1f ) ParentRagdollHandler.OverrideSpringsValueOnFall = to;
        }

#if UNITY_EDITOR

        public override string Editor_FeatureDescription => "Overriding muscles spring power when character is switching to falling state with smooth value transition.";

        public override void Editor_InspectorGUI( SerializedProperty handlerProp, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            var powerV = helper.RequestVariable( "Power", 250f );
            powerV.Editor_DisplayVariableGUI();
            if( powerV.GetFloat() < 0f ) powerV.SetValue( 0f );

            var durationV = helper.RequestVariable( "Transition Duration:", 1.5f );
            durationV.SetMinMaxSlider( 0.0f, 5f );
            durationV.Editor_DisplayVariableGUI();
        }

#endif
    }
}