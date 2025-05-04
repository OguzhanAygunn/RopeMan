#if UNITY_EDITOR

using UnityEditor;
using UnityEditorInternal;

#endif

using FIMSpace.FGenerating;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_FallingBlendTreePoser : RagdollAnimatorFeatureUpdate
    {
        public override bool UseUpdate => true;
        public Animator Mecanim => ParentRagdollHandler.Mecanim;

        private float fallingModeDuration = 0f;

        private float stuckDetectTimer = 0f;
        private float unstuckPerformTimer = 0f;
        private float unstuckVeloPushTimer = 0f;
        private int unstuckStage = 0;
        private float groundAngle = 0f;

        private Vector3 safeUpRaycastOffset = Vector3.zero;
        private Vector3 lastAppliedImpact = Vector3.zero;
        internal float velocityMagnitude = 0f;
        private RaycastHit lastHit;

        #region Hashes

        private int _hash_FallX = -1;
        private int _hash_FallZ = -1;
        private int _hash_FallG = -1;

        private int _additiveLayer = 0;

        public void PrepareHashesAndLayer()
        {
            _hash_FallX = Animator.StringToHash( InitializedWith.RequestVariable( "Fall X", "Fall X" ).GetString() );
            _hash_FallZ = Animator.StringToHash( InitializedWith.RequestVariable( "Fall Z", "Fall Z" ).GetString() );
            _hash_FallG = Animator.StringToHash( InitializedWith.RequestVariable( "Fall Ground", "Fall Ground" ).GetString() );

            string additiveName = InitializedWith.RequestVariable( "Additive Body Layer:", "" ).GetString();

            if( !string.IsNullOrWhiteSpace( additiveName ) )
            {
                for( int i = 0; i < Mecanim.layerCount; i++ )
                    if( Mecanim.GetLayerName( i ) == additiveName )
                        _additiveLayer = i;
            }
        }

        #endregion Hashes

        #region Animator Properties

        public float FallX
        { get { return Mecanim.GetFloat( _hash_FallX ); } protected set { Mecanim.SetFloat( _hash_FallX, value ); } }
        public float FallZ
        { get { return Mecanim.GetFloat( _hash_FallZ ); } protected set { Mecanim.SetFloat( _hash_FallZ, value ); } }
        public float FallG
        { get { return Mecanim.GetFloat( _hash_FallG ); } protected set { Mecanim.SetFloat( _hash_FallG, value ); } }

        #endregion Animator Properties

        private float smoothDampDuration = 0.1f;
        private float SetFallX
        { set { FallX = Mathf.SmoothDamp( FallX, value, ref sd_FallX, smoothDampDuration ); } }
        private float SetFallZ
        { set { FallZ = Mathf.SmoothDamp( FallZ, value, ref sd_FallZ, smoothDampDuration ); } }
        private float SetFallG
        { set { FallG = Mathf.SmoothDamp( FallG, value, ref sd_FallG, smoothDampDuration * 1.25f ); } }
        private float GetAdditiveLayerWeight
        { get { return Mecanim.GetLayerWeight( _additiveLayer ); } }
        private float SmoothSetAdditiveLayer
        { set { SetAdditiveLayerWeight = Mathf.SmoothDamp( GetAdditiveLayerWeight, value, ref sd_layer, 0.15f ); } }
        private float SetAdditiveLayerWeight
        { set { Mecanim.SetLayerWeight( _additiveLayer, value ); } }

        private float sd_FallX = 0f;
        private float sd_FallZ = 0f;
        private float sd_FallG = 0f;
        private float sd_layer = 0f;

        internal Vector3 localVelocity;
        internal ERagdollGetUpType backLay;
        internal ERagdollGetUpType sideLay;

        private FUniversalVariable groundMaskV;
        private FUniversalVariable transitionSpeedV;
        private FUniversalVariable unstuckSensitivityV;
        private FUniversalVariable additiveLayerMaxVelocityV;
        private FUniversalVariable averageBodyVelocityV;
        private FUniversalVariable nearToGroundHeightV;
        private RagdollBonesChain coreChain;

        public override bool OnInit()
        {
            if( ParentRagdollHandler.Mecanim == null ) return false;

            groundMaskV = Helper.RequestVariable( "Ground Mask:", 0 << 0 );
            transitionSpeedV = Helper.RequestVariable( "Transitioning Duration:", 0.25f );
            unstuckSensitivityV = Helper.RequestVariable( "Unstuck Sensitivity:", 0f );
            additiveLayerMaxVelocityV = Helper.RequestVariable( "Additive Layer Max Velocity:", 5f );
            averageBodyVelocityV = Helper.RequestVariable( "Average Fall Velocity:", 2f );
            nearToGroundHeightV = Helper.RequestVariable( "Near To Ground Height:", 2f );

            coreChain = ParentRagdollHandler.GetChain( ERagdollChainType.Core );
            PrepareHashesAndLayer();

            return base.OnInit();
        }

        public override void Update()
        {
            if( ParentRagdollHandler.IsFallingOrSleep == false || Helper.Enabled == false )
            {
                if( _additiveLayer != 0 )
                {
                    SmoothSetAdditiveLayer = Mathf.Max( GetAdditiveLayerWeight - Time.deltaTime * ( Helper.Enabled ? 18f : 30f ), 0f );
                }

                return;
            }

            var ragdoll = ParentRagdollHandler;
            var anchor = ragdoll.GetAnchorBoneController;

            safeUpRaycastOffset = Vector3.up * coreChain.ChainBonesLength * 0.1f;

            // Checking distance to ground below
            Physics.Raycast( ParentRagdollHandler.User_GetPosition_AnchorCenter() + safeUpRaycastOffset, Vector3.down, out lastHit, 100f, groundMaskV.GetInt(), QueryTriggerInteraction.Ignore );

            Vector3 pelvisVelo = anchor.GameRigidbody.velocity;
            Quaternion refRotation = ragdoll.User_GetRotation_Mapped( Vector3.up );

            // Computing local velocity vector
            Matrix4x4 refMx = Matrix4x4.TRS( anchor.GameRigidbody.position, refRotation, Vector3.one );
            Matrix4x4 refMxInv = refMx.inverse;

            velocityMagnitude = pelvisVelo.magnitude;

            if( fallingModeDuration < 0.3f && lastAppliedImpact != Vector3.zero )
            {
                localVelocity = refMxInv.MultiplyVector( lastAppliedImpact );
                velocityMagnitude = lastAppliedImpact.magnitude;
            }
            else
            {
                localVelocity = refMxInv.MultiplyVector( pelvisVelo );
                lastAppliedImpact = Vector3.zero;
            }

            backLay = ragdoll.User_CanGetUpByRotation( true, Vector3.up, false, 0.35f, !ParentRagdollHandler.IsHumanoid );
            sideLay = ragdoll.User_LayingOnSide( Vector3.up );

            smoothDampDuration = transitionSpeedV.GetFloat();

            bool nearGround = false;
            if( lastHit.transform )
            {
                if( lastHit.distance < nearToGroundHeightV.GetFloat() )
                {
                    nearGround = true;
                    groundAngle = Vector3.Angle( lastHit.normal, Vector3.up );
                }
            }

            fallingModeDuration += Time.deltaTime;

            #region Unstuck

            // Unstuck actions
            if( unstuckSensitivityV.GetFloat() > 0f && unstuckPerformTimer > 0f )
            {
                smoothDampDuration = 0.1f;
                unstuckPerformTimer -= Time.deltaTime * 0.1f;

                if( unstuckPerformTimer > 0.8f )
                {
                    UnstuckHelperPush( 0 );
                    SetFallZ = Mathf.Sin( Time.time * 1.5f ) * 2f;
                    SetFallX = 0f;
                    if( fallingModeDuration > 0.3f ) SetFallG = -1f;
                }
                else if( unstuckPerformTimer > 0.6f )
                {
                    UnstuckHelperPush( 1, 1f + unstuckSensitivityV.GetFloat() );
                    SetFallZ = Mathf.Sin( Time.time * 1.9f ) * 2f;
                    SetFallX = Mathf.Cos( Time.time * 1.9f ) * 2f;
                    SetFallG = 1f;
                }
                else if( unstuckPerformTimer > 0.4f )
                {
                    UnstuckHelperPush( 0, 1.25f + unstuckSensitivityV.GetFloat() );
                    SetFallZ = Mathf.Sin( Time.time * 1.9f ) * 0.5f;
                    SetFallX = Mathf.Cos( Time.time * 1.9f ) * 2f;
                    SetFallG = 1f;
                }
                else if( unstuckPerformTimer > 0.2f )
                {
                    UnstuckHelperPush( 1, 1.75f + unstuckSensitivityV.GetFloat() );
                    SetFallZ = Mathf.Sin( Time.time * 1.9f ) * 2f;
                    SetFallX = Mathf.Cos( Time.time * 1.9f ) * 2f;
                    SetFallG = 0f;
                }
                else
                {
                    UnstuckHelperPush( 0, 2.0f + unstuckSensitivityV.GetFloat() );
                    SetFallZ = Mathf.Sin( Time.time * 1.7f ) * 2f;
                    SetFallX = Mathf.Cos( Time.time * 1.9f ) * 2f;
                    if( fallingModeDuration > 0.3f ) SetFallG = -1f;
                }

                unstuckVeloPushTimer -= Time.deltaTime * ( 1f + +unstuckSensitivityV.GetFloat() * 0.25f );

                if( unstuckVeloPushTimer <= 0f )
                {
                    if( groundAngle < 20f )
                    {
                        DoExtraRaycasts( ref groundAngle );
                    }

                    if( velocityMagnitude > averageBodyVelocityV.GetFloat() * 0.75f || groundAngle < 20f )
                    {
                        unstuckPerformTimer = 0f;
                    }
                }

                return;
            }

            #endregion Unstuck

            bool additiveOverride = false;

            if( velocityMagnitude < averageBodyVelocityV.GetFloat() && nearGround )
            {
                smoothDampDuration = transitionSpeedV.GetFloat() * 1.1f;

                if( groundAngle > 26f )
                {
                    DoExtraRaycasts( ref groundAngle );
                }

                if( groundAngle < 26f && velocityMagnitude < averageBodyVelocityV.GetFloat() * 0.6f ) // Stable slope
                {
                    SetFallG = 0f;

                    if( backLay == ERagdollGetUpType.FromBack )
                    {
                        SetFallX = 0f;
                        SetFallZ = -1f;
                    }
                    else if( backLay == ERagdollGetUpType.FromFacedown )
                    {
                        SetFallX = 0f;
                        SetFallZ = 1f;
                    }
                    else
                    {
                        if( sideLay == ERagdollGetUpType.FromLeftSide )
                        {
                            smoothDampDuration = transitionSpeedV.GetFloat() * 1.15f;
                            SetFallX = -.5f;
                            SetFallZ = 0f;
                            if( velocityMagnitude < averageBodyVelocityV.GetFloat() * 0.2f ) { SetFallX = -1.25f; SmoothSetAdditiveLayer = 0.5f; additiveOverride = true; }
                        }
                        else if( sideLay == ERagdollGetUpType.FromRightSide )
                        {
                            smoothDampDuration = transitionSpeedV.GetFloat() * 1.15f;
                            SetFallX = .5f;
                            SetFallZ = 0f;
                            if( velocityMagnitude < averageBodyVelocityV.GetFloat() * 0.2f ) { SetFallX = 1.25f; SmoothSetAdditiveLayer = 0.5f; additiveOverride = true; }
                        }
                        else
                        {
                            smoothDampDuration = transitionSpeedV.GetFloat() * 1.3f;
                            SetFallX = 0f;
                            SetFallZ = 0f;
                        }
                    }
                }
                else // Rolling from large angle slope
                {
                    float tgtX = 0f;

                    if( fallingModeDuration > 0.3f ) SetFallG = -1f;

                    if( backLay == ERagdollGetUpType.FromBack )
                    {
                        SetFallZ = -1f;
                    }
                    else if( backLay == ERagdollGetUpType.FromFacedown )
                    {
                        SetFallZ = 1f;
                    }

                    if( sideLay == ERagdollGetUpType.FromLeftSide || sideLay == ERagdollGetUpType.FromRightSide )
                    {
                        SmoothSetAdditiveLayer = 0.4f;
                        additiveOverride = true;

                        if( velocityMagnitude < averageBodyVelocityV.GetFloat() * 0.5f )
                        {
                            if( sideLay == ERagdollGetUpType.FromLeftSide ) tgtX = 1f;
                            else if( sideLay == ERagdollGetUpType.FromRightSide ) tgtX = -1f;
                        }
                    }
                    else
                    {
                        SmoothSetAdditiveLayer = 0.25f; additiveOverride = true;
                    }

                    SetFallX = tgtX;
                }

                if( unstuckSensitivityV.GetFloat() > 0f )
                {
                    if( velocityMagnitude < averageBodyVelocityV.GetFloat() * 0.1f )
                    {
                        bool sideLayReq = backLay == ERagdollGetUpType.None;
                        if( sideLayReq ) sideLayReq = sideLay != ERagdollGetUpType.None;

                        if( groundAngle > 15f || sideLayReq ) stuckDetectTimer += Time.deltaTime;
                        if( stuckDetectTimer > .8f ) { unstuckPerformTimer = 1f; unstuckStage = 0; }
                    }
                    else
                    {
                        if( stuckDetectTimer < 0f ) stuckDetectTimer = 0f; else stuckDetectTimer -= Time.deltaTime * 2f;
                    }
                }
            }
            else // Far from ground or high velocity fall pose
            {
                Vector3 locAbs = new Vector3( Mathf.Abs( localVelocity.x ), Mathf.Abs( localVelocity.y ), Mathf.Abs( localVelocity.z ) );
                smoothDampDuration = transitionSpeedV.GetFloat() * 1.35f;

                SetFallG = 1f;

                if( locAbs.y * 0.5f > locAbs.x && locAbs.y * 0.5f > locAbs.z ) // Y Dominant
                {
                    smoothDampDuration = transitionSpeedV.GetFloat() * 1.5f;
                    if( localVelocity.y > 0f )
                    {
                        SetFallZ = 1f;
                        SetFallX = 0f;
                    }
                    else
                    {
                        SetFallZ = -1f;
                        SetFallX = 0f;
                    }
                }
                else // X or Z Dominant
                {
                    if( fallingModeDuration < 0.4f ) if( velocityMagnitude > averageBodyVelocityV.GetFloat() * 1.15f ) smoothDampDuration = transitionSpeedV.GetFloat() * 0.2f;

                    float maxSpd; // Blend Power
                    if( velocityMagnitude > averageBodyVelocityV.GetFloat() ) maxSpd = VelocityLimiter( velocityMagnitude ); else maxSpd = 1.2f;

                    if( locAbs.x > locAbs.z ) // X Dominant
                    {
                        if( localVelocity.x < 0f ) SetFallX = -maxSpd; else SetFallX = maxSpd;
                        SetFallZ = Mathf.Clamp( localVelocity.z * 0.5f, -1f, 1f );
                    }
                    else // Z dominant
                    {
                        if( localVelocity.z < 0f ) SetFallZ = -maxSpd; else SetFallZ = maxSpd;
                        SetFallX = Mathf.Clamp( localVelocity.x * 0.5f, -1f, 1f );
                    }
                }
            }

            // Additive layer blending if whole body velocity is big enough
            if( additiveOverride == false )
                if( _additiveLayer > 0 )
                {
                    float additiveLayerWeight = Mathf.InverseLerp( 0f, additiveLayerMaxVelocityV.GetFloat(), velocityMagnitude );

                    if( velocityMagnitude > averageBodyVelocityV.GetFloat() )
                    {
                        SmoothSetAdditiveLayer = additiveLayerWeight;
                    }
                    else
                    {
                        if( lastHit.distance > 2.5f ) // When above ground then don't slow down with additive
                            SmoothSetAdditiveLayer = 0.25f + additiveLayerWeight * 0.5f;
                        else
                            SmoothSetAdditiveLayer = additiveLayerWeight;
                    }
                }
        }

        /// <summary> Doing raycasts in feet and head position to get more detailed ground info </summary>
        private void DoExtraRaycasts( ref float groundAngle )
        {
            // Additional raycasts for average angle
            RaycastHit extraHit;
            Physics.Raycast( ParentRagdollHandler.GetChain( ERagdollChainType.Core ).GetBone( 1000 ).SourceBone.position + safeUpRaycastOffset, Vector3.down, out extraHit, 2f, groundMaskV.GetInt(), QueryTriggerInteraction.Ignore );
            if( extraHit.transform ) groundAngle = Mathf.LerpUnclamped( groundAngle, Vector3.Angle( extraHit.normal, Vector3.up ), 0.35f );

            Physics.Raycast( ParentRagdollHandler.User_GetPosition_FeetMiddle() + safeUpRaycastOffset, Vector3.down, out extraHit, 2f, groundMaskV.GetInt(), QueryTriggerInteraction.Ignore );
            if( extraHit.transform ) groundAngle = Mathf.LerpUnclamped( groundAngle, Vector3.Angle( extraHit.normal, Vector3.up ), 0.35f );
        }

        /// <summary> Limiting maximum property value with certain velocity power </summary>
        private float VelocityLimiter( float magnitude )
        {
            return Mathf.Lerp( 1.3f, 2f, Mathf.InverseLerp( 2.2f, 7f, magnitude ) );
        }

        /// <summary> In case of long unstuck, pushing body to help get it out of problematic placement </summary>
        private void UnstuckHelperPush( int stage, float powerMul = 1f )
        {
            if( unstuckStage == stage )
            {
                if( stage == 0 ) unstuckStage = 1; else unstuckStage = 0;
                unstuckVeloPushTimer = 1f;
                ParentRagdollHandler.User_AddAllBonesImpact( safeUpRaycastOffset * 2.5f * powerMul, 0f, ForceMode.VelocityChange );
                ParentRagdollHandler.User_AddAllBonesImpact( safeUpRaycastOffset * 7f * powerMul, .125f + 0.03f * powerMul );
            }
        }

        #region Editor GUI

#if UNITY_EDITOR

        public override bool Editor_DisplayEnableSwitch => true;
        public override string Editor_FeatureDescription => "Animating three animator properties, which should control blend tree, resnponsible for falling poses.";

        public override void Editor_InspectorGUI( SerializedProperty toDirty, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {

            GUILayout.Space( 4 );
            var groundMask = helper.RequestVariable( "Ground Mask:", 0 << 0 );
            int layer = groundMask.GetInt();
            layer = EditorGUILayout.MaskField(new GUIContent("Ground Mask:", "Ground below check layer mask"), InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layer), InternalEditorUtility.layers);
            layer = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(layer);
            groundMask.SetValue( layer );

            EditorGUIUtility.labelWidth = 0;
            var transitionSpeed = helper.RequestVariable( "Transitioning Duration:", 0.25f );
            transitionSpeed.AssignTooltip( "Transition duration for poses animator properties" );
            transitionSpeed.SetMinMaxSlider( 0f, 0.5f );
            transitionSpeed.Editor_DisplayVariableGUI();

            var unstuckSensitivityV = helper.RequestVariable( "Unstuck Sensitivity:", 0f );
            unstuckSensitivityV.AssignTooltip( "How sensitive should be unstuck calcuations, which are trying to move character out of problematic placement on the scene" );
            unstuckSensitivityV.SetMinMaxSlider( 0f, 1f );
            unstuckSensitivityV.Editor_DisplayVariableGUI();

            var averageBodyVeloV = helper.RequestVariable( "Average Fall Velocity:", 2f );
            averageBodyVeloV.AssignTooltip( "Average character body velocity to controll sensitivity of poses transitions" );
            averageBodyVeloV.Editor_DisplayVariableGUI();

            var nearTogRoundHeightV = helper.RequestVariable( "Near To Ground Height:", 2f );
            averageBodyVeloV.AssignTooltip( "Height at which character should be trated as near to ground, to perform different falling poses" );
            nearTogRoundHeightV.Editor_DisplayVariableGUI();

            GUILayout.Space( 8 );
            EditorGUILayout.HelpBox( "Fall X is controlling roll left - right poses with value -1 to 1", MessageType.None );

            GUI.enabled = !ragdollHandler.WasInitialized;
            EditorGUIUtility.labelWidth = 48;
            EditorGUILayout.BeginHorizontal();
            var fallXV = helper.RequestVariable( "Fall X", "Fall X" );
            fallXV.Editor_DisplayVariableGUI();
            RAF_FallGetUpAnimate.AnimatorPropertySelector( ragdollHandler, fallXV, toDirty );
            EditorGUILayout.EndHorizontal();

            GUILayout.Space( 4 );
            EditorGUILayout.HelpBox( "Fall Z is controlling lean forward - back poses with value -1 to 1", MessageType.None );
            EditorGUILayout.BeginHorizontal();
            var fallZV = helper.RequestVariable( "Fall Z", "Fall Z" );
            fallZV.Editor_DisplayVariableGUI();
            RAF_FallGetUpAnimate.AnimatorPropertySelector( ragdollHandler, fallZV, toDirty );
            EditorGUILayout.EndHorizontal();

            GUILayout.Space( 4 );
            EditorGUIUtility.labelWidth = 84;
            EditorGUILayout.HelpBox( "Fall Ground is blending between falling in air pose and fall on ground pose", MessageType.None );
            EditorGUILayout.BeginHorizontal();
            var fallGV = helper.RequestVariable( "Fall Ground", "Fall Ground" );
            fallGV.Editor_DisplayVariableGUI();
            RAF_FallGetUpAnimate.AnimatorPropertySelector( ragdollHandler, fallGV, toDirty );
            EditorGUILayout.EndHorizontal();

            GUILayout.Space( 6 );
            GUI.enabled = !ragdollHandler.WasInitialized;
            EditorGUIUtility.labelWidth = 144;

            var additiveBodyLayerNameV = helper.RequestVariable( "Additive Body Layer:", "" );
            EditorGUILayout.BeginHorizontal();
            additiveBodyLayerNameV.Editor_DisplayVariableGUI();
            EditorGUIUtility.labelWidth = 0;
            RAF_FallGetUpAnimate.AnimatorLayerSelector( ragdollHandler, additiveBodyLayerNameV, toDirty );
            if( string.IsNullOrWhiteSpace( additiveBodyLayerNameV.GetString() ) ) EditorGUILayout.HelpBox( "Not Used", MessageType.None );
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;
            if( !string.IsNullOrWhiteSpace( additiveBodyLayerNameV.GetString() ) )
            {
                var additiveLayerMaxVelocity = helper.RequestVariable( "Additive Layer Max Velocity:", 5f );
                additiveLayerMaxVelocity.AssignTooltip( "Body velocity at which Additive Body Layer will triiger weight = 1" );
                additiveLayerMaxVelocity.Editor_DisplayVariableGUI();

                EditorGUILayout.HelpBox( "Additive Body Layer is adding Fall animation to the static fall poses", MessageType.None );
            }

            GUILayout.Space( 8 );

            if( ragdollHandler.WasInitialized )
            {
                EditorGUILayout.EnumPopup( "Back Lay:", backLay );
                EditorGUILayout.EnumPopup( "Side Lay:", sideLay );
                EditorGUILayout.LabelField( "Body Velocity: " + velocityMagnitude );

                if( unstuckSensitivityV.GetFloat() > 0f )
                    EditorGUILayout.LabelField( "Unstuck Stage:" + unstuckStage );

                EditorGUILayout.LabelField( "Ground Angle: " + groundAngle );
                EditorGUILayout.LabelField( "Ground Distance: " + lastHit.distance );
            }
            else
            {
                EditorGUILayout.HelpBox( "During playmode displaying there debug values", UnityEditor.MessageType.None );
            }
        }

#endif

        #endregion Editor GUI
    }
}