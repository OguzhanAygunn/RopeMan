using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_BaloonForce : MonoBehaviour
    {
        public Vector3 AddForce = new Vector3( 0f, 3f, 0f );
        public ForceMode ForceMode = ForceMode.Force;
        public Rigidbody Rigid;
        public float MaxVelocity = 1000f;
        public Transform Baloon;
        public Transform LineEnd;
        public RA2CopyJointToDummyBone TransportJoint;

        private void Start()
        {
            Baloon.localScale = Vector3.one * 0.01f;
        }

        private void Update()
        {
            Baloon.localScale = Vector3.MoveTowards( Baloon.localScale, Vector3.one, Time.deltaTime );
        }

        private void FixedUpdate()
        {
            Rigid.SetMaxLinearVelocityU2022( MaxVelocity );
            Rigid.AddForce( AddForce * Time.fixedDeltaTime, ForceMode );
        }

        public void AttachTo( Rigidbody target, Vector3 hitPoint )
        {
            LineEnd.SetParent( target.transform, true );
            LineEnd.transform.localPosition = target.transform.InverseTransformPoint( hitPoint );
            LineEnd.transform.localRotation = Quaternion.identity;

            TransportJoint.TargetParent = target.transform;
            Joint j = TransportJoint.GetComponent<Joint>();
            j.autoConfigureConnectedAnchor = false;
            j.anchor = target.transform.InverseTransformPoint( hitPoint );
            TransportJoint.gameObject.SetActive( true );
        }
    }
}