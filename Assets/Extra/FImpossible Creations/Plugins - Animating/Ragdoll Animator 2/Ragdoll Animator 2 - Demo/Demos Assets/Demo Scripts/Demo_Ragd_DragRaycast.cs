using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_DragRaycast : FimpossibleComponent
    {
        public override string HeaderInfo => "Ragdoll needs to have added bone indicators with Extra Features in order to make this component work!";
#if UNITY_EDITOR
        public override UnityEditor.MessageType HeaderInfoType => UnityEditor.MessageType.Info;
#endif

        public LayerMask RaycastMask;

        [Tooltip( "Use right mouse button to drag" )]
        [Range( 0f, 2f )]
        public float DragPower = 0.75f;

        public bool SetKinematic = false;

        [Range( 0f, 2f )]
        public float DragRotatePower = 0.0f;

        [Range( 0f, 1f )]
        public float FadeMusclesTo = 0.4f;

        [Space( 4 )]
        [Tooltip( "Used in demos to play animations on dragged character" )]
        public bool PlayAnimations = false;

        public bool SetFall = true;
        public bool RestoreStandingMode = false;
        public MonoBehaviour DisableOnDrag = null;

        private Rigidbody dragging = null;
        private RagdollAnimator2BoneIndicator draggingIndicator = null;

        private Vector3 startDragPosition = Vector3.zero;
        private Vector3 dragScreenPos = Vector3.zero;
        private Vector3 dragHitLocalPos = Vector3.zero;

        private Quaternion startDragRotation = Quaternion.identity;

        private void Update()
        {
            // Udpate Input in Update()

            if( dragging )
            {
                if( Input.GetMouseButtonUp( 1 ) ) EndDragging();
                return;
            }

            if( Input.GetMouseButtonDown( 1 ) )
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

                        if( SetFall ) ragdollHandler.User_SwitchFallState();

                        draggingIndicator = indicator;
                        StartDrag( hit, indicator.DummyBoneRigidbody ); // Get dummy bone rigidbody in case it is animator collider indicator

                        ragdollHandler.User_FadeMusclesPower( FadeMusclesTo, 0.7f );

                        // You can use ragdollHandler.Mecanim or ragdollHandler.Caller references to access hitted ragdoll controller
                        if( PlayAnimations )
                        {
                            ragdollHandler.Mecanim.CrossFadeInFixedTime( "Fall", 0.25f );
                            ragdollHandler.Mecanim.SetBool( "Action", true ); // For demo scenes animator purposes
                        }
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            // Update Physics in FixedUpdate()

            if( dragging == null ) return;

            Vector3 newPos = startDragPosition;

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.transform.InverseTransformPoint( startDragPosition ).z;

            newPos += Camera.main.ScreenToWorldPoint( mousePos ) - dragScreenPos;

            //RagdollHandler.DragRigidbodyTowards( dragging, newPos, DragPower, dragHitLocalPos);

            if( dragging.isKinematic )
            {
                dragging.position = newPos;
                return;
            }

            RagdollHandlerUtilities.AddRigidbodyForceToMoveTowards( dragging, newPos, DragPower );

            if( DragRotatePower > 0f )
            {
                RagdollHandlerUtilities.AddRigidbodyTorqueToRotateTowards( dragging, startDragRotation, DragPower );
            }
        }

        private void StartDrag( RaycastHit hit, Rigidbody dummyBone )
        {
            dragging = dummyBone;
            startDragPosition = hit.point;

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.transform.InverseTransformPoint( startDragPosition ).z;
            dragScreenPos = Camera.main.ScreenToWorldPoint( mousePos );

            dragHitLocalPos = hit.rigidbody.transform.InverseTransformPoint( hit.point );
            startDragRotation = dummyBone.rotation;

            if( SetKinematic ) dragging.isKinematic = true;
            if( DisableOnDrag ) DisableOnDrag.enabled = false;
        }

        private void EndDragging()
        {
            if( SetKinematic ) dragging.isKinematic = false;

            if( RestoreStandingMode ) if( draggingIndicator ) draggingIndicator.ParentHandler.AnimatingMode = RagdollHandler.EAnimatingMode.Standing;

            draggingIndicator = null;
            dragging = null;
            if( DisableOnDrag ) DisableOnDrag.enabled = true;
        }
    }
}