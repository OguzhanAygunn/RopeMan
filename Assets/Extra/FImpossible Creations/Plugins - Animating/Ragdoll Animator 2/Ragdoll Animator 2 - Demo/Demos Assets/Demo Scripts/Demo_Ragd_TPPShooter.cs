using FIMSpace.FProceduralAnimation;
using FIMSpace.FTools;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    [DefaultExecutionOrder( 100001 )]
    public class Demo_Ragd_TPPShooter : FimpossibleComponent
    {
        public FBasic_RigidbodyMover Mover;
        public Transform Spine;
        public Animator Mecanim;

        [Space( 5 )]
        public Transform ShotPoint;

        public GameObject Muzzle;
        public GameObject Bullet;
        public AudioSource ShotAudio;

        [Space( 5 )]
        public float BulletImpact = 2f;
        public float HoldShotCulldown = 0.12f;

        private UniRotateBone rotator;

        private void Start()
        {
            rotator = new UniRotateBone( Spine, transform );
        }

        private void Update()
        {
            Vector2 moveDirectionLocal = Vector2.zero;
            if( Input.GetKey( KeyCode.A ) ) moveDirectionLocal += Vector2.left;
            else if( Input.GetKey( KeyCode.D ) ) moveDirectionLocal += Vector2.right;
            if( Input.GetKey( KeyCode.W ) ) moveDirectionLocal += Vector2.up;
            else if( Input.GetKey( KeyCode.S ) ) moveDirectionLocal += Vector2.down;
            moveDirectionLocal.Normalize();

            Mover.MoveTowards( transform.position + transform.TransformDirection( new Vector3( moveDirectionLocal.x, 0f, moveDirectionLocal.y ) ), false );

            Vector3 lookDir = Vector3.ProjectOnPlane( Camera.main.transform.forward, Vector3.up ).normalized;
            Mover.SetRotation( lookDir );
            Mover.RotateToSpeed = 10000f;

            rotator.PreCalibrate();
        }

        private void Shot()
        {
            shotCulldown = HoldShotCulldown;
            Mecanim.Play( "Shot", 2, 0 );

            if( Muzzle )
            {
                GameObject muzzle = Instantiate( Muzzle );
                muzzle.transform.SetParent( ShotPoint, true );
                muzzle.transform.localPosition = Vector3.zero;
                muzzle.transform.localRotation = Quaternion.identity;
                Destroy( muzzle, 0.02f );

                GameObject bullet = Instantiate( Bullet );
                bullet.transform.position = ShotPoint.position;

                Demo_Ragd_Bullet b = bullet.GetComponent<Demo_Ragd_Bullet>();

                Ray r = Camera.main.ScreenPointToRay( Input.mousePosition );
                RaycastHit frontHit;
                Physics.Raycast( r.origin, r.direction, out frontHit, 1000f, b.ProjectiletHitMask );

                Quaternion targetRot = ShotPoint.rotation;

                if( frontHit.transform )
                {
                    targetRot = Quaternion.LookRotation( ( frontHit.point - ShotPoint.position ).normalized );
                }
                else
                {
                    targetRot = Quaternion.LookRotation( ( ( r.origin + r.direction * 10f ) - ShotPoint.position ) );
                }

                bullet.transform.rotation = targetRot;
                b.OnBulletHit = OnBulletHit;
                b.StepForward( 0.25f );
            }

            if( ShotAudio ) ShotAudio.Play();
        }

        /// <summary>
        /// Update with procedural aim animation coordinates for bullet creation
        /// </summary>
        private void LateUpdate()
        {
            if( Spine )
            {
                Vector3 lookDir = Quaternion.LookRotation( transform.InverseTransformDirection( Camera.main.transform.forward ) ).eulerAngles;
                rotator.RotateByDynamic( lookDir );
            }

            if( Input.GetKeyDown( KeyCode.F ) || Input.GetMouseButtonDown( 0 ) )
            {
                Shot();
            }
            else if( Input.GetKey( KeyCode.F ) || Input.GetMouseButton( 0 ) )
            {
                if( shotCulldown <= 0f ) { Shot();  }
                else shotCulldown -= Time.deltaTime;
            }
            else
            {
                shotCulldown = 0f;
            }
        }

        float shotCulldown = 0f;

        private void OnBulletHit( Demo_Ragd_Bullet.HitInfo hitInfo )
        {
            if( hitInfo.rHit.collider == null ) return;

            RagdollAnimator2BoneIndicator indic = hitInfo.rHit.collider.transform.GetComponent<RagdollAnimator2BoneIndicator>();

            if( indic )
            {
                //indic.DummyBoneRigidbody.AddForce( -hitInfo.rHit.normal * BulletImpact, ForceMode.Force );

                if( BulletImpact > 0f )
                {
                    indic.ParentHandler.User_AddBoneImpact( indic.BoneSettings, -hitInfo.rHit.normal * BulletImpact, 0.1f );
                    if( indic.ParentHandler.IsFallingOrSleep == false ) indic.ParentChain.User_ForceOverrideAllBonesBlendFor( 0.45f );
                }

                indic.ParentRagdollAnimator.SendMessage( "Hitted", hitInfo );
            }
            else
            {
                if( BulletImpact > 0f )
                    if( hitInfo.rHit.rigidbody != null )
                        hitInfo.rHit.rigidbody.AddForce( -hitInfo.rHit.normal * BulletImpact, ForceMode.Force );
            }
        }
    }
}