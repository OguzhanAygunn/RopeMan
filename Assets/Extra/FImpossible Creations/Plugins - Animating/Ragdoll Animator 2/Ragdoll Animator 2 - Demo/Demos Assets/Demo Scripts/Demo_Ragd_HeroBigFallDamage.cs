using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_HeroBigFallDamage : Demo_Ragd_HeroFallDamage
    {
        [Space( 5 )]
        public float FallAfter = 1f;

        public float DragOnVelocity = 10f;

        private bool fallSwitchFlag = false;
        private float fallingTime = 0f;
        private float lastFallingTime = 0f;
        float startFallY = 0f;

        protected override void Start()
        {
            base.Start();
            Mover.Rigb.SetMaxLinearVelocityU2022( 25f );
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if( Mover.isGrounded == false )
            {
                if( Mover.Rigb.velocity.y < -0.25f ) fallingTime += Time.fixedDeltaTime;
                lastFallingTime = fallingTime;

                if( fallSwitchFlag == false && !Ragdoll.IsInFallingOrSleepMode )
                {
                    if( fallingTime > FallAfter )
                    {
                        startFallY = Ragdoll.User_GetPosition_BottomCenter().y;
                        Ragdoll.User_SwitchFallState();
                        Ragdoll.Handler.Mecanim.CrossFadeInFixedTime( "Fall", 0.25f );
                        Ragdoll.Handler.GetAnchorBoneController.GameRigidbody.maxAngularVelocity = 20f;
                        Ragdoll.User_SetPhysicalTorqueOnRigidbody( Ragdoll.Handler.GetAnchorBoneController.GameRigidbody, Ragdoll.User_BoneWorldRight( Ragdoll.Handler.GetAnchorBoneController ) * 15f, 0.75f, false, ForceMode.VelocityChange );
                        Ragdoll.User_ChangeAllRigidbodiesDrag( 0.5f );
                        Ragdoll.User_SwitchAllBonesMaxVelocity( 30f );
                        fallSwitchFlag = true;
                    }
                }
            }
            else
            {
                fallingTime = 0f;

                if( fallSwitchFlag == true )
                {
                    Ragdoll.User_ChangeAllRigidbodiesDrag( 0f );
                    Ragdoll.User_SwitchAllBonesMaxVelocity( 10000f );
                    fallSwitchFlag = false;
                }
            }
        }

        protected override float GetDamage( float velocity )
        {
            float fallYUnits = startFallY - Ragdoll.User_GetPosition_BottomCenter().y; // Height difference

            if( fallYUnits > 20f ) // If travelled height is really big - do 100 damage
            {
                lastFallingTime = 0f;
                fallingTime = 0f;
                startFallY = Ragdoll.User_GetPosition_BottomCenter().y; // Reset after 100
                return 100f;
            }

            return base.GetDamage( velocity );
        }
    }
}