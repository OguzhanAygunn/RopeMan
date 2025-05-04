using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_WaterDropCharacter : FimpossibleComponent
    {
        public RagdollAnimator2 ParentRagdoll;
        public AudioSource HittedAudio;

        public float ImpactPower = 2f;
        public float FallOnHitPower = 10f;
        public float BodyPushPower = 10f;

        private float hitCulldown = -1f;

        public void RagdollAnimator2BoneCollision( RA2BoneCollisionHandler hitted )
        {
            var lastimpact = hitted.LatestEnterCollision;
            float velocity = lastimpact.relativeVelocity.magnitude;
            float impulse = lastimpact.impulse.magnitude;

            // Play audio only on ball layer hits, when helocity is big enough and with some culldown to avoid duplicated sounds
            if( HittedAudio ) if( hitted.LatestEnterCollision.collider.gameObject.layer == 4 ) if( velocity >= FallOnHitPower || impulse > 40f )
                    {
                        if( Time.unscaledTime - hitCulldown > 0.1f )
                        {
                            hitCulldown = Time.unscaledTime;
                            HittedAudio.Play();
                        }
                    }

            if( ParentRagdoll.IsInFallingOrSleepMode ) return;

            if( velocity < FallOnHitPower ) return;

            // Fall impact is calling few methods inside, like switch FallState and AddImpact
            ParentRagdoll.User_FallImpact( lastimpact.relativeVelocity.normalized, ImpactPower, 0.0f, BodyPushPower, hitted.DummyBoneRigidbody );

            ParentRagdoll.Handler.Mecanim.CrossFadeInFixedTime( "Fall", 0.2f );
        }
    }
}