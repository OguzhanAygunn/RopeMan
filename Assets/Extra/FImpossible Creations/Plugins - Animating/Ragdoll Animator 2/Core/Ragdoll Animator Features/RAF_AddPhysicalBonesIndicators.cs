#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_AddPhysicalBonesIndicators : RagdollAnimatorFeatureBase
    {
#if UNITY_EDITOR
        public override bool Editor_DisplayEnableSwitch => false;
        public override string Editor_FeatureDescription => "Adding indicator components to the physical dummy bones which will help bone type indication or collisions detection per bone.";
#endif

        public override bool OnInit()
        {
            var ragdoll = ParentRagdollHandler;
            var physDetectors = InitializedWith.RequestVariable( "Add Collision Detectors:", false );
            bool phys = physDetectors.GetBool();

            if( phys == false )
            {
                ragdoll.PrepareDummyBonesCollisionIndicators( false );
            }
            else
            {
                ragdoll.PrepareDummyBonesCollisionIndicators( true );
            }

            return true;
        }

#if UNITY_EDITOR

        public override void Editor_InspectorGUI( SerializedProperty toDirty, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            base.Editor_InspectorGUI( toDirty, ragdollHandler, helper );

            GUI.enabled = !helper.ParentRagdollHandler.WasInitialized;
            var physDetectors = helper.RequestVariable( "Add Collision Detectors:", false );
            EditorGUIUtility.labelWidth = 176;
            physDetectors.Editor_DisplayVariableGUI();
            EditorGUIUtility.labelWidth = 0;

            GUI.enabled = true;
        }

#endif
    }
}