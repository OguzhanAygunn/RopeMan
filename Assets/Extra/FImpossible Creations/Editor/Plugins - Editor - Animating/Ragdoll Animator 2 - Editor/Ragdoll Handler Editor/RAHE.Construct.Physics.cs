using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static partial class RagdollHandlerEditor
    {
        private static RagdollChainBone _selectedPhysicsSetupBone = null;
        private static SerializedProperty _selectedPhysicsSetupBoneProp = null;

        public static void GUI_DrawBonePhysicsSetupList( SerializedProperty ragdollHandlerProp, RagdollHandler handler, SerializedProperty boneSetups, RagdollBonesChain chain )
        {
            EditorGUILayout.BeginVertical( FGUI_Resources.BGInBoxBlankStyle );

            if( _selectedPhysicsSetupBone != null )
            {
                if( chain.BoneSetups.Contains( _selectedPhysicsSetupBone ) == false )
                    _selectedPhysicsSetupBone = null;
            }

            if( chain.BoneSetups.Count > 0 )
            {
                for( int i = 0; i < boneSetups.arraySize; i++ )
                {
                    var bone = chain.BoneSetups[i];
                    var boneProp = boneSetups.GetArrayElementAtIndex( i );
                    EditorGUILayout.BeginVertical( FGUI_Resources.BGInBoxBlankStyle );

                    EditorGUILayout.BeginHorizontal();

                    if( GUILayout.Button( new GUIContent( FGUI_Resources.TexTargetingIcon, "Solo View - Hiding other bones (foldout) and showing only this one." ), FGUI_Resources.ButtonStyle, w22h18 ) ) { handler.Editor_HandlesUndoRecord(); GUI_Contruct_SoloEditBone( chain, bone ); _selectedPhysicsSetupBone = null; }

                    bool preEd = bone._EditorCollFoldout;
                    if( GUILayout.Button( FGUI_Resources.Tex_Physics, EditorStyles.label, w22h18 ) )
                    {
                        if( RagdollHandlerEditor.IsRightMouseButton() )
                        {
                            GenericMenu_PhysicsOperations( handler, ragdollHandlerProp, chain, bone );
                        }
                        else
                        {
                            handler.Editor_HandlesUndoRecord();
                            bone._EditorCollFoldout = !bone._EditorCollFoldout; _selectedPhysicsSetupBone = null;
                        }
                    }

                    bool foldouted = bone._EditorCollFoldout && _selectedPhysicsSetupBone == null;
                    if( GUILayout.Button( FGUI_Resources.GetFoldSimbol( foldouted, true ), EditorStyles.label, w22h18 ) ) { handler.Editor_HandlesUndoRecord(); bone._EditorCollFoldout = !bone._EditorCollFoldout; _selectedPhysicsSetupBone = null; }

                    if( _selectedPhysicsSetupBone == bone ) GUI.color = new Color( 0.2f, 1f, 0.4f, 1f );

                    if( GUILayout.Button( new GUIContent( bone.SourceBone.name, "Click here on the bone name label, to display its settings in different way than foldout." ), EditorStyles.label ) )
                    {
                        handler.Editor_HandlesUndoRecord();

                        GUI_Contruct_SoloEditBone( chain, bone );
                        bone._EditorCollFoldout = true;

                        if( _selectedPhysicsSetupBone == bone )
                        {
                            _selectedPhysicsSetupBone = null;
                            bone._EditorCollFoldout = false;
                        }
                        else _selectedPhysicsSetupBone = bone;

                        _selectedPhysicsSetupBoneProp = boneProp;
                    }

                    GUI.color = Color.white;

                    if( preEd != bone._EditorCollFoldout ) if( bone._EditorCollFoldout == false )
                        {
                            if( bone._EditorCollFoldout == false ) if( chain.ParentHandler != null ) chain.ParentHandler.Editor_EndPreviewBone( bone.SourceBone );
                        }

                    GUILayout.Space( 3 );

                    float calculatedMass = (float)System.Math.Round( bone.GetMass( chain ), 2 );

                    if( !bone._EditorCollFoldout )
                    {
                        if( GUILayout.Button( new GUIContent( "Mass:", "Slider for target rigidbody mass value." ), EditorStyles.whiteMiniLabel, GUILayout.Width( 32 ) ) )
                        {
                            handler.Editor_HandlesUndoRecord();
                            bone._EditorCollFoldout = !bone._EditorCollFoldout;
                        }

                        var sp = boneProp.FindPropertyRelative( "MassMultiplier" );
                        float max = constructPhysicsMaxSlider;
                        //if (sp.floatValue < 0.109f) max = 0.11f;
                        //else if (sp.floatValue < 0.5f) max = 0.51f;
                        sp.floatValue = GUILayout.HorizontalSlider( sp.floatValue, 0f, max );
                        GUILayout.Space( 6 );
                    }
                    else
                    {
                        GUI.enabled = !Application.isPlaying;

                        //if (GUI.enabled) if (_referencePoseReport == null || _referencePoseReport.Value != RagdollHandler.EReferencePoseReport.ReferencePoseOK) GUI.backgroundColor = new Color(1f, 1f, 0.5f);

                        bool anyPreviewing = bone._EditorMainAxisPreview || bone._EditorSecondAxisPreview || bone._EditorThirdPreview;

                        //if (anyPreviewing)
                        {
                            if( bone.SourceBone != null )
                            {
                                if( handler.Editor_IsPreviewingBone( bone.SourceBone ) ) GUI.backgroundColor = Color.green;

                                if( anyPreviewing == false && handler.Editor_IsPreviewingBone( bone.SourceBone ) == false ) GUI.enabled = false;
                                if( GUILayout.Button( new GUIContent( " Preview", FGUI_Resources.Tex_Limits, "Preview angles on the source bones on the scene" ), GUILayout.Height( 18 ) ) )
                                {
                                    handler.Editor_HandlesUndoRecord();
                                    //if (_referencePoseReport.Value != RagdollHandler.EReferencePoseReport.ReferencePoseOK)
                                    //{
                                    //    EditorUtility.DisplayDialog("No Reference T-Pose!", "Reference T-Pose is required in order to turn on bone preview on the scene view.\n\nGo to Construct -> Setup and store T-Pose.", "Ok");
                                    //}
                                    //else
                                    {
                                        if( handler.Editor_IsPreviewingBone( bone.SourceBone ) )
                                        {
                                            handler.Editor_EndPreviewBone( bone.SourceBone );
                                        }
                                        else
                                        {
                                            handler.Editor_StartPreviewBone( bone.SourceBone );
                                        }
                                    }
                                }
                                GUI.enabled = true;
                            }
                        }
                        //else
                        {
                            //handler.Editor_EndPreviewBone(bone.SourceBone);
                        }

                        GUI.backgroundColor = Color.white;
                        GUI.enabled = true;
                        GUILayout.Space( 6 );
                    }

                    EditorGUILayout.LabelField( new GUIContent( calculatedMass.ToString(), "Target Rigidbody.Mass for the bone collider" ), EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth( 30 ) );
                    var rect = GUILayoutUtility.GetLastRect();
                    if( GUI.Button( rect, GUIContent.none, EditorStyles.label ) ) EditorUtility.DisplayDialog( "Info", "Target Rigidbody.Mass for the bone collider", "Ok" );
                    EditorGUILayout.LabelField( "=", EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth( 12 ) );
                    EditorGUILayout.LabelField( new GUIContent( Mathf.Round( ( calculatedMass / handler.ReferenceMass ) * 100f ).ToString() + "%", "Percentage value in comparison to the Max Mass value" ), EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth( 28 ) );

                    //if (GUILayout.Button(new GUIContent("Reset", "Reset auto settings"), FGUI_Resources.ButtonStyle, GUILayout.Width(40)))
                    //{
                    //    bone.DoAutoSettings(handler, chain);
                    //}

                    EditorGUILayout.EndHorizontal();

                    if( _selectedPhysicsSetupBone == null )
                        if( bone._EditorCollFoldout )
                        {
                            GUILayout.Space( 8 );
                            DrawBonePhysicsSettings( ragdollHandlerProp, boneProp, bone, handler, chain );
                        }

                    GUILayout.Space( 3 );
                    EditorGUILayout.EndVertical();

                    if( i < boneSetups.arraySize - 1 ) GUILayout.Space( 16 );
                }

                if( _selectedPhysicsSetupBone != null )
                {
                    if( chain.BoneSetups.Contains( _selectedPhysicsSetupBone ) == false )
                        _selectedPhysicsSetupBone = null;
                    else if( _selectedPhysicsSetupBoneProp != null )
                    {
                        GUILayout.Space( 6 );
                        DrawBonePhysicsSettings( ragdollHandlerProp, _selectedPhysicsSetupBoneProp, _selectedPhysicsSetupBone, handler, chain );
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawBonePhysicsSettings( SerializedProperty ragdollHandlerProp, SerializedProperty boneProp, RagdollChainBone bone, RagdollHandler handler, RagdollBonesChain chain )
        {
            if( boneProp.serializedObject == null ) return;

            try
            {
                if( boneProp.serializedObject.targetObject == null ) return;
            }
            catch( System.Exception )
            {
                ragdollHandlerProp = null;
                return;
            }

            var sp = boneProp.FindPropertyRelative( "MassMultiplier" );
            Contruct_Physics_CalculateMinMaxSliders( handler );

            EditorGUILayout.BeginHorizontal();

            EditorGUIUtility.labelWidth = 100;

            //EditorGUILayout.Slider( sp, 0, constructPhysicsMaxSlider, new GUIContent( "Mass Multiplier", sp.tooltip ), GUILayout.MinWidth( 260f ) );
            EditorGUILayout.PropertyField( sp, new GUIContent( "Mass Multiplier:", sp.tooltip ), GUILayout.MinWidth( 260f ) );

            //if( chain.ChainType != ERagdollChainType.Unknown )
            //{
            //    if( GUILayout.Button( new GUIContent( "Auto Mass", "Applying predicted averate mass values for the limbs" ) ) )
            //    {
            //        UnityEngine.Debug.Log( "to implement" );
            //    }

            //    EditorGUILayout.LabelField( "Suggested Mass for " + chain.ChainType.ToString() + " : " + 1.ToString(), EditorStyles.centeredGreyMiniLabel );
            //}

            //if( GUILayout.Button( new GUIContent( "Reset", "Reset auto mass value" ), FGUI_Resources.ButtonStyle, GUILayout.Width( 40 ) ) )
            //{
            //    bone.DoAutoMassSettings( handler, chain );
            //}

            if( bone._EditorPhysicsExtraSettings ) GUI.backgroundColor = Color.green;

            if( GUILayout.Button( new GUIContent( FGUI_Resources.Tex_AB, "Display other physics settings" ), FGUI_Resources.ButtonStyle, _w22h18 ) )
            {
                handler.Editor_HandlesUndoRecord();
                bone._EditorPhysicsExtraSettings = !bone._EditorPhysicsExtraSettings;
            }

            GUI.backgroundColor = Color.white;

            //if (GUILayout.Button(new GUIContent(FGUI_Resources.Tex_Prepare, "Apply settings of this bone, to all other bones in the chain"), FGUI_Resources.ButtonStyle, w22h18))
            //{ //}
            // Apply to symmetrical bone button
            if( chain.ChainType.IsLeg() || chain.ChainType.IsArm() )
            {
                DisplaySymmetryActionButton( ( RagdollChainBone symmetry ) => { symmetry.PastePhysicsSettingsOfOtherBoneSymmetrical( bone ); }, handler, bone, chain );
                GUILayout.Space( 4 );
            }
            else GUI.backgroundColor = ( bone.OverrideMaterial != null || bone.UseIndividualParameters ) ? new Color( 0.65f, 0.85f, 1f, 1f ) : Color.white;
            if( GUILayout.Button( FGUI_Resources.GUIC_More, FGUI_Resources.ButtonStyle, w22h18 ) )
            {
                handler.Editor_HandlesUndoRecord();
                GenericMenu_PhysicsOperations( handler, ragdollHandlerProp, chain, bone );
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space( 4 );

            EditorGUILayout.BeginHorizontal();
            sp.Next( false );
            ExtensiveSlider( sp );

            GUILayout.Space( 8 );
            sp.Next( false );
            EditorGUIUtility.labelWidth = 40;
            EditorGUILayout.PropertyField( sp, new GUIContent( "Boost:", sp.tooltip ), GUILayout.MaxWidth( 78 ) );
            sp.floatValue = Mathf.Clamp( sp.floatValue, 0f, 2f );
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndHorizontal();

            FGUI_Inspector.DrawUILineCommon( 12, 2 );
            EditorGUIUtility.labelWidth = 0;

            if( bone._EditorPhysicsExtraSettings == false )
            {
                if( chain.UnlimitedRotations )
                {
                    GUILayout.Space( 4 );
                    EditorGUILayout.HelpBox( "Chain <UnlimitedRotations> is true. So axis angle settings are hidden.", MessageType.None );
                }
                else
                {
                    if( chain.AxisLimitRange != 1f )
                    {
                        EditorGUILayout.HelpBox( "Chain <Axis Range Multiply> is not equal one! Axis angles are modified!", MessageType.None );
                        GUILayout.Space( 4 );
                    }

                    // Main Axis
                    EditorGUILayout.BeginHorizontal();
                    GUI.backgroundColor = bone._EditorMainAxisPreview ? Color.green : Color.white;
                    if( GUILayout.Button( new GUIContent( FGUI_Resources.Tex_Limits, "Click to preview joint rotation limits on the scene view" ), FGUI_Resources.ButtonStyle, _w22h18 ) )
                    {
                        handler.Editor_HandlesUndoRecord();

                        if( RagdollHandlerEditor.IsRightMouseButton() )
                            bone._EditorMainAxisPreview = !bone._EditorMainAxisPreview;
                        else
                        {
                            bone._EditorMainAxisPreview = !bone._EditorMainAxisPreview;
                            bone._EditorSecondAxisPreview = false;
                            bone._EditorThirdPreview = false;
                        }
                    }

                    FGUI_Inspector.RestoreGUIBackground();
                    EditorGUIUtility.labelWidth = 77;
                    GUI.enabled = !Application.isPlaying;
                    sp.Next( false ); EditorGUILayout.PropertyField( sp, new GUIContent( "Main Axis:", sp.tooltip ) );
                    sp.Next( false );

                    if( bone.MainAxis == EJointAxis.Custom )
                    {
                        EditorGUILayout.PropertyField( sp, GUIContent.none );
                        sp.Next( false );
                    }
                    else
                    {
                        sp.Next( false );
                        GUILayout.Space( 6 );
                        EditorGUIUtility.labelWidth = 28;
                        EditorGUILayout.PropertyField( sp, new GUIContent( "Inv:", "Inverse Axis" ), GUILayout.MaxWidth( 44 ) );
                    }

                    GUI.enabled = true;

                    EditorGUILayout.EndHorizontal();

                    // Main Axis Limits
                    EditorGUILayout.BeginHorizontal();
                    sp.Next( false ); //EditorGUILayout.PropertyField(sp, new GUIContent("Min:", "Main Axis Low Angle limit value for the unity joint"));
                    var minprop = sp.Copy();
                    //GUILayout.Space(6);
                    sp.Next( false ); //EditorGUILayout.PropertyField(sp, new GUIContent("Max:", "Main Axis High Angle limit value for the unity joint"));
                    var maxprop = sp.Copy();

                    float vmin = minprop.floatValue;
                    float vmax = maxprop.floatValue;
                    EditorGUIUtility.labelWidth = 107;
                    EditorGUILayout.MinMaxSlider( new GUIContent( "Main Axis Limits:", "Min max rotation angle value for unity physical joint" ), ref vmin, ref vmax, -177, 177f );

                    //EditorGUILayout.LabelField(Mathf.Round(vmin).ToString() + "  :  " + Mathf.Round(vmax).ToString(), EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth(80));
                    vmin = Mathf.Round( EditorGUILayout.FloatField( vmin, GUILayout.MaxWidth( 34 ) ) );
                    vmax = Mathf.Round( EditorGUILayout.FloatField( vmax, GUILayout.MaxWidth( 34 ) ) );

                    if( vmin > vmax ) vmin = vmax;
                    if( vmax < vmin ) vmax = vmin;

                    minprop.floatValue = vmin;
                    maxprop.floatValue = vmax;

                    EditorGUILayout.EndHorizontal();

                    EditorGUIUtility.labelWidth = 0;
                    GUILayout.Space( 8 );

                    // Secondary Axis
                    EditorGUILayout.BeginHorizontal();
                    GUI.backgroundColor = bone._EditorSecondAxisPreview ? Color.green : Color.white;
                    if( GUILayout.Button( new GUIContent( FGUI_Resources.Tex_Limits, "Click to preview joint rotation limits on the scene view" ), FGUI_Resources.ButtonStyle, _w22h18 ) )
                    {
                        handler.Editor_HandlesUndoRecord();

                        if( RagdollHandlerEditor.IsRightMouseButton() )
                            bone._EditorSecondAxisPreview = !bone._EditorSecondAxisPreview;
                        else
                        {
                            bone._EditorSecondAxisPreview = !bone._EditorSecondAxisPreview;
                            bone._EditorMainAxisPreview = false;
                            bone._EditorThirdPreview = false;
                        }
                    }
                    FGUI_Inspector.RestoreGUIBackground();
                    EditorGUIUtility.labelWidth = 77;
                    GUI.enabled = !Application.isPlaying;
                    sp.Next( false ); EditorGUILayout.PropertyField( sp, new GUIContent( "Secondary:", sp.tooltip ) );
                    sp.Next( false );

                    if( bone.SecondaryAxis == EJointAxis.Custom )
                    {
                        EditorGUILayout.PropertyField( sp, GUIContent.none );
                        sp.Next( false );
                    }
                    else
                    {
                        sp.Next( false );
                        GUILayout.Space( 6 );
                        EditorGUIUtility.labelWidth = 28;
                        EditorGUILayout.PropertyField( sp, new GUIContent( "Inv:", "Inverse Axis" ), GUILayout.MaxWidth( 44 ) );
                    }

                    GUI.enabled = true;

                    EditorGUILayout.EndHorizontal();
                    EditorGUIUtility.labelWidth = 0;
                    sp.Next( false ); EditorGUILayout.PropertyField( sp, new GUIContent( "Both Sides Angle Limit:", sp.tooltip ) );

                    GUILayout.Space( 8 );
                    EditorGUILayout.BeginHorizontal();
                    GUI.backgroundColor = bone._EditorThirdPreview ? Color.green : Color.white;

                    if( GUILayout.Button( new GUIContent( FGUI_Resources.Tex_Limits, "Click to preview joint rotation limits on the scene view" ), FGUI_Resources.ButtonStyle, _w22h18 ) )
                    {
                        handler.Editor_HandlesUndoRecord();

                        if( RagdollHandlerEditor.IsRightMouseButton() )
                            bone._EditorThirdPreview = !bone._EditorThirdPreview;
                        else
                        {
                            bone._EditorThirdPreview = !bone._EditorThirdPreview;
                            bone._EditorSecondAxisPreview = false;
                            bone._EditorMainAxisPreview = false;
                        }
                    }

                    FGUI_Inspector.RestoreGUIBackground();
                    sp.Next( false ); EditorGUILayout.PropertyField( sp, new GUIContent( "Third Axis Angle Limit:", sp.tooltip ) );
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                sp = boneProp.FindPropertyRelative( "OverrideMaterial" );
                EditorGUILayout.PropertyField( sp );
                GUILayout.Space( 4 );
                EditorGUIUtility.labelWidth = 160;
                sp.Next( false ); EditorGUILayout.PropertyField( sp );
                EditorGUIUtility.labelWidth = 0;

                if( sp.boolValue ) // Individual params
                {
                    GUILayout.Space( 3 );
                    sp.Next( false ); EditorGUILayout.PropertyField( sp );
                    sp.Next( false ); EditorGUILayout.PropertyField( sp );
                    sp.Next( false ); EditorGUILayout.PropertyField( sp );
                    sp.Next( false ); EditorGUILayout.PropertyField( sp );
                }
                else
                {
                    sp.Next( false ); sp.Next( false ); sp.Next( false ); sp.Next( false );
                }

                GUILayout.Space( 10 );
                sp.Next( false ); EditorGUILayout.PropertyField( sp ); if( sp.floatValue < 0f ) sp.floatValue = 0f; // Spring pow overr
                sp.Next( false ); EditorGUILayout.PropertyField( sp ); if( sp.floatValue < 0f ) sp.floatValue = 0f; // Spring damp overr

                sp.Next( false ); EditorGUILayout.PropertyField( sp ); // Hard Match mul
                sp.Next( false ); EditorGUILayout.PropertyField( sp ); // Hard Match Override
                sp.Next( false ); EditorGUILayout.PropertyField( sp ); // Conn Override

                GUILayout.Space( 4 );
                sp.Next( false ); EditorGUILayout.PropertyField( sp ); // Disable Events
                sp.Next( false ); EditorGUILayout.PropertyField( sp ); // Force Limits Switch

                GUILayout.Space( 4 );
                sp.Next( false ); EditorGUILayout.PropertyField( sp ); // Force Kinematic
                GUILayout.Space( 2 );
                sp.Next( false ); EditorGUILayout.PropertyField( sp ); // Bone Blend

                GUILayout.Space( 10 );
                if( GUILayout.Button( "Close Extra Settings" ) ) { handler.Editor_HandlesUndoRecord(); bone._EditorPhysicsExtraSettings = false; }
            }

            //GUILayout.Space(10);

            //EditorGUILayout.BeginHorizontal();
            //GUI.backgroundColor = !bone._EditorPhysicsExtraSettings ? Color.green : Color.white;
            //if (GUILayout.Button("Main", EditorStyles.miniButtonLeft)) { bone._EditorPhysicsExtraSettings = false; }

            //if (bone._EditorPhysicsExtraSettings) GUI.backgroundColor = Color.green;
            //else
            //    GUI.backgroundColor = (bone.OverrideMaterial != null || bone.UseIndividualParameters) ? new Color(0.65f, 0.85f, 1f, 1f) : Color.white;

            //if (GUILayout.Button("Extra Settings", EditorStyles.miniButtonRight)) { bone._EditorPhysicsExtraSettings = true; }
            //GUI.backgroundColor = Color.white;
            //EditorGUILayout.EndHorizontal();

            GUILayout.Space( 2 );

            EditorGUIUtility.labelWidth = 0;
        }

        private static void ExtensiveSlider( SerializedProperty prop, float min = 0f, float max = 1f, float overMax = 5f )
        {
            float range = max + 0.0001f;
            if( prop.floatValue > max ) range = overMax;
            EditorGUILayout.Slider( prop, min, range );
            //new GUIContent(overrideName == ""? prop.displayName : overrideName, prop.tooltip),
        }

        private static void GUI_Contruct_SoloEditBone( RagdollBonesChain chain, RagdollChainBone bone )
        {
            for( int i = 0; i < chain.BoneSetups.Count; i++ )
            {
                if( bone == chain.BoneSetups[i] ) { bone._EditorCollFoldout = !bone._EditorCollFoldout; }
                else chain.BoneSetups[i]._EditorCollFoldout = false;
            }
        }

        private static float constructPhysicsMaxSlider = 1f;

        private static void Contruct_Physics_CalculateMinMaxSliders( RagdollHandler handler )
        {
            float max = 0f;

            foreach( var chain in handler.Chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    if( bone.MassMultiplier > max ) max = bone.MassMultiplier;
                }
            }

            constructPhysicsMaxSlider = (float)System.Math.Round( max + 0.02, 2 );
        }
    }
}