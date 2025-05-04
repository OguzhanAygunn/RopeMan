#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_VelocitySolverIterations : RagdollAnimatorFeatureBase
    {

#if UNITY_EDITOR

        public override void Editor_OnRemoveFeatureInEditorGUI( RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            ragdollHandler.UnityVelocitySolverIterations = 0;
        }

        public override string Editor_FeatureDescription => "Changing ragdoll rigidbodies velocity solver iterations count.";

        public override void Editor_InspectorGUI( SerializedProperty toDirty, RagdollHandler handler, RagdollAnimatorFeatureHelper helper )
        {
            int preChange = handler.UnityVelocitySolverIterations;
            handler.UnityVelocitySolverIterations = EditorGUILayout.IntSlider( new GUIContent("Velocity Solver Iterations:", "Quality of unity rigidbody velocity iterations. 1 is default for unity projects. 0 will use default physics settings value." ), handler.UnityVelocitySolverIterations, 0, 6 );
            GUILayout.Space( 4 );

            if (handler.WasInitialized)
            {
                if( preChange != handler.UnityVelocitySolverIterations ) ParentRagdollHandler.User_UpdateRigidbodyParametersForAllBones();
            }
        }

#endif
    }
}