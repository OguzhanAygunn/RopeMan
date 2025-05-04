using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_AI_GoBackToInitialPosition : FimpossibleComponent
    {
        public FBasic_RigidbodyMover Mover;
        public Animator Mecanim;

        private Vector3 startPos;
        private Quaternion startRot;

        private void Start()
        {
            startPos = transform.position;
            startRot = transform.rotation;
        }

        private void Update()
        {
            if( Mecanim.GetBool( "Action" ) ) return;

            float distance = Distance2D( transform.position, startPos );

            if( distance < 0.3f )
            {
                Mover.moveDirectionWorld = Vector3.zero;
                Mover.moveDirectionLocal = Vector2.zero;
                if( distance > 0.01f ) Mover.SetTargetRotation( startRot * Vector3.forward );
            }
            else
            {
                Mover.MoveTowards( startPos );
            }
        }

        private float Distance2D( Vector3 a, Vector3 b )
        {
            return Vector2.Distance( new Vector2( a.x, a.z ), new Vector2( b.x, b.z ) );
        }
    }
}