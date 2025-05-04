using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    /// <summary>
    /// Helper interface to identify ragdoll handler within any type of mono behaviour
    /// </summary>
    public interface IRagdollAnimator2HandlerOwner
    {
        RagdollHandler GetRagdollHandler { get; }
    }

    /// <summary>
    /// Implement it on some MonoBehaviour to call ragdoll animator bones collision events
    /// </summary>
    public interface IRagdollAnimator2Receiver
    {
        void RagdollAnimator2_OnCollisionEnterEvent( RA2BoneCollisionHandler hitted, Collision mainCollision );
    }
}