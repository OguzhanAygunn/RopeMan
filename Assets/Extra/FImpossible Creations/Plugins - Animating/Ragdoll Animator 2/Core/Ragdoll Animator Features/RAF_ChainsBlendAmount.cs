#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_ChainsBlendAmount : RagdollAnimatorFeatureBase
    {
#if UNITY_EDITOR

        public override bool Editor_DisplayEnableSwitch => false;

        public override void Editor_OnRemoveFeatureInEditorGUI( RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            base.Editor_OnRemoveFeatureInEditorGUI( ragdollHandler, helper );

            // Restore blends
            foreach( var chain in ragdollHandler.Chains )
            {
                chain.ChainBlend = 1f;

                foreach( var bone in chain.BoneSetups )
                {
                    bone.BoneBlendMultiplier = 1f;
                }
            }
        }

        public override string Editor_FeatureDescription => "Giving access to selective bone chains blending settings. (GUI feature)";

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
                EditorGUILayout.HelpBox( "Select chain to display it's blend settings.", MessageType.None );
                return;
            }

            var selectedChain = ragdollHandler.Chains[selectedV.GetInt()];
            if( selectedChain == null ) return;

            selectedChain.ChainBlend = EditorGUILayout.Slider( "Chain Blend:", selectedChain.ChainBlend, 0f, 1f );
            GUILayout.Space( 4 );

            EditorGUIUtility.labelWidth = 50;

            for( int b = 0; b < selectedChain.BoneSetups.Count; b++ )
            {
                var bone = selectedChain.BoneSetups[b];
                if( bone.SourceBone == null ) EditorGUILayout.LabelField( "Null Bone Reference!" );
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField( bone.SourceBone, typeof( Transform ), true, GUILayout.Width( 120 ) );
                    if( ragdollHandler.WasInitialized ) EditorGUILayout.ObjectField( bone.PhysicalDummyBone, typeof( Transform ), true, GUILayout.MaxWidth( 50 ) );
                    bone.BoneBlendMultiplier = EditorGUILayout.Slider( " Blend:", bone.BoneBlendMultiplier, 0f, 1f );
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUIUtility.labelWidth = 0;

        }

#endif
    }
}