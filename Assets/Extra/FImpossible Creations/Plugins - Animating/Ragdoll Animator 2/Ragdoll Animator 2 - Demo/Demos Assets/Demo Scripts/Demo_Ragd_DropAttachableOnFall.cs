using FIMSpace.FProceduralAnimation;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_DropAttachableOnFall : FimpossibleComponent
    {
        public RagdollAnimator2 ragdoll;
        public RA2AttachableObject attachable;
        void Update()
        {
            if( ragdoll.IsInFallingOrSleepMode )
            {
                ragdoll.Handler.UnwearAttachable( attachable );
                enabled = false;
            }
        }
    }
}