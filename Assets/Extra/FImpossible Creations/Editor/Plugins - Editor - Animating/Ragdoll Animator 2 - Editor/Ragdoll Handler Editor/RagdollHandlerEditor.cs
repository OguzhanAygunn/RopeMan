using FIMSpace.FEditor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static partial class RagdollHandlerEditor
    {
        private static SerializedObject lastOwner = null;
        private static SerializedProperty lastHandler = null;
        private static readonly Dictionary<string, SerializedProperty> properties = new Dictionary<string, SerializedProperty>();
        public static Texture _tex_ragdoll
        { get { return FGUI_Resources.FindIcon( "Ragdoll Animator/SPR_RagdollAnim2" ); } }
        public static Texture _tex_ragdollSmall
        { get { return FGUI_Resources.FindIcon( "Ragdoll Animator/SPR_RagdollAnim2s" ); } }
        public static bool RequestRepaint = true;
        public static bool? DisplayCopyOtherRagdollAnimatorSettings = false;
        public static bool? DisplayLoadRagdollPreset = false;
        private static RagdollAnimator2 copyChainsSetupOf = null;

        private static SerializedProperty GetProperty( string name )
        {
            if( lastOwner == null ) return null;
            if( properties.ContainsKey( name ) ) return properties[name];
            var prop = lastHandler.FindPropertyRelative( name );
            if( prop == null ) return null;
            properties.Add( name, prop );
            return prop;
        }

        public static void OnChange( SerializedProperty ragdollHandlerProp, RagdollHandler handler )
        {
            if( ragdollHandlerProp != null )
            {
                if( ragdollHandlerProp.serializedObject != null ) if( ragdollHandlerProp.serializedObject.targetObject != null ) EditorUtility.SetDirty( ragdollHandlerProp.serializedObject.targetObject );
            }
            else
            {
                if( handler != null )
                {
                    if( handler.Caller != null )
                    {
                        EditorUtility.SetDirty( handler.Caller );
                    }
                    else if( handler.GetBaseTransform() != null )
                    {
                        EditorUtility.SetDirty( handler.GetBaseTransform() );
                    }
                }
            }

            RequestRepaint = true;

            handler.Editor_CheckToStopPreviewingAll();
        }

        public static void RefreshBaseReferences( SerializedProperty ragdollHandlerProp )
        {
            SerializedObject owner = ragdollHandlerProp.serializedObject;
            if( owner != lastOwner ) properties.Clear();

            lastHandler = ragdollHandlerProp;
            lastOwner = ragdollHandlerProp.serializedObject;
        }

        public static RagdollHandler.EReferencePoseReport? _referencePoseReport = null;

        private static void DisplayStoreTPoseButton( RagdollHandler handler, SerializedProperty handlerProp, bool drawInfo = true )
        {
            if( handler.WasInitialized ) return;
            GUILayout.Space( 14 );

            RagdollHandler.EReferencePoseReport refPoseReport;
            if( _referencePoseReport == null ) _referencePoseReport = handler.ValidateReferencePose();
            refPoseReport = _referencePoseReport.Value;

            EditorGUILayout.BeginHorizontal();

            if( refPoseReport != RagdollHandler.EReferencePoseReport.ReferencePoseOK )
            {
                GUI.backgroundColor = Color.Lerp( new Color( 1f, 0.8f, 0.6f, 1f ), new Color( 0.5f, 1f, 0.5f, 1f ), ( 1f + Mathf.Sin( (float)EditorApplication.timeSinceStartup * 3.5f ) ) * 0.5f );
            }
            else
                GUI.backgroundColor = new Color( 0.7f, 0.7f, 0.7f, 1f );

            if( GUILayout.Button( new GUIContent( "  Store Reference T-Pose ", FGUI_Resources.Tex_Save, "Reference T-Pose can help ragdoll animator to initialize itself with more precision for target animation matching\nYou should store it when all bones are added in the construct.\n\nRefernce T-Pose is most important for the ragdoll Reconstruction mode. (can be enabled in the Extra Features)" ), FGUI_Resources.ButtonStyle, GUILayout.Height( 24 ) ) )
            {
                handler.StoreReferenceTPose();
                OnChange( handlerProp, handler );
                _referencePoseReport = handler.ValidateReferencePose();
            }

            GUI.backgroundColor = Color.white;

            if( refPoseReport == RagdollHandler.EReferencePoseReport.NoReferencePose )
            {
                GUILayout.Space( 6 );
                GUI.backgroundColor = new Color( 1f, 1f, 0.2f, 1f );
                if( GUILayout.Button( new GUIContent( FGUI_Resources.Tex_Warning, _info_NoRefPose ), EditorStyles.label, w30h22 ) ) EditorUtility.DisplayDialog( "Reference Pose Info", _info_NoRefPose, "Ok" );
            }
            else if( refPoseReport == RagdollHandler.EReferencePoseReport.ReferencePoseChanged )
            {
                GUILayout.Space( 6 );
                GUI.backgroundColor = new Color( 1f, 1f, 0.5f, 1f );
                if( GUILayout.Button( new GUIContent( FGUI_Resources.TexWaitIcon, _info_ChangedRefPose ), EditorStyles.label, w30h22 ) ) EditorUtility.DisplayDialog( "Reference Pose Info", _info_ChangedRefPose, "Ok" );
            }
            else if( refPoseReport == RagdollHandler.EReferencePoseReport.ReferencePoseError )
            {
                GUILayout.Space( 6 );
                GUI.backgroundColor = new Color( 1f, 0.4f, 0.4f, 1f );
                if( GUILayout.Button( new GUIContent( FGUI_Resources.Tex_Error, _info_ErrorRefPose ), EditorStyles.label, w30h22 ) ) EditorUtility.DisplayDialog( "Reference Pose Info", _info_ErrorRefPose, "Ok" );
            }
            else if( refPoseReport == RagdollHandler.EReferencePoseReport.ReferencePoseOK )
            {
                GUILayout.Space( 6 );
                if( GUILayout.Button( new GUIContent( FGUI_Resources.FindIcon( "Fimp/Small Icons/FCorrect" ) ), EditorStyles.label, w30h22 ) ) EditorUtility.DisplayDialog( "Reference Pose Info", _info_OKRefPose, "Ok" );
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            if( refPoseReport != RagdollHandler.EReferencePoseReport.ReferencePoseOK )
                if( drawInfo )
                    EditorGUILayout.HelpBox( "When all bones of the ragdoll are added, it's recommended to store the T-Pose of the character", MessageType.None );
        }

        private static void DisplayPreGenerateDummyButton( RagdollHandler handler, SerializedProperty handlerProp )
        {
            if( handler.WasInitialized ) return;

            GUILayout.Space( 6 );
            if( handler.DummyWasGenerated ) GUI.backgroundColor = new Color( .5f, 1f, 0.5f, 1f );

            EditorGUILayout.BeginHorizontal();

            if( GUILayout.Button( FGUI_Resources.GUIC_More, EditorStyles.label, w22h18 ) )
            {
                GenericMenu_ReferencePoseOptions( handler );
            }

            if( DisplayCopyOtherRagdollAnimatorSettings != false )
            {
                if( DisplayCopyOtherRagdollAnimatorSettings == true )
                {
                    EditorGUIUtility.labelWidth = 76;
                    copyChainsSetupOf = EditorGUILayout.ObjectField( " Copy from:", copyChainsSetupOf, typeof( RagdollAnimator2 ), true ) as RagdollAnimator2;
                    if( copyChainsSetupOf != null )
                    {
                        ApplyToAllSelectedRagdollAnimators( ( RagdollHandler h ) => { h.CopyChainsSettingsOf( copyChainsSetupOf.Handler ); OnChange( null, h ); }, handler );
                        copyChainsSetupOf = null;
                        DisplayCopyOtherRagdollAnimatorSettings = null;

                        OnChange( handlerProp, handler );
                    }

                    EditorGUIUtility.labelWidth = 0;
                }
                else
                {
                    GUI.backgroundColor = Color.green;
                    if( GUILayout.Button( "Construct Settings Copied!" ) ) { DisplayCopyOtherRagdollAnimatorSettings = false; }
                    GUI.backgroundColor = Color.white;
                }

                GUILayout.Space( 12 );
            }
            else if (DisplayLoadRagdollPreset != false)
            {
                if( DisplayLoadRagdollPreset == true )
                {
                    EditorGUIUtility.labelWidth = 100;
                    RagdollAnimator2Preset presetSettings = null;
                    presetSettings = EditorGUILayout.ObjectField( " Load Settings:", presetSettings, typeof( RagdollAnimator2Preset ), true ) as RagdollAnimator2Preset;
                    
                    if( presetSettings != null )
                    {
                        ApplyToAllSelectedRagdollAnimators( ( RagdollHandler h ) => { presetSettings.Settings.ApplyAllPropertiesToOtherRagdoll( h ); OnChange( null, h ); }, handler );
                        DisplayLoadRagdollPreset = null;
                        OnChange( handlerProp, handler );
                    }

                    EditorGUIUtility.labelWidth = 0;
                }
                else
                {
                    GUI.backgroundColor = Color.green;
                    if( GUILayout.Button( "Preset Settings Applied!" ) ) { DisplayLoadRagdollPreset = false; }
                    GUI.backgroundColor = Color.white;
                }

                GUILayout.Space( 12 );
            }

            if( DisplayCopyOtherRagdollAnimatorSettings != false || DisplayLoadRagdollPreset != false )
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space( 24 );
            }

            if( handler.DummyWasGenerated || handler.RagdollLogic == ERagdollLogic.JustBoneComponents ) GUI.enabled = false;
            if( GUILayout.Button( new GUIContent( handler.DummyWasGenerated ? "  Now Using Pre-Generated Dummy" : "  Use Pre-Generated Dummy ", FGUI_Resources.Tex_Movement, "If you want to customize ragdoll dummy with extra components or colliders, you can generate it on the scene before entering playmode." ), FGUI_Resources.ButtonStyle, GUILayout.Height( 20 ) ) )
            {
                handler.SwitchPreGeneratedDummy();
                OnChange( handlerProp, handler );
            }

            if( handler.DummyWasGenerated )
            {
                GUI.enabled = true;

                FGUI_Inspector.RedGUIBackground();
                if( GUILayout.Button( FGUI_Resources.GUIC_Remove, FGUI_Resources.ButtonStyle, w22h18 ) )
                {
                    handler.SwitchPreGeneratedDummy();
                    OnChange( handlerProp, handler );
                }

                FGUI_Inspector.RestoreGUIBackground();
            }

            GUI.backgroundColor = Color.white;

            GUILayout.Space( 24 );
            EditorGUILayout.EndHorizontal();

            if( handler.DummyWasGenerated )
            {
                GUI.enabled = false;
                EditorGUILayout.ObjectField( handler.Dummy_Container, typeof( Transform ), true );
                GUI.enabled = true;
            }

            DisplayStoreTPoseButton( handler, handlerProp );
        }

        private static void ApplyToAllSelectedRagdollAnimators(Action<RagdollHandler> action, RagdollHandler mainHandler)
        {
            var selected = Selection.gameObjects;

            if( mainHandler != null ) action.Invoke( mainHandler );

            for( int i = 0; i < selected.Length; i++ )
            {
                IRagdollAnimator2HandlerOwner handler = selected[i].GetComponent<IRagdollAnimator2HandlerOwner>();
                if( handler == null ) continue;
                if( handler.GetRagdollHandler == mainHandler ) continue;
                action.Invoke( handler.GetRagdollHandler );
            }
        }

        private static string _info_NoRefPose => "Not yet stored reference pose for the ragdoll dummy processor. It's recommended to store reference pose when you finish adding all character bones, during the dummy constructor setup stage.";
        private static string _info_ChangedRefPose => "Bones structure of the reference pose changed. You probably need to update the reference pose.";
        private static string _info_ErrorRefPose => "Bones structure of the reference pose detected errors. Probably some bone references was lost. Check the skeleton structure to find source of the problem.";
        private static string _info_OKRefPose => "Checked correctness of Reference T-Pose and no error was detected.";


        public static bool IsRightMouseButton()
        {
            if( UnityEngine.Event.current == null ) return false;

            if( UnityEngine.Event.current.type == UnityEngine.EventType.Used )
                if( UnityEngine.Event.current.button == 1 )
                    return true;

            return false;
        }

        private static int _stupidUnityTransformsChangingDelayCounterWhenUsingGenericMenu = 0;

        public static void OnDrawingGUI()
        {
            if( _ActionsToCall != null )
            {
                if( _stupidUnityTransformsChangingDelayCounterWhenUsingGenericMenu < 3 )
                {
                    if( _ActionsToCall.Count > 0 ) _stupidUnityTransformsChangingDelayCounterWhenUsingGenericMenu += 1;
                    return;
                }

                for( int i = 0; i < _ActionsToCall.Count; i++ )
                {
                    if( _ActionsToCall[i] == null ) continue;
                    _ActionsToCall[i].Invoke();
                }

                _ActionsToCall.Clear();
                _stupidUnityTransformsChangingDelayCounterWhenUsingGenericMenu = 0;
            }
        }
    }
}