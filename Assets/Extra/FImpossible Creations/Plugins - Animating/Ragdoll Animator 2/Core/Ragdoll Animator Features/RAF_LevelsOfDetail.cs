#if UNITY_EDITOR

using FIMSpace.FEditor;
using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_LevelsOfDetail : RagdollAnimatorFeatureBase
    {
        private int initialSolverIterations = 6;

        private FGenerating.FUniversalVariable[] dists = new FGenerating.FUniversalVariable[3];
        private FGenerating.FUniversalVariable[] disableHM = new FGenerating.FUniversalVariable[3];
        private FGenerating.FUniversalVariable[] solverIterations = new FGenerating.FUniversalVariable[3];
        private FGenerating.FUniversalVariable[] disableInterpolation = new FGenerating.FUniversalVariable[3];
        private FGenerating.FUniversalVariable[] onlyDiscrete = new FGenerating.FUniversalVariable[3];

        public override bool OnInit()
        {
            ParentRagdollHandler.AddToUpdateLoop( Update );

            initialSolverIterations = ParentRagdollHandler.UnitySolverIterations;

            for( int i = 0; i < 3; i++ )
            {
                int id = i + 1;
                dists[i] = InitializedWith.RequestVariable( "Dist" + id, (float)( id ) * 10f );
                disableHM[i] = InitializedWith.RequestVariable( "Hard" + id, false );
                solverIterations[i] = InitializedWith.RequestVariable( "Iter" + id, 1 + Mathf.Lerp( ParentRagdollHandler.UnitySolverIterations, 1, ( (float)id / 3f ) ) );
                disableInterpolation[i] = InitializedWith.RequestVariable( "Interp" + id, false );
                onlyDiscrete[i] = InitializedWith.RequestVariable( "Discr" + id, false );
            }

            return base.OnInit();
        }

        /// <summary> Removing used loop from the parent ragdoll handler </summary>
        public override void OnDestroyFeature()
        {
            ParentRagdollHandler.RemoveFromUpdateLoop( Update );
        }

        public virtual void Update()
        {
            float distance = CalculateDistance();

            if( distance > dists[2].GetFloat() )
            {
                ApplyLOD( 2 );
            }
            else if( distance > dists[1].GetFloat() )
            {
                ApplyLOD( 1 );
            }
            else if( distance > dists[0].GetFloat() )
            {
                ApplyLOD( 0 );
            }
            else
            {
                ApplyLOD( -1 );
            }
        }

        private int currentIndex = -1;

        private void ApplyLOD( int index )
        {
            if( currentIndex == index ) return;
            currentIndex = index;

            if( index == -1 )
            {
                ParentRagdollHandler.UnitySolverIterations = initialSolverIterations;
                ParentRagdollHandler.disableHardMatching = false;
                ParentRagdollHandler.disableInterpolation = false;
                ParentRagdollHandler.onlyDiscreteDetection = false;

                ParentRagdollHandler.RefreshAllChainsRigidbodyOptimizationParameters();
                return;
            }

            ParentRagdollHandler.UnitySolverIterations = solverIterations[index].GetInt();
            ParentRagdollHandler.disableHardMatching = disableHM[index].GetBool();
            ParentRagdollHandler.disableInterpolation = disableInterpolation[index].GetBool();
            ParentRagdollHandler.onlyDiscreteDetection = onlyDiscrete[index].GetBool();

            ParentRagdollHandler.RefreshAllChainsRigidbodyOptimizationParameters();
        }

        /// <summary>
        /// Can be overrided for custom camera distance measurement, like multiple cameras implementation
        /// </summary>
        protected virtual float CalculateDistance()
        {
            var cam = Camera.main;
            if( cam == null ) return 0f;
            float currentDistance = Vector3.Distance( ParentRagdollHandler.GetAnchorSourceBone().position, cam.transform.position );
            return currentDistance;
        }

        #region Editor GUI Code

#if UNITY_EDITOR
        public override bool Editor_DisplayEnableSwitch => false;
        public override string Editor_FeatureDescription => "This feature is handling Ragdoll Animator's physics settings switching for distance based optimization.";
#endif

#if UNITY_EDITOR

        public override void Editor_InspectorGUI( SerializedProperty toDirty, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            EditorGUILayout.HelpBox( "Below LOD1 distance, there will be applied initial Ragdoll Animator settings!", UnityEditor.MessageType.Info );
            GUILayout.Space( 3 );

            for( int i = 1; i <= 3; i++ )
            {
                DrawLOD( i, ragdollHandler, helper );
                if( i != 3 ) FGUI_Inspector.DrawUILineCommon();
            }

            if( ragdollHandler.WasInitialized )
            {
                GUILayout.Space( 3 );
                EditorGUILayout.LabelField( "Current LOD Level: " + ( currentIndex + 1 ) );
            }
            else
            {
                if( ragdollHandler.GetExtraFeature<RAF_Optimize>() == null )
                    EditorGUILayout.HelpBox( "Consider using 'Optimize' feature in combine with this feature", MessageType.None );
            }
        }

        public override void Editor_OnSceneGUI( RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            int id = 1;
            var distV = helper.RequestVariable( "Dist" + id, (float)id * 10f );
            Handles.color = Color.green * 0.6f;
            Handles.CircleHandleCap( 0, ragdollHandler.GetBaseTransform().position, Quaternion.Euler( 90f, 0f, 0f ), distV.GetFloat(), EventType.Repaint );
            id = 2;
            distV = helper.RequestVariable( "Dist" + id, (float)id * 10f );
            Handles.color = Color.yellow * 0.6f;
            Handles.CircleHandleCap( 0, ragdollHandler.GetBaseTransform().position, Quaternion.Euler( 90f, 0f, 0f ), distV.GetFloat(), EventType.Repaint );
            id = 3;
            distV = helper.RequestVariable( "Dist" + id, (float)id * 10f );
            Handles.color = Color.red * 0.6f;
            Handles.CircleHandleCap( 0, ragdollHandler.GetBaseTransform().position, Quaternion.Euler( 90f, 0f, 0f ), distV.GetFloat(), EventType.Repaint );
        }

        private void DrawLOD( int id, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            var distV = helper.RequestVariable( "Dist" + id, (float)id * 10f );
            var disableHardMatchV = helper.RequestVariable( "Hard" + id, false );
            var solverIterationsV = helper.RequestVariable( "Iter" + id, 1 + Mathf.Lerp( ragdollHandler.UnitySolverIterations, 1, ( (float)id / 3f ) ) );

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField( "LOD " + id + ":", EditorStyles.boldLabel, GUILayout.Width( 50 ) );
            GUILayout.Space( 6 );

            EditorGUIUtility.labelWidth = 78;
            distV.SetValue( EditorGUILayout.FloatField( new GUIContent( "Distance:", "LOD level will be applied when distance is greater or equal this value" ), distV.GetFloat() ) );
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 178;
            disableHardMatchV.SetValue( EditorGUILayout.Toggle( "Disable Hard Matching:", disableHardMatchV.GetBool() ) );
            EditorGUIUtility.labelWidth = 178;
            solverIterationsV.SetValue( EditorGUILayout.IntSlider( "Solver Iterations:", solverIterationsV.GetInt(), 1, 16 ) );

            var disableInterpV = helper.RequestVariable( "Interp" + id, false );
            var onlyDiscr = helper.RequestVariable( "Discr" + id, false );

            EditorGUILayout.BeginHorizontal();
            disableInterpV.SetValue( EditorGUILayout.Toggle( "Disable Interpolation:", disableInterpV.GetBool() ) );
            onlyDiscr.SetValue( EditorGUILayout.Toggle( "Only Discrete Collisions:", onlyDiscr.GetBool() ) );
            EditorGUILayout.EndHorizontal();
        }

#endif

        #endregion Editor GUI Code
    }
}