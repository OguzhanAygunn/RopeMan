using FIMSpace.FProceduralAnimation;
using System.Collections;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    [DefaultExecutionOrder( 100 )]
    public class Demo_Ragd_SnareTrapTriggerCollision : FimpossibleComponent
    {
        public GameObject Target;

        public Animator SnareTrapBranch;
        public Rigidbody SnareTrapLightElement;
        public Rigidbody SnareTrapCatch;
        public Transform Circle;
        public RagdollAnimator2 Ragdoll;
        public FixedJoint FootAttach;
        public Transform LineEnd;

        private void OnTriggerEnter( Collider other )
        {
            if( other.gameObject == Target )
            {
                SnareTrapLightElement.isKinematic = false;
                SnareTrapBranch.SetTrigger( "Use" );

                Ragdoll.User_SwitchFallState();
                StartCoroutine( IECircle() );

                FootAttach.connectedBody = Ragdoll.User_GetBoneSetupByHumanoidBone( HumanBodyBones.LeftFoot ).GameRigidbody;
            }
        }

        private IEnumerator IECircle()
        {
            float elapsed = 0f;

            while( elapsed < 0.12f )
            {
                elapsed += Time.deltaTime;
                float prog = elapsed / 0.12f;
                if( prog > 1f ) prog = 1f;
                prog = 1f - prog;
                Circle.localScale = new Vector3( prog, prog, prog );
                yield return null;
            }
        }

        private void LateUpdate()
        {
            if( FootAttach.connectedBody == null ) return;
            LineEnd.position = FootAttach.connectedBody.transform.position;
        }
    }
}