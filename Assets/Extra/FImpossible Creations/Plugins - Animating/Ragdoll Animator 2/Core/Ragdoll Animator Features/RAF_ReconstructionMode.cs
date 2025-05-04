#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_ReconstructionMode : RagdollAnimatorFeatureBase
    {

#if UNITY_EDITOR

        public override void Editor_OnRemoveFeatureInEditorGUI( RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            ragdollHandler.UseReconstruction = false;
        }

        public override string Editor_FeatureDescription => "Switching using 'Reconstruction' mode. With this feature enabled, dummy will be generated will all lacking skeleton bones, as kinematic rigidbodies. It can improve animation matching mode, but it works poorly with Falling mode and generates garbage collector when switching from Standing mode. It also requires chains 'Detach' disabled.";

        public override void Editor_InspectorGUI( SerializedProperty toDirty, RagdollHandler handler, RagdollAnimatorFeatureHelper helper )
        {
            GUI.enabled = !handler.WasInitialized;
            handler.UseReconstruction = EditorGUILayout.Toggle( new GUIContent( "Reconstruction On:", "Can increase precision of animation matching.\nTurning on using extra joints on chain-skipped bones and dummy bones without direct connection with parent bones. It will make physics cost a bit more and can generate GC when switching AnimatingState from 'Standing' mode to other." ), handler.UseReconstruction );
            GUILayout.Space( 4 );

            if( handler.UseReconstruction )
            {
                bool hasDetached = false;
                foreach( var chain in handler.Chains ) if( chain.Detach ) { hasDetached = true; break; }
                if( hasDetached )
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.HelpBox( "Ragdoll Reconstruction Mode is not working on detached chains.", UnityEditor.MessageType.Warning );

                    if( GUILayout.Button( "Disable Detach" ) )
                    {
                        foreach( var chchain in handler.Chains ) chchain.Detach = false;
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }

            GUI.enabled = true;

        }

#endif
    }
}