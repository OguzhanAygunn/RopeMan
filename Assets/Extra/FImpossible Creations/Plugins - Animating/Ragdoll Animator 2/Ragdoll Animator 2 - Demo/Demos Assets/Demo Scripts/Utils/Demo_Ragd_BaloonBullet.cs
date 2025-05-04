using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_BaloonBullet : Demo_Ragd_Bullet
    {
        public GameObject BaloonPrefab;

        protected override void OnHitEnd()
        {
            if( bulletHit.transform == null ) return;

            RagdollAnimator2BoneIndicator indicator = bulletHit.collider.GetComponent<RagdollAnimator2BoneIndicator>();

            if( indicator != null )
            {
                GameObject baloon = Instantiate( BaloonPrefab );
                baloon.transform.position = bulletHit.point;
                baloon.GetComponent<Rigidbody>().position = baloon.transform.position;

                Demo_Ragd_BaloonForce baloonAttach = baloon.GetComponent<Demo_Ragd_BaloonForce>();
                baloonAttach.AttachTo( indicator.DummyBoneRigidbody, bulletHit.point );
            }

            Destroy( gameObject );
        }
    }
}