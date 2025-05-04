#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_ProvideAnimatorParameter : RagdollAnimatorFeatureUpdate
    {
        public override bool UseUpdate => true;

        private int _h_velocity = -1;
        private float _sd = 0f;

        public override bool OnInit()
        {
            var pv = InitializedWith.RequestVariable( "Set Velocity For:", "" );
            if( string.IsNullOrWhiteSpace( pv.GetString() ) == false ) _h_velocity = Animator.StringToHash( pv.GetString() );

            return base.OnInit();
        }

        public override void Update()
        {
            if( InitializedWith.Enabled == false ) return;

            if( _h_velocity != -1 )
            {
                float magn = ParentRagdollHandler.User_GetChainBonesVelocity( ERagdollChainType.Core ).magnitude;
                float newVal = ParentRagdollHandler.Mecanim.GetFloat( _h_velocity );
                newVal = Mathf.SmoothDamp( newVal, magn, ref _sd, 0.125f, 10000f, ParentRagdollHandler.Delta );
                ParentRagdollHandler.Mecanim.SetFloat( _h_velocity, newVal );
            }

            base.Update();
        }

#if UNITY_EDITOR

        public override string Editor_FeatureDescription => "Sending ragdoll velocity value to the Mecanim Animator Property.";

        public override void Editor_InspectorGUI( SerializedProperty handlerProp, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            base.Editor_InspectorGUI( handlerProp, ragdollHandler, helper );

            var pv = helper.RequestVariable( "Set Velocity For:", "" );
            pv.Editor_DisplayVariableGUI();

            GUILayout.Space( 3 );
        }

#endif
    }
}