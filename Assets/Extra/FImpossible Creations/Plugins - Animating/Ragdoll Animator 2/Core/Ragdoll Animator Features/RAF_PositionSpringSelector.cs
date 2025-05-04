#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_PositionSpringSelector : RagdollAnimatorFeatureBase
    {
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
                    bone.AllowConfigurablePosition = false;
                }
            }

            if( ragdollHandler.WasInitialized ) ragdollHandler.RefreshAllChainsDynamicParameters();
        }

        public override string Editor_FeatureDescription => "Giving access to selective bones position spring settings. (GUI feature)";

        public override void Editor_InspectorGUI( SerializedProperty handlerProp, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            if( ragdollHandler.ApplyPositions == false )
            {
                GUILayout.Space( 4 );

                EditorGUILayout.HelpBox( "You need to enable 'Apply Positions' in Motion Settings to see bones position effect!.", UnityEditor.MessageType.Info );
                if( GUILayout.Button( "Enable 'Apply Positions'" ) )
                {
                    ragdollHandler.ApplyPositions = true;
                    EditorUtility.SetDirty( handlerProp.serializedObject.targetObject );
                }

                GUILayout.Space( 8 );
            }

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
                EditorGUILayout.HelpBox( "Select chain to display it's position spring settings.", MessageType.None );
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
                    bone.AllowConfigurablePosition = EditorGUILayout.Toggle( " Position Spring:", bone.AllowConfigurablePosition );
                    EditorGUILayout.EndHorizontal();
                    if( bone.AllowConfigurablePosition )
                    {
                        EditorGUILayout.BeginHorizontal();
                        bone.LinearSpringLimit = EditorGUILayout.FloatField( " Linear Spring:", bone.LinearSpringLimit );
                        bone.LinearSpringDamping = EditorGUILayout.FloatField( " Damping:", bone.LinearSpringDamping );
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space( 6 );
                    }
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