using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_KinematicsInterpolate : RagdollAnimatorFeatureBase
    {
        public override bool OnInit()
        {
            ParentRagdollHandler.AddToPostLateUpdateLoop( PostLateUpdate );
            return base.OnInit();
        }

        public override void OnDestroyFeature()
        {
            ParentRagdollHandler.RemoveFromPostLateUpdateLoop( PostLateUpdate );
        }

        float lastFixedTime = 0f;
        void PostLateUpdate()
        {
            if( InitializedWith.Enabled == false ) return;
            if( ParentRagdollHandler.AnimatingMode != RagdollHandler.EAnimatingMode.Standing ) return;

            float fixedElapsed = Time.fixedTime - lastFixedTime;

            foreach( var chain in ParentRagdollHandler.Chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    if( bone.GameRigidbody.isKinematic == false ) continue;
                    if( bone.BypassKinematicControl ) continue;

                    bone.SourceBone.position = UnityEngine.Vector3.LerpUnclamped( bone.BoneProcessor.AnimatorPosition, bone.GameRigidbody.position, fixedElapsed );
                }
            }

            lastFixedTime = Time.fixedTime;
        }

#if UNITY_EDITOR

        public override bool Editor_DisplayEnableSwitch => true;
        public override string Editor_FeatureDescription => "Interpolating bones which are kinamtic during standing mode to remove low-fps jitter";

#endif
    }
}