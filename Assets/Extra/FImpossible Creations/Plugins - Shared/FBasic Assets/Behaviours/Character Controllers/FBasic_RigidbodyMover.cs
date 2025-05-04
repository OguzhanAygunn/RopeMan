using System;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    [AddComponentMenu( "FImpossible Creations/Demos/Fimpossible Demo Mover" )]
    [DefaultExecutionOrder( -100 )]
    public class FBasic_RigidbodyMover : FimpossibleComponent
    {
        public Rigidbody Rigb;

        [Space( 4 )]
        public float MovementSpeed = 2f;
        [Range( 0f, 1f )]
        public float RotateToSpeed = 0.8f;
        [Tooltip("When true, applying rotation by rigidbody.rotation = ...\nWhen false, applying rotation using angular velocity (smoother interpolation)")]
        public bool FixedRotation = true;

        [Range( 0f, 1f )] public float DirectMovement = 0f;
        [Range( 0f, 1f )] public float Interia = 1f;

        [Space( 4 )] public LayerMask GroundMask = 0 >> 1;

        [Space( 4 )] public float ExtraRaycastDistance = 0.01f;

        [Tooltip( "Using Spherecast is Radius greater than zero" )]
        public float RaycastRadius = 0f;

        [Space( 10 )]
        [Tooltip( "Setting 'Grounded','Moving' and 'Speed' parameters for mecanim" )]
        public Animator Mecanim;

        [Tooltip( "Animator property which will not allowing character movement is set to true" )]
        public string IsBusyProperty = "";
        public bool DisableRootMotion = false;

        public void SetTargetRotation( Vector3 dir )
        {
            targetInstantRotation = Quaternion.LookRotation( dir );
            if( currentWorldAccel == Vector3.zero ) currentWorldAccel = new Vector3( 0.0000001f, 0f, 0f );
        }

        public void SetRotation( Vector3 dir )
        {
            targetInstantRotation = Quaternion.LookRotation( dir );
            rotationAngle = targetInstantRotation.eulerAngles.y;
            targetRotation = Quaternion.Euler( 0f, rotationAngle, 0f );
        }

        public void MoveTowards( Vector3 wPos, bool setDir = true )
        {
            Vector3 tPos = new Vector3( wPos.x, 0f, wPos.z );
            Vector3 mPos = new Vector3( transform.position.x, 0f, transform.position.z );
            Vector3 dir = ( tPos - mPos ).normalized;
            moveDirectionWorld = dir;
            if( setDir ) SetTargetRotation( dir );
        }

        [Space( 6 )]
        public bool UpdateInput = true;
        [Space( 1 )]
        public float JumpPower = 3f;
        public float HoldShiftForSpeed = 0f;
        public float HoldCtrlForSpeed = 0f;

        public Action OnJump = null;

        bool wasInitialized = false;

        public void ResetTargetRotation()
        {
            targetRotation = transform.rotation;
            targetInstantRotation = transform.rotation;
            rotationAngle = transform.eulerAngles.y;

            currentWorldAccel = Vector3.zero;
            jumpRequest = 0f;
        }

        void Start()
        {
            if( !Rigb ) Rigb = GetComponent<Rigidbody>();

            if( Rigb )
            {
                Rigb.maxAngularVelocity = 30f;
                if( Rigb.interpolation == RigidbodyInterpolation.None ) Rigb.interpolation = RigidbodyInterpolation.Interpolate;
                Rigb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }

            isGrounded = true;
            targetRotation = transform.rotation;
            targetInstantRotation = transform.rotation;
            rotationAngle = transform.eulerAngles.y;

            if( Mecanim ) Mecanim.SetBool( "Grounded", true );

            wasInitialized = true;
        }

        private void OnEnable()
        {
            if( !wasInitialized ) return;
            ResetTargetRotation();
            Rigb.isKinematic = false;
            Rigb.detectCollisions = true;
            isGrounded = true;
            if( Mecanim ) isGrounded = Mecanim.GetBool( "Grounded" );
            CheckGroundedState();
        }

        private void OnDisable()
        {
            Rigb.isKinematic = true;
            Rigb.detectCollisions = true;
        }


        // Movement Calculation Params

        [NonSerialized] public Vector2 moveDirectionLocal = Vector3.zero;
        public Vector2 moveDirectionLocalNonZero { get; private set; }

        public Vector3 moveDirectionWorld { get;  set; }
        public Vector3 currentWorldAccel { get; private set; }

        [NonSerialized] public float jumpRequest = 0f;

        Quaternion targetRotation;
        Quaternion targetInstantRotation;

        float rotationAngle = 0f;
        float sd_rotationAngle = 0f;
        float toJump = 0f;

        public bool isGrounded { get; private set; } = true;


        protected virtual void Update()
        {
            if( Rigb == null ) return;

            bool updateMovement = true;
            if( Mecanim ) if( string.IsNullOrWhiteSpace( IsBusyProperty ) == false ) updateMovement = !Mecanim.GetBool( IsBusyProperty );

            if( UpdateInput && updateMovement )
            {
                if( Input.GetKeyDown( KeyCode.Space ) )
                {
                    if( toJump <= 0f )
                    {
                        jumpRequest = JumpPower;
                        toJump = 0f;
                    }
                }

                moveDirectionLocal = Vector2.zero;

                if( Input.GetKey( KeyCode.A ) ) moveDirectionLocal += Vector2.left;
                else if( Input.GetKey( KeyCode.D ) ) moveDirectionLocal += Vector2.right;

                if( Input.GetKey( KeyCode.W ) ) moveDirectionLocal += Vector2.up;
                else if( Input.GetKey( KeyCode.S ) ) moveDirectionLocal += Vector2.down;

                Quaternion flatCamRot = Quaternion.Euler( 0f, Camera.main.transform.eulerAngles.y, 0f );

                if( moveDirectionLocal != Vector2.zero )
                {
                    moveDirectionLocal.Normalize();
                    moveDirectionWorld = flatCamRot * new Vector3( moveDirectionLocal.x, 0f, moveDirectionLocal.y );
                    moveDirectionLocalNonZero = moveDirectionLocal;
                }
                else
                {
                    moveDirectionWorld = Vector3.zero;
                }

                if( moveDirectionWorld != Vector3.zero ) targetInstantRotation = Quaternion.LookRotation( moveDirectionWorld );
            }
            else if( updateMovement == false ) moveDirectionWorld = Vector3.zero;



            bool moving = false;
            if( moveDirectionWorld != Vector3.zero )
            {
                moving = true;
            }

            if( RotateToSpeed > 0f )
                if( currentWorldAccel != Vector3.zero )
                {
                    rotationAngle = Mathf.SmoothDampAngle( rotationAngle, targetInstantRotation.eulerAngles.y, ref sd_rotationAngle, Mathf.Lerp( 0.5f, 0.01f, RotateToSpeed ) );
                    targetRotation = Quaternion.Euler( 0f, rotationAngle, 0f );// Quaternion.RotateTowards(targetRotation, targetInstantRotation, Time.deltaTime * 90f * RotateToSpeed);
                }

            if( Mecanim ) Mecanim.SetBool( "Moving", moving );

            float spd = MovementSpeed;

            if( UpdateInput )
            {
                if( HoldShiftForSpeed != 0f ) if( Input.GetKey( KeyCode.LeftShift ) ) spd = HoldShiftForSpeed;
                if( HoldCtrlForSpeed != 0f ) if( Input.GetKey( KeyCode.LeftControl ) ) spd = HoldCtrlForSpeed;
            }

            float accel = 5f * MovementSpeed;
            if( !moving ) accel = 7f * MovementSpeed;

            if ( Interia < 1f)
                currentWorldAccel = Vector3.Lerp( Vector3.Slerp( currentWorldAccel, moveDirectionWorld * spd, Time.deltaTime * accel ), Vector3.MoveTowards( currentWorldAccel, moveDirectionWorld * spd, Time.deltaTime * accel ), Interia);
            else
                currentWorldAccel = Vector3.MoveTowards( currentWorldAccel, moveDirectionWorld * spd, Time.deltaTime * accel );

            if( Mecanim ) if( moving ) Mecanim.SetFloat( "Speed", currentWorldAccel.magnitude );

            moveDirectionWorld = Vector3.zero;
        }


        private void FixedUpdate()
        {
            if( Rigb == null ) return;

            Vector3 targetVelo = currentWorldAccel;

            float yAngleDiff = Mathf.DeltaAngle( Rigb.rotation.eulerAngles.y, targetInstantRotation.eulerAngles.y );
            float directMovement = DirectMovement;

            directMovement *= Mathf.Lerp(1f, Mathf.InverseLerp( 180f, 50f, Mathf.Abs( yAngleDiff )), Interia);

            targetVelo = Vector3.Lerp( targetVelo, ( transform.forward ) * targetVelo.magnitude, directMovement );
            targetVelo.y = Rigb.velocity.y;

            toJump -= Time.fixedDeltaTime;

            if( jumpRequest != 0f && toJump <= 0f )
            {
                Rigb.position += transform.up * jumpRequest * 0.01f;
                targetVelo.y = jumpRequest;
                isGrounded = false;
                jumpRequest = 0f;
                jumpTime = Time.time;
                if( Mecanim ) Mecanim.SetBool( "Grounded", false );
                if( OnJump != null ) OnJump.Invoke();
            }
            else
            {
                if( isGrounded ) // Basic not recommended but working solution - snapping to the ground (this approach will push player down quick when loosing ground)
                {
                    targetVelo.y -= 2.5f * Time.fixedDeltaTime;
                }
            }

            if( wasRootmotion == false )
            {
                if ( Rigb.isKinematic == false) Rigb.velocity = targetVelo;
            }

            if ( FixedRotation)
                Rigb.rotation = targetRotation;
            else
                Rigb.angularVelocity = FEngineering.QToAngularVelocity( Rigb.rotation, targetRotation, true );

            if( Time.time - jumpTime > 0.2f )
            {
                CheckGroundedState();
            }
            else
            {
                if( isGrounded == true )
                {
                    isGrounded = false;
                    if( Mecanim ) Mecanim.SetBool( "Grounded", false );
                }
            }

        }

        public void CheckGroundedState()
        {
            if( DoRaycasting() )
            {
                if( isGrounded == false )
                {
                    isGrounded = true;
                    if( Mecanim ) Mecanim.SetBool( "Grounded", true );
                }
            }
            else
            {
                if( isGrounded == true )
                {
                    isGrounded = false;
                    if( Mecanim ) Mecanim.SetBool( "Grounded", false );
                }
            }
        }

        bool wasRootmotion = false;
        private void OnAnimatorMove()
        {
            if( DisableRootMotion ) return;
            if( Mecanim.deltaPosition.magnitude > Time.unscaledDeltaTime * 0.1f ) wasRootmotion = true; else wasRootmotion = false;
            Mecanim.ApplyBuiltinRootMotion();
        }

        bool DoRaycasting()
        {
            if( RaycastRadius <= 0f )
            {
                return Physics.Raycast( transform.position + transform.up, -transform.up, ( isGrounded ? 1.2f : 1.01f ) + ExtraRaycastDistance, GroundMask, QueryTriggerInteraction.Ignore );
            }
            else
            {
                return Physics.SphereCast( new Ray( transform.position + transform.up, -transform.up ), RaycastRadius, ( isGrounded ? 1.2f : 1.01f ) + ExtraRaycastDistance - RaycastRadius * 0.5f, GroundMask, QueryTriggerInteraction.Ignore );
            }
        }

        float jumpTime = -1f;

    }

}