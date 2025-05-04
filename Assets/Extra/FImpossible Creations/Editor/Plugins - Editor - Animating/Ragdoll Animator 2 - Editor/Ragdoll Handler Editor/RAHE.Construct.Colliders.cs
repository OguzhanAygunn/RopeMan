using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static partial class RagdollHandlerEditor
    {
        public static RagdollChainBone copyBoneSettingsOf = null;

        private static GUILayoutOption[] w22h18
        { get { if( _w22h18 == null ) _w22h18 = new GUILayoutOption[] { GUILayout.Width( 22 ), GUILayout.Height( 18 ) }; return _w22h18; } }
        private static GUILayoutOption[] _w22h18 = null;

        private static GUILayoutOption[] w30h22
        { get { if( _w30h22 == null ) _w30h22 = new GUILayoutOption[] { GUILayout.Width( 30 ), GUILayout.Height( 22 ) }; return _w30h22; } }
        private static GUILayoutOption[] _w30h22 = null;

        private static RagdollChainBone _selectedCollidersSetupBone = null;
        private static SerializedProperty _selectedCollidersSetupBoneProp = null;

        public static void ClearReferencesOnDestroy()
        {
            _selectedCollidersSetupBone = null;
            _selectedCollidersSetupBoneProp = null;
        }

        public static void GenericMenu_BoneColliderOptions( RagdollHandler handler, RagdollBonesChain chain, RagdollChainBone bone )
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem( new GUIContent( "Assign " + bone.BaseColliderSetup.ColliderType + " collider type to all bones in chain" ), false, () =>
            {
                foreach( var b in chain.BoneSetups )
                {
                    b.BaseColliderSetup.ColliderType = bone.BaseColliderSetup.ColliderType;
                }

                OnChange( null, handler );
            } );

            menu.AddItem( new GUIContent( "Assign collider settings of this bone, to all bones in chain" ), false, () =>
            {
                foreach( var b in chain.BoneSetups )
                {
                    b.PasteColliderSettingsOfOtherBone( bone );
                }

                OnChange( null, handler );
            } );

            int idx = chain.GetIndex( bone );

            if( idx < chain.BoneSetups.Count - 1 )
            {
                menu.AddItem( new GUIContent( "Adjust collider size, basing on the child bone position" ), false, () =>
                {
                    chain.AdjustColliderSettingsBasingOnTheStartEndPosition( bone, idx, bone.SourceBone.position, chain.GetBone( idx + 1 ).SourceBone.position );

                    OnChange( null, handler );
                } );
            }

            menu.AddItem( new GUIContent( "Copy <Collider> settings of this single bone" ), false, () =>
            {
                copyBoneSettingsOf = bone;
                OnChange( null, handler );
            } );

            if( copyBoneSettingsOf != null )
            {
                menu.AddItem( new GUIContent( "Paste Size <Collider> settings of :" + ( copyBoneSettingsOf.SourceBone != null ? copyBoneSettingsOf.SourceBone.name : "" ) + ":" ), false, () =>
                {
                    bone.PasteColliderSizeSettingsOfOtherBone( copyBoneSettingsOf );
                    OnChange( null, handler );
                } );

                menu.AddItem( new GUIContent( "Paste :All: <Collider> settings of :" + ( copyBoneSettingsOf.SourceBone != null ? copyBoneSettingsOf.SourceBone.name : "" ) + ":" ), false, () =>
                {
                    bone.PasteColliderSettingsOfOtherBone( copyBoneSettingsOf );
                    OnChange( null, handler );
                } );
            }

            menu.ShowAsContext();
        }

        public static void GenericMenu_BonesSetupOperations( RagdollHandler handler, SerializedProperty ragdollHandlerProp, RagdollBonesChain chain, int i )
        {
            GenericMenu menu = new GenericMenu();

            int idx = i;
            menu.AddItem( new GUIContent( "Move this bone to be first in list ↑↑" ), false, () => { EditorUtility_MoveBone( idx, 0, chain.BoneSetups ); OnChange( ragdollHandlerProp, handler ); } );
            menu.AddItem( new GUIContent( "Move this bone to be last in list ↓↓" ), false, () => { EditorUtility_MoveBone( idx, chain.BoneSetups.Count - 1, chain.BoneSetups ); OnChange( ragdollHandlerProp, handler ); } );
            menu.AddItem( new GUIContent( "Move this bone 1 index back ↑" ), false, () => { EditorUtility_MoveBone( idx, idx - 1, chain.BoneSetups ); OnChange( ragdollHandlerProp, handler ); } );
            menu.AddItem( new GUIContent( "Move this bone 1 index forward ↓" ), false, () => { EditorUtility_MoveBone( idx, idx + 1, chain.BoneSetups ); OnChange( ragdollHandlerProp, handler ); } );

            menu.ShowAsContext();
        }

        public static void GUI_DrawBoneCollidersSetupList( RagdollHandler handler, SerializedProperty boneSetups, RagdollBonesChain chain )
        {
            EditorGUILayout.BeginVertical( FGUI_Resources.BGInBoxBlankStyle );

            if( _selectedCollidersSetupBone != null )
            {
                if( chain.BoneSetups.Contains( _selectedCollidersSetupBone ) == false )
                    _selectedCollidersSetupBone = null;
            }

            if( chain.BoneSetups.Count > 0 )
            {
                for( int i = 0; i < boneSetups.arraySize; i++ )
                {
                    var bone = chain.BoneSetups[i];
                    var boneProp = boneSetups.GetArrayElementAtIndex( i );
                    EditorGUILayout.BeginVertical( FGUI_Resources.BGInBoxBlankStyle );
                    //EditorGUILayout.BeginVertical(FGUI_Resources.BGInBoxStyleH);

                    EditorGUILayout.BeginHorizontal();

                    if( GUILayout.Button( new GUIContent( FGUI_Resources.TexTargetingIcon, "Solo View - Hiding other bones (foldout) and showing only this one." ), FGUI_Resources.ButtonStyle, w22h18 ) ) { GUI_Contruct_SoloEditBone( chain, bone ); _selectedCollidersSetupBone = null; }

                    if( GUILayout.Button( Icon_Collider, EditorStyles.label, w22h18 ) )
                    {
                        if( RagdollHandlerEditor.IsRightMouseButton() )
                        {
                            GenericMenu_BoneColliderOptions( handler, chain, bone );
                        }
                        else
                        {
                            _selectedCollidersSetupBone = null;
                            bone._EditorCollFoldout = !bone._EditorCollFoldout;
                        }
                    }

                    bool foldouted = bone._EditorCollFoldout && _selectedCollidersSetupBone == null;
                    if( GUILayout.Button( FGUI_Resources.GetFoldSimbol( foldouted, true ), EditorStyles.label, w22h18 ) )
                    {
                        bone._EditorCollFoldout = !bone._EditorCollFoldout;
                        _selectedCollidersSetupBone = null;
                        EditorGUI.FocusTextInControl( "" );
                    }

                    if( _selectedCollidersSetupBone == bone ) GUI.color = new Color( 0.2f, 1f, 0.4f, 1f );

                    if( bone.SourceBone ) if( GUILayout.Button( new GUIContent( bone.SourceBone.name, "Click here on the bone name label, to display its settings in different way than foldout." ), EditorStyles.label ) )
                        {
                            GUI_Contruct_SoloEditBone( chain, bone );
                            bone._EditorCollFoldout = true;

                            if( _selectedCollidersSetupBone == bone )
                            {
                                _selectedCollidersSetupBone = null;
                                bone._EditorCollFoldout = false;
                            }
                            else _selectedCollidersSetupBone = bone;

                            EditorGUI.FocusTextInControl( "" );

                            _selectedCollidersSetupBoneProp = boneProp;
                            //bone._EditorCollFoldout = !bone._EditorCollFoldout;
                        }

                    GUI.color = Color.white;

                    GUILayout.Space( 6 );
                    var coll = boneProp.FindPropertyRelative( "colliders" );
                    coll = coll.GetArrayElementAtIndex( bone._Editor_SelectedCollider );
                    var sp = coll.FindPropertyRelative( "ColliderType" );
                    EditorGUILayout.PropertyField( sp, GUIContent.none );

                    if( bone.Editor_SelectedColliderSettings.ColliderType == RagdollChainBone.EColliderType.Capsule )
                    {
                        GUILayout.Space( 3 );
                        EditorGUIUtility.labelWidth = 31;
                        sp = coll.FindPropertyRelative( "CapsuleDirection" );
                        EditorGUILayout.PropertyField( sp, new GUIContent( "Axis:", "Capsule collider axis" ), GUILayout.MaxWidth( 68 ) );
                        EditorGUIUtility.labelWidth = 0;
                    }

                    GUILayout.Space( 3 );

                    //if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Refresh, "Reset auto settings"), FGUI_Resources.ButtonStyle, w22h18))
                    //{
                    //    bone.DoAutoSettings(handler, chain);
                    //}

                    EditorGUILayout.EndHorizontal();

                    if( _selectedCollidersSetupBone == null )
                        if( bone._EditorCollFoldout )
                        {
                            DrawBoneColliderSettings( boneProp, bone, handler, chain );
                        }

                    EditorGUILayout.EndVertical();

                    if( i < boneSetups.arraySize - 1 ) GUILayout.Space( 16 );
                }

                if( _selectedCollidersSetupBone != null )
                {
                    if( chain.BoneSetups.Contains( _selectedCollidersSetupBone ) == false )
                        _selectedCollidersSetupBone = null;
                    else if( _selectedCollidersSetupBoneProp != null )
                        DrawBoneColliderSettings( _selectedCollidersSetupBoneProp, _selectedCollidersSetupBone, handler, chain );
                }
            }

            EditorGUILayout.EndVertical();
        }

        public static Texture Icon_Collider
        { get { if( _icon_coll == null ) _icon_coll = FGUI_Resources.FindIcon( "Ragdoll Animator/SPR_RagdollCollider" ); return _icon_coll; } }
        private static Texture _icon_coll = null;

        private static void DrawBoneColliderSettings( SerializedProperty boneProp, RagdollChainBone bone, RagdollHandler handler, RagdollBonesChain chain )
        {
            if( boneProp == null || boneProp.serializedObject == null )
            {
                EditorGUILayout.HelpBox( "property disposed!", UnityEditor.MessageType.Warning );
                boneProp = null;
                return;
            }

            GUILayout.Space( 6 );
            EditorGUILayout.BeginHorizontal();

            bool drawingSliders = bone._EditorCollPosSliders || bone._EditorBoxCollSliders || bone._EditorRadiusSliders;

            if( bone.Colliders.Count < 2 )
            {
                EditorGUILayout.LabelField( drawingSliders ? "Collider Parameters" : "Bone Collider Parameters", EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth( drawingSliders ? 100 : 130 ) );
                GUILayout.Space( 10 );
            }

            var maxSlider = boneProp.FindPropertyRelative( "_EditorMaxSliderValue" );

            GUILayout.FlexibleSpace();

            if( drawingSliders )
            {
                GUI.color = new Color( 0.7f, 0.85f, 0.7f, 1f );
                EditorGUIUtility.labelWidth = 114;
                EditorGUIUtility.fieldWidth = 34;
                EditorGUILayout.PropertyField( maxSlider, new GUIContent( "Sliders Max Value: ", "Helper value for tweaking collider size with the sliders below." ), GUILayout.MaxWidth( 148 ) );
                GUILayout.Space( 4 );
                EditorGUIUtility.fieldWidth = 0;
                EditorGUIUtility.labelWidth = 0;
                GUI.color = Color.white;
            }

            //EditorGUILayout.LabelField("(Helper)", EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth(46));

            // Apply to symmetrical bone button
            if( chain.ChainType.IsLeg() || chain.ChainType.IsArm() )
            {
                DisplaySymmetryActionButton( ( RagdollChainBone symmetry ) => 
                { 
                    symmetry.PasteColliderSettingsOfOtherBoneSymmetrical( bone, handler );
                }, handler, bone, chain );

                GUILayout.Space( 4 );
            }

            if( bone.Colliders.Count > 1 )
            {
                string colliderDispName = "[0] Main";
                if( bone._Editor_SelectedCollider > 0 ) colliderDispName = "[" + bone._Editor_SelectedCollider + "] " + bone.Colliders[bone._Editor_SelectedCollider].ColliderType.ToString();

                if( GUILayout.Button( new GUIContent( colliderDispName, "Select collider to edtit" ), EditorStyles.popup, GUILayout.MaxWidth( 74 ) ) )
                {
                    GenericMenu menu = new GenericMenu();

                    for( int i = 0; i < bone.Colliders.Count; i++ )
                    {
                        int id = i;
                        menu.AddItem( new GUIContent( "[" + i + "] " + bone.Colliders[i].ColliderType.ToString() ), i == bone._Editor_SelectedCollider, () => { bone._Editor_SelectedCollider = id; } );
                    }

                    menu.ShowAsContext();
                }

                if( bone._Editor_SelectedCollider > 0 )
                {
                    FGUI_Inspector.RedGUIBackground();
                    if( GUILayout.Button( FGUI_Resources.GUIC_Remove, FGUI_Resources.ButtonStyle, _w22h18 ) )
                    {
                        bone.RemoveColliderSetup( bone._Editor_SelectedCollider );
                        bone._Editor_SelectedCollider -= 1;
                        OnChange( null, handler );
                    }
                    FGUI_Inspector.RestoreGUIBackground();
                }
            }

            if( GUILayout.Button( new GUIContent( " +", Icon_Collider, "Add Extra Collider to the bone" ), FGUI_Resources.ButtonStyle, GUILayout.MaxWidth( 40 ), GUILayout.Height( 18 ) ) )
            {
                var newColl = bone.AddColliderSetup();
                newColl.ColliderCenter += new Vector3( 0.1f, 0.1f, 0.1f );
                bone._Editor_SelectedCollider += 1;
                boneProp.serializedObject.ApplyModifiedProperties();
                boneProp.serializedObject.Update();
            }

            //if( GUILayout.Button( FGUI_Resources.GUIC_More, FGUI_Resources.ButtonStyle, _w22h18 ) )
            //{
            //    GenericMenu menu = new GenericMenu();

            //    menu.AddItem( new GUIContent( "Add Extra Collider to the bone" ), false, () => { bone.AddExtraCollider(); bone._Editor_SelectedCollider = bone.Colliders.Count - 1; } );

            //    if( bone._Editor_SelectedCollider > 0 )
            //        menu.AddItem( new GUIContent( "Remove selected extra colllider" ), false, () => { bone.RemoveExtraCollider( bone._Editor_SelectedCollider ); bone._Editor_SelectedCollider -= 1; } );

            //    menu.ShowAsContext();
            //}

            EditorGUILayout.EndHorizontal();
            if( maxSlider.floatValue < 0f ) maxSlider.floatValue = 0f;

            float maxSliderV = maxSlider.floatValue; if( bone._EditorMaxSliderValue <= 0f ) maxSliderV = 0f;

            FGUI_Inspector.DrawUILineCommon( 12, 2 );

            var colliderProp = boneProp.FindPropertyRelative( "colliders" );
            colliderProp = colliderProp.GetArrayElementAtIndex( bone._Editor_SelectedCollider );

            var colliderSettings = bone.Colliders[bone._Editor_SelectedCollider];

            EditorGUIUtility.labelWidth = 114;

            if( colliderSettings.ColliderType == RagdollChainBone.EColliderType.Capsule )
            {
                var spRadius = colliderProp.FindPropertyRelative( "ColliderRadius" );
                var spLength = spRadius.Copy(); spLength.Next( false );

                EditorGUILayout.BeginHorizontal();
                if( maxSlider.floatValue > 0f && bone._EditorMaxSliderValue > 0f ) if( GUILayout.Button( new GUIContent( FGUI_Resources.Tex_Sliders, "Use values or sliders for adjustments" ), FGUI_Resources.ButtonStyle, w22h18 ) ) { bone._EditorRadiusSliders = !bone._EditorRadiusSliders; }

                EditorGUI.BeginChangeCheck();
                if( maxSlider.floatValue > 0f && bone._EditorRadiusSliders ) EditorGUILayout.Slider( spRadius, 0.0001f, maxSlider.floatValue ); else EditorGUILayout.PropertyField( spRadius );
                if( EditorGUI.EndChangeCheck() ) if( spRadius.floatValue > spLength.floatValue / 2f ) spLength.floatValue = spRadius.floatValue * 2f;

                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                if( maxSlider.floatValue > 0f && bone._EditorRadiusSliders ) EditorGUILayout.Slider( spLength, 0.0001f, maxSlider.floatValue * 2f ); else EditorGUILayout.PropertyField( spLength );
                if( EditorGUI.EndChangeCheck() ) if( spLength.floatValue < spRadius.floatValue * 2f ) spRadius.floatValue = spLength.floatValue / 2f;

                GUILayout.Space( 6 );
                DrawBoneOffset( bone, colliderSettings, chain, colliderProp, maxSlider.floatValue );
            }
            else if( colliderSettings.ColliderType == RagdollChainBone.EColliderType.Sphere )
            {
                var spRadius = colliderProp.FindPropertyRelative( "ColliderRadius" );
                bool drawSliders = bone._EditorBoxCollSliders && maxSlider.floatValue > 0f && bone._EditorMaxSliderValue > 0f;

                EditorGUILayout.BeginHorizontal();

                if( maxSlider.floatValue > 0f && bone._EditorMaxSliderValue > 0f ) if( GUILayout.Button( new GUIContent( FGUI_Resources.Tex_Sliders, "Use values or sliders for adjustments" ), FGUI_Resources.ButtonStyle, w22h18 ) ) { bone._EditorBoxCollSliders = !bone._EditorBoxCollSliders; }

                if( drawSliders ) EditorGUILayout.Slider( spRadius, 0.001f, maxSliderV );
                else EditorGUILayout.PropertyField( spRadius );
                if( spRadius.floatValue < 0.0001f ) spRadius.floatValue = 0.0001f;

                EditorGUILayout.EndHorizontal();

                GUILayout.Space( 6 );
                DrawBoneOffset( bone, colliderSettings, chain, colliderProp, maxSlider.floatValue );
            }
            else if( colliderSettings.ColliderType == RagdollChainBone.EColliderType.Box )
            {
                var spBoxSize = colliderProp.FindPropertyRelative( "ColliderBoxSize" );

                EditorGUILayout.BeginHorizontal();
                Vector3 boxSize = colliderSettings.ColliderBoxSize;

                bool drawSliders = bone._EditorBoxCollSliders && maxSlider.floatValue > 0f && bone._EditorMaxSliderValue > 0f;

                if( maxSlider.floatValue > 0f && bone._EditorMaxSliderValue > 0f ) if( GUILayout.Button( new GUIContent( FGUI_Resources.Tex_Sliders, "Use values or sliders for adjustments" ), FGUI_Resources.ButtonStyle, w22h18 ) ) { bone._EditorBoxCollSliders = !bone._EditorBoxCollSliders; }
                if( drawSliders ) boxSize.x = EditorGUILayout.Slider( "Size X: ", boxSize.x, 0f, maxSlider.floatValue );
                else EditorGUILayout.PropertyField( spBoxSize );

                EditorGUILayout.EndHorizontal();

                if( drawSliders )
                {
                    boxSize.y = EditorGUILayout.Slider( "Size Y: ", boxSize.y, 0f, maxSlider.floatValue );
                    boxSize.z = EditorGUILayout.Slider( "Size Z: ", boxSize.z, 0f, maxSlider.floatValue );
                    spBoxSize.vector3Value = boxSize;
                }

                GUILayout.Space( 6 );
                DrawBoneOffset( bone, colliderSettings, chain, colliderProp, maxSlider.floatValue );
            }
            else if( colliderSettings.ColliderType == RagdollChainBone.EColliderType.Mesh )
            {
                //EditorGUILayout.HelpBox( "Mesh collider can only be offssetted (also mesh collider will be assigned as new child object of the bone)", MessageType.None );
                var spColliderMesh = colliderProp.FindPropertyRelative( "ColliderMesh" );
                EditorGUILayout.PropertyField( spColliderMesh );

                var spBoxSize = colliderProp.FindPropertyRelative( "ColliderBoxSize" );

                EditorGUILayout.BeginHorizontal();
                Vector3 boxSize = colliderSettings.ColliderBoxSize;

                bool drawSliders = bone._EditorMeshCollSliders && maxSlider.floatValue > 0f && bone._EditorMaxSliderValue > 0f;

                if( maxSlider.floatValue > 0f && bone._EditorMaxSliderValue > 0f ) if( GUILayout.Button( new GUIContent( FGUI_Resources.Tex_Sliders, "Use values or sliders for adjustments" ), FGUI_Resources.ButtonStyle, w22h18 ) ) { bone._EditorMeshCollSliders = !bone._EditorMeshCollSliders; }
                if( drawSliders ) boxSize.x = EditorGUILayout.Slider( "Size X: ", boxSize.x, 0f, maxSlider.floatValue );
                else EditorGUILayout.PropertyField( spBoxSize );

                EditorGUILayout.EndHorizontal();

                if( drawSliders )
                {
                    boxSize.y = EditorGUILayout.Slider( "Size Y: ", boxSize.y, 0f, maxSlider.floatValue );
                    boxSize.z = EditorGUILayout.Slider( "Size Z: ", boxSize.z, 0f, maxSlider.floatValue );
                    spBoxSize.vector3Value = boxSize;
                }

                GUILayout.Space( 6 );
                DrawBoneOffset( bone, colliderSettings, chain, colliderProp, maxSlider.floatValue );
            }
            else if( colliderSettings.ColliderType == RagdollChainBone.EColliderType.Other )
            {
                EditorGUILayout.HelpBox( "Using collider which is already attached to the source bone", MessageType.None );
                var spColliderMesh = colliderProp.FindPropertyRelative( "OtherReference" );

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField( spColliderMesh );

                if( colliderSettings.OtherReference == null )
                    if( bone.SourceBone.GetComponent<Collider>() )
                    {
                        if( GUILayout.Button( "Get" ) ) { colliderSettings.OtherReference = bone.SourceBone.GetComponent<Collider>(); OnChange( null, handler ); }
                    }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUIUtility.labelWidth = 0;

            if( colliderSettings.ColliderType != RagdollChainBone.EColliderType.Other )
            {
                FGUI_Inspector.DrawUILineCommon( 12, 2 );

                EditorGUIUtility.labelWidth = 144;
                var sp_RotCorr = colliderProp.FindPropertyRelative( "RotationCorrection" );
                EditorGUILayout.PropertyField( sp_RotCorr, new GUIContent( " " + sp_RotCorr.displayName, FGUI_Resources.Tex_Rotation, sp_RotCorr.tooltip ), GUILayout.Height( 17 ) );
                if( sp_RotCorr.vector3Value != Vector3.zero ) EditorGUILayout.HelpBox( "Rotation offset will put target collider as child object of the target bone.", MessageType.None );

                GUILayout.Space( 4 );
                EditorGUIUtility.labelWidth = 144;
                var sp_scaleMultiply = colliderProp.FindPropertyRelative( "ColliderSizeMultiply" );
                EditorGUILayout.PropertyField( sp_scaleMultiply, new GUIContent( " " + sp_scaleMultiply.displayName, EditorGUIUtility.IconContent( "ScaleTool" ).image, sp_scaleMultiply.tooltip ), GUILayout.Height( 17 ) );
                EditorGUIUtility.labelWidth = 0;
            }
        }

        private static void DrawBoneOffset( RagdollChainBone bone, RagdollChainBone.ColliderSetup colliderSettings, RagdollBonesChain chain, SerializedProperty colliderProp, float maxSlider )
        {
            EditorGUILayout.BeginHorizontal();

            if( maxSlider > 0f )
                if( GUILayout.Button( new GUIContent( FGUI_Resources.Tex_Sliders, "Use values or sliders for adjustments" ), FGUI_Resources.ButtonStyle, w22h18 ) ) { bone._EditorCollPosSliders = !bone._EditorCollPosSliders; }

            Vector3 off = colliderSettings.ColliderCenter;
            var sp_ColliderCenter = colliderProp.FindPropertyRelative( "ColliderCenter" );

            if( bone._EditorCollPosSliders && maxSlider > 0f )
                off.x = EditorGUILayout.Slider( "Center X: ", off.x, -maxSlider, maxSlider );
            else
                EditorGUILayout.PropertyField( sp_ColliderCenter );

            EditorGUILayout.EndHorizontal();

            if( bone._EditorCollPosSliders && maxSlider > 0f )
            {
                off.y = EditorGUILayout.Slider( "Center Y: ", off.y, -maxSlider, maxSlider );
                off.z = EditorGUILayout.Slider( "Center Z: ", off.z, -maxSlider, maxSlider );
                sp_ColliderCenter.vector3Value = off;
            }
        }
    }
}