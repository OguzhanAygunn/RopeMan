using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static partial class RagdollHandlerEditor
    {
        public static void GUI_DrawMotionCategory( SerializedProperty ragdollHandlerProp, RagdollHandler handler )
        {
            if( handler.RagdollLogic == ERagdollLogic.JustBoneComponents )
            {
                EditorGUILayout.HelpBox( "When using Just Bone Components ragdoll logic. Motion category is useless.", UnityEditor.MessageType.Info );
                GUI.enabled = false;
            }

            RefreshBaseReferences( ragdollHandlerProp );

            EditorGUILayout.BeginVertical( FGUI_Resources.BGInBoxBlankStyle );
            EditorGUIUtility.labelWidth = 120;

            var sp = GetProperty( "RagdollBlend" ).Copy();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField( sp, new GUIContent( sp.displayName, FGUI_Resources.FindIcon( "Ragdoll Animator/SPR_RAnimator" ), sp.tooltip ) ); // RagdollBlend

            if( sp.floatValue != handler.GetTotalBlend() )
            {
                EditorGUILayout.LabelField( Mathf.Round( handler.GetTotalBlend() * 100 ) + " %", EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth( 30 ) );
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space( 4 );

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 130;
            sp.Next( false );

            int preFall = sp.intValue;
            EditorGUILayout.PropertyField( sp, new GUIContent( sp.displayName, handler.IsInFallingMode ? FGUI_Resources.FindIcon( "Ragdoll Animator/SPR_RagdollState" ) : FGUI_Resources.FindIcon( "Ragdoll Animator/SPR_RagdollAnim2s" ), sp.tooltip ), true, GUILayout.MinWidth( 220 ) ); // AnimatingState

            if( sp.intValue != preFall && handler.WasInitialized )
            {
                int newVal = sp.intValue;
                sp.intValue = preFall;
                handler.AnimatingMode = (RagdollHandler.EAnimatingMode)newVal;
                sp.intValue = newVal;
                // trigger property actions
            }

            GUILayout.Space( 8 );
            sp.Next( false );
            GUILayout.FlexibleSpace();
            EditorGUIUtility.labelWidth = 112;
            //if( Application.isPlaying ) GUI.enabled = false;

            var limitSp = GetProperty( "AnimationMatchLimits" ).Copy();

            if( EditorGUIUtility.currentViewWidth > 400 )
            {
                EditorGUIUtility.labelWidth = 78;
                EditorGUILayout.PropertyField( limitSp, new GUIContent( "Joint Limits:", limitSp.tooltip ), GUILayout.MinWidth( 170 ) );
            }

            //GUI.enabled = true;
            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.EndHorizontal();

            if( EditorGUIUtility.currentViewWidth <= 400 )
            {
                EditorGUIUtility.labelWidth = 130;
                EditorGUI.indentLevel += 2;
                EditorGUILayout.PropertyField( limitSp, new GUIContent( "Joint Limits:", limitSp.tooltip ), GUILayout.MinWidth( 170 ) );
                EditorGUI.indentLevel -= 2;
            }

            GUILayout.Space( 4 );

            EditorGUIUtility.labelWidth = 160;
            sp.Next( false ); EditorGUILayout.PropertyField( sp, true ); // Solvers
            EditorGUIUtility.labelWidth = 0;

            EditorGUIUtility.labelWidth = 0;
            FGUI_Inspector.DrawUILineCommon( 10 );

            EditorGUILayout.BeginHorizontal();
            if( handler._EditorMotionCategory == RagdollHandler.ERagdollMotionSection.Main ) GUI.backgroundColor = Color.green;
            if( GUILayout.Button( new GUIContent( " Main", FGUI_Resources.FindIcon( "Ragdoll Animator/SPR_RagdollSprings" ), "Main ragdoll dummy muscles behaviour settings" ), EditorStyles.miniButtonLeft ) ) { handler._EditorMotionCategory = RagdollHandler.ERagdollMotionSection.Main; }
            if( handler._EditorMotionCategory == RagdollHandler.ERagdollMotionSection.Limbs ) GUI.backgroundColor = Color.green; else GUI.backgroundColor = Color.white;
            if( GUILayout.Button( new GUIContent( " Limbs", FGUI_Resources.FindIcon( "SPR_SHand" ), "Limb specific settings + shortcuts to display more settings (click on the limb icon)" ), EditorStyles.miniButtonMid ) ) { handler._EditorMotionCategory = RagdollHandler.ERagdollMotionSection.Limbs; }
            if( handler._EditorMotionCategory == RagdollHandler.ERagdollMotionSection.Extra ) GUI.backgroundColor = Color.green; else GUI.backgroundColor = Color.white;
            if( GUILayout.Button( new GUIContent( " Settings", FGUI_Resources.Tex_Tweaks, "Ragdoll playmode behaviour extra settings" ), EditorStyles.miniButtonRight ) ) { handler._EditorMotionCategory = RagdollHandler.ERagdollMotionSection.Extra; }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space( 4 );

            if( handler._EditorMotionCategory == RagdollHandler.ERagdollMotionSection.Main )
            {
                Motion_Main( ragdollHandlerProp, handler );
            }
            else if( handler._EditorMotionCategory == RagdollHandler.ERagdollMotionSection.Limbs )
            {
                Motion_Limbs( ragdollHandlerProp, handler );
            }
            else if( handler._EditorMotionCategory == RagdollHandler.ERagdollMotionSection.Extra )
            {
                Motion_Extra( ragdollHandlerProp, handler );
            }

            EditorGUILayout.EndVertical();
        }

        private static void Motion_Main( SerializedProperty ragdollHandlerProp, RagdollHandler handler )
        {
            EditorGUIUtility.labelWidth = 130;

            var sp = GetProperty( "MusclesPower" ).Copy();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField( sp, new GUIContent( "  Muscles Power:", FGUI_Resources.FindIcon( "Ragdoll Animator/SPR_JointRotation" ), sp.tooltip ) ); // SpringsPower
            if( handler.WasInitialized ) if( handler.targetMusclesPower != handler.MusclesPower ) EditorGUILayout.LabelField( "(" + System.Math.Round( handler.targetMusclesPower, 2 ) + ")", EditorStyles.centeredGreyMiniLabel, GUILayout.Width( 30 ) );
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;


            GUILayout.Space( 5 );
            sp.Next( false );

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField( sp, new GUIContent( " Springs Value:", EditorGUIUtility.IconContent( "SpringJoint Icon" ).image, sp.tooltip ) ); // SpringsValue

            sp.Next( false );
            EditorGUIUtility.labelWidth = 104;
            GUILayout.FlexibleSpace();
            if( sp.floatValue <= 0f )
            {
                float prev = handler.SpringsValue;
                GUI.color = new Color( 1f, 1f, 1f, 0.6f );
                float sval = EditorGUILayout.FloatField( new GUIContent( "On Fall Mode:", sp.tooltip ), prev, GUILayout.MinWidth( 146 ), GUILayout.MaxWidth( 158 ) );
                if( prev != sval ) handler.SpringsOnFall = sval;
                GUI.color = Color.white;
            }
            else
            {
                EditorGUILayout.PropertyField( sp, new GUIContent( "On Fall Mode:", sp.tooltip ), GUILayout.MinWidth( 146 ), GUILayout.MaxWidth( 158 ) ); // SpringsValue on fall
            }

            if( handler.WasInitialized ) if( handler.OverrideSpringsValueOnFall != null && handler.OverrideSpringsValueOnFall.Value != handler.SpringsValue ) EditorGUILayout.LabelField( "(" + Mathf.Round( handler.OverrideSpringsValueOnFall.Value ) + ")", EditorStyles.centeredGreyMiniLabel, GUILayout.Width( 48 ) );
            EditorGUILayout.EndHorizontal();

            if( !handler.IsInFallingMode ) GUI.backgroundColor = new Color( 0.85f, 1f, 0.85f, 1f );


            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 134;
            sp.Next( false ); EditorGUILayout.PropertyField( sp, new GUIContent( " Damping Value:", FGUI_Resources.FindIcon( "Ragdoll Animator/SPR_Damping" ), sp.tooltip ), GUILayout.MaxWidth( 174 ) ); // DampingValue
            GUI.backgroundColor = Color.white;

            if( sp.floatValue < 0f ) sp.floatValue = 0f;

            sp.NextVisible( false );
            if( handler.IsInFallingMode ) GUI.backgroundColor = new Color( 0.85f, 1f, 0.85f, 1f );
            EditorGUIUtility.labelWidth = 104;
            GUILayout.FlexibleSpace();
            EditorGUILayout.PropertyField( sp, new GUIContent( "On Fall Mode:", sp.tooltip ), GUILayout.MinWidth( 146 ), GUILayout.MaxWidth( 158 ) ); // DampingValue on fall
            GUI.backgroundColor = Color.white;
            if( sp.floatValue < 0f ) sp.floatValue = 0f;

            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 0;


            GUILayout.Space( 6 );
            sp.Next( false );
            if( !handler.IsInFallingMode ) GUI.backgroundColor = new Color( 0.85f, 1f, 0.85f, 1f );

            EditorGUILayout.BeginHorizontal();

            //EditorGUIUtility.fieldWidth = 26;
            EditorGUILayout.PropertyField( sp, new GUIContent( " Hard Matching:", EditorGUIUtility.IconContent( "Animator Icon" ).image, sp.tooltip ), GUILayout.ExpandWidth(true) ); // Hard Matching
            sp.NextVisible( false );
            sp.NextVisible( false );
            sp.NextVisible( false );

            //EditorGUIUtility.fieldWidth = 0;
            //EditorGUIUtility.labelWidth = 76;
            //EditorGUILayout.PropertyField( sp, new GUIContent( "Positions:", sp.tooltip ), GUILayout.Width(94) );
            //EditorGUIUtility.labelWidth = 0;

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            if( handler.HardMatching > 0f )
            {
                EditorGUI.indentLevel++;
                GUILayout.Space( 1 );
                sp.NextVisible( false );
                if( handler.IsInFallingMode ) GUI.backgroundColor = new Color( 0.85f, 1f, 0.85f, 1f );
                EditorGUILayout.PropertyField( sp, new GUIContent( "On Fall Mode:", sp.tooltip ) ); // Hard Matching on fall
                GUI.backgroundColor = Color.white;
                EditorGUI.indentLevel--;
            }
            else sp.NextVisible( false );


            GUILayout.Space( 6 );
            sp.Next( false );
            if( handler.IsInStandingMode == false ) GUI.color = new Color( 1f, 1f, 1f, 0.6f );
            EditorGUILayout.PropertyField(sp, new GUIContent(" Motion Influence:", FGUI_Resources.Tex_Movement, sp.tooltip), GUILayout.ExpandWidth(true)); // Motion Influence
            GUI.color = Color.white;

            EditorGUI.indentLevel--;
            GUILayout.Space( 12 );

            GUI.color = handler.AnimatingMode == RagdollHandler.EAnimatingMode.Standing ? Color.white : new Color( 0.8f, 0.8f, 0.8f, 0.9f );
            sp.Next( false );

            EditorGUIUtility.labelWidth = 150;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField( sp, new GUIContent( " Anchor Bone Attach:", FGUI_Resources.FindIcon( "Ragdoll Animator/GAnchor" ), sp.tooltip ) );
            if( handler.WasInitialized ) if( handler.AnchorBoneSpringMultiplier != 1f ) EditorGUILayout.LabelField( "(" + ( System.Math.Round( handler.AnchorBoneSpringMultiplier * handler.AnchorBoneSpring, 2 ) ) + ")", EditorStyles.centeredGreyMiniLabel, GUILayout.Width( 40 ) );
            EditorGUILayout.EndHorizontal();

            if( handler.AnchorBoneSpring <= 0f )
            {
                EditorGUILayout.HelpBox( "Anchor Bone Attach = 0 is triggering On Fall parameters", UnityEditor.MessageType.None );
            }

            sp.Next( false );
            GUILayout.Space( 2 );
            EditorGUI.indentLevel++;

            EditorGUIUtility.labelWidth = 180;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField( sp, new GUIContent( "Kinematic Anchor on Max:", sp.tooltip ) ); // Set kinematic on max
            EditorGUIUtility.labelWidth = 100;
            sp.Next( false );
            GUILayout.FlexibleSpace();

            EditorGUIUtility.labelWidth = 100;
            if (handler.MakeAnchorKinematicOnMaxSpring == false) GUI.color = new Color(1f, 1f, 1f, 0.6f);
            EditorGUILayout.PropertyField(sp, new GUIContent("Unaffected:", sp.tooltip)); sp.Next(false); // Unaffected
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 120;
            EditorGUILayout.PropertyField( sp, new GUIContent( "Auto Unstuck:", sp.tooltip ) ); sp.Next( false );// Auto Unstuck

            if( handler.MakeAnchorKinematicOnMaxSpring ) GUI.color = new Color( 1f, 1f, 1f, 0.6f );
            EditorGUIUtility.labelWidth = 160;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField( sp, new GUIContent( "Lock Anchor Rotation:", sp.tooltip ) ); // Lock Rotation
            EditorGUIUtility.labelWidth = 100;
            sp.Next( false );
            GUILayout.FlexibleSpace();
            if( handler.LockAnchorRotation ) GUI.color = new Color( 1f, 1f, 1f, 0.6f );
            EditorGUILayout.PropertyField( sp, new GUIContent( "Limit Anchor:", sp.tooltip ) ); // Anchor Limit
            if( handler.LockAnchorRotation ) GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;

            //sp.Next( false );
            //EditorGUILayout.PropertyField( sp, new GUIContent( "Fall On Zero Anchor:", sp.tooltip ) );
            EditorGUI.indentLevel--;

            GUI.color = Color.white;
        }

        private static void Motion_Limbs( SerializedProperty ragdollHandlerProp, RagdollHandler handler )
        {
            var spChains = GetProperty( "chains" );
            bool skipHor = false;
            EditorGUIUtility.fieldWidth = 26;

            GUILayout.Space( 4 );

            for( int i = 0; i < handler.Chains.Count; i++ )
            {
                if( !skipHor ) EditorGUILayout.BeginHorizontal();

                var chain = handler.Chains[i];
                GUIContent title = new GUIContent( " " + chain.ChainName, GetChainIcon( chain.ChainType ) );

                GUI.backgroundColor = new Color( 1f, 1f, 1f, 0.3f );
                if( GUILayout.Button( title, FGUI_Resources.ButtonStyle, GUILayout.MaxWidth( 100 ), GUILayout.Height( 19 ) ) )
                {
                    handler._EditorCategory = RagdollHandler.ERagdollAnimSection.Construct;
                    handler._Editor_ChainCategory = EBoneChainCategory.Physics;
                    handler._Editor_SelectedChain = i;
                }

                GUI.backgroundColor = Color.white;

                EditorGUIUtility.labelWidth = 100;
                var chainProp = spChains.GetArrayElementAtIndex( i ).FindPropertyRelative( "MusclesForce" );

                float max = chainProp.floatValue > 1f ? 2f : 1f;

                if( i == 0 ) EditorGUILayout.Slider( chainProp, 0f, max );
                else EditorGUILayout.Slider( chainProp, 0f, max, GUIContent.none, GUILayout.MinWidth( 50 ) );

                EditorGUIUtility.labelWidth = 0;

                if( skipHor )
                {
                    skipHor = false;
                }
                else
                {
                    if( chain.ChainType.IsArm() || chain.ChainType.IsLeg() ) skipHor = true; else skipHor = false;
                }

                if( !skipHor ) { EditorGUILayout.EndHorizontal(); GUILayout.Space( 5 ); } else { GUILayout.Space( 5 ); }
            }

            EditorGUIUtility.fieldWidth = 0;
        }

        private static void Motion_Extra( SerializedProperty ragdollHandlerProp, RagdollHandler handler )
        {
            bool wideEnough = EditorGUIUtility.currentViewWidth > 400;

            var sp = ragdollHandlerProp.FindPropertyRelative( "Calibrate" );
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 70;
            EditorGUILayout.PropertyField( sp, true ); // Calibrate
            EditorGUIUtility.labelWidth = 110; GUILayout.FlexibleSpace();
            bool preGuiE = GUI.enabled;
            sp.Next( false );
            if( handler.Mecanim )
            {
                GUI.enabled = false;
                EditorGUILayout.Toggle( new GUIContent( "Animate Physics:", sp.tooltip ), handler.Mecanim.updateMode == AnimatorUpdateMode.AnimatePhysics );
                GUI.enabled = preGuiE;
            }
            else EditorGUILayout.PropertyField( sp, true );
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            sp.Next( false ); EditorGUIUtility.labelWidth = 111; EditorGUILayout.PropertyField( sp, true ); GUILayout.FlexibleSpace();
            GUI.enabled = preGuiE;
            sp.Next( false ); EditorGUIUtility.labelWidth = 148; EditorGUILayout.PropertyField( sp, true );
            EditorGUILayout.EndHorizontal();

            if( preGuiE ) GUI.enabled = !handler.WasInitialized;
            if( wideEnough ) EditorGUILayout.BeginHorizontal();

            sp.Next( false );
            if( sp.boolValue ) GUI.backgroundColor = Color.green;
            EditorGUIUtility.labelWidth = 178; EditorGUILayout.PropertyField( sp, true ); GUILayout.FlexibleSpace(); // Hide dummy
            GUI.backgroundColor = Color.white;

            sp.Next( false ); EditorGUIUtility.labelWidth = 148; EditorGUILayout.PropertyField( sp, true );  // Bounded Colliders Ignore
            if( wideEnough ) EditorGUILayout.EndHorizontal();

            if( preGuiE ) GUI.enabled = !handler.WasInitialized;
            wideEnough = EditorGUIUtility.currentViewWidth > 320;
            if( wideEnough ) EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color( 0.75f, 1f, 0.75f, 1f );
            sp.Next( false ); EditorGUIUtility.labelWidth = 144; EditorGUILayout.PropertyField( sp, true ); GUILayout.FlexibleSpace(); // dummy layer
            GUI.backgroundColor = Color.white;
            sp.Next( false ); EditorGUIUtility.labelWidth = 94; EditorGUILayout.PropertyField( sp, true ); // Unscaled time
            if( wideEnough ) EditorGUILayout.EndHorizontal();
            GUI.enabled = preGuiE;

            EditorGUILayout.BeginHorizontal();
            sp.Next( false ); EditorGUIUtility.labelWidth = 124;
            if( sp.floatValue <= 0f )
            {
                bool en = EditorGUILayout.Toggle( new GUIContent( sp.displayName, sp.tooltip ), false );
                if( en ) sp.floatValue = 0.1f;
            }
            else EditorGUILayout.PropertyField( sp, true ); // fade anim

            EditorGUILayout.EndHorizontal();

            if( preGuiE ) GUI.enabled = !handler.WasInitialized;
            EditorGUILayout.BeginHorizontal();
            sp.Next( false ); EditorGUIUtility.labelWidth = 210; EditorGUILayout.PropertyField( sp, true ); // ignore source skeleton colls
            GUILayout.FlexibleSpace();
            sp.Next( false ); EditorGUIUtility.labelWidth = 74; EditorGUILayout.PropertyField( sp, true ); // wait for init
            EditorGUILayout.EndHorizontal();
            GUI.enabled = preGuiE;

            EditorGUIUtility.labelWidth = 170; EditorGUILayout.PropertyField(ragdollHandlerProp.FindPropertyRelative("DisableMecanimOnSleep"), true); // disable mecanim on sleep
            EditorGUIUtility.labelWidth = 0;

            FGUI_Inspector.DrawUILineCommon( 10 );

            sp.Next( false ); //EditorGUIUtility.labelWidth = 180; EditorGUILayout.PropertyField( sp, true ); // matching limits
            EditorGUILayout.BeginHorizontal();
            if( preGuiE ) GUI.enabled = !handler.WasInitialized;
            sp.Next( false ); EditorGUIUtility.labelWidth = 200; EditorGUILayout.PropertyField( sp, true ); // Target parent for dummy
            if( handler.TargetParentForRagdollDummy != handler.GetBaseTransform() ) if( GUILayout.Button( "Self", GUILayout.MaxWidth( 40 ) ) ) { handler.TargetParentForRagdollDummy = handler.GetBaseTransform(); OnChange( ragdollHandlerProp, handler ); }
            GUI.enabled = preGuiE;
            EditorGUILayout.EndHorizontal();
        }

        private static void GUI_DrawDummyLayer()
        {
            EditorGUILayout.PropertyField( GetProperty( "RagdollDummyLayer" ) );
        }
    }
}