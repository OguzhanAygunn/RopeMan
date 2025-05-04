using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    [DefaultExecutionOrder( -100 )]
    public class Demo_Ragd_ChangeCoords : FimpossibleComponent
    {
        public Transform ToMove;
        public Transform ToFollow;
        public Vector3 LocalPosition = Vector3.zero;
        public Vector3 LocalRotation = Vector3.zero;

        private Vector3 toFollowPos;

        private void Reset()
        {
            ToMove = transform;
        }

        private void Start()
        {
            gameObject.AddComponent<LateFixedUpdate>().parent = this;

            if (ToFollow) toFollowPos = ToFollow.position;

            // WaitForEndOfFrame is not called when displaying just scene view - I wanted to prevent this glitch from happening so used 'class LateFixedUpdate : MonoBehaviour'
            //StartCoroutine( IEWaitForFinalPosition() );
            //private IEnumerator IEWaitForFinalPosition()
            //{
            //    while( true )
            //    {
            //        yield return new WaitForEndOfFrame();
            //        toFollowPos = ToFollow.position;
            //    }
            //}
        }

        // Get relevant physics update position after all body physics calculations
        private void CallLateFixedUpdate()
        {
            toFollowPos = ToFollow.position;
        }


        private void LateUpdate()
        {
            ToMove.transform.position = toFollowPos + ToFollow.TransformVector( LocalPosition );
            ToMove.transform.rotation = ToFollow.rotation * Quaternion.Euler( LocalRotation );
        }


        // WaitForEndOfFrame is not called when displaying just scene view - I wanted to prevent this glitch from happening so used 'class LateFixedUpdate : MonoBehaviour'
        [DefaultExecutionOrder( 100 )]
        class LateFixedUpdate : MonoBehaviour
        {
            public Demo_Ragd_ChangeCoords parent;
            private void FixedUpdate()
            {
                parent.CallLateFixedUpdate();
            }
        }

    }
}