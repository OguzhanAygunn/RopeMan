using FIMSpace.Basics;
using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    /// <summary>
    /// Warning, this code is like a draft version
    /// </summary>
    public class Demo_Ragd_Fred2AI : FimpossibleComponent
    {
        [Header( "It's just example script - not dedicated for real gameplay" )]
        public RagdollAnimator2 Ragdoll;

        public Animator TargetAnimator => Ragdoll.Handler.Mecanim;
        public LayerMask GroundMask;

        private Rigidbody rig;
        private FAnimationClips anim;

        [Space( 5 )]
        public Transform Enemy;

        private bool initialized = false;

        private void Start()
        {
            PrepareHashes();

            // Helper animator class
            anim = new FAnimationClips( TargetAnimator );
            anim.AddClip( "Idle" );
            anim.AddClip( "Jump Attack" );
            anim.AddClip( "Roll" );
            anim.AddClip( "Walk" );
            anim.AddClip( "Fall" );
            anim.AddClip( "Try Get Up" );
            anim.AddClip( "Get Up" );

            rig = GetComponent<Rigidbody>();

            initialized = true;
        }

        public enum EAIStage
        {
            FindPosition,
            PositionToAttack,
            StartAttacking,
            DuringAttack,
            OnTheGround,
            GetUp,
            None
        }

        private EAIStage aiStage = EAIStage.FindPosition;
        private Vector3 attackPosition = Vector3.zero;
        private bool canWalkAnimation = true;
        private float getUpDur = 0f;

        [Header( "Debug AI Settings" )]
        public Vector2 ToAttackDistance = new Vector2( 4f, 5f );

        public float MovSpeed = 1f;
        public float RotSpeed = 2f;

        public Vector3 JumpImpact = new Vector3( 0f, 1f, 4f );

        public Vector3 JumpHelpTorque = Vector3.zero;
        public float JumpBoost = 1f;
        public Vector3 GetUpHelperTorque = Vector3.zero;
        private float nearPointToleranceLowerer = 1f;
        private float stuckTimer = 0f;

        private Vector3 ZeroY( Vector3 v )
        {
            v.y = 0f;
            return v;
        }

        private void FixedUpdate()
        {
            if( !initialized ) return;

            wasMoving = false;
            forceWalkAnim = false;
            disableRigMove = false;
            bool additiveFadeIn = false;
            MovementDir = transform.forward;

            if( aiStage == EAIStage.FindPosition )
            {
                canWalkAnimation = true;
                aiStage = EAIStage.PositionToAttack;
                attackPosition = Enemy.position;

                tryGetSign = -1f;
                Vector3 enemyToMe = ( transform.position - Enemy.position );
                attackPosition += enemyToMe.normalized * Random.Range( ToAttackDistance.x, ToAttackDistance.y );
                nearPointToleranceLowerer = 1f;
                stuckTimer = 0f;
            }
            else if( aiStage == EAIStage.PositionToAttack )
            {
                canWalkAnimation = true;

                float distance = FVectorMethods.DistanceTopDown( transform.position, attackPosition );

                if( distance < 0.2f * nearPointToleranceLowerer )
                {
                    //lastMaxRootVelo = Vector3.zero;
                    aiStage = EAIStage.StartAttacking;
                }
                else if( distance < 1.25f )
                {
                    nearPointToleranceLowerer += Time.fixedDeltaTime * 0.1f;
                    if( accel > 0.3f ) accel -= Time.fixedDeltaTime * 2f;
                    MovementDir = Vector3.Slerp( MovementDir, ( ZeroY( attackPosition ) - ZeroY( transform.position ) ).normalized, distance * 0.7f );
                }

                GoForward();
                RotateTowards( attackPosition );
            }
            else if( aiStage == EAIStage.StartAttacking )
            {
                hittedEnemy = false;
                Vector3 meToEnemy = ( Enemy.position - transform.position ).normalized;

                float angle = Vector3.Angle( transform.forward, meToEnemy );

                if( Mathf.Abs( angle ) < 7f )
                {
                    attackExecution = true;
                    disableRigMove = true;
                    anim.CrossFadeInFixedTime( "Jump Attack", 0.25f, 0f );
                    canWalkAnimation = false;
                    attackDur = 0f;
                }

                if( attackExecution == false )
                {
                    canWalkAnimation = true;
                    RotateTowards( Enemy.position );
                    forceWalkAnim = true;
                }
            }
            else if( aiStage == EAIStage.DuringAttack )
            {
                attackExecution = false;
                disableRigMove = true;
                canWalkAnimation = false;

                if( hittedEnemy )
                {
                    hittedEnemy = false;
                    anim.CrossFadeInFixedTime( "Fall" );
                }

                attackDur += Time.fixedDeltaTime;

                if( attackDur > 0.6f )
                {
                    if( Ragdoll.User_GetChainBonesVelocity( ERagdollChainType.Core ).magnitude < 1f )
                    {
                        stuckTimer += Time.fixedDeltaTime;
                        if( stuckTimer > 4f )
                        {
                            stuckTimer = 0f;
                            aiStage = EAIStage.OnTheGround;
                        }
                    }

                    RaycastHit checkGround = Ragdoll.User_ProbeGroundBelowAnchorBone( GroundMask, 0.45f );

                    if( checkGround.transform )
                    {
                        aiStage = EAIStage.OnTheGround;
                        anim.CrossFadeInFixedTime( "Fall" );
                    }
                }
            }
            else if( aiStage == EAIStage.OnTheGround )
            {
                canWalkAnimation = false;
                //anim.CrossFadeInFixedTime("Fall", 0.5f);
                var canGetup = Ragdoll.User_CanGetUpByRotation( true, null, false, 0.35f, true );

                var probe = Ragdoll.User_ProbeGroundBelow( Ragdoll.Handler.GetChain( ERagdollChainType.Core ).GetBone( 2 ), GroundMask, 0.4f );
                bool probeBelow = probe.transform;
                //bool probeBelow = Ragdoll.User_ProbeGroundBelowAnchorBone( GroundMask, 0.425f ).transform;

                if( !probeBelow )
                    if( canGetup != ERagdollGetUpType.None )
                        if( Ragdoll.User_GetChainBonesVelocity( ERagdollChainType.Core ).magnitude < .2f )
                        {
                            stuckTimer += Time.fixedDeltaTime;
                            if( stuckTimer > 2f )
                            {
                                stuckTimer = 0f;
                                Ragdoll.User_AddAllBonesImpact( Vector3.up * 0.15f, 0.125f );
                                Ragdoll.User_SetAllPhysicalTorque( Vector3.one, 0.2f, false );
                            }
                        }

                if( tryGetUpDur < 0.1f && canGetup == ERagdollGetUpType.FromFacedown )
                {
                    SetX = 0f;
                    SetZ = 1f;

                    if( probeBelow )
                    {
                        if( Ragdoll.User_GetChainBonesVelocity( ERagdollChainType.Core ).magnitude < 0.3f )
                        {
                            anim.CrossFadeInFixedTime( "Get Up", 0.2f );

                            rig.rotation = Ragdoll.User_GetMappedRotationHipsToHead( Vector3.up, false );
                            rig.position = Ragdoll.User_ProbeGroundBelow( Ragdoll.Handler.GetChain( ERagdollChainType.Core ).GetBone( 2 ), GroundMask, 1f ).point;

                            Ragdoll.User_TransitionToStandingMode( 0.6f, 0.3f, 0f, 0f );

                            aiStage = EAIStage.GetUp;
                            getUpDur = 0.6f;
                            accel = 0f;
                        }
                    }
                }
                else
                {
                    var canGetupSide = Ragdoll.User_LayingOnSide();

                    if( canGetup == ERagdollGetUpType.FromBack )
                    {
                        SetZ = -0.5f;
                    }
                    else if( canGetup == ERagdollGetUpType.FromFacedown )
                    {
                        SetZ = 0.7f;
                    }
                    else
                        SetZ = 0f;

                    if( canGetupSide == ERagdollGetUpType.FromLeftSide )
                    {
                        SetX = 0.5f;
                    }
                    else if( canGetupSide == ERagdollGetUpType.FromRightSide )
                    {
                        SetX = -0.5f;
                    }
                    else
                    {
                        SetX = Mathf.Sin( Time.fixedTime * 1.5f );
                    }

                    if( tryGetUpDur <= 0f )
                    {
                        Vector3 velo = Ragdoll.User_GetChainBonesVelocity( ERagdollChainType.Core, false );
                        float veloMagn = velo.magnitude;
                        additiveFadeIn = true;

                        if( Time.time - lastTryGetup > 1.5f )
                            if( veloMagn < 0.8f )
                                if( canGetup == ERagdollGetUpType.FromBack )
                                {
                                    tryGetSign = -tryGetSign;
                                    tryGetUpDur = 2f;
                                    anim.CrossFadeInFixedTime( "Try Get Up" );
                                }
                    }
                    else
                    {
                        tryGetUpDur -= Time.fixedDeltaTime;

                        if( canGetup != ERagdollGetUpType.FromFacedown )
                        {
                            Ragdoll.User_SetPhysicalTorqueOnRigidbody( Ragdoll.Handler.GetAnchorBoneController.GameRigidbody, GetUpHelperTorque * tryGetSign, 0f, true, ForceMode.Acceleration, true );
                            Ragdoll.User_SetPhysicalTorqueOnRigidbody( Ragdoll.Handler.GetChain( ERagdollChainType.Core ).GetBone( 1 ).GameRigidbody, GetUpHelperTorque * tryGetSign * 0.9f, 0f, true, ForceMode.Acceleration, true );
                            Ragdoll.User_SetPhysicalTorqueOnRigidbody( Ragdoll.Handler.GetChain( ERagdollChainType.Core ).GetBone( 2 ).GameRigidbody, GetUpHelperTorque * tryGetSign * 0.8f, 0f, true, ForceMode.Acceleration, true );
                        }

                        if( tryGetUpDur <= 0f )
                        {
                            anim.CrossFadeInFixedTime( "Fall" );
                            lastTryGetup = Time.time;
                        }
                    }
                }
            }
            else if( aiStage == EAIStage.GetUp )
            {
                //canWalkAnimation = false;
                getUpDur -= Time.fixedDeltaTime;
                if( getUpDur < 0f ) aiStage = EAIStage.FindPosition;
            }

            if( !disableRigMove )
            {
                //if( wasRootMotion == false )
                Vector3 vel = MovementDir * MovSpeed * accel;
                vel.y = rig.velocity.y;
                rig.velocity = vel;
            }

            if( !wasMoving )
            {
                accel = Mathf.Lerp( accel, 0f, Time.fixedDeltaTime * 4f );
            }

            HandleBasicAnimations();

            if( additiveFadeIn )
            {
                SetAdditive = 0.4f + Mathf.Abs( Mathf.Sin( Time.fixedTime * 1.5f ) * 0.6f );
            }
            else
                SetAdditive = 0f;
        }

        private float SetAdditive
        { set { SetAdditiveW = Mathf.SmoothDamp( TargetAnimator.GetLayerWeight( 1 ), value, ref sd_layer, 0.1f ); } }
        private float SetAdditiveW
        { set { TargetAnimator.SetLayerWeight( 1, value ); } }
        private float sd_layer = 0f;

        private float smoothDampSpd = 0.25f;
        private float SetX
        { set { ExtraX = Mathf.SmoothDamp( ExtraX, value, ref sd_extraX, smoothDampSpd ); } }
        private float SetZ
        { set { ExtraZ = Mathf.SmoothDamp( ExtraZ, value, ref sd_extraZ, smoothDampSpd ); } }

        private float sd_extraX = 0f;
        private float sd_extraZ = 0f;

        private Vector3 MovementDir;

        #region Hashes

        private int _hash_ExtraX = -1;
        private int _hash_ExtraZ = -1;

        protected virtual void PrepareHashes()
        {
            _hash_ExtraX = Animator.StringToHash( "ExtraX" );
            _hash_ExtraZ = Animator.StringToHash( "ExtraZ" );
        }

        #endregion Hashes

        #region Animator Properties

        public float ExtraX
        { get { return TargetAnimator.GetFloat( _hash_ExtraX ); } protected set { TargetAnimator.SetFloat( _hash_ExtraX, value ); } }
        public float ExtraZ
        { get { return TargetAnimator.GetFloat( _hash_ExtraZ ); } protected set { TargetAnimator.SetFloat( _hash_ExtraZ, value ); } }

        #endregion Animator Properties

        private void HandleBasicAnimations()
        {
            if( canWalkAnimation )
            {
                if( forceWalkAnim )
                    anim.CrossFadeInFixedTime( "Walk", 0.2f );
                else
                {
                    if( accel < 0.05f ) anim.CrossFadeInFixedTime( "Idle", 0.2f );
                    else anim.CrossFadeInFixedTime( "Walk", 0.2f );
                }
            }
        }

        private bool attackExecution = false;
        private bool forceWalkAnim = false;
        private bool hittedEnemy = false;
        private float attackDur = 0f;
        private float tryGetUpDur = 0f;
        private float tryGetSign = 1f;
        private float lastTryGetup = -1f;

        public void EJumpAttack()
        {
            aiStage = EAIStage.DuringAttack;
            Ragdoll.User_SwitchFallState();

            Vector3 velocity = transform.TransformVector( JumpImpact );
            Ragdoll.User_SetAllBonesVelocity( velocity );

            Vector3 boost = velocity;
            boost.y *= 0.4f;
            Ragdoll.User_AddAllBonesImpact( boost * 0.02f * JumpBoost, 0.15f );

            Ragdoll.User_SetAllPhysicalTorque( JumpHelpTorque, 0.125f, true );
        }

        private void OnDrawGizmosSelected()
        {
            if( Enemy )
            {
                Gizmos.DrawRay( Enemy.position, ( transform.position - Enemy.position ).normalized * ToAttackDistance.y );
                if( attackPosition != Vector3.zero ) Gizmos.DrawRay( attackPosition, Vector3.up );
            }
        }

        private bool disableRigMove = false;
        private float accel = 0f;
        private bool wasMoving = false;

        private void GoForward()
        {
            wasMoving = true;
            accel = Mathf.Lerp( accel, 1f, Time.fixedDeltaTime * 4f );
        }

        private void RotateTowards( Vector3 pos )
        {
            //rig.rotation = Quaternion.Slerp(rig.rotation, Quaternion.LookRotation(transform.position, pos), Time.fixedDeltaTime * RotSpeed);
            rig.angularVelocity = Vector3.zero;
            Vector3 rotDir = Vector3.ProjectOnPlane( pos - transform.position, Vector3.up );
            rig.rotation = Quaternion.Slerp( rig.rotation, Quaternion.LookRotation( rotDir ), Time.fixedDeltaTime * RotSpeed );
        }
    }
}