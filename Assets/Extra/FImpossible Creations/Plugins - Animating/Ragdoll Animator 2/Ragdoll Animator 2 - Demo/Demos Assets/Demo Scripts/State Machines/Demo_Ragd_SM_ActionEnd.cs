using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_SM_ActionEnd : StateMachineBehaviour
    {
        [FPD_Header( "Changing 'Action' variable in animator controller" )]
        [Range( 0.1f, .99f )] public float EndsAt = 0.99f;

        private int actionHash = Animator.StringToHash( "Action" );
        private bool wasCalled = false;

        public override void OnStateEnter( Animator animator, AnimatorStateInfo stateInfo, int layerIndex )
        {
            wasCalled = false;
            animator.SetBool( actionHash, true );
        }

        public override void OnStateUpdate( Animator animator, AnimatorStateInfo stateInfo, int layerIndex )
        {
            if( wasCalled ) return;

            if( stateInfo.normalizedTime >= EndsAt )
            {
                SetFalse( animator );
            }
        }

        public override void OnStateExit( Animator animator, AnimatorStateInfo stateInfo, int layerIndex )
        {
            if( !wasCalled ) SetFalse( animator );
        }

        public override void OnStateMachineExit( Animator animator, int stateMachinePathHash )
        {
            if( !wasCalled ) SetFalse( animator );
        }

        private void SetFalse( Animator animator )
        {
            animator.SetBool( actionHash, false );
            wasCalled = true;
        }
    }
}