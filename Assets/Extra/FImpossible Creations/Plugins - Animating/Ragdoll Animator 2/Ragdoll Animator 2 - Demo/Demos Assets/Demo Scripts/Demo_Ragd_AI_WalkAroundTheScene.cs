using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_AI_WalkAroundTheScene : FimpossibleComponent
    {
        public FBasic_RigidbodyMover Mover;
        public Animator Mecanim;
        public float WalkDistanceRange = 4f;

        private Vector3 startPos;
        private float waitForNextPoint;
        private bool atDestination = true;
        private Vector3 targetPosition;

        private void Start()
        {
            startPos = transform.position;
            targetPosition = transform.position;
            waitForNextPoint = Random.Range( 1f, 3f );
        }

        private void Update()
        {
            if( Mecanim.GetBool( "Action" ) ) return;

            if( atDestination )
            {
                if( waitForNextPoint > 0f )
                {
                    waitForNextPoint -= Time.deltaTime;
                }
                else
                {
                    waitForNextPoint = Random.Range( 1f, 3f );
                    targetPosition = startPos + new Vector3( Random.Range( -WalkDistanceRange, WalkDistanceRange ), 0f, Random.Range( -WalkDistanceRange, WalkDistanceRange ) );
                }
            }

            float distance = Distance2D( transform.position, targetPosition );
            if( distance < 0.3f )
            {
                atDestination = true;
            }
            else if( distance > .3f )
            {
                Mover.MoveTowards( targetPosition );
            }
        }

        private float Distance2D( Vector3 a, Vector3 b )
        {
            return Vector2.Distance( new Vector2( a.x, a.z ), new Vector2( b.x, b.z ) );
        }
    }
}