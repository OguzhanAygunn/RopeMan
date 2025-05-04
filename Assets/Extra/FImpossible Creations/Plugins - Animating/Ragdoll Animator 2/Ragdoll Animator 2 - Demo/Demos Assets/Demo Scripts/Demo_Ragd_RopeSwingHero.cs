using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_RopeSwingHero : FimpossibleComponent
    {
        public RagdollAnimator2 RagdollAnimator;
        public FBasic_RigidbodyMover Mover;
        public Animator Mecanim;
        public RA2MagnetPoint RopeAttacher;
        public LineRenderer LineRend;
        public float LeftHandToRightHandPushPower = 0f;

        [Space( 5 )]
        public LayerMask CatchLayer;

        [Space( 5 )]
        public float WavingImputPower = 1f;

        private bool shooting = false;
        private bool attached = false;
        private RagdollChainBone lHand;
        private RagdollChainBone rHand;
        private RagdollAnimatorFeatureHelper getup;
        private RagdollAnimatorFeatureHelper fallPose;

        private void Start()
        {
            Mover.OnJump = OnJump;

            lHand = RagdollAnimator.User_GetBoneSetupByHumanoidBone( HumanBodyBones.LeftHand );
            rHand = RagdollAnimator.User_GetBoneSetupByHumanoidBone( HumanBodyBones.RightHand );

            getup = RagdollAnimator.Handler.GetExtraFeatureHelper<RAF_AutoGetUp>();
            fallPose = RagdollAnimator.Handler.GetExtraFeatureHelper<RAF_FallingBlendTreePoser>();
            RagdollAnimator.Handler.GetExtraFeature<RAF_FallGetUpAnimate>().ClipTimePlayOffset = 0.125f;
        }

        private void Update()
        {
            if( Input.GetMouseButtonDown( 0 ) )
            {
                shooting = true;
                Mecanim.SetBool( "Fall", false );
                RagdollAnimator.Handler.Mecanim.CrossFadeInFixedTime( "Shot", 0.2f );
            }

            getup.Enabled = !attached;
            fallPose.Enabled = !attached;

            // Reposition mover
            if( attached )
            {
                var anchor = RagdollAnimator.Handler.GetAnchorBoneController;
                Mover.Rigb.velocity = anchor.GameRigidbody.velocity;
                Mover.Rigb.position = RagdollAnimator.User_GetPosition_FeetMiddle();
                Mover.UpdateInput = false;

                if( Input.GetKey( KeyCode.W ) || Input.GetKey( KeyCode.UpArrow ) ) WaveImpact( GetCameraDirection( Vector3.forward ) );
                if( Input.GetKey( KeyCode.S ) || Input.GetKey( KeyCode.DownArrow ) ) WaveImpact( -GetCameraDirection( Vector3.forward ) );
                if( Input.GetKey( KeyCode.D ) || Input.GetKey( KeyCode.RightArrow ) ) WaveImpact( GetCameraDirection( Vector3.right ) );
                if( Input.GetKey( KeyCode.A ) || Input.GetKey( KeyCode.LeftArrow ) ) WaveImpact( -GetCameraDirection( Vector3.right ) );
                if( Input.GetKeyDown( KeyCode.Space ) )
                {
                    Detach();
                }
            }
            else
            {
                Mover.UpdateInput = true;

                if( !Mover.isGrounded )
                {
                    if( RagdollAnimator.IsInFallingOrSleepMode )
                    {
                        var anchor = RagdollAnimator.Handler.GetAnchorBoneController;
                        Mover.Rigb.velocity = anchor.GameRigidbody.velocity;
                        Mover.Rigb.position = RagdollAnimator.User_GetPosition_FeetMiddle();
                    }
                }
            }
        }

        // Animation Clip Event
        private void Shot()
        {
            Ray r = Camera.main.ScreenPointToRay( Input.mousePosition );
            RaycastHit hit;

            if( Physics.Raycast( r, out hit, Mathf.Infinity, CatchLayer ) )
            {
                RopeAttacher.transform.position = hit.point;
                RopeAttacher.enabled = true;
                RopeAttacher.transform.rotation = Quaternion.LookRotation( Vector3.ProjectOnPlane( Camera.main.transform.forward, Vector3.up ) );
                LineRend.enabled = true;
                attached = true;
                shooting = false;

                RagdollAnimator.User_SwitchFallState();
                Mover.enabled = false;
                Mecanim.SetBool( "Grounded", false );
            }
        }

        private Vector3 GetCameraDirection( Vector3 direction )
        {
            return Vector3.ProjectOnPlane( Camera.main.transform.rotation * direction, Vector3.up );
        }

        private void FixedUpdate()
        {
            if( scheduledWaveImpact != Vector3.zero )
            {
                Vector3 dir = scheduledWaveImpact;
                dir *= WavingImputPower;
                var chain = RagdollAnimator.Handler.GetChain( ERagdollChainType.LeftLeg );
                RagdollAnimator.User_AddChainImpact( chain, dir, 0f );

                chain = RagdollAnimator.Handler.GetChain( ERagdollChainType.RightLeg );
                RagdollAnimator.User_AddChainImpact( chain, dir, 0f );

                chain = RagdollAnimator.Handler.GetChain( ERagdollChainType.Core );

                for( int i = 0; i < 2; i++ )
                {
                    var bone = chain.BoneSetups[i];
                    RagdollAnimator.User_AddBoneImpact( bone, dir, 0f );
                }

                scheduledWaveImpact = Vector3.zero;
            }

            if( attached )
            {
                if( LeftHandToRightHandPushPower > 0f )
                {
                    // Dragging left hand towards right, to force it be similar like in source animation
                    RagdollHandlerUtilities.DragRigidbodyTowards( lHand.GameRigidbody, rHand.GameRigidbody.position, LeftHandToRightHandPushPower * 1.25f );
                }

                RagdollAnimator.User_TransitionMusclesPowerMultiplier( 0.7f, Time.fixedDeltaTime * 2f );
            }
            else if( Mecanim.GetBool( "Grounded" ) == false )
            {
                if( RagdollAnimator.IsInFallingOrSleepMode )
                {
                    RagdollAnimator.User_TransitionMusclesPowerMultiplier( 0.9f, Time.fixedDeltaTime * 2f );

                    float velo = RagdollAnimator.User_GetChainBonesVelocity( ERagdollChainType.Core ).magnitude;

                    if( shooting == false )
                    {
                        if( velo > 3f )
                            Mecanim.SetBool( "Fall", true );
                        else
                            Mecanim.SetBool( "Fall", false );
                    }
                }
            }
            else
            {
                if( RagdollAnimator.IsInFallingOrSleepMode )
                {
                    RagdollAnimator.User_TransitionMusclesPowerMultiplier( 1f, Time.fixedDeltaTime * 2f );
                    Mecanim.SetBool( "Fall", false );
                }
            }
        }

        private Vector3 scheduledWaveImpact = Vector3.zero;

        private void WaveImpact( Vector3 dir )
        {
            scheduledWaveImpact = dir;
        }

        private void Detach()
        {
            RopeAttacher.enabled = false;
            LineRend.enabled = false;
            attached = false;
        }

        private void OnJump()
        {
            if( attached )
            {
                Detach();
            }
            else
            {
                RagdollAnimator.Mecanim.CrossFadeInFixedTime( "Jump", 0.125f );
            }
        }

        #region Editor Class

#if UNITY_EDITOR

        [UnityEditor.CanEditMultipleObjects]
        [UnityEditor.CustomEditor( typeof( Demo_Ragd_RopeSwingHero ) )]
        public class Demo_Ragd_RopeSwingHeroEditor : UnityEditor.Editor
        {
            public Demo_Ragd_RopeSwingHero Get
            { get { if( _get == null ) _get = (Demo_Ragd_RopeSwingHero)target; return _get; } }
            private Demo_Ragd_RopeSwingHero _get;

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                GUILayout.Space( 4f );
                DrawPropertiesExcluding( serializedObject, "m_Script" );

                serializedObject.ApplyModifiedProperties();
            }
        }

#endif

        #endregion Editor Class
    }
}