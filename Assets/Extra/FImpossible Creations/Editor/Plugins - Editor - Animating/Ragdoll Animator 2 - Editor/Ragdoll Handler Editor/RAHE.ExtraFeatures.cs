using FIMSpace.FEditor;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif


namespace FIMSpace.FProceduralAnimation
{
    public static partial class RagdollHandlerEditor
    {
        public static void GUI_DrawExtraCategory( SerializedProperty ragdollHandlerProp, RagdollHandler handler )
        {
            if( handler.RagdollLogic == ERagdollLogic.JustBoneComponents )
            {
                EditorGUILayout.HelpBox( "When using Just Bone Components ragdoll logic. Motion category is useless.", UnityEditor.MessageType.Info );
                GUI.enabled = false;
            }

            bool preGuiE = GUI.enabled;

            RefreshBaseReferences( ragdollHandlerProp );
            EditorGUILayout.BeginVertical( FGUI_Resources.BGInBoxBlankStyle );

            EditorGUILayout.BeginHorizontal();

            handler.UseExtraFeatures = EditorGUILayout.Toggle( handler.UseExtraFeatures, GUILayout.Width( 18 ) );
            if( GUILayout.Button( "Customize Ragdoll with Extra Features", FGUI_Resources.HeaderStyle ) )
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem( new GUIContent( "Copy All Extra Features" ), false, () => { RagdollAnimatorFeatureBase.Editor_CopyFeaturesSetup( handler ); } );

                if( RagdollAnimatorFeatureBase.IsPasteFeaturesSetupPossible( handler ) != null )
                {
                    menu.AddItem( new GUIContent( "Paste All Extra Features" ), false, () => { RagdollAnimatorFeatureBase.Editor_PasteFeaturesSetup( handler ); OnChange( ragdollHandlerProp, handler ); } );
                }

                menu.ShowAsContext();
            }


            if( GUILayout.Button( new GUIContent( FGUI_Resources.Tex_SearchDirectory, "Select available extra feature, to be added to this Ragdoll Animator" ), FGUI_Resources.ButtonStyle, GUILayout.Width( 44 ), GUILayout.Height( 18 ) ) )
            {
                GenericMenu_ExtraFeatures( handler, ragdollHandlerProp );
            }

            if( preGuiE ) GUI.enabled = !Application.isPlaying;

            if( GUILayout.Button( new GUIContent( " + ", "Add field for a new Ragdoll Animator Feature" ), FGUI_Resources.ButtonStyle, GUILayout.Width( 22 ), GUILayout.Height( 18 ) ) )
            {
                RagdollAnimatorFeatureHelper helper = new RagdollAnimatorFeatureHelper();
                handler.ExtraFeatures.Add( helper );
            }

            GUI.enabled = preGuiE;

            EditorGUILayout.EndHorizontal();

            FGUI_Inspector.DrawUILineCommon( 8 );

            if( handler.ExtraFeatures.Count == 0 )
            {
                EditorGUILayout.LabelField( "No Features Added Yet", EditorStyles.centeredGreyMiniLabel );
            }
            else if( handler.UseExtraFeatures == false )
            {
                EditorGUILayout.LabelField( "Disabling calculations on all the features, but initialization will be called.", EditorStyles.centeredGreyMiniLabel );
                GUI.color = new Color( 1f, 1f, 1f, 0.65f );
                ExtraFeatures_DisplayFeaturesList( handler, ragdollHandlerProp );
                GUI.color = Color.white;
            }
            else
            {
                ExtraFeatures_DisplayFeaturesList( handler, ragdollHandlerProp );
            }

            EditorGUILayout.EndVertical();
        }

        private static void GenericMenu_ExtraFeatures( RagdollHandler handler, SerializedProperty handlerProp )
        {
            if( handler == null )
            {
                EditorUtility.DisplayDialog( "Not Found Presets Directory!", "Can't find Ragdoll Features Presets directory. You probably removed it from the project. Please try importing the Ragdoll Animator plugin again.", "Ok" );
                return;
            }

            string path = AssetDatabase.GetAssetPath( RagdollAnimator2Editor.GetExtraFeaturesDirectory );
            var files = System.IO.Directory.GetFiles( path, "*.asset" );

            if( files != null )
            {
                if( files.Length == 0 )
                {
                    EditorUtility.DisplayDialog( "Not Found Presets in the Directory!", "Can't find Ragdoll Features Preset files. You probably removed them from the project. Please try importing the Ragdoll Animator plugin again.", "Ok" );
                    return;
                }

                // Reorder
                for( int i = files.Length - 1; i >= 0; i-- )
                {
                    if( System.IO.Path.GetFileNameWithoutExtension( files[i] ).Contains( "_" ) )
                    {
                        for( int o = files.Length - 1; o >= 0; o-- )
                            if( !System.IO.Path.GetFileNameWithoutExtension( files[o] ).Contains( "_" ) )
                            {
                                string swap = files[o];
                                files[o] = files[i];
                                files[i] = swap;
                                break;
                            }
                    }
                }

                GenericMenu draftsMenu = new GenericMenu();

                for( int i = 0; i < files.Length; i++ )
                {
                    RagdollAnimatorFeatureBase modl = AssetDatabase.LoadAssetAtPath<RagdollAnimatorFeatureBase>( files[i] );
                    if( modl )
                    {
                        string displayName = modl.name;
                        displayName = displayName.Replace( "_", "/" );

                        draftsMenu.AddItem( new GUIContent( displayName ), false, (GenericMenu.MenuFunction)( () =>
                        {
                            handler.AddRagdollFeature( modl ); ;
                            RagdollHandler._Editor_selectedModuleIndex = handler.ExtraFeatures.Count - 1;
                            OnChange( handlerProp, handler );
                        } ) );
                    }
                }

                draftsMenu.ShowAsContext();
            }
        }

        private static int _customModuleToRemove = -1;

        private static void ExtraFeatures_DisplayFeaturesList( RagdollHandler handler, SerializedProperty handlerProp )
        {
            for( int i = 0; i < handler.ExtraFeatures.Count; i++ )
            {
                RagdollAnimatorFeatureHelper feature = handler.ExtraFeatures[i];

                if( RagdollHandler._Editor_selectedModuleIndex == i ) EditorGUILayout.BeginVertical( FGUI_Resources.BGInBoxStyle );
                else EditorGUILayout.BeginVertical( FGUI_Resources.BGInBoxBlankStyle );

                ExtraFeatures_DisplayFeatureField( handler, i, feature, handlerProp );

                if( RagdollHandler._Editor_selectedModuleIndex == i )
                {
                    FGUI_Inspector.DrawUILineCommon();
                    ExtraFeatures_DisplaySelectedFeaturePanel( handlerProp, handler, feature );
                }

                EditorGUILayout.EndVertical();
            }

            if( _customModuleToRemove > -1 )
            {
                if ( _customModuleToRemove >= handler.ExtraFeatures.Count)
                {
                    _customModuleToRemove = -1;
                    return;
                }

                var removing = handler.ExtraFeatures[_customModuleToRemove];
                handler.ExtraFeatures.RemoveAt( _customModuleToRemove );

                if( removing != null && removing.ActiveFeature != null )
                {
                    removing.ActiveFeature.Editor_OnRemoveFeatureInEditorGUI( handler, removing );
                    if( handler.WasInitialized ) removing.ActiveFeature.OnDestroyFeature();
                }

                _customModuleToRemove = -1;
                OnChange( handlerProp, handler );
            }
        }

        private static void ExtraFeatures_DisplayFeatureField( RagdollHandler handler, int index, RagdollAnimatorFeatureHelper featureHandler, SerializedProperty handlerProp )
        {
            string fullName = "Null";
            string disp = "";
            int wdth = 22;

            featureHandler.Editor_AssignHandler( handler );

            if( featureHandler.FeatureReference == null )
            {
                if( index > -1 ) disp = index.ToString();
            }
            else
            {
                fullName = featureHandler.FeatureReference.name;

                if( string.IsNullOrWhiteSpace( featureHandler.formattedName ) )
                {
                    int ind = featureHandler.FeatureReference.name.IndexOf( "_" );

                    if( ind > 0 )
                        featureHandler.formattedName = featureHandler.FeatureReference.name.Substring( ind + 1, featureHandler.FeatureReference.name.Length - ( ind + 1 ) );
                    else
                        featureHandler.formattedName = featureHandler.FeatureReference.name;

                    if( featureHandler.formattedName.Length > 24 )
                    {
                        featureHandler.formattedName = featureHandler.formattedName.Substring( 0, 20 ) + "...";
                    }
                }

                disp = featureHandler.formattedName;
                wdth = 170;
            }

            EditorGUILayout.BeginHorizontal();

            if( featureHandler.FeatureReference != null )
            {
                if( featureHandler.FeatureReference.Editor_DisplayEnableSwitch )
                    featureHandler.Enabled = EditorGUILayout.Toggle( featureHandler.Enabled, GUILayout.Width( 18 ) );
            }

            GUILayout.Space( 4 );

            if( RagdollHandler._Editor_selectedModuleIndex == index ) GUI.backgroundColor = Color.green;

            if( string.IsNullOrWhiteSpace( featureHandler.CustomName ) == false ) disp = featureHandler.CustomName;

            if( GUILayout.Button( new GUIContent( disp, fullName ), FGUI_Resources.ButtonStyle, GUILayout.MaxWidth( wdth ), GUILayout.Height( 18 ) ) )
            {
                if( RagdollHandlerEditor.IsRightMouseButton() )
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem( new GUIContent( "Set Custom Name" ), false, () => { featureHandler.Editor_RenamePopup(); } );

                    if( featureHandler.FeatureReference )
                    {
                        menu.AddItem( new GUIContent( "Ping Feature Instance" ), false, () => { EditorGUIUtility.PingObject( featureHandler.FeatureReference ); } );
                        menu.AddItem( new GUIContent( "Open Feature's Source Code" ), false, () => { MonoScript script = MonoScript.FromScriptableObject( featureHandler.FeatureReference ); AssetDatabase.OpenAsset( script ); } );
                        menu.AddItem( new GUIContent( "Copy Feature" ), false, () => { RagdollAnimatorFeatureBase.Editor_CopyFeature( featureHandler ); OnChange( handlerProp, handler ); } );

                        if( RagdollAnimatorFeatureBase.Editor_HasCopyFeatureReference() != null && RagdollAnimatorFeatureBase.Editor_HasCopyFeatureReference() != featureHandler)
                        {

                            if( RagdollAnimatorFeatureBase.Editor_HasFeatureToPasteClipboardSettings( featureHandler ) )
                            {
                                menu.AddItem( new GUIContent( "Paste Settings" ), false, () => { RagdollAnimatorFeatureBase.Editor_PasteFeatures( featureHandler ); OnChange( handlerProp, handler ); } );
                            }
                            else
                            {
                                menu.AddItem( new GUIContent( "Paste Feature As New" ), false, () =>
                                {
                                    handler.ExtraFeatures.Add( new RagdollAnimatorFeatureHelper() );
                                    var nFeature = handler.ExtraFeatures[handler.ExtraFeatures.Count - 1];
                                    nFeature.CopySettingsFrom( RagdollAnimatorFeatureBase.Editor_HasCopyFeatureReference() );

                                    OnChange( handlerProp, handler );
                                } );
                            }
                        }
                    }

                    menu.ShowAsContext();
                }
                else
                {
                    if( RagdollHandler._Editor_selectedModuleIndex == index )
                        RagdollHandler._Editor_selectedModuleIndex = -1;
                    else
                        RagdollHandler._Editor_selectedModuleIndex = index;
                }
            }

            GUI.backgroundColor = Color.white;

            if( !Application.isPlaying )
            {
                var preFeat = featureHandler.FeatureReference;
                featureHandler.FeatureReference = (RagdollAnimatorFeatureBase)EditorGUILayout.ObjectField( featureHandler.FeatureReference, typeof( RagdollAnimatorFeatureBase ), false );
                if( preFeat != null && featureHandler.FeatureReference == null ) { preFeat.Editor_OnRemoveFeatureInEditorGUI( handler, featureHandler ); OnChange( handlerProp, handler ); }
            }
            else
            {
                GUI.enabled = false;
                var preFeat = featureHandler.FeatureReference;
                EditorGUILayout.ObjectField( featureHandler.FeatureReference, typeof( RagdollAnimatorFeatureBase ), false, GUILayout.Width( 48 ) );
                if( preFeat != null && featureHandler.FeatureReference == null ) { preFeat.Editor_OnRemoveFeatureInEditorGUI( handler, featureHandler ); preFeat.OnDestroyFeature(); OnChange( handlerProp, handler ); }
                GUILayout.Space( 4 );
                EditorGUIUtility.labelWidth = 70;
                EditorGUILayout.ObjectField( "Runtime:", featureHandler.RuntimeFeature, typeof( RagdollAnimatorFeatureBase ), true );
                EditorGUIUtility.labelWidth = 0;
                GUI.enabled = true;
            }

            if( index > -1 )
            {
                GUI.backgroundColor = new Color( 1f, 0.75f, 0.75f, 1f );
                //GUI.enabled = !Application.isPlaying;

                if( GUILayout.Button( FGUI_Resources.GUIC_Remove, FGUI_Resources.ButtonStyle, GUILayout.Width( 22 ), GUILayout.Height( 18 ) ) )
                {
                    _customModuleToRemove = index;
                }

                GUI.backgroundColor = Color.white;
                //GUI.enabled = true;
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void ExtraFeatures_DisplaySelectedFeaturePanel( SerializedProperty handlerProp, RagdollHandler ragd, RagdollAnimatorFeatureHelper featurehandler )
        {
            if( featurehandler.ActiveFeature == null )
            {
                EditorGUILayout.HelpBox( "First choose some Ragdoll Animator Feature file for this slot", MessageType.None );
                return;
            }

            if( featurehandler.ActiveFeature.Editor_FeatureDescription != "" )
            {
                EditorGUILayout.HelpBox( featurehandler.ActiveFeature.Editor_FeatureDescription, UnityEditor.MessageType.None );
            }

            EditorGUI.BeginChangeCheck();

            featurehandler.ActiveFeature.Editor_InspectorGUI( handlerProp, ragd, featurehandler );

            if( EditorGUI.EndChangeCheck() ) OnChange( handlerProp, ragd );
        }
    }
}