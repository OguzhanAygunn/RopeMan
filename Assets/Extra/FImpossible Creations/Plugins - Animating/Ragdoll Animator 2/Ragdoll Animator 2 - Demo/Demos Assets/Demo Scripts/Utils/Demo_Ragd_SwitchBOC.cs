using FIMSpace.FProceduralAnimation;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_SwitchBOC : FimpossibleComponent
    {
        public RagdollAnimator2 Ragdoll;

        public void SwitchBlendOnCollision( bool enabled )
        {
            var boc = Ragdoll.Handler.GetExtraFeatureHelper<RAF_BlendOnCollisions>();
            if( boc != null ) boc.Enabled = enabled;
        }
    }
}