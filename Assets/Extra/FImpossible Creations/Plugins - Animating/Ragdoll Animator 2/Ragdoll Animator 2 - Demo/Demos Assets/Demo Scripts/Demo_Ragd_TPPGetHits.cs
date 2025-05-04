using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_TPPGetHits : FimpossibleComponent
    {
        public int HPPoints = 5;
        public RagdollAnimator2 Ragdoll;
        public float FallImpactPower = 9f;
        public ForceMode Force = ForceMode.Force;
        public bool Sleep = true;

        public void Hitted( Demo_Ragd_Bullet.HitInfo info )
        {
            HPPoints -= Mathf.RoundToInt( info.Damage );

            RagdollAnimator2BoneIndicator indic = info.rHit.collider.GetComponent<RagdollAnimator2BoneIndicator>();
            if( indic )
            {
                if( indic.BodyBoneID == ERagdollBoneID.Head ) HPPoints -= Mathf.RoundToInt( info.Damage ) * 4;
            }

            if( HPPoints <= 0 )
            {
                Ragdoll.Mecanim.SetBool( "Action", true );
                Ragdoll.User_SwitchFallState( Sleep ? RagdollHandler.EAnimatingMode.Sleep : RagdollHandler.EAnimatingMode.Falling );
                Ragdoll.Settings.User_ResetOverrideBlends();
                Ragdoll.User_DisableMecanimAfter( 2.5f );

                if( indic == null || indic.ParentChain == null )
                {
                    Ragdoll.Mecanim.CrossFadeInFixedTime( "Fall", 0.12f );
                }
                else
                {
                    float crossfade = 0.12f;

                    if( indic.ParentChain.ChainType == ERagdollChainType.Core )
                    {
                        if( indic.BodyBoneID == ERagdollBoneID.Head )
                        {
                            Ragdoll.Mecanim.CrossFadeInFixedTime( "Hit Head", crossfade );
                        }
                        else if( indic.BodyBoneID == ERagdollBoneID.Hips )
                        {
                            Ragdoll.Mecanim.CrossFadeInFixedTime( "Hit Stomach", crossfade );
                        }
                        else
                        {
                            Ragdoll.Mecanim.CrossFadeInFixedTime( "Hit Chest", crossfade );
                        }
                    }
                    else if( indic.ParentChain.ChainType == ERagdollChainType.LeftLeg )
                    {
                        Ragdoll.Mecanim.CrossFadeInFixedTime( "Hit L Leg", crossfade );
                    }
                    else if( indic.ParentChain.ChainType == ERagdollChainType.RightLeg )
                    {
                        Ragdoll.Mecanim.CrossFadeInFixedTime( "Hit R Leg", crossfade );
                    }
                    else if( indic.ParentChain.ChainType == ERagdollChainType.LeftArm )
                    {
                        Ragdoll.Mecanim.CrossFadeInFixedTime( "Hit L Arm", crossfade );
                    }
                    else if( indic.ParentChain.ChainType == ERagdollChainType.RightArm )
                    {
                        Ragdoll.Mecanim.CrossFadeInFixedTime( "Hit R Arm", crossfade );
                    }
                }

                if( indic ) if( indic.BoneSettings != null ) Ragdoll.User_AddBoneImpact( indic.BoneSettings, info.flightDirection * FallImpactPower, 0.05f, Force );
            }
        }
    }
}