using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_Receiver : MonoBehaviour, IRagdollAnimator2Receiver
    {
        public void RagdollAnimator2_OnCollisionEnterEvent( RA2BoneCollisionHandler hitted, Collision mainCollision )
        {
            if( mainCollision.transform == null ) return;
            UnityEngine.Debug.Log( hitted.PhysicalBone.name + " received collision from:  " + mainCollision.transform.name + "  with magnitude = " + mainCollision.relativeVelocity.magnitude );
        }

        public void RagdollAnimator2BoneCollision( RA2BoneCollisionHandler hitted )
        {
            if( hitted.LatestEnterCollision.transform == null ) return;
            UnityEngine.Debug.Log( hitted.PhysicalBone.name + " received collision message from:  " + hitted.LatestEnterCollision.transform.name + "  with magnitude = " + hitted.LatestEnterCollision.relativeVelocity.magnitude );
        }
    }
}