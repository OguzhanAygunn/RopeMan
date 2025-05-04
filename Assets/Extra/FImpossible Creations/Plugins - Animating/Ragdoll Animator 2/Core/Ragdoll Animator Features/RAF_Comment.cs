#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_Comment : RagdollAnimatorFeatureBase
    {
#if UNITY_EDITOR
        public override bool Editor_DisplayEnableSwitch => false;
        public override string Editor_FeatureDescription => editComment ? "" : comment;
#endif

#if UNITY_EDITOR

        private bool editComment = false;
        private string comment = "";

        public override void Editor_InspectorGUI( SerializedProperty toDirty, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            var commentV = helper.RequestVariable( "C", "Use this to write custom comments, to help out other users,\nwhat is purpose of this ragdoll's features setup." );
            comment = commentV.GetString();

            if( editComment )
            {
                comment = EditorGUILayout.TextArea( comment );
                commentV.SetValue( comment );
            }

            if( GUILayout.Button( editComment ? "Stop Editing" : "Edit Comment", EditorStyles.miniButton ) )
            {
                editComment = !editComment;
            }
        }

#endif
    }
}