using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_FallSwitchOnKey : MonoBehaviour
    {
        public RagdollAnimator2 TargetRagdoll;
        public KeyCode key = KeyCode.Q;
        public KeyCode resetKey = KeyCode.R;

        Vector3 initPos;
        Quaternion initRot;

        private void Start()
        {
            initPos = transform.position;
            initRot = transform.rotation;
        }

        void Update()
        {
            if( Input.GetKeyDown( key ) )
            {
                TargetRagdoll.User_SwitchFallState( !TargetRagdoll.Handler.IsInStandingMode );
            }

            if( Input.GetKeyDown( resetKey ) )
            {
                TargetRagdoll.User_SwitchFallState( true );
                TargetRagdoll.User_Teleport( initPos, initRot );
            }
        }
    }
}