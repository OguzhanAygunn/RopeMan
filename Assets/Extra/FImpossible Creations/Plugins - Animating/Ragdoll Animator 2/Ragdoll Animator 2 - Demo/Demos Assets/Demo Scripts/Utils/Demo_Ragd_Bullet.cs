using System;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_Bullet : FimpossibleComponent
    {
        [Tooltip( "Speed of the bullet" )]
        public float FlySpeed = 100f;

        [Tooltip( "How far bullet can fly then beeing destroyed" )]
        public float DistanceLimit = 400f;

        private Vector3 initPosition;
        public LayerMask ProjectiletHitMask = 1 << 0;
        public float BulletDamage = 1f;
        public GameObject CreateOnHit;
        public bool SetAsChild = false;

        public Action<HitInfo> OnBulletHit = null;

        public struct HitInfo
        {
            public RaycastHit rHit;
            public Demo_Ragd_Bullet bullet;
            public Vector3 flightDirection;
            public float Damage;
        }

        protected virtual void Start()
        {
            transform.position += StepForward( .1f );
            initPosition = transform.position;
        }

        protected RaycastHit bulletHit;

        protected virtual void Update()
        {
            Vector3 newPosition = transform.position + StepForward();

            bool hitted = DoRaycast( newPosition );

            transform.position = newPosition;

            if( hitted ) HitTarget();

            if( Vector3.Distance( initPosition, transform.position ) >= DistanceLimit ) GameObject.Destroy( gameObject );
        }

        protected virtual bool DoRaycast( Vector3 newPosition )
        {
            return Physics.Linecast( transform.position, newPosition, out bulletHit, ProjectiletHitMask, QueryTriggerInteraction.Ignore );
        }

        /// <summary>
        /// When bullet hit target
        /// </summary>
        protected virtual void HitTarget()
        {
            if( bulletHit.collider )
            {
                //FBasic_Shared_BulletHittable hittable = hit.transform.GetComponent<FBasic_Shared_BulletHittable>();
                //if( hittable )
                //{
                //    if( hittable.OnProjectileHit != null ) hittable.OnProjectileHit.Invoke();
                //}

                if( CreateOnHit )
                {
                    GameObject newObject = Instantiate( CreateOnHit, bulletHit.point, Quaternion.LookRotation( bulletHit.normal ) );
                    if( SetAsChild ) newObject.transform.SetParent( bulletHit.collider.transform, true );
                }
            }

            if( OnBulletHit != null ) OnBulletHit.Invoke( new HitInfo() { bullet = this, rHit = bulletHit, flightDirection = transform.forward, Damage = BulletDamage } );

            OnHitEnd();
        }

        protected virtual void OnHitEnd()
        {
            GameObject.Destroy( gameObject );
        }

        /// <summary>
        /// Returning offset position for bullet movement speed
        /// </summary>
        internal Vector3 StepForward( float multiply = 1f )
        {
            return transform.forward * FlySpeed * multiply * Time.deltaTime;
        }
    }
}