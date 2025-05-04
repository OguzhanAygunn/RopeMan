using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    [DefaultExecutionOrder(-2)]
    public class Demo_Ragd_Follow : FimpossibleComponent
    {
        public Transform ToFollow;
        public bool InitPositionIsRelative;
        Vector3 localPos = Vector3.zero;

        private void Start()
        {
            if( ToFollow )
                if( InitPositionIsRelative ) localPos = ToFollow.InverseTransformPoint( transform.position );
        }

        private void LateUpdate()
        {
            if( ToFollow == null ) return;

            transform.position = ToFollow.TransformPoint( localPos );
        }
    }
}