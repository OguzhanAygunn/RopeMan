#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_PositionHardMatching : RagdollAnimatorFeatureBase
    {
        public override bool OnInit()
        {
            RefreshHardMatchingProperty( ParentRagdollHandler, Helper );
            return base.OnInit();
        }

        public void RefreshHardMatchingProperty( RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            ragdollHandler.HardMatchPositions = helper.Enabled;
        }

        public override void OnEnabledSwitch()
        {
            base.OnEnabledSwitch();
            RefreshHardMatchingProperty( ParentRagdollHandler, Helper );
        }

        public override void OnDestroyFeature()
        {
            ParentRagdollHandler.HardMatchPositions = false;
            ParentRagdollHandler.HardMatchPositionsOnFall = false;
        }

#if UNITY_EDITOR

        public override bool Editor_DisplayEnableSwitch => true;
        public override string Editor_FeatureDescription => "Using Hard Matching parameter will apply bones position animation matching during Standing mode. It can provide more precise animation match results but make bones more stiff. (Just GUI feature)";

        public override void Editor_InspectorGUI( SerializedProperty handlerProp, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            base.Editor_InspectorGUI( handlerProp, ragdollHandler, helper );

            EditorGUIUtility.labelWidth = 200;
            ragdollHandler.PositionHardMatchingMultiplier = EditorGUILayout.Slider( new GUIContent( "Position Hard Matching Multiplier:", "Use if you want to keep rotation hard matching stronger but position hard matching weaker" ), ragdollHandler.PositionHardMatchingMultiplier, 0f, 1f );
            EditorGUILayout.Space( 4 );
            EditorGUIUtility.labelWidth = 0;
            ragdollHandler.HardMatchPositionsOnFall = EditorGUILayout.Toggle( new GUIContent( "Apply On Fall:", "Applying position hard matching also for fall mode. It probably will have use only in very specific cases."), ragdollHandler.HardMatchPositionsOnFall );
            EditorGUILayout.Space( 4 );

            if( !ragdollHandler.WasInitialized )
            {
                RefreshHardMatchingProperty( ragdollHandler, helper );
            }
        }

        public override void Editor_OnRemoveFeatureInEditorGUI( RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            ragdollHandler.HardMatchPositions = false;
            base.Editor_OnRemoveFeatureInEditorGUI( ragdollHandler, helper );
        }
#endif
    }
}