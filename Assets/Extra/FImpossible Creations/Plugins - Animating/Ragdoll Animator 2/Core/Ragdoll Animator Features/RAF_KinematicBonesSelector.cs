#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_KinematicBonesSelector : RagdollAnimatorFeatureBase
    {

        public override void OnDestroyFeature()
        {
            if( ParentRagdollHandler == null ) return;

            // Restore values
            foreach( var chain in ParentRagdollHandler.Chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    bone.ForceKinematicOnStanding = false;
                    chain.BoneSetups[chain.BoneSetups.Count - 1].RefreshDynamicPhysicalParameters( chain, ParentRagdollHandler.IsInFallingMode );
                }
            }
        }

#if UNITY_EDITOR

        public override bool Editor_DisplayEnableSwitch => false;

        public override void Editor_OnRemoveFeatureInEditorGUI( RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            base.Editor_OnRemoveFeatureInEditorGUI( ragdollHandler, helper );

            // Restore values
            foreach( var chain in ragdollHandler.Chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    bone.ForceKinematicOnStanding = false;
                    if ( ragdollHandler.WasInitialized) chain.BoneSetups[chain.BoneSetups.Count - 1].RefreshDynamicPhysicalParameters( chain, ragdollHandler.IsInFallingMode );
                }
            }
        }

        public override string Editor_FeatureDescription => "Giving access to selective bone chains kinematic on Standing Mode switch. (GUI feature)";

        public override void Editor_InspectorGUI( SerializedProperty handlerProp, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            EditorGUILayout.BeginHorizontal();

            var selectedV = helper.RequestVariable( "Selected", -1 );

            for( int i = 0; i < ragdollHandler.Chains.Count; i++ )
            {
                var chain = ragdollHandler.Chains[i];

                if( selectedV.GetInt() == i ) GUI.backgroundColor = Color.green;
                if( GUILayout.Button( chain.ChainName ) ) { if( selectedV.GetInt() == i ) selectedV.SetValue( -1 ); else selectedV.SetValue( i ); }
                GUI.backgroundColor = Color.white;
            }

            //var selectedV = helper.RequestVariable( "Chain " + i, false );
            EditorGUILayout.EndHorizontal();

            if( selectedV.GetInt() < 0 || selectedV.GetInt() > ragdollHandler.Chains.Count - 1 )
            {
                EditorGUILayout.HelpBox( "Select chain to display it's kinematic switch settings.", MessageType.None );
                return;
            }

            var selectedChain = ragdollHandler.Chains[selectedV.GetInt()];
            if( selectedChain == null ) return;

            EditorGUI.BeginChangeCheck();

            EditorGUIUtility.labelWidth = 80;

            for( int b = 0; b < selectedChain.BoneSetups.Count; b++ )
            {
                var bone = selectedChain.BoneSetups[b];
                if( bone.SourceBone == null ) EditorGUILayout.LabelField( "Null Bone Reference!" );
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField( bone.SourceBone, typeof( Transform ), true );
                    if( ragdollHandler.WasInitialized ) EditorGUILayout.ObjectField( bone.PhysicalDummyBone, typeof( Transform ), true, GUILayout.MaxWidth( 50 ) );
                    bone.ForceKinematicOnStanding = EditorGUILayout.Toggle( " Kinematic:", bone.ForceKinematicOnStanding );
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUIUtility.labelWidth = 0;

            if( EditorGUI.EndChangeCheck() )
            {
                if( Application.isPlaying && ragdollHandler.WasInitialized ) ragdollHandler.RefreshAllChainsDynamicParameters();
                EditorUtility.SetDirty( handlerProp.serializedObject.targetObject );
            }
        }

#endif
    }
}