using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_ClimberHero : FimpossibleComponent
    {
        public RagdollAnimator2 RagdollAnimator;
        public FBasic_RigidbodyMover Mover;
        public Animator Mecanim;

        [Space( 5 )]
        public LayerMask CatchLayer;

        public float CatchRadius = 0.1f;

        [Space( 5 )]
        public float WavingImputPower = 1f;

        [Space( 5 )]
        [Tooltip( "Divider for the animator velocity property (for animation clip blend tree)" )]
        public float VelocityParamDiv = 2f;

        public bool PreventStretch = true;
        public bool StopOnGrounded = true;

        private float _sd = 0f;
        private int _hash;
        private RagdollAnimatorFeatureHelper blendOnCollision;
        private bool wasGrounded = true;
        private float catchCulldown = 0f;

        private class HandControll
        {
            public RagdollChainBone ragdollBone;
            public bool isAttached = false;
            public Collider attachedTo = null;
            public Vector3 attachLocalPos = Vector3.zero;
            public Quaternion attachLocalRot = Quaternion.identity;

            //public List<Collider> IgnoredCollisionsWith = new List<Collider>();

            public void Detach()
            {
                ragdollBone.GameRigidbody.isKinematic = false;
                isAttached = false;
                attachedTo = null;
                ragdollBone.MainBoneCollider.enabled = true;

                //foreach( var coll in IgnoredCollisionsWith )
                //{
                //    ragdollBone.IgnoreCollisionsWith( coll, false );
                //}

                //IgnoredCollisionsWith.Clear();
            }
        }

        private HandControll leftHand;
        private HandControll rightHand;

        private HandControll GetHand( int i )
        { if( i <= 0 ) return leftHand; else return rightHand; }

        private void Start()
        {
            _hash = Animator.StringToHash( "HandsLevel" );
            blendOnCollision = RagdollAnimator.Handler.GetExtraFeatureHelper<RAF_BlendOnCollisions>();

            leftHand = new HandControll() { ragdollBone = RagdollAnimator.Handler.GetChain( ERagdollChainType.LeftArm ).LastBone };
            rightHand = new HandControll() { ragdollBone = RagdollAnimator.Handler.GetChain( ERagdollChainType.RightArm ).LastBone };

            Mover.OnJump = OnJump;
        }

        private bool AnyCatched()
        {
            bool anyCatched = false;
            for( int i = 0; i < 2; i++ ) { if( GetHand( i ).isAttached ) { anyCatched = true; break; } }
            return anyCatched;
        }

        private void Update()
        {
            float velocity = Mover.Rigb.velocity.y / VelocityParamDiv;
            velocity = Mathf.Clamp( velocity, -1f, 1f );

            bool anyCatched = AnyCatched();
            float level = Mecanim.GetFloat( _hash );
            level = Mathf.SmoothDamp( level, AnyCatched() ? 2f : velocity, ref _sd, 0.15f, 1000000f, Time.deltaTime );
            Mecanim.SetFloat( _hash, level );

            if( anyCatched )
            {
                RagdollAnimator.Handler.AnimatingMode = RagdollHandler.EAnimatingMode.Falling;
                Mover.UpdateInput = false;

                if( Input.GetKey( KeyCode.W ) || Input.GetKey( KeyCode.UpArrow ) ) WaveImpact( GetCameraDirection( Vector3.forward ) );
                if( Input.GetKey( KeyCode.S ) || Input.GetKey( KeyCode.DownArrow ) ) WaveImpact( -GetCameraDirection( Vector3.forward ) );
                if( Input.GetKey( KeyCode.D ) || Input.GetKey( KeyCode.RightArrow ) ) WaveImpact( GetCameraDirection( Vector3.right ) );
                if( Input.GetKey( KeyCode.A ) || Input.GetKey( KeyCode.LeftArrow ) ) WaveImpact( -GetCameraDirection( Vector3.right ) );

                if( Input.GetKeyDown( KeyCode.Space ) ) Mover.jumpRequest = Mover.JumpPower;
            }
            else
            {
                Mover.UpdateInput = true;
                RagdollAnimator.User_TransitionToStandingMode( 0.4f, 0f );
            }

            if( wasGrounded != Mover.isGrounded )
            {
                if( blendOnCollision != null ) blendOnCollision.Enabled = Mover.isGrounded; // Ragdoll blend 100% when jumping

                wasGrounded = Mover.isGrounded;

                if( Mover.isGrounded )
                {
                    wasJump = false;
                }
                else
                {
                    if( !wasJump ) Mecanim.CrossFadeInFixedTime( "Jump Motion", 0.1f );
                }

                if( StopOnGrounded )
                {
                    for( int i = 0; i < 2; i++ ) GetHand( i ).isAttached = false;

                    if( Mover.isGrounded && !anyCatched )
                        for( int i = 0; i < 2; i++ )
                        {
                            var hand = GetHand( i );
                            hand.Detach();
                        }
                }
            }
        }

        private Vector3 scheduledWaveImpact = Vector3.zero;
        private Vector3 GetCameraDirection( Vector3 direction )
        {
            return Vector3.ProjectOnPlane( Camera.main.transform.rotation * direction, Vector3.up );
        }

        private void WaveImpact( Vector3 dir )
        {
            scheduledWaveImpact = dir;
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

            catchCulldown -= Time.fixedDeltaTime;

            if( Mover.isGrounded == false )
            {
                if( catchCulldown < 0f ) CheckHands();

                if( AnyCatched() )
                {
                    var anchor = RagdollAnimator.Handler.GetAnchorBoneController;
                    Mover.Rigb.velocity = anchor.GameRigidbody.velocity;
                    Mover.Rigb.position = RagdollAnimator.User_GetPosition_FeetMiddle();
                }
            }

            if( PreventStretch )
            {
                // Check if hands are stretched away from each other
                float dist = Vector3.Distance( GetHand( 0 ).ragdollBone.BoneProcessor.lastAppliedPosition, GetHand( 1 ).ragdollBone.BoneProcessor.lastAppliedPosition );
                float len = GetHand( 0 ).ragdollBone.ParentChain.ChainBonesLength + GetHand( 1 ).ragdollBone.ParentChain.ChainBonesLength;

                if( dist > len * 1.7f )
                {
                    GetHand( 0 ).Detach();
                    GetHand( 1 ).Detach();
                }
            }
        }

        private bool wasJump = false;

        private void OnJump()
        {
            wasJump = true;
            var anchor = RagdollAnimator.Handler.GetAnchorBoneController;
            anchor.GameRigidbody.velocity = Mover.Rigb.velocity;
            Mecanim.CrossFadeInFixedTime( "Jump", 0.07f );
            catchCulldown = 0.6f;

            for( int i = 0; i < 2; i++ )
            {
                var hand = GetHand( i );
                hand.Detach();
            }
        }

        private void LateUpdate()
        {
            for( int i = 0; i < 2; i++ )
            {
                var hand = GetHand( i );

                if( hand.isAttached )
                {
                    hand.ragdollBone.GameRigidbody.position = hand.attachedTo.transform.TransformPoint( hand.attachLocalPos );
                    hand.ragdollBone.GameRigidbody.rotation = hand.attachedTo.transform.rotation * hand.attachLocalRot;
                    continue;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if( leftHand != null )
            {
                Gizmos.color = Color.green * 0.7f;

                for( int i = 0; i < 2; i++ )
                {
                    var hand = GetHand( i );
                    Gizmos.DrawWireSphere( hand.ragdollBone.GameRigidbody.position, CatchRadius );
                }
            }
        }

        private int overlaps = 0;
        private Collider[] overlapColliders = new Collider[8];

        private void CheckHands()
        {
            for( int i = 0; i < 2; i++ )
            {
                var hand = GetHand( i );

                if( hand.isAttached )
                {
                    hand.ragdollBone.GameRigidbody.position = hand.attachedTo.transform.TransformPoint( hand.attachLocalPos );
                    hand.ragdollBone.GameRigidbody.rotation = hand.attachedTo.transform.rotation * hand.attachLocalRot;
                    continue;
                }

                hand.ragdollBone.GameRigidbody.isKinematic = false;

                overlaps = Physics.OverlapSphereNonAlloc( hand.ragdollBone.GameRigidbody.position, CatchRadius, overlapColliders, CatchLayer );

                for( int o = 0; o < overlaps; o++ )
                {
                    var overlap = overlapColliders[o];

                    hand.ragdollBone.MainBoneCollider.enabled = false;

                    if( overlap == hand.attachedTo ) continue;

                    Vector3 catchPos = overlap.ClosestPoint( hand.ragdollBone.GameRigidbody.position );

                    hand.isAttached = true;
                    hand.attachedTo = overlap;
                    hand.ragdollBone.GameRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    hand.ragdollBone.GameRigidbody.isKinematic = true;
                    hand.ragdollBone.GameRigidbody.position = catchPos;
                    hand.attachLocalRot = FEngineering.QToLocal( overlap.transform.rotation, hand.ragdollBone.GameRigidbody.transform.rotation );
                    hand.attachLocalPos = overlap.transform.InverseTransformPoint( catchPos );

                    break;
                }
            }
        }

        #region Editor Class

#if UNITY_EDITOR

        [UnityEditor.CanEditMultipleObjects]
        [UnityEditor.CustomEditor( typeof( Demo_Ragd_ClimberHero ) )]
        public class Demo_Ragd_ClimberHeroEditor : UnityEditor.Editor
        {
            public Demo_Ragd_ClimberHero Get
            { get { if( _get == null ) _get = (Demo_Ragd_ClimberHero)target; return _get; } }
            private Demo_Ragd_ClimberHero _get;

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                GUILayout.Space( 4f );
                DrawPropertiesExcluding( serializedObject, "m_Script" );

                serializedObject.ApplyModifiedProperties();

                if( Get.leftHand != null )
                {
                    for( int i = 0; i < 2; i++ )
                    {
                        var hand = Get.GetHand( i );
                        UnityEditor.EditorGUILayout.ObjectField( hand.attachedTo, typeof( Collider ), true );
                    }
                }
            }
        }

#endif

        #endregion Editor Class
    }
}