using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_FeetleMover : FBasic_RigidbodyMover
    {
        public string Idle = "Idle";
        public string Walk = "Walk";

        protected override void Update()
        {
            base.Update();

            if( moveDirectionLocal != Vector2.zero )
            {
                if( played != Walk )
                {
                    played = Walk;
                    Mecanim.CrossFadeInFixedTime( Walk, 0.2f );
                }
            }
            else
            {
                if( played != Idle )
                {
                    played = Idle;
                    Mecanim.CrossFadeInFixedTime( Idle, 0.2f );
                }
            }
        }

        private string played = "";
    }
}