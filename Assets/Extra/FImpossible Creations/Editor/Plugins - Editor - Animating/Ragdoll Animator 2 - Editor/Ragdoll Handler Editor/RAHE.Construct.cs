using FIMSpace.FEditor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static partial class RagdollHandlerEditor
    {
        public static void GUI_DrawRagdollConstructor( SerializedProperty ragdollHandlerProp, RagdollHandler handler, bool initialChains = false )
        {
            int preSelChain = handler._Editor_SelectedChain;
            RefreshBaseReferences( ragdollHandlerProp );
            GUILayout.Space( 4 );

            EditorGUILayout.BeginVertical( FGUI_Resources.BGInBoxBlankStyle );

            GUILayout.Space( -6 );

            EditorGUILayout.BeginVertical( FGUI_Resources.BGInBoxStyle );

            //if (GUILayout.Button(new GUIContent("  + Add Bones Chain + ", FGUI_Resources.FindIcon("SPR_BodyBonesChain")), FGUI_Resources.ButtonStyle, GUILayout.Height(15)))

            if( handler._Editor_ChainCategory == EBoneChainCategory.Setup )
            {

                EditorGUILayout.BeginHorizontal();

                if( handler.Chains.Count < 1 ) GUI.backgroundColor = new Color( 0.4f, 1f, 0.4f, 1f );

                if( GUILayout.Button( new GUIContent( " + Add Bones Chain +   ", FGUI_Resources.FindIcon( "SPR_BodyBonesChain" ) ), FGUI_Resources.ButtonStyle, GUILayout.Height( handler.Chains.Count < 1 ? 29 : 23 ) ) )
                {
                    ERagdollChainType targetType = ERagdollChainType.OtherLimb;
                    string tgtName = "Bones Chain " + handler.Chains.Count;
                    if( handler.Chains.Count == 0 ) { tgtName = "Core"; targetType = ERagdollChainType.Core; }
                    else if( handler.Chains.Count == 1 ) { tgtName = "Left Arm"; targetType = ERagdollChainType.LeftArm; }
                    else if( handler.Chains.Count == 2 ) { tgtName = "Right Arm"; targetType = ERagdollChainType.RightArm; }
                    else if( handler.Chains.Count == 3 ) { tgtName = "Left Leg"; targetType = ERagdollChainType.LeftLeg; }
                    else if( handler.Chains.Count == 4 ) { tgtName = "Right Leg"; targetType = ERagdollChainType.RightLeg; }

                    handler.AddNewBonesChain( tgtName, targetType );
                    OnChange( ragdollHandlerProp, handler );

                    if( handler.Chains.Count == 1 )
                    {
                        handler._Editor_SelectedChain = 0;
                    }
                    else
                    {
                        handler._Editor_SelectedChain = handler.Chains.Count - 1;
                    }

                    ragdollHandlerProp.serializedObject.ApplyModifiedProperties();
                    ragdollHandlerProp.serializedObject.Update();
                }

                GUI.backgroundColor = Color.white;

                if( handler.Chains.Count > 0 )
                {
                    GUILayout.Space( 4 );

                    if( GUILayout.Button( FGUI_Resources.GUIC_More, FGUI_Resources.ButtonStyle, GUILayout.Width( 30 ) ) )
                    {
                        RagdollBonesChain hChain = null;
                        if( handler.Chains.ContainsIndex( handler._Editor_SelectedChain ) ) hChain = handler.Chains[handler._Editor_SelectedChain];
                        GenericMenu_SetupHelperOptions( handler, ragdollHandlerProp );
                    }
                }

                EditorGUILayout.EndHorizontal();

                //GUILayout.Space(8);
                FGUI_Inspector.DrawUILineCommon( 8, 1, 1f );
            }

            if( handler.Chains.Count == 0 )
            {
                EditorGUILayout.HelpBox( "Add first bone chain in order to construct ragdoll dummy", MessageType.None );
                EditorGUILayout.EndVertical();
            }
            else
            {
                #region Display Horizontal List of Bone Chains

                float width = 0;
                float maxW = EditorGUIUtility.currentViewWidth - 30;

                GUILayout.BeginHorizontal();
                for( int i = 0; i < handler.Chains.Count; i++ )
                {
                    var cl = handler.Chains[i];

                    GUIContent content = new GUIContent( cl.ChainName );
                    Vector2 size = EditorStyles.miniButton.CalcSize( content );
                    content.image = GetChainIcon( cl.ChainType );
                    size.x += 2;
                    size.x += 20;

                    width += size.x + 2;
                    if( width > maxW - 14 )
                    {
                        width = size.x + 2;
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                    }

                    if( handler._Editor_SelectedChain == i ) GUI.backgroundColor = Color.green;

                    if( GUI_DrawBoneChainSelectButton( size.x, content ) )
                    {
                        handler.Editor_HandlesUndoRecord();
                        if( handler._Editor_SelectedChain == i ) handler._Editor_SelectedChain = -1;
                        else handler._Editor_SelectedChain = i;
                        SceneView.RepaintAll();
                    }

                    if( handler._Editor_SelectedChain == i ) GUI.backgroundColor = Color.white;
                }

                GUILayout.EndHorizontal();

                #endregion Display Horizontal List of Bone Chains

                if( handler._Editor_SelectedChain >= handler.Chains.Count ) handler._Editor_SelectedChain = -1;
                int toRemove = -1;
                GUILayout.Space( 4 );

                EditorGUILayout.EndVertical();

                GUILayout.Space( 6 );

                if( handler._Editor_SelectedChain < 0 )
                {
                    EditorGUILayout.HelpBox( "Select Bone Chain First", MessageType.None );
                    GUILayout.Space( 6 );

                    if( initialChains == false )
                        DrawConstructorCategoriesButtons( ragdollHandlerProp, handler );

                    if( initialChains == false )
                    {
                        FGUI_Inspector.DrawUILineCommon( 24 );

                        DisplayMaxMassParameter( ragdollHandlerProp, handler );

                        //GUILayout.Space( 10 );
                        //DisplayStoreTPoseButton( handler, ragdollHandlerProp );

                        GUILayout.Space( 12 );
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space( 10 );
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    var chain = handler.Chains[handler._Editor_SelectedChain];
                    var chainListProp = GetProperty( "chains" );
                    var chainProp = chainListProp.GetArrayElementAtIndex( handler._Editor_SelectedChain );

                    EditorGUILayout.BeginHorizontal();

                    if( GUILayout.Button( new GUIContent( GetChainIcon( chain.ChainType ) ), EditorStyles.label, GUILayout.Height( 18 ), GUILayout.Width( 22 ) ) )
                    {
                        // placeholder
                    }

                    chain.ChainType = (ERagdollChainType)EditorGUILayout.EnumPopup( chain.ChainType, GUILayout.MaxWidth( 74 ) );
                    chain.ChainName = EditorGUILayout.TextField( chain.ChainName );

                    GUILayout.FlexibleSpace();

                    FGUI_Inspector.RedGUIBackground();
                    if( chain.ChainType == ERagdollChainType.Core ) GUI.enabled = false;
                    if( GUILayout.Button( FGUI_Resources.GUIC_Remove, FGUI_Resources.ButtonStyle, GUILayout.Height( 18 ), GUILayout.Width( 22 ) ) ) toRemove = handler._Editor_SelectedChain;
                    GUI.enabled = true;
                    FGUI_Inspector.RestoreGUIBackground();

                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space( 6 );

                    if( initialChains == false )
                        DrawConstructorCategoriesButtons( ragdollHandlerProp, handler );

                    GUILayout.Space( 4 );

                    if( handler._Editor_ChainCategory == EBoneChainCategory.Setup ) // Constructor Category Setup
                    {
                        EditorGUILayout.BeginVertical();

                        GUILayout.Space( 1 );

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField( "Prepare Limb's Bone List", EditorStyles.centeredGreyMiniLabel );
                        if( chain.BoneSetups.Count == 0 || chain.BoneSetups[0].SourceBone == null ) if( chain.ChainType != ERagdollChainType.Unknown && chain.ChainType != ERagdollChainType.OtherLimb ) if( GUILayout.Button( "Try Auto Find" ) ) { handler.TryAutoFindChainFirstBone( chain ); OnChange( ragdollHandlerProp, handler ); }

                        bool areUnknown = false;
                        foreach( var boneCheck in chain.BoneSetups )
                        {
                            if( boneCheck.BoneID == ERagdollBoneID.Unknown ) { areUnknown = true; break; }
                        }

                        if( areUnknown && chain.BoneSetups.Count > 1 )
                        {
                            if( GUILayout.Button( new GUIContent( "Detect IDs", "Trying to auto-detect bone body parts IDs which are set as Unknown right now" ), GUILayout.MaxWidth( 80 ) ) )
                            {
                                chain.TryIdentifyBoneIDs( false );
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space( 2 );

                        var boneList = chainProp.FindPropertyRelative( "BoneSetups" );

                        if( handler.DummyWasGenerated ) GUI.enabled = false;
                        GUI_DrawBonesSetupTransformsList( ragdollHandlerProp, handler, boneList, chain, initialChains );
                        GUI.enabled = true;

                        #region Get child bones button

                        GUILayout.Space( 12 );
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space( 12 );

                        float btHeight = 21;

                        GUI.enabled = !handler.DummyWasGenerated;

                        if( chain.BoneSetups.Count == 0 )
                        {
                            GUI.backgroundColor = Color.green;
                            if( GUILayout.Button( new GUIContent( " + Add First Bone", FGUI_Resources.Tex_Bone ), GUILayout.Height( btHeight ) ) )
                            {
                                chain.AddNewBone( false );
                                OnChange( ragdollHandlerProp, handler );
                            }
                            GUI.backgroundColor = Color.white;
                        }
                        else
                        {
                            if( GUILayout.Button( new GUIContent( " + Add Bone", FGUI_Resources.Tex_Bone ), GUILayout.Height( btHeight ) ) )
                            {
                                var bone = chain.AddNewBone();
                                bone.TryDoAutoSettings( handler, chain );
                                OnChange( ragdollHandlerProp, handler );
                            }
                        }

                        if( chain.ChainType != ERagdollChainType.Core && chain.BoneSetups.Count == 1 && chain.BoneSetups[0].SourceBone != null )
                        {
                            GUILayout.Space( 12 );
                            if( GUILayout.Button( new GUIContent( " Get All Child Bones", FGUI_Resources.FindIcon( "SPR_BodyExtra" ), "Gathering all child bones, one by one. Useful for settings up tails." ), GUILayout.Height( btHeight ), GUILayout.MaxWidth( 150 ) ) )
                            {
                                chain.Setup_GatherChildBones();
                            }
                        }

                        GUI.enabled = true;

                        GUILayout.Space( 12 );
                        EditorGUILayout.EndHorizontal();
                        GUILayout.Space( 6 );

                        if( handler.DummyWasGenerated ) EditorGUILayout.HelpBox( "You can't add/remove bones when using Pre-Generated dummy", MessageType.None );

                        #endregion Get child bones button

                        FGUI_Inspector.DrawUILineCommon( 14 );

                        if( initialChains == false )
                        {
                            var settProp = chainProp.FindPropertyRelative( "Detach" );
                            EditorGUILayout.BeginHorizontal();

                            if( GUILayout.Button( new GUIContent( GetChainIcon( chain.ChainType ) ), EditorStyles.label, GUILayout.Height( 18 ), GUILayout.Width( 22 ) ) )
                            {
                                // placeholder
                            }

                            EditorGUIUtility.labelWidth = 58;
                            EditorGUILayout.PropertyField( settProp ); settProp.Next( false );
                            EditorGUIUtility.labelWidth = 0;
                            //if (chain.Detach) EditorGUILayout.PropertyField(settProp);

                            GUILayout.FlexibleSpace();

                            if( GUILayout.Button( FGUI_Resources.GUIC_More, EditorStyles.label, _w22h18 ) )
                            {
                                GenericMenu_SetupHelperOptions( handler, ragdollHandlerProp );
                            }

                            EditorGUILayout.EndHorizontal();

                            if( chain.Detach )
                            {
                                if( handler.UseReconstruction )
                                {
                                    //if ( hasLackingBones )
                                    //{
                                    EditorGUILayout.HelpBox( "Ragdoll Reconstruction Mode (Motion -> Reconstruction) is not working on chains with lacking bones.", UnityEditor.MessageType.Warning );

                                    //}
                                }
                            }

                            GUILayout.Space( 6 );
                            EditorGUILayout.HelpBox( "More Bone Settings under other bookmarks", MessageType.None ); // →

                            //GUILayout.Space(12);
                            //DisplayMaxMassParameter(ragdollHandlerProp, handler);

                            //GUILayout.Space(6);
                            //DisplayStoreTPoseButton(handler, ragdollHandlerProp);

                            //GUILayout.Space( 12 );
                            //if( GUILayout.Button( new GUIContent( "  Helper Options Menu", FGUI_Resources.Tex_MoreMenu ), FGUI_Resources.ButtonStyle ) )
                            //{
                            //    GenericMenu_SetupHelperOptions( handler, ragdollHandlerProp, chain );
                            //}

                            if( handler.CheckIfBoneDuplicatesExistsInTheBoneSetups() )
                            {
                                EditorGUILayout.HelpBox( "Detected using same bones in multiple chains! You need to fix it!", MessageType.Warning );
                            }
                        }

                        EditorGUILayout.EndVertical();

                        #region Drag and Drop

                        var rect = GUILayoutUtility.GetLastRect();
                        var dropEvent = UnityEngine.Event.current;

                        if( dropEvent != null )
                        {
                            if( dropEvent.type == UnityEngine.EventType.DragPerform || dropEvent.type == UnityEngine.EventType.DragUpdated )
                                if( rect.Contains( dropEvent.mousePosition ) )
                                {
                                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                                    if( dropEvent.type == UnityEngine.EventType.DragPerform )
                                    {
                                        DragAndDrop.AcceptDrag();
                                        foreach( var dragged in DragAndDrop.objectReferences )
                                        {
                                            GameObject draggedObject = dragged as GameObject;
                                            if( draggedObject != null && draggedObject.scene.isLoaded )
                                            {
                                                var nBone = chain.AddNewBone( draggedObject.transform );
                                                nBone.TryDoAutoSettings( handler, chain );
                                                OnChange( ragdollHandlerProp, handler );
                                            }
                                        }
                                    }

                                    UnityEngine.Event.current.Use();
                                }
                        }

                        #endregion Drag and Drop
                    }
                    else if( handler._Editor_ChainCategory == EBoneChainCategory.Colliders ) // Constructor Category Colliders
                    {
                        GUILayout.Space( 2 );
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space( 5 );

                        var settProp = chainProp.FindPropertyRelative( "ChainThicknessMultiplier" );

                        //EditorGUILayout.PropertyField(settProp, new GUIContent("Thickness Multiplier", settProp.tooltip), true); settProp.Next(false);
                        DrawParamAsAdjustableSlider( settProp, new GUIContent( "Thickness Multiplier", settProp.tooltip ), 2f ); settProp.Next( false );

                        var foldRagSize = GUILayoutUtility.GetLastRect();
                        foldRagSize.size = new Vector2( 18, 14 );
                        foldRagSize.x = 6; foldRagSize.y += 3;
                        if( GUI.Button( foldRagSize, FGUI_Resources.GetFoldSimbolTex( foldoutRagdollSize, true ), EditorStyles.label ) ) { foldoutRagdollSize = !foldoutRagdollSize; }

                        GUILayout.Space( 4 );

                        if( GUILayout.Button( FGUI_Resources.GUIC_More, FGUI_Resources.ButtonStyle, w22h18 ) )
                        {
                            GenericMenu_ChainOperations( handler, ragdollHandlerProp, chain );
                        }

                        GUILayout.Space( 5 );

                        // Draw Bone Colliders Setup List
                        EditorGUILayout.EndHorizontal();

                        if( foldoutRagdollSize )
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space( 15 );
                            if( settProp.floatValue == 0f ) settProp.floatValue = 1f;
                            //EditorGUILayout.PropertyField(settProp, new GUIContent("Scale Multiplier", settProp.tooltip), true); settProp.Next(false);
                            DrawParamAsAdjustableSlider( settProp, new GUIContent( "Scale Multiplier", settProp.tooltip ), 2f ); settProp.Next( false );
                            GUILayout.Space( 5 );
                            EditorGUILayout.EndHorizontal();
                        }


                        GUILayout.Space( 4 );
                        var boneList = chainProp.FindPropertyRelative( "BoneSetups" );
                        GUI_DrawBoneCollidersSetupList( handler, boneList, chain );

                        // Separator
                        FGUI_Inspector.DrawUILineCommon( 8, 2 );

                        //GUILayout.Space( 4 );

                        // Helper Options Menu
                        //EditorGUILayout.BeginHorizontal();
                        //GUILayout.Space( 20 );

                        //if( GUILayout.Button( new GUIContent( "  Helper Options Menu", FGUI_Resources.Tex_MoreMenu ), FGUI_Resources.ButtonStyle ) )
                        //{
                        //    GenericMenu_ChainOperations( handler, ragdollHandlerProp, chain );
                        //}

                        //GUILayout.Space(4);
                        //if (GUILayout.Button(FGUI_Resources.GUIC_More, FGUI_Resources.ButtonStyle, w22h18)) { }
                        //GUILayout.Space( 20 );
                        //EditorGUILayout.EndHorizontal();

                        GUILayout.Space( 6 );
                        EditorGUIUtility.labelWidth = 80;
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space( 20 );
                        RagdollHandler.WireframeMode = EditorGUILayout.Toggle( "Wireframe:", RagdollHandler.WireframeMode );
                        EditorGUIUtility.labelWidth = 100;
                        RagdollHandler.MeshMode = EditorGUILayout.Toggle( "Collider Mesh:", RagdollHandler.MeshMode );
                        EditorGUIUtility.labelWidth = 70;
                        RagdollHandler.DrawFlatColliders = EditorGUILayout.Toggle( "Draw 2D:", RagdollHandler.DrawFlatColliders );
                        GUILayout.Space( 20 );
                        EditorGUILayout.EndHorizontal();
                        EditorGUIUtility.labelWidth = 0;
                        //GUILayout.Space( -10 );
                    }
                    else if( handler._Editor_ChainCategory == EBoneChainCategory.Physics ) // Constructor Category Physics
                    {
                        var settProp = chainProp.FindPropertyRelative( "MassMultiplier" );

                        EditorGUIUtility.labelWidth = 130;

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space( 5 );
                        EditorGUILayout.PropertyField( settProp, new GUIContent( "Chain Mass Multiply:", settProp.tooltip ), true ); settProp.Next( false );
                        GUILayout.Space( 4 );
                        if( GUILayout.Button( FGUI_Resources.GUIC_More, FGUI_Resources.ButtonStyle, w22h18 ) ) GenericMenu_ChainOperations( handler, ragdollHandlerProp, chain );
                        GUILayout.Space( 5 );
                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space( 4 );
                        //EditorGUILayout.BeginHorizontal();
                        //GUILayout.Space( 5 );
                        var chainPowerProp = settProp.Copy();
                        //EditorGUILayout.PropertyField( settProp, new GUIContent( "Axis Range Multiply:", settProp.tooltip ), true );
                        settProp.Next( false );
                        settProp.Next( false );
                        //GUILayout.Space( 5 );
                        //EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space( 5 );
                        EditorGUILayout.PropertyField( settProp, new GUIContent( "Unlimited Rotations:", settProp.tooltip ), true ); settProp.Next( false );
                        GUILayout.FlexibleSpace();
                        EditorGUIUtility.labelWidth = 112;
                        EditorGUIUtility.fieldWidth = 28;
                        EditorGUILayout.PropertyField( settProp, new GUIContent( "Connected Mass:", settProp.tooltip ), true );

                        //if( settProp.numericType == SerializedPropertyNumericType.Float ) 
                        if( settProp.floatValue < 0f ) settProp.floatValue = 0f;

                        EditorGUIUtility.fieldWidth = 0;
                        EditorGUIUtility.labelWidth = 0;
                        GUILayout.Space( 5 );

                        settProp.Next( false );

                        if( chain.BoneSetups.Count > 7 )
                        {
                            EditorGUIUtility.labelWidth = 58;
                            EditorGUILayout.PropertyField( settProp, new GUIContent( "Override:", "Using connected mass as multiplier if this toggle is turned off. When enabled, overriding connected mass value." ), true, GUILayout.MaxWidth( 88 ) );
                        }
                        else
                        {
                            EditorGUIUtility.labelWidth = 18;
                            EditorGUILayout.PropertyField( settProp, new GUIContent( "O:", "Using connected mass as multiplier if this toggle is turned off. When enabled, overriding connected mass value." ), true, GUILayout.MaxWidth( 38 ) );
                        }

                        EditorGUIUtility.labelWidth = 0;
                        EditorGUILayout.EndHorizontal();

                        if( chain.BoneSetups.Count > 7 )
                        {
                            if( ( !chain.ConnectedMassOverride || chain.ConnectedMassScale < 0.8f ) )
                                EditorGUILayout.HelpBox( "This chain is long, consider toggling override and using connected mass scale = 1 to avoid physics glitches", MessageType.Warning );
                        }

                        // Draw Bone Physics Setup List
                        GUILayout.Space( 4 );
                        var boneList = chainProp.FindPropertyRelative( "BoneSetups" );
                        GUI_DrawBonePhysicsSetupList( ragdollHandlerProp, handler, boneList, chain );

                        // Separator
                        FGUI_Inspector.DrawUILineCommon( 12, 2 );

                        GUILayout.Space( 7 );

                        // Detail Options

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space( 5 );
                        EditorGUILayout.PropertyField( chainPowerProp, new GUIContent( "Chain Force Multiply:", chainPowerProp.tooltip ), true ); chainPowerProp.Next( false );
                        GUILayout.Space( 5 );
                        EditorGUILayout.EndHorizontal();

                        if( handler.HardMatching > 0f )
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space( 5 );
                            var hmMul = chainProp.FindPropertyRelative( "HardMatchMultiply" );
                            EditorGUILayout.PropertyField( hmMul, new GUIContent( "Hard Matching Multiply:", hmMul.tooltip ), true );
                            GUILayout.Space( 5 );
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space( 5 );
                        EditorGUILayout.PropertyField( chainPowerProp, new GUIContent( "Axis Range Multiply:", chainPowerProp.tooltip ), true ); GUILayout.Space( 5 );
                        GUILayout.Space( 5 );
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();

                        chainPowerProp = chainProp.FindPropertyRelative( "AlternativeTensors" );
                        EditorGUILayout.PropertyField( chainPowerProp, new GUIContent( "Alternative Tensors:", chainPowerProp.tooltip ), true );

                        var rect = GUILayoutUtility.GetLastRect();
                        rect.x -= 8;
                        rect.width = 8;

                        if( GUI.Button( rect, GUIContent.none, EditorStyles.label ) )
                        {
                            GenericMenu menu = new GenericMenu();

                            menu.AddItem( new GUIContent( "Set this tensor settings to all chains" ), false, () => { foreach( var ochain in handler.Chains ) { ochain.AlternativeTensors = chain.AlternativeTensors; ochain.AlternativeTensorsOnFall = chain.AlternativeTensorsOnFall; } OnChange( ragdollHandlerProp, handler ); } );
                            menu.ShowAsContext();
                        }

                        if( chain.AlternativeTensors )
                        {
                            GUILayout.FlexibleSpace();
                            chainPowerProp.Next( false );
                            EditorGUIUtility.labelWidth = 50;
                            EditorGUILayout.PropertyField( chainPowerProp, new GUIContent( "On Fall:", chainPowerProp.tooltip ), true );
                            EditorGUIUtility.labelWidth = 0;
                        }

                        GUILayout.Space( 5 );
                        EditorGUILayout.EndHorizontal();

                        //GUILayout.Space( 12 );

                        // Helper Options Menu
                        //EditorGUILayout.BeginHorizontal();
                        //GUILayout.Space( 20 );
                        //if( GUILayout.Button( new GUIContent( "  Helper Options Menu", FGUI_Resources.Tex_MoreMenu ), FGUI_Resources.ButtonStyle ) )
                        //{
                        //    GenericMenu_ChainOperations( handler, ragdollHandlerProp, chain );
                        //}
                        //GUILayout.Space( 20 );
                        //EditorGUILayout.EndHorizontal();

                        GUILayout.Space( 12 );
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space( 10 );
                        DisplayMaxMassParameter( ragdollHandlerProp, handler );
                        GUILayout.Space( 10 );
                        EditorGUILayout.EndHorizontal();
                    }
                }

                if( toRemove > -1 )
                {
                    handler._Editor_SelectedChain -= 1;
                    handler.Chains.RemoveAt( toRemove );
                    OnChange( ragdollHandlerProp, handler );
                }
            };

            EditorGUILayout.EndVertical();
            if( preSelChain != handler._Editor_SelectedChain ) handler.Editor_StopPreviewingAll();
        }

        static void DrawParamAsAdjustableSlider( SerializedProperty sp, GUIContent title, float defaultMax = 1f )
        {
            //EditorGUILayout.PropertyField(settProp, new GUIContent("Thickness Multiplier", settProp.tooltip), true); settProp.Next(false);
            float currentMax = Mathf.Max( defaultMax, Mathf.Ceil( sp.floatValue ) ) + 0.001f;
            EditorGUILayout.Slider( sp, 0f, currentMax, title );
        }

        private static void DisplayMaxMassParameter( SerializedProperty ragdollHandlerProp, RagdollHandler handler )
        {
            var massProp = ragdollHandlerProp.FindPropertyRelative( "ReferenceMass" );
            EditorGUILayout.BeginHorizontal();

            EditorGUIUtility.labelWidth = 126;
            EditorGUILayout.PropertyField( massProp, new GUIContent( "  Reference Mass:", FGUI_Resources.Tex_Physics, massProp.tooltip ) );
            EditorGUIUtility.labelWidth = 0;

            GUI.backgroundColor = handler._Editor_DrawMassIndicators ? Color.green : Color.white;
            if( GUILayout.Button( new GUIContent( EditorGUIUtility.currentViewWidth < 390 ? "Mass (i)" : "  Draw Mass Indicators", "Click to display limbs bones mass indicators in the scene view" ), GUILayout.Height( 20 ) ) )
            {
                handler._Editor_DrawMassIndicators = !handler._Editor_DrawMassIndicators;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space( 6 );
            if( GUILayout.Button( FGUI_Resources.GUIC_Info, EditorStyles.label, w22h18 ) )
            {
                EditorUtility.DisplayDialog( "Max Mass Info", "Value which is distributed over ragdoll bones rigidbodies as fractional value.", "Ok" );
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DisplaySymmetryActionButton( System.Action<RagdollChainBone> clickAction, RagdollHandler handler, RagdollChainBone bone, RagdollBonesChain chain, int width = 30 )
        {
            if( GUILayout.Button( new GUIContent( FGUI_Resources.FindIcon( "Fimp/Small Icons/Symmetry" ), "Apply settings of this bone for detected symmetrical bone in bones structure.\n\nRight mouse button to select the symmetrical bone if exists." ), FGUI_Resources.ButtonStyle, GUILayout.Width( width ) ) )
            {
                var symmetry = chain.GetSymmetryTo( bone );

                if( symmetry == null )
                {
                    EditorUtility.DisplayDialog( "No Symmetry", "Not found symmetry bone for :" + bone.SourceBone.name + ":", "Ok" );
                }
                else
                {
                    if( IsRightMouseButton() )
                    {
                        handler.Editor_HandlesUndoRecord();
                        handler._Editor_SelectedChain = handler.GetIndexOfChain( handler.GetChain( symmetry ) );
                        GUI_Contruct_SoloEditBone( handler.GetChain( symmetry ), symmetry );
                    }
                    else
                    {
                        handler.Editor_HandlesUndoRecord();
                        clickAction.Invoke( symmetry );
                    }
                    //symmetry.PastePhysicsSettingsOfOtherBoneSymmetrical(bone);
                }
            }
        }

        private static void GenericMenu_ChainOperations( RagdollHandler handler, SerializedProperty handlerProp, RagdollBonesChain chain )
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem( new GUIContent( "Copy Settings" ), false, () => { RagdollAnimator2Extensions.SetCopyingSource( chain ); } );
            var copyingFrom = RagdollAnimator2Extensions.CopyingFrom;

            if( copyingFrom != null && copyingFrom != chain )
            {
                string fromName = copyingFrom.ChainName;

                menu.AddItem( new GUIContent( "Paste <Collider> Settings of [" + fromName + "] chain", Icon_Collider ), false, () => { chain.PasteColliderSettingsOfOtherChain( copyingFrom ); OnChange( handlerProp, handler ); } );
                menu.AddItem( new GUIContent( "Paste <Collider> Settings Symmetrical" ), false, () => { chain.PasteColliderSettingsOfOtherChainSymmetrical( copyingFrom, handler ); OnChange( handlerProp, handler ); } );

                menu.AddSeparator( "" );

                menu.AddItem( new GUIContent( "Paste <Physics> Settings of [" + fromName + "] chain", FGUI_Resources.Tex_Physics ), false, () => { chain.PastePhysicsSettingsOfOtherChain( copyingFrom ); OnChange( handlerProp, handler ); } );
                //menu.AddItem(new GUIContent("Paste <Physics> Settings Symmetrical"), false, () => { chain.PastePhysicsSettingsOfOtherChainSymmetrical(copyingFrom); OnChange(handlerProp); });
            }

            if( handler._Editor_ChainCategory == EBoneChainCategory.Physics )
            {
                if( chain.ChainType.IsLeg() || chain.ChainType.IsArm() )
                {
                    var getSymmetryChain = RagdollBonesChain.FindSymmetryChainTo( handler, chain );
                    if( getSymmetryChain != null )
                    {
                        menu.AddSeparator( "" );
                        menu.AddItem( new GUIContent( "Paste <Mass> Settings to the [ Symmetrical ] chain" ), false, () => { handler.Editor_HandlesUndoRecord(); getSymmetryChain.PastePhysics_Mass_OfOtherChain( chain ); OnChange( handlerProp, handler ); OnChange( handlerProp, handler ); } );
                    }
                }

                menu.AddItem( new GUIContent( "Auto Adjust Physics (discarding current settings)" ), false, () => { chain.AutoAdjustPhysics(); OnChange( handlerProp, handler ); } );
            }
            else if( handler._Editor_ChainCategory == EBoneChainCategory.Colliders )
            {
                menu.AddItem( new GUIContent( "Auto Adjust Colliders (discarding current settings)" ), false, () => { chain.AutoAdjustColliders( handler.IsHumanoid ); OnChange( handlerProp, handler ); } );
            }

            //menu.AddSeparator("");
            //menu.AddItem(new GUIContent("Debug add components"), false, () => { chain.RefreshRagdollComponents(true); });

            menu.ShowAsContext();
        }

        private static void GenericMenu_SetupHelperOptions( RagdollHandler handler, SerializedProperty handlerProp )
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem( new GUIContent( "Call auto <Colliders> and <Physics> adjustement on all chains (discarding current settings)" ), false, () =>
            {
                RequestRepaint = true;
                _ActionsToCall.Add( () => { foreach( var chain in handler.Chains ) { var cChain = chain; cChain.AutoAdjustColliders( handler.IsHumanoid ); cChain.AutoAdjustPhysics(); OnChange( handlerProp, handler ); } } );
                _ActionsToCall.Add( () => { OnChange( null, handler ); } );
                OnChange( null, handler );
            } );

            menu.AddItem( new GUIContent( "Call auto <Colliders> adjustement on all chains (discarding current settings)" ), false, () =>
            {
                RequestRepaint = true;
                _ActionsToCall.Add( () => { foreach( var chain in handler.Chains ) { var cChain = chain; cChain.AutoAdjustColliders( handler.IsHumanoid ); OnChange( handlerProp, handler ); } } );
                _ActionsToCall.Add( () => { OnChange( null, handler ); } );
                OnChange( null, handler );
            } );

            menu.AddItem( new GUIContent( "Switch All Chains 'Detach' parameter" ), false, () =>
            {
                foreach( var chain in handler.Chains ) chain.Detach = !chain.Detach;
                OnChange( handlerProp, handler );
            } );

            menu.AddSeparator( "" );

            menu.AddItem( new GUIContent( "Search Skeleton for Colliders and assign them as Target Colliders for Physical Dummy if found" ), false, () => { RagdollHandlerUtilities.FindBonesCollidersInSourceBonesAndAssignAsReferenceCollidersIfFound( handler, true, true ); OnChange( handlerProp, handler ); } );
            menu.AddItem( new GUIContent( "Search Skeleton for Colliders and assign their settings to the bone collider setups" ), false, () => { RagdollHandlerUtilities.FindBonesCollidersInSourceBonesAndAssignAsReferenceCollidersIfFound( handler, false, true ); OnChange( handlerProp, handler ); } );
            menu.AddItem( new GUIContent( "Try identify all bone body part types" ), false, () => { foreach( var chain in handler.Chains ) chain.TryIdentifyBoneIDs(); OnChange( handlerProp, handler ); OnChange( handlerProp, handler ); } );

            menu.AddSeparator( "" );
            menu.AddSeparator( "" );

            menu.AddItem( new GUIContent( "[Optional Utility] Add colliders on the character bones (for custom use)" ), false, () => { RagdollHandlerUtilities.AddCollidersOnTheCharacterBones( handler ); OnChange( handlerProp, handler ); } );
            menu.AddItem( new GUIContent( "[Optional Utility] Add physics components on the character bones (for custom use)" ), false, () => { RagdollHandlerUtilities.AddPhysicsComponentsOnTheCharacterBones( handler ); OnChange( handlerProp, handler ); } );

            menu.AddSeparator( "" );
            menu.AddSeparator( "" );

            menu.AddItem( new GUIContent( "[Removing Components] Find and remove joint-rigidbody components on the character bones" ), false, () => { RagdollHandlerUtilities.FindAndRemoveJointAndRigidbodyComponentsOnTheCharacterBones( handler, true ); OnChange( handlerProp, handler ); } );
            menu.AddItem( new GUIContent( "[Removing Components] Find and remove all physical components on the character bones" ), false, () => { RagdollHandlerUtilities.FindAndRemoveAllPhysicalComponentsOnTheCharacterBones( handler, true ); OnChange( handlerProp, handler ); } );

            menu.ShowAsContext();
        }

        private static List<Action> _ActionsToCall = new List<Action>();

        private static void GenericMenu_ReferencePoseOptions( RagdollHandler handler )
        {
            GenericMenu menu = new GenericMenu();

            if( handler.Mecanim != null && handler.Mecanim.isHuman )
            {
                menu.AddItem( new GUIContent( "Change character pose to humanoid T-Pose" ), false, () => { _ActionsToCall.Add( handler.SetTPoseFromAnimationClip ); _ActionsToCall.Add( () => { _referencePoseReport = handler.ValidateReferencePose(); } ); RequestRepaint = true; OnChange( null, handler ); } );
            }

            menu.AddSeparator( "" );
            menu.AddSeparator( "" );
            menu.AddItem( new GUIContent( "Call auto colliders adjustement on all chains (discarding current settings)" ), false, () =>
            {
                RequestRepaint = true;
                _ActionsToCall.Add( () => { foreach( var chain in handler.Chains ) { var cChain = chain; cChain.AutoAdjustColliders( handler.IsHumanoid ); OnChange( null, handler ); } } );
                _ActionsToCall.Add( () => { OnChange( null, handler ); } );
                OnChange( null, handler );
            } );

            menu.AddSeparator( "" );
            menu.AddSeparator( "" );
            menu.AddItem( new GUIContent( "Copy Chains Setup Settings from other Ragdoll Animator" ), false, () =>
            {
                DisplayLoadRagdollPreset = false;
                DisplayCopyOtherRagdollAnimatorSettings = !DisplayCopyOtherRagdollAnimatorSettings;
            } );

            menu.AddItem( new GUIContent( "Load settings from the Ragdoll Animator Preset" ), false, () =>
            {
                DisplayCopyOtherRagdollAnimatorSettings = false;
                DisplayLoadRagdollPreset = !DisplayLoadRagdollPreset;
            } );

            menu.AddItem( new GUIContent( "Export Settings of this Ragdoll Animator as Preset File" ), false, () =>
            {
                ExportSettingsAsPresetFile( handler );
            } );

            if( _referencePoseReport != RagdollHandler.EReferencePoseReport.NoReferencePose )
            {
                menu.AddSeparator( "" );
                menu.AddSeparator( "" );
                menu.AddItem( new GUIContent( "Change character pose to stored reference pose" ), false, () => { _ActionsToCall.Add( handler.ApplyTPoseOnModel ); _ActionsToCall.Add( () => { _referencePoseReport = handler.ValidateReferencePose(); } ); RequestRepaint = true; OnChange( null, handler ); } );
            }

            menu.ShowAsContext();
        }

        public static void ExportSettingsAsPresetFile(RagdollHandler handler, RagdollAnimator2 optionalRef = null)
        {
            RagdollAnimator2Preset preset = RagdollAnimator2Preset.CreateInstance<RagdollAnimator2Preset>();
            handler.ApplyAllPropertiesToOtherRagdoll( preset.Settings );
            preset.name = "RagdollAnimator2Preset";
            
            if( handler.Mecanim ) preset.name = handler.Mecanim.gameObject.name + " - " + preset.name;
            else if ( optionalRef) preset.name = optionalRef.gameObject.name + " - " + preset.name;

            AssetDatabase.CreateAsset( preset, "Assets/" + preset.name + ".asset" );
            EditorGUIUtility.PingObject( preset );
        }

        private static void GenericMenu_PhysicsOperations( RagdollHandler handler, SerializedProperty handlerProp, RagdollBonesChain chain, RagdollChainBone bone )
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem( new GUIContent( "Copy Settings" ), false, () => { RagdollAnimator2Extensions.SetCopyingSource( bone ); } );

            var copyingFrom = RagdollAnimator2Extensions.CopyingFromBone;

            if( copyingFrom != null && copyingFrom != bone )
            {
                menu.AddItem( new GUIContent( "Paste <Physics> Settings", Icon_Collider ), false, () => { bone.PastePhysicsSettingsOfOtherBone( copyingFrom ); OnChange( handlerProp, handler ); } );
                menu.AddItem( new GUIContent( "Paste <Physics> Settings Symmetrical" ), false, () => { bone.PastePhysicsSettingsOfOtherBoneSymmetrical( copyingFrom ); OnChange( handlerProp, handler ); } );
            }

            menu.AddSeparator( "" );
            menu.AddItem( new GUIContent( "Apply <Physics> Settings to rest of the bones" ), false, () => { bone.ApplyPhysicsSettingsToAllBonesInChain( chain ); OnChange( handlerProp, handler ); } );
            menu.AddItem( new GUIContent( "Reset Auto Mass Settings" ), false, () => { bone.DoAutoMassSettings( handler, chain ); OnChange( handlerProp, handler ); } );

            menu.AddSeparator( "" );
            menu.AddItem( new GUIContent( "Display Bone Extra Physics Settings" ), bone._EditorPhysicsExtraSettings, () => { bone._EditorPhysicsExtraSettings = !bone._EditorPhysicsExtraSettings; } );

            //menu.AddItem(new GUIContent("Debug add components"), false, () => { chain.RefreshRagdollComponents(true); });

            menu.ShowAsContext();
        }

        public static Texture GetChainIcon( ERagdollChainType chain )
        {
            if( chain == ERagdollChainType.Core ) return FGUI_Resources.FindIcon( "SPR_BodySpine" );
            else if( chain.IsArm() ) return FGUI_Resources.FindIcon( "SPR_BodyArm" );
            else if( chain.IsLeg() ) return FGUI_Resources.FindIcon( "SPR_BodyLeg" );
            else return FGUI_Resources.FindIcon( "SPR_BodyBonesChain" );
        }

        private static bool GUI_DrawBoneChainSelectButton( float width, GUIContent label )
        {
            if( GUILayout.Button( label, /*EditorStyles.miniButton, */GUILayout.Width( width ), GUILayout.Height( 26 ) ) ) { GUI.FocusControl( "" ); return true; }
            return false;
        }

        private static void DrawConstructorCategoriesButtons( SerializedProperty ragdollHandlerProp, RagdollHandler handler )
        {
            EditorGUILayout.BeginHorizontal();

            if( handler._Editor_ChainCategory == EBoneChainCategory.Setup ) GUI.backgroundColor = Color.green;
            if( GUILayout.Button( new GUIContent( "  Setup", FGUI_Resources.Tex_Gear ), EditorStyles.miniButtonLeft ) )
            {
                if( RagdollHandlerEditor.IsRightMouseButton() && handler._Editor_ChainCategory == EBoneChainCategory.Setup )
                {
                    GenericMenu_SetupHelperOptions( handler, ragdollHandlerProp );
                }
                else
                {
                    handler.Editor_HandlesUndoRecord();
                    handler._Editor_ChainCategory = EBoneChainCategory.Setup;
                    OnChange( ragdollHandlerProp, handler );
                }
            }

            FGUI_Inspector.RestoreGUIBackground();
            if( handler._Editor_ChainCategory == EBoneChainCategory.Colliders ) GUI.backgroundColor = Color.green;
            if( GUILayout.Button( new GUIContent( "  Colliders", Icon_Collider ), EditorStyles.miniButtonMid ) )
            {
                if( handler._Editor_ChainCategory == EBoneChainCategory.Colliders && RagdollHandlerEditor.IsRightMouseButton() )
                {
                    var chain = handler.GetChain( handler._Editor_SelectedChain );
                    if( chain != null ) GenericMenu_ChainOperations( handler, ragdollHandlerProp, chain );
                    else
                    {
                        GenericMenu menu = new GenericMenu();

                        menu.AddItem( new GUIContent( "Call auto <Colliders> adjustement on all chains (discarding current settings)" ), false, () =>
                        {
                            RequestRepaint = true;
                            _ActionsToCall.Add( () => { for( int i = 0; i < handler.Chains.Count; i++ ) handler.Chains[i].AutoAdjustColliders( handler.IsHumanoid ); } );
                            _ActionsToCall.Add( () => { OnChange( ragdollHandlerProp, handler ); } );
                            OnChange( null, handler );
                        } );

                        menu.ShowAsContext();
                    }
                }
                else
                {
                    handler.Editor_HandlesUndoRecord();
                    handler._Editor_ChainCategory = EBoneChainCategory.Colliders;
                    OnChange( ragdollHandlerProp, handler );
                }
            }

            FGUI_Inspector.RestoreGUIBackground();
            if( handler._Editor_ChainCategory == EBoneChainCategory.Physics ) GUI.backgroundColor = Color.green;
            if( GUILayout.Button( new GUIContent( "  Physics", FGUI_Resources.Tex_Physics ), EditorStyles.miniButtonRight ) )
            {
                if( handler._Editor_ChainCategory == EBoneChainCategory.Physics && RagdollHandlerEditor.IsRightMouseButton() )
                {
                    var chain = handler.GetChain( handler._Editor_SelectedChain );
                    if( chain != null ) GenericMenu_ChainOperations( handler, ragdollHandlerProp, chain );
                    else
                    {
                        GenericMenu menu = new GenericMenu();

                        menu.AddItem( new GUIContent( "Call auto<Physics> adjustement on all chains (discarding current settings)" ), false, () =>
                        {
                            RequestRepaint = true;
                            _ActionsToCall.Add( () => { for( int i = 0; i < handler.Chains.Count; i++ ) handler.Chains[i].AutoAdjustPhysics(); } );
                            _ActionsToCall.Add( () => { OnChange( ragdollHandlerProp, handler ); } );
                            OnChange( null, handler );
                        } );

                        menu.ShowAsContext();
                    }
                }
                else
                {
                    handler.Editor_HandlesUndoRecord();
                    handler._Editor_ChainCategory = EBoneChainCategory.Physics;
                    OnChange( ragdollHandlerProp, handler );
                }
            }

            FGUI_Inspector.RestoreGUIBackground();
            EditorGUILayout.EndHorizontal();
        }
    }
}