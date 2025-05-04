using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_Rotate : FimpossibleComponent
    {
        public Rigidbody ToRotate;
        public Vector3 AngularVelocity = Vector3.up;
        public bool resetRot = false;

        private void Update()
        {
            if( ToRotate == null ) transform.Rotate( AngularVelocity * Time.deltaTime, Space.Self );
        }

        private void FixedUpdate()
        {
            if( ToRotate )
            {
                if( resetRot )
                {
                    ToRotate.rotation = Quaternion.Euler( 0f, ToRotate.rotation.eulerAngles.y, 0f );
                }

                if( ToRotate.isKinematic )
                    ToRotate.rotation *= Quaternion.Euler( AngularVelocity * Time.fixedDeltaTime );
                else
                    ToRotate.angularVelocity = AngularVelocity;
            }
        }
    }
}