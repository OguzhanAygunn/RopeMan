using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_PushRaycast : FimpossibleComponent
    {
        public override string HeaderInfo => "Ragdoll needs to have added bone indicators with Extra Features in order to make this component work!";

        public LayerMask RaycastMask;

        [Header( "Use Left mouse button to apply impact on any detected ragdoll limb" )]
        public float PowerMul = 3f;

        [Range( 0f, 0.65f )] public float ImpactDuration = 0.2f;

        [Space( 6 )]
        [Range( 0f, 1f )] public float FadeMusclesTo = 0.175f;

        [Range( 0f, 1.25f )] public float FadeMusclesDuration = 0.75f;

        [Space( 4 )]
        [Tooltip( "Used in demos to play animations on dragged character" )]
        public bool PlayAnimations = false;

        private void Update()
        {
            if( Input.GetMouseButtonDown( 0 ) )
            {
                Ray r = Camera.main.ScreenPointToRay( Input.mousePosition );
                RaycastHit hit;

                if( Physics.Raycast( r, out hit, Mathf.Infinity, RaycastMask ) )
                {
                    if( hit.rigidbody )
                    {
                        var indicator = hit.transform.GetComponent<RagdollAnimator2BoneIndicator>();
                        if( indicator == null ) return; // No Ragdoll Limb

                        var ragdollHandler = indicator.ParentHandler; // Ragdoll Controller

                        var dummyRigibody = indicator.DummyBoneRigidbody; // Get dummy bone rigidbody in case it is animator collider indicator

                        ragdollHandler.User_SwitchFallState( RagdollHandler.EAnimatingMode.Falling );

                        ragdollHandler.User_AddRigidbodyImpact( dummyRigibody, r.direction * ( PowerMul ), ImpactDuration, ForceMode.Impulse );

                        ragdollHandler.User_FadeMusclesPowerMultiplicator( FadeMusclesTo, FadeMusclesDuration );

                        if( PlayAnimations )
                        {
                            ragdollHandler.Mecanim.CrossFadeInFixedTime( "Fall", 0.05f );
                            ragdollHandler.Mecanim.SetBool( "Action", true ); // For demo scenes animator purposes
                        }
                    }
                }
            }
        }
    }
}