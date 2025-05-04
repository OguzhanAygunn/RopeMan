using FIMSpace.FProceduralAnimation;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_ScaredGuyScene : FimpossibleComponent
    {
        public RagdollAnimator2 Ragdoll;
        public float ScareAfter = 1f;

        private void Start()
        {
            Invoke( "Scared", ScareAfter );
        }

        public void Scared()
        {
            Ragdoll.Handler.Mecanim.CrossFadeInFixedTime( "Scared", 0.2f );
        }

        public void Catched()
        {
            Ragdoll.Handler.Mecanim.CrossFadeInFixedTime( "Fall", 0.2f );
            Ragdoll.User_SwitchFallState();
        }

        public void Throw()
        {
        }
    }
}