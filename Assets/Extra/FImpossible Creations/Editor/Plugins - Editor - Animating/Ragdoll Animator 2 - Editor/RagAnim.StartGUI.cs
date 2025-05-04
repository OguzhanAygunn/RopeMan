using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollAnimator2Editor
    {
        private bool wasWalid = false;

        private void DrawBaseCategories()
        {
            EditorGUILayout.BeginHorizontal();
            DrawCategoryButton( RagdollHandler.ERagdollAnimSection.Setup, true );
            DrawCategoryButton( RagdollHandler.ERagdollAnimSection.Construct, true );

            if( Get.RagdollLogic == ERagdollLogic.JustBoneComponents ) GUI.backgroundColor = Color.gray;
            DrawCategoryButton( RagdollHandler.ERagdollAnimSection.Motion );
            if( Get.RagdollLogic == ERagdollLogic.JustBoneComponents ) GUI.backgroundColor = Color.gray;
            DrawCategoryButton( RagdollHandler.ERagdollAnimSection.Extra );
            if( Get.RagdollLogic == ERagdollLogic.JustBoneComponents ) GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            GUILayout.Space( 4f );

            if( Get._EditorCategory == RagdollHandler.ERagdollAnimSection.Setup )
            {
                RagdollHandlerEditor.GUI_DrawSetupCategory( sp_Handler, Get );
            }
            else if( Get._EditorCategory == RagdollHandler.ERagdollAnimSection.Construct )
            {
                RagdollHandlerEditor.GUI_DrawRagdollConstructor( sp_Handler, Get );
            }
            else if( Get._EditorCategory == RagdollHandler.ERagdollAnimSection.Motion )
            {
                RagdollHandlerEditor.GUI_DrawMotionCategory( sp_Handler, Get );
            }
            else if( Get._EditorCategory == RagdollHandler.ERagdollAnimSection.Extra )
            {
                RagdollHandlerEditor.GUI_DrawExtraCategory( sp_Handler, Get );
            }
        }

        private void DrawRagdollAnimator2GUI()
        {
            wasWalid = ( Get.RagdollLogic == ERagdollLogic.JustBoneComponents ) || ( Get.IsBaseSetupValid() ) && Get.IsRagdollConstructionValid();

            EditorGUI.BeginChangeCheck();

            if( wasWalid )
            {
                DrawBaseCategories();
            }
            else
            {
                Helper_Header( "Initial Prepare", FGUI_Resources.Tex_GearSetup );

                EditorGUILayout.Space( 4 );

                RagdollHandlerEditor.RefreshBaseReferences( sp_Handler );
                RagdollHandlerEditor.DrawBaseTransformField( sp_Handler.FindPropertyRelative( "BaseTransform" ), Get );
                EditorGUILayout.PropertyField( sp_Handler.FindPropertyRelative( "Mecanim" ), new GUIContent( "Mecanim (Optional)", "Assign to help auto-finding ragdoll bones and give access for few other features." ) ); // Animator

                EditorGUILayout.Space( 4 );
                if( !Get.IsBaseSetupValid() )
                {
                    EditorGUILayout.HelpBox( "The setup is not valid yet. Prepare bone references first!\nThen more options will be unlocked!", MessageType.Warning );
                    Get._EditorCategory = RagdollHandler.ERagdollAnimSection.Construct;
                    Get._Editor_ChainCategory = EBoneChainCategory.Setup;
                }
                else
                    EditorGUILayout.HelpBox( "Prepare ragdoll construct to unlock more settings!", MessageType.None );

                GUILayout.Space( 4 );

                if( Get.Chains.Count < 1 || Get.Chains[0].BoneSetups.Count < 1 )
                {
                    GUI.backgroundColor = Color.green;

                    if( GUILayout.Button( new GUIContent( "  Try Auto-Find Required Bones", FGUI_Resources.Tex_Bone ), FGUI_Resources.ButtonStyle, GUILayout.Height( 28 ) ) )
                    {
                        rGet.TryFindBonesAndDoFullSetup();
                        OnChange();
                        RagdollHandlerEditor._referencePoseReport = Get.ValidateReferencePose();

                        sp_Handler.serializedObject.ApplyModifiedProperties();
                        sp_Handler.serializedObject.Update();

                        Get._EditorCategory = RagdollHandler.ERagdollAnimSection.Construct;
                        Get._Editor_ChainCategory = EBoneChainCategory.Setup;
                        Get._Editor_SelectedChain = 0;
                    }

                    bool drawHips = false;
                    if( Get.Mecanim == null ) drawHips = true;
                    else if( Get.Mecanim.isHuman == false ) drawHips = true;

                    if( drawHips )
                    {
                        EditorGUIUtility.labelWidth = 46;
                        Transform hipsRef = null; hipsRef = EditorGUILayout.ObjectField( new GUIContent( "Hips:", "Assign hips reference to guide algorithm for skeleton detection" ), hipsRef, typeof( Transform ), true ) as Transform;
                        EditorGUIUtility.labelWidth = 0;

                        if( hipsRef != null )
                        {
                            var cChain = Get.AddNewBonesChain( "Core", ERagdollChainType.Core );
                            var cBone = cChain.AddNewBone( false );
                            cBone.SourceBone = hipsRef;

                            rGet.TryFindBonesAndDoFullSetup();
                            OnChange();
                            RagdollHandlerEditor._referencePoseReport = Get.ValidateReferencePose();

                            sp_Handler.serializedObject.ApplyModifiedProperties();
                            sp_Handler.serializedObject.Update();

                            Get._EditorCategory = RagdollHandler.ERagdollAnimSection.Construct;
                            Get._Editor_ChainCategory = EBoneChainCategory.Setup;
                            Get._Editor_SelectedChain = 0;
                        }
                    }

                    GUI.backgroundColor = Color.white;
                }

                GUILayout.Space( 8 );

                if( !Get.IsBaseSetupValid() ) FGUI_Inspector.DrawUILineCommon( 12 );

                RagdollHandlerEditor.GUI_DrawRagdollConstructor( sp_Handler, Get, true );

                //GUILayout.Space(24);
                GUILayout.Space( 6 );
                if( !Get.IsBaseSetupValid() ) FGUI_Inspector.DrawUILineCommon( 4 );
                FGUI_Inspector.DrawUILineCommon( 4 );

                EditorGUILayout.BeginHorizontal();

                if( GUILayout.Button( new GUIContent( "  Watch Tutorials", FGUI_Resources.Tex_Tutorials, "Opening link to the tutorials playlist on the youtube" ), FGUI_Resources.ButtonStyle, GUILayout.Height( 22 ) ) )
                {
                    Application.OpenURL( "https://www.youtube.com/playlist?list=PL6MURe5By90nYgMbXHucsy8wUvuvPJGuT" );
                }

                if( UserManualFile )
                    if( GUILayout.Button( new GUIContent( "  User Manual", FGUI_Resources.Tex_Manual, "Opening User Manual .pdf file" ), FGUI_Resources.ButtonStyle, GUILayout.Height( 22 ) ) )
                    {
                        EditorGUIUtility.PingObject( UserManualFile );
                        Application.OpenURL( Application.dataPath + AssetDatabase.GetAssetPath( UserManualFile ).Replace( "Assets", "" ) );
                    }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space( 4 );

                if( !Get.IsBaseSetupValid() )
                {
                    if( DemosPackage )
                    {
                        bool loaded = false;
                        string demosPath = AssetDatabase.GetAssetPath( DemosPackage );
                        if( AssetDatabase.LoadAssetAtPath( demosPath.Replace( "Demos - Ragdoll Animator 2.unitypackage", "Ragdoll Animator 2 - Demo" ), typeof( UnityEngine.Object ) ) != null ) loaded = true;

                        if( loaded == false )
                        {
                            EditorGUILayout.BeginHorizontal();
                            if( GUILayout.Button( new GUIContent( " Import Ragdoll Animator Demos", EditorGUIUtility.IconContent( "UnityLogo" ).image ), GUILayout.Height( 22 ) ) ) { AssetDatabase.ImportPackage( demosPath, true ); EditorGUIUtility.PingObject( DemosPackage ); }
                            if( GUILayout.Button( new GUIContent( FGUI_Resources.TexTargetingIcon, "Go to ragdoll animator directory in the project window." ), GUILayout.Width( 24 ), GUILayout.Height( 22 ) ) ) { EditorGUIUtility.PingObject( DemosPackage ); }
                            EditorGUILayout.EndHorizontal();
                        }
                    }

                    if( AssemblyDefinitions || AssemblyDefinitionsAll )
                    {
                        GUI.color = new Color( 1f, 1f, 1f, 0.5f );
                        EditorGUILayout.BeginHorizontal();
                        if( GUILayout.Button( "Import Assembly Definitions" ) ) AssetDatabase.ImportPackage( AssetDatabase.GetAssetPath( AssemblyDefinitions ), true );
                        if( GUILayout.Button( new GUIContent( "All Fimpossible AssemDefs", "Importing all fimpossible creations assembly definitions, if you use multiple plugins from Fimpossible Creations." ) ) ) AssetDatabase.ImportPackage( AssetDatabase.GetAssetPath( AssemblyDefinitionsAll ), true );
                        EditorGUILayout.EndHorizontal();
                        GUI.color = Color.white;
                    }

                    if( Time.fixedDeltaTime >= 0.02f && Get.Chains.Count < 1 )
                    {
                        GUILayout.Space( 6 );
                        EditorGUILayout.HelpBox( "Project settings Fixed Timestep is greater or equals 0.02. With this value, physics are cheap but with limited precision. Consider changing it to about 0.01 to improve physics quality for ragdoll animator.", MessageType.Info );
                        GUILayout.Space( 4 );
                        if( GUILayout.Button( "Change Fixed Timestep to 0.01" ) )
                        {
                            Time.fixedDeltaTime = 0.01f;
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
            }

            RagdollHandlerEditor.OnDrawingGUI();

            if( EditorGUI.EndChangeCheck() )
            {
                OnChange();
                _requestRepaint = true;
            }

            if( Application.isPlaying ) DrawPerformance();
        }
    }
}