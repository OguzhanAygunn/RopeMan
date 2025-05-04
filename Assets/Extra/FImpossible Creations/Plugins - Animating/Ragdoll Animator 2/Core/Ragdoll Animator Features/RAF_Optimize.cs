#if UNITY_EDITOR

using FIMSpace.FEditor;
using UnityEditor;

#endif

using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_Optimize : RagdollAnimatorFeatureBase
    {
        protected List<Renderer> visibilityMeshes;

        protected FGenerating.FUniversalVariable distanceV;
        protected FGenerating.FUniversalVariable enterThresholdV;
        protected FGenerating.FUniversalVariable fadeSpeedV;
        protected FGenerating.FUniversalVariable storeCalibrateV;

#if UNITY_EDITOR
        protected FGenerating.FUniversalVariable visibilityDebug;

        /// <summary> Only for the debug mode </summary>
        public Plane[] currentFrustumPlanes = null;

#endif

        private RagdollHandler.OptimizationHandler lodHandler;

        public override bool OnInit()
        {
            ParentRagdollHandler.AddToAlwaysUpdateLoop( Update );

            distanceV = InitializedWith.RequestVariable( "Max Distance:", 0f );
            enterThresholdV = InitializedWith.RequestVariable( "Enter Threshold:", 2f );

#if UNITY_EDITOR
            visibilityDebug = InitializedWith.RequestVariable( "Visibility Debug", false );
#endif
            fadeSpeedV = InitializedWith.RequestVariable( "Fade Speed", 1f );
            storeCalibrateV = InitializedWith.RequestVariable( "Store Pose", false );

            lodHandler = new RagdollHandler.OptimizationHandler( ParentRagdollHandler );

            visibilityMeshes = new List<Renderer>();

            foreach( var rend in InitializedWith.customObjectList )
            {
                Renderer renderer = rend as Renderer;
                if( renderer ) visibilityMeshes.Add( renderer );
            }

            return base.OnInit();
        }

        /// <summary> Removing used loop from the parent ragdoll handler </summary>
        public override void OnDestroyFeature()
        {
            ParentRagdollHandler.RemoveFromAlwaysUpdateLoop( Update );
        }

        public virtual void Update()
        {
            bool turnOn = CalculateShouldBeTurnedOn();

            if( turnOn )
            {
                if( storeCalibrateV.GetBool() ) ParentRagdollHandler.StoreCalibrationPose();

                lodHandler.TurnOnTick( Time.unscaledDeltaTime * fadeSpeedV.GetFloat() );
            }
            else
                lodHandler.TurnOffTick( Time.unscaledDeltaTime * fadeSpeedV.GetFloat() );
        }

        protected bool CalculateShouldBeTurnedOn()
        {
            if( CalculateMeshVisibilityRequirement() == false ) return false;
            if( CalculateCameraDistanceRequirement() == false ) return false;

            return true;
        }

        /// <summary>
        /// Can be overrided for custom mesh visibility calculations
        /// return true if ragdoll animator is in visibility range
        /// </summary>
        protected virtual bool CalculateMeshVisibilityRequirement()
        {
            bool isVisible = false;

            #region Editor Debug Mode for mesh visibility check only in Game View

#if UNITY_EDITOR
#if UNITY_2017_4_OR_NEWER

            if( visibilityDebug.GetBool() )
            {
                Camera mainC = Camera.main;
                if( mainC == null ) return true;
                if( currentFrustumPlanes == null ) currentFrustumPlanes = new Plane[6];

                GeometryUtility.CalculateFrustumPlanes( mainC, currentFrustumPlanes );

                foreach( var renerer in visibilityMeshes )
                {
                    if( GeometryUtility.TestPlanesAABB( currentFrustumPlanes, renerer.bounds ) ) { isVisible = true; break; }
                }

                return isVisible;
            }

#endif
#endif

            #endregion Editor Debug Mode for mesh visibility check only in Game View

            if( visibilityMeshes.Count == 0 ) isVisible = true;
            else
            {
                foreach( var renderer in visibilityMeshes ) if( renderer.isVisible ) { isVisible = true; break; }
            }

            return isVisible;
        }

        /// <summary>
        /// Can be overrided for custom camera distance measurement, like multiple cameras implementation
        /// return true if ragdoll animator is in visibility range
        /// </summary>
        protected virtual bool CalculateCameraDistanceRequirement()
        {
            var cam = Camera.main;
            if( cam == null ) return true;
            if( distanceV.GetFloat() <= 0f ) return true;

            float currentDistance = Vector3.Distance( ParentRagdollHandler.GetAnchorSourceBone().position, cam.transform.position );

            if( currentDistance < distanceV.GetFloat() )
            {
                closeEnough = true;
                return true;
            }
            else if( currentDistance > distanceV.GetFloat() + enterThresholdV.GetFloat() )
            {
                closeEnough = false;
                return false;
            }

            return closeEnough;
        }

        /// <summary> Helper bool for thresholded range </summary>
        protected bool closeEnough = true;

        #region Editor GUI Code

#if UNITY_EDITOR
        public override bool Editor_DisplayEnableSwitch => false;
        public override string Editor_FeatureDescription => "This feature is handling Ragdoll Animator optimization when character is offscreen or far away the main camera.";
#endif

#if UNITY_EDITOR

        public override void Editor_InspectorGUI( SerializedProperty toDirty, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            GUILayout.Space( 3 );

            var fadeSpeedV = helper.RequestVariable( "Fade Speed", 1f );
            fadeSpeedV.AssignTooltip( "How fast should happen transition from disabled/enabled state" );
            fadeSpeedV.SetMinMaxSlider( 0.5f, 10f );
            fadeSpeedV.Editor_DisplayVariableGUI();

            var distanceV = helper.RequestVariable( "Max Distance:", 0f );

            if( distanceV.GetFloat() <= 0f )
            {
                float val = 0f;
                EditorGUILayout.BeginHorizontal();
                val = EditorGUILayout.FloatField( "Max Distance:", val );
                EditorGUILayout.LabelField( "(not using)", EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth( 60 ) );
                EditorGUILayout.EndHorizontal();
                distanceV.SetValue( val );
            }
            else
            {
                if( ragdollHandler.WasInitialized )
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 100;

                    distanceV.Editor_DisplayVariableGUI();

                    if( Camera.main == null )
                    {
                        EditorGUILayout.HelpBox( "No Main Camera!", MessageType.Error );
                    }
                    else
                    {
                        EditorGUILayout.ObjectField( Camera.main, typeof( Camera ), true, GUILayout.MaxWidth( 50 ) );
                        EditorGUILayout.LabelField( "Distance:", GUILayout.MaxWidth( 60 ) );
                        EditorGUILayout.LabelField( System.Math.Round( Vector3.Distance( Camera.main.transform.position, ParentRagdollHandler.GetAnchorSourceBone().position ), 1 ).ToString(), EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth( 40 ) );

                        if( CalculateCameraDistanceRequirement() )
                            EditorGUILayout.LabelField( "Ok", EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth( 20 ) );
                        else
                            EditorGUILayout.LabelField( "Too Far", EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth( 40 ) );
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUIUtility.labelWidth = 0;
                }
                else
                {
                    distanceV.Editor_DisplayVariableGUI();
                }

                EditorGUI.indentLevel++;
                var enterV = helper.RequestVariable( "Enter Threshold:", 2f );
                enterV.AssignTooltip( "When character entered min distance range, it needs to go out for max distance + this threshold value in order to be in 'far away' range. Preventing sudden On/Off when character is roaming around max distance." );
                float val = enterV.GetFloat();
                val = EditorGUILayout.FloatField( "Enter Threshold:", val );
                if( val < 0f ) val = 0f;
                enterV.SetValue( val );
                EditorGUI.indentLevel--;
            }

            GUILayout.Space( 5 );

            GUI.enabled = !ragdollHandler.WasInitialized;

            if( helper.customObjectList == null ) helper.customObjectList = new List<Object>();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField( "Character's Visibility Meshes:", EditorStyles.boldLabel );
            GUILayout.FlexibleSpace();

            var visibilityDebug = helper.RequestVariable( "Visibility Debug", false );
            if( visibilityDebug.GetBool() ) GUI.backgroundColor = Color.green;

            GUI.enabled = true;
            if( GUILayout.Button( new GUIContent( FGUI_Resources.Tex_Debug, "Use only main camera for visibility detection to debug optimization in scene view (not included when building game)" ), FGUI_Resources.ButtonStyle, GUILayout.Height( 18 ) ) )
            {
                visibilityDebug.SetValue( !visibilityDebug.GetBool() );
            }
            GUI.backgroundColor = Color.white;
            GUI.enabled = !ragdollHandler.WasInitialized;

            if( GUILayout.Button( "+", FGUI_Resources.ButtonStyle, GUILayout.Width( 24 ) ) ) helper.customObjectList.Add( null );

            EditorGUILayout.EndHorizontal();

            if( visibilityDebug.GetBool() )
            {
                EditorGUILayout.HelpBox( "Debug Mode -> Only Main Game Camera Visibility Detection", MessageType.None );
            }

            int toRemove = -1;
            for( int i = 0; i < helper.customObjectList.Count; i++ )
            {
                bool draw = helper.customObjectList[i] == null || ( helper.customObjectList[i] is Renderer );
                if( draw == false ) continue; // Wrong type in list

                EditorGUILayout.BeginHorizontal();

                helper.customObjectList[i] = EditorGUILayout.ObjectField( helper.customObjectList[i], typeof( Renderer ), true ) as Renderer;
                Renderer r = helper.customObjectList[i] as Renderer;

                if( r && ragdollHandler.WasInitialized )
                {
                    if( visibilityDebug.GetBool() && currentFrustumPlanes != null )
                    {
                        bool isVisible = GeometryUtility.TestPlanesAABB( currentFrustumPlanes, r.bounds );
                        EditorGUILayout.LabelField( isVisible ? "Visible" : "Not Visible", EditorStyles.helpBox, GUILayout.MaxWidth( 60 ) );
                    }
                    else
                        EditorGUILayout.LabelField( r.isVisible ? "Visible" : "Not Visible", EditorStyles.helpBox, GUILayout.MaxWidth( 60 ) );
                }

                FGUI_Inspector.RedGUIBackground();
                GUILayout.FlexibleSpace();
                if( GUILayout.Button( FGUI_Resources.GUIC_Remove, FGUI_Resources.ButtonStyle, GUILayout.Height( 18 ) ) ) toRemove = i;
                FGUI_Inspector.RestoreGUIBackground();
                EditorGUILayout.EndHorizontal();
            }

            if( toRemove > -1 )
            {
                helper.customObjectList.RemoveAt( toRemove );
                EditorUtility.SetDirty( toDirty.serializedObject.targetObject );
            }

            GUI.enabled = true;
            var storeCalibrateV = helper.RequestVariable( "Store Pose", false );
            storeCalibrateV.AssignTooltip( "Storing animation pose if animator is visible, to prevent offscreen pose calibration issues" );
            storeCalibrateV.Editor_DisplayVariableGUI();
            GUI.enabled = !ragdollHandler.WasInitialized;

            if( ragdollHandler.WasInitialized )
            {
                if( CalculateShouldBeTurnedOn() )
                    EditorGUILayout.LabelField( "Current State: Enabled" );
                else
                    EditorGUILayout.LabelField( "Current State: Disabled" );
            }
            else
            {
                EditorGUILayout.LabelField( "Runtime you will see here debug values", EditorStyles.centeredGreyMiniLabel );

                if( ragdollHandler.GetExtraFeature<RAF_LevelsOfDetail>() == null )
                    EditorGUILayout.HelpBox( "Consider using 'Levels Of Details' feature in combine with this feature", MessageType.None );
            }

            GUI.enabled = true;
        }

        public override void Editor_OnSceneGUI( RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            var distanceV = helper.RequestVariable( "Max Distance:", 0f );
            var enterV = helper.RequestVariable( "Enter Threshold:", 2f );

            if( distanceV.GetFloat() > 0f )
            {
                Handles.color = Color.green;
                Handles.CircleHandleCap( 0, ragdollHandler.GetBaseTransform().position, Quaternion.Euler( 90f, 0f, 0f ), distanceV.GetFloat(), EventType.Repaint );

                if( enterV.GetFloat() > 0f )
                {
                    Handles.color = Color.yellow * 0.7f;
                    Handles.CircleHandleCap( 0, ragdollHandler.GetBaseTransform().position, Quaternion.Euler( 90f, 0f, 0f ), distanceV.GetFloat() + enterV.GetFloat(), EventType.Repaint );
                }
            }

            Handles.color = Color.blue;

            foreach( var item in helper.customObjectList )
            {
                Renderer rend = item as Renderer;
                if( rend ) Handles.SphereHandleCap( 0, rend.transform.position, rend.transform.rotation, 0.1f, EventType.Repaint );
            }

            Handles.color = Color.white;
        }

#endif

        #endregion Editor GUI Code
    }
}