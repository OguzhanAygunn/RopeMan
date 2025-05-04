using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static partial class RagdollHandlerEditor
    {
        public static void GUI_DrawSetupCategory( SerializedProperty ragdollHandlerProp, RagdollHandler handler )
        {
            RefreshBaseReferences( ragdollHandlerProp );
            GUI_DrawFundmentalReferences( ragdollHandlerProp, handler );
        }

        public static void GUI_DrawFundmentalReferences( SerializedProperty ragdollHandlerProp, RagdollHandler handler )
        {
            EditorGUILayout.BeginVertical( FGUI_Resources.BGInBoxBlankStyle );
            EditorGUILayout.BeginVertical( FGUI_Resources.BGInBoxBlankStyle );

            bool drawJustBaseRefs;

            if( handler.WasInitialized && handler.RagdollLogic == ERagdollLogic.JustBoneComponents ) drawJustBaseRefs = false;
            else drawJustBaseRefs = !handler.IsBaseSetupValid();

            if( drawJustBaseRefs )
            {
                Setup_DrawBaseReferences( ragdollHandlerProp, handler );
            }
            else
            {
                GUILayout.Space( -3 );
                EditorGUILayout.BeginHorizontal();

                if( handler._EditorMainCategory == RagdollHandler.ERagdollSetupSection.Main ) GUI.backgroundColor = Color.green; else GUI.backgroundColor = Color.white;
                if( GUILayout.Button( new GUIContent( "  References ", FGUI_Resources.FindIcon( "Fimp/Small Icons/Anchor" ) ), EditorStyles.miniButtonLeft ) ) { handler.Editor_HandlesUndoRecord(); handler._EditorMainCategory = RagdollHandler.ERagdollSetupSection.Main; }
                if( handler._EditorMainCategory == RagdollHandler.ERagdollSetupSection.Physics ) GUI.backgroundColor = Color.green; else GUI.backgroundColor = Color.white;
                if( GUILayout.Button( new GUIContent( "  Main Physics ", FGUI_Resources.Tex_Physics ), EditorStyles.miniButtonRight ) ) { handler.Editor_HandlesUndoRecord(); handler._EditorMainCategory = RagdollHandler.ERagdollSetupSection.Physics; }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();

                GUILayout.Space( 10 );

                if( handler._EditorMainCategory == RagdollHandler.ERagdollSetupSection.Main )
                {
                    Setup_DrawBaseReferences( ragdollHandlerProp, handler );
                }
                else
                {
                    Setup_DrawPhysicalReferenceValues( ragdollHandlerProp, handler );
                }
            }

            GUILayout.Space( -7 );
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        public static void DrawBaseTransformField( SerializedProperty sp_BaseTransform, RagdollHandler handler )
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField( sp_BaseTransform ); // Base Transform

            #region Base Transform Ghosting

            if( sp_BaseTransform.objectReferenceValue == null )
            {
                Transform parent;
                Component comp = lastOwner.targetObject as Component;
                if( comp ) parent = comp.transform;
                else parent = handler.HelperOwnerTransform;

                if( parent )
                {
                    GUI.enabled = false;
                    EditorGUILayout.ObjectField( parent, typeof( Transform ), true );
                    GUI.enabled = true;
                }
            }

            #endregion Base Transform Ghosting

            EditorGUILayout.EndHorizontal();
        }

        static bool foldoutRagdollSize = false;

        private static void Setup_DrawBaseReferences( SerializedProperty ragdollHandlerProp, RagdollHandler handler )
        {
            EditorGUIUtility.labelWidth = 156;
            var BaseTransform = GetProperty( "BaseTransform" );
            var spc = BaseTransform.Copy();

            DrawBaseTransformField( spc, handler );

            spc.Next( false );

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField( spc, new GUIContent( "Mecanim (Optional)", spc.tooltip ) ); // Animator
            EditorGUILayout.EndHorizontal();
            spc.Next( false );

            var coreBone = GetProperty( "chains" );
            if( coreBone != null )
            {
                if( coreBone.arraySize > 0 )
                {
                    coreBone = coreBone.GetArrayElementAtIndex( 0 );
                    coreBone = coreBone.FindPropertyRelative( "BoneSetups" );
                    coreBone = coreBone.GetArrayElementAtIndex( 0 );
                    coreBone = coreBone.FindPropertyRelative( "SourceBone" );

                    if( handler.Chains[0].BoneSetups[0].SourceBone == null ) GUI.backgroundColor = Color.yellow;
                    EditorGUILayout.PropertyField( coreBone, new GUIContent( handler.Mecanim ? " Anchor Bone (Pelvis):" : " Anchor Bone:", FGUI_Resources.FindIcon( "Fimp/Small Icons/Anchor" ), "Anchor bone which holds whole physical structure. If its humanoid setup, it should be pelvis -> parent of spine and legs." ) );
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    EditorGUILayout.HelpBox( "No Core chain! It is required for ragdoll animator to work!", MessageType.Error );
                }
            }

            GUI.backgroundColor = new Color( 0.75f, 1f, 0.75f, 1f );
            EditorGUILayout.PropertyField( ragdollHandlerProp.FindPropertyRelative( "RagdollDummyLayer" ) );
            GUI.backgroundColor = Color.white;

            EditorGUILayout.PropertyField( GetProperty( "IsHumanoid" ) );

            //GUILayout.Space( 4 );
            //if( handler.WasInitialized ) GUI.enabled = false;
            //EditorGUILayout.PropertyField( spc ); // Ragdoll Logic Enum
            //GUI.enabled = true;

            //if (handler.RagdollLogic == ERagdollLogic.JustBoneComponents)
            //{
            //    EditorGUILayout.PropertyField( GetProperty( "SpringsValue" ) );
            //}

            EditorGUIUtility.labelWidth = 0;

            //EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.PropertyField(spc); // Root Bone
            //if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("SkinnedMeshRenderer Icon").image, "Hit to try searching for root bone inside skinned meshes."), FGUI_Resources.ButtonStyle, GUILayout.Width(26), GUILayout.Height(18)))
            //    handler.TryFindRootBone();
            //EditorGUILayout.EndHorizontal();

            if( handler.RagdollLogic == ERagdollLogic.JustBoneComponents || handler.IsBaseSetupValid() )
            {
                GUILayout.Space( 12 );
                DisplayMaxMassParameter( ragdollHandlerProp, handler );

                GUILayout.Space( 4 );

                EditorGUIUtility.labelWidth = 170;
                EditorGUILayout.PropertyField( GetProperty( "RagdollThicknessMultiplier" ) );
                EditorGUIUtility.labelWidth = 0;

                var foldRagSize = GUILayoutUtility.GetLastRect();
                foldRagSize.size = new Vector2( 18, 14 );
                foldRagSize.x = 9; foldRagSize.y += 3;

                if( GUI.Button( foldRagSize, FGUI_Resources.GetFoldSimbolTex( foldoutRagdollSize, true ), EditorStyles.label ) ) { foldoutRagdollSize = !foldoutRagdollSize; }

                if( foldoutRagdollSize )
                {
                    EditorGUI.indentLevel++;
                    var sthic = GetProperty( "RagdollSizeMultiplier" );
                    EditorGUILayout.PropertyField( sthic, new GUIContent( "Size Multiplier", sthic.tooltip ) );
                    EditorGUI.indentLevel--;
                }

                var physProp = GetProperty( "ConnectedMassMultiply" ).Copy();

                GUI.backgroundColor = new Color( 0.75f, 1f, 0.75f, 1f );
                GUILayout.Space( 12 ); EditorGUIUtility.labelWidth = 172; EditorGUIUtility.fieldWidth = 28;
                EditorGUILayout.PropertyField( physProp, new GUIContent( " " + physProp.displayName, FGUI_Resources.FindIcon( "Ragdoll Animator/SPR_RagdollSprings" ), physProp.tooltip ), true );
                EditorGUI.indentLevel++;
                physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true );
                EditorGUI.indentLevel--;
                GUI.backgroundColor = Color.white;

                GUILayout.Space( 8 ); EditorGUIUtility.fieldWidth = 0;
                physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true ); // Phys Mat
                EditorGUI.indentLevel++;
                if( handler.CollidersPhysicMaterial == null ) GUI.enabled = false;
                physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true );
                GUI.enabled = true;
                EditorGUI.indentLevel--;

                GUILayout.Space( 12 );

                if( handler.WasInitialized ) GUI.enabled = false;
                EditorGUILayout.PropertyField( GetProperty( "RagdollLogic" ) ); // Ragdoll Logic Enum
                GUI.enabled = true;

                if( handler.RagdollLogic == ERagdollLogic.JustBoneComponents )
                {
                    EditorGUILayout.PropertyField( GetProperty( "SpringsValue" ) );
                }

                GUILayout.Space( 8 );

                //DisplayStoreTPoseButton( handler, ragdollHandlerProp );
                DisplayPreGenerateDummyButton( handler, ragdollHandlerProp );
            }
        }

        private static void Setup_DrawPhysicalReferenceValues( SerializedProperty ragdollHandlerProp, RagdollHandler handler )
        {
            //EditorGUILayout.LabelField( " Reference Values:", EditorStyles.boldLabel );
            //GUILayout.Space( -4 );

            EditorGUIUtility.labelWidth = 182;

            GUI.backgroundColor = new Color( 0.75f, 1f, 0.75f, 1f );
            EditorGUILayout.PropertyField( ragdollHandlerProp.FindPropertyRelative( "RagdollDummyLayer" ) );
            GUI.backgroundColor = Color.white;
            GUILayout.Space( 6 );

            var physProp = ragdollHandlerProp.FindPropertyRelative( "RigidbodiesInterpolation" );
            EditorGUILayout.PropertyField( physProp, new GUIContent( " " + physProp.displayName, EditorGUIUtility.IconContent( "Rigidbody Icon" ).image, physProp.tooltip ), true );
            physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true );
            physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true ); if( physProp.floatValue < 0f ) physProp.floatValue = 0f; // drag
            physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true ); if( physProp.floatValue < 0f ) physProp.floatValue = 0f; // angular drag

            GUILayout.Space( 8 );
            physProp.Next( false ); EditorGUILayout.PropertyField( physProp, new GUIContent( " " + physProp.displayName, EditorGUIUtility.IconContent( "ConfigurableJoint Icon" ).image, physProp.tooltip ), true );
            if( physProp.floatValue < 0f ) physProp.floatValue = 0f; // joint contact distance
            EditorGUI.indentLevel++;
            physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true ); if( physProp.floatValue < 0f ) physProp.floatValue = 0f;
            EditorGUI.indentLevel--;
            GUILayout.Space( 4 );

            GUI.backgroundColor = new Color( 0.75f, 1f, 0.75f, 1f );
            physProp.Next( false ); EditorGUILayout.PropertyField( physProp, new GUIContent( " " + physProp.displayName, EditorGUIUtility.IconContent( "SpringJoint Icon" ).image, physProp.tooltip ), true ); if( physProp.floatValue < 0f ) physProp.floatValue = 0f;
            EditorGUI.indentLevel++;
            physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true ); if( physProp.floatValue < 0f ) physProp.floatValue = 0f;
            EditorGUI.indentLevel--;
            GUI.backgroundColor = Color.white;

            // Moved
            //GUI.backgroundColor = new Color( 0.75f, 1f, 0.75f, 1f );
            //GUILayout.Space( 8 ); EditorGUIUtility.labelWidth = 172; EditorGUIUtility.fieldWidth = 28;
            //physProp.Next( false ); EditorGUILayout.PropertyField( physProp, new GUIContent( " " + physProp.displayName, FGUI_Resources.FindIcon( "Ragdoll Animator/SPR_RagdollSprings" ), physProp.tooltip ), true );
            //EditorGUI.indentLevel++;
            //physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true );
            ////physProp.Next( false ); EditorGUILayout.PropertyField( physProp, new GUIContent( " " + physProp.displayName, FGUI_Resources.FindIcon( "SPR_RagdollSprings" ), physProp.tooltip ), true );
            //EditorGUI.indentLevel--;
            //GUI.backgroundColor = Color.white;

            //GUILayout.Space( 8 ); EditorGUIUtility.fieldWidth = 0;
            //physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true ); // Phys Mat
            //EditorGUI.indentLevel++;
            //if( handler.CollidersPhysicalMaterial == null ) GUI.enabled = false;
            //physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true );
            //GUI.enabled = true;
            //EditorGUI.indentLevel--;
            physProp.Next( false );
            physProp.Next( false );
            physProp.Next( false );
            physProp.Next( false );

            GUILayout.Space( 4 );
            physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true );
            if( physProp.floatValue < 0f ) physProp.floatValue = 0f; // max angular velocity
            physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true );
            if( physProp.floatValue < 0f ) physProp.floatValue = 0f; // max velocity
            physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true ); // gravity
            physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true );
            if( physProp.floatValue < 0f ) physProp.floatValue = 0f; // max depenetration
            EditorGUIUtility.labelWidth = 0;

            GUILayout.Space( 6 ); EditorGUIUtility.labelWidth = 172;
            physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true );
            physProp.Next( false ); EditorGUILayout.PropertyField( physProp, true );

            GUILayout.Space( 7 );
            EditorGUILayout.HelpBox( "Changing these values runtime, is triggering update on the all joints and colliders only when changed through INSPECTOR WINDOW.\nWhen using coding, after modifying these values, call: \nragdollAnimator.User_UpdateAllBonesParametersAfterManualChanges()", UnityEditor.MessageType.None );
            EditorGUIUtility.labelWidth = 0;
        }
    }
}