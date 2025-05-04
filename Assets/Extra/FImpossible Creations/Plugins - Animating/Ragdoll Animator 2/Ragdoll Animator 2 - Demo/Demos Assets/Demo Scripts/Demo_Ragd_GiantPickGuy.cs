using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_GiantPickGuy : FimpossibleComponent
    {
        public RagdollAnimator2 GiantRagdoll;
        public RagdollAnimator2 ToCatch;
        private GameObject catchObject = null;

        public void Pick()
        {
            ToCatch.SendMessage( "Catched" );

            var catchHead = ToCatch.User_GetBoneSetupByHumanoidBone( HumanBodyBones.Head );
            var giantHand = GiantRagdoll.User_GetBoneSetupByHumanoidBone( HumanBodyBones.RightHand );

            catchObject = new GameObject( "Generated Catch Joints" );
            catchObject.transform.position = catchHead.PhysicalDummyBone.position;
            catchObject.transform.rotation = catchHead.PhysicalDummyBone.rotation;

            catchObject.AddComponent<Rigidbody>();
            ConfigurableJoint handJoint = catchObject.AddComponent<ConfigurableJoint>();
            handJoint.connectedBody = giantHand.GameRigidbody;
            RagdollHandler.SetConfigurableJointMotionLock( handJoint, ConfigurableJointMotion.Locked );
            handJoint.autoConfigureConnectedAnchor = false;
            handJoint.connectedAnchor = giantHand.GameRigidbody.transform.InverseTransformPoint( catchHead.SourceBone.position );

            ConfigurableJoint catchJoint = catchObject.AddComponent<ConfigurableJoint>();
            catchJoint.connectedBody = catchHead.GameRigidbody;
            RagdollHandler.SetConfigurableJointMotionLock( catchJoint, ConfigurableJointMotion.Locked );
        }

        public void Throw()
        {
            GameObject.Destroy( catchObject );
            ToCatch.SendMessage( "Throw" );
        }
    }
}