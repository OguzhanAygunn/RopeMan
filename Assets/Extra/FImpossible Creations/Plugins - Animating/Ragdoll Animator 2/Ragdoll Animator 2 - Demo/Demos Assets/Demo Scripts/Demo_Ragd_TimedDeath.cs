using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_TimedDeath : FimpossibleComponent
    {
        public float DeathAfter = 3f;
        public float FallAfter = 1f;
        public float DisableMecanimAfter = 1.5f;
        public RagdollAnimator2 Ragdoll;
        public float FallImpactPower = 1f;
        public ERagdollChainType HitBody = ERagdollChainType.Core;
        public ForceMode Force = ForceMode.Force;
        public bool Sleep = true;

        float elapsed = 0f;
        int stage = 0;
        private void Update()
        {
            if( Ragdoll.AnimatingMode != RagdollHandler.EAnimatingMode.Standing ) return;

            elapsed += Time.deltaTime;
            if( elapsed > DeathAfter )
            {
                Ragdoll.Handler.AnchorBoneAttach = Mathf.InverseLerp( DeathAfter + FallAfter, DeathAfter* 0.8f, elapsed );

                if( stage == 0)
                {
                    Ragdoll.Handler.GetExtraFeature<RAF_BlendOnCollisions>().Helper.Enabled = false; // disable before falling mode
                    Ragdoll.Mecanim.SetBool( "Action", true );
                    Ragdoll.Mecanim.CrossFadeInFixedTime( "Death Back", 0.1f );
                    stage += 1;
                }

                if( elapsed > DeathAfter + FallAfter )
                {
                    Ragdoll.User_SwitchFallState( Sleep ? RagdollHandler.EAnimatingMode.Sleep : RagdollHandler.EAnimatingMode.Falling );
                    Ragdoll.User_DisableMecanimAfter( DisableMecanimAfter );
                    Ragdoll.User_AddChainImpact( Ragdoll.Handler.GetChain( HitBody ), -transform.forward * FallImpactPower, 0.15f, Force );
                }
            }
        }
    }
}