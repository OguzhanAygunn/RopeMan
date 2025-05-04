using FIMSpace.FProceduralAnimation;
using UnityEngine;
using UnityEngine.UI;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_HeroFallDamage : MonoBehaviour, IRagdollAnimator2Receiver
    {
        public FBasic_RigidbodyMover Mover;
        public RagdollAnimator2 Ragdoll;

        [Tooltip( "Falling Y Velocity to trigger fall ragdoll on" )]
        public float RagdollOnFallVelocityAbove = 10f;

        public float AdditionalImpactOnFall = 0.1f;

        [Space( 5 )]
        public GameObject TextPrefab;

        public Transform Canvas;
        public AudioSource HitAudio;

        [Space( 5 )]
        public bool OnlyCoreCollisions = true;

        public float HitThreshold = 5f;
        public float MaxHitDamageAt = 10f;
        public float MaxSingleHitDamage = 30f;

        /// <summary> Memory for velocity before hitting ground </summary>
        private Vector3 previousFrameVelocity = Vector3.zero;

        /// <summary> Helper velocity memory </summary>
        private Vector3 lastVelocity = Vector3.zero;

        /// <summary> Flag for ground hit check </summary>
        private bool wasGrounded = true;

        protected RAF_BlendOnCollisions blendOnCollisions;

        protected virtual void Start()
        {
            blendOnCollisions = Ragdoll.Handler.GetExtraFeature<RAF_BlendOnCollisions>();
        }

        protected virtual void FixedUpdate()
        {
            previousFrameVelocity = lastVelocity;
            lastVelocity = Mover.Rigb.velocity;

            if( wasGrounded != Mover.isGrounded )
            {
                if( Ragdoll.AnimatingMode == RagdollHandler.EAnimatingMode.Standing )
                {
                    lastFallY = 1000000f; // Reset Fall y

                    float veloY = previousFrameVelocity.y;
                    if( veloY <= -RagdollOnFallVelocityAbove )
                    {
                        SpawnDamage( GetDamage( -veloY * 4f ) ); // Since using impulses on hit events, we need to provide higher velocity
                        OnHit( 20f );

                        Ragdoll.User_SwitchFallState();
                        if( AdditionalImpactOnFall > 0f ) Ragdoll.User_AddAllBonesImpact( previousFrameVelocity * AdditionalImpactOnFall, 0f, ForceMode.Impulse );
                    }
                }

                if( blendOnCollisions ) blendOnCollisions.Helper.Enabled = Mover.isGrounded;
            }

            wasGrounded = Mover.isGrounded;
        }

        private float lastHitAt = -100f;

        public void RagdollAnimator2_OnCollisionEnterEvent( RA2BoneCollisionHandler hitted, Collision mainCollision )
        {
            if( hitted.ParentHandler.AnimatingMode != RagdollHandler.EAnimatingMode.Falling ) return;

            if( Time.fixedTime - lastHitAt < 0.25f ) return; // Wait 0.25 of sec between hits

            float currentY = Ragdoll.User_GetPosition_BottomCenter().y;
            if( currentY - lastFallY > -3.5f )
            {
                return; // Height difference between ground hits
            }

            float hitPower = mainCollision.impulse.magnitude;

            if( OnlyCoreCollisions )
            {
                if( hitted.ParentChain.ChainType != ERagdollChainType.Core ) return;
            }
            else
            {
                if( hitted.ParentChain.ChainType.IsLeg() ) hitPower *= 0.8f; // Hit power of leg is lower than rest of the body
                if( hitted.ParentChain.ChainType.IsArm() ) hitPower *= 0.6f; // Hit power of arm is lower than rest of the body
            }

            if( hitPower > HitThreshold )
            {
                lastFallY = currentY;
                OnHit( mainCollision.relativeVelocity.magnitude );
                SpawnDamage( GetDamage( hitPower ) );
            }
        }

        protected virtual float GetDamage( float velocity )
        {
            return Mathf.Lerp( MaxSingleHitDamage * 0.2f, MaxSingleHitDamage, Mathf.InverseLerp( HitThreshold, MaxHitDamageAt, velocity ) );
        }

        private float audioCulldown = -1f;
        float lastFallY = 1000000f;

        private void OnHit( float velocity )
        {
            if( Time.unscaledTime - audioCulldown < 0.1f ) return;
            if( velocity < 2f ) return;

            audioCulldown = Time.unscaledTime;
            if( HitAudio ) HitAudio.PlayOneShot( HitAudio.clip, 0.3f + Mathf.InverseLerp( 2f, 10f, velocity ) * 0.7f );
        }

        private void SpawnDamage( float dmg )
        {
            lastHitAt = Time.fixedTime;

            GameObject textObj = Instantiate( TextPrefab );
            Text text = textObj.GetComponent<Text>();
            text.text = Mathf.Round( -dmg ).ToString();
            textObj.transform.SetParent( Canvas );
            text.rectTransform.anchoredPosition = new Vector2( Mathf.Sin( Time.time * 7f ) * 120f, 0f );
            text.rectTransform.localScale = Vector3.one;
        }
    }
}