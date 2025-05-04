using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public abstract class RagdollAnimatorFeatureCollisions : RagdollAnimatorFeatureBase
    {
        public virtual bool EnableCollectCollision => false;

        /// <summary>
        /// You need to call base.OnInit( helper ) in order to make RagdollAnimatorFeatureCollisions work properly!
        /// It is configuring collision detection components on the physical dummy to work with.
        /// </summary>
        public override bool OnInit()
        {
            ParentRagdollHandler.PrepareDummyBonesCollisionIndicators( EnableCollectCollision );
            ParentRagdollHandler.AddToDummyBoneCollisionEnterActions( OnCollisionEnterAction );
            return true;
        }

        public override void OnEnableRagdoll()
        {
            ParentRagdollHandler.AddToDummyBoneCollisionEnterActions( OnCollisionEnterAction );
        }

        public override void OnDisableRagdoll()
        {
            ParentRagdollHandler.RemoveFromDummyBoneCollisionEnterActions( OnCollisionEnterAction );
        }

        public override void OnDestroyFeature()
        {
            ParentRagdollHandler.RemoveFromDummyBoneCollisionEnterActions( OnCollisionEnterAction );
        }

        public virtual void OnCollisionEnterAction( RA2BoneCollisionHandler hitted, Collision collision )
        {
            //if( SendOnlyOnFreeFall ) if( FreeFallRagdoll == false ) return;
            //if( SendCollisionEventsTo == null ) return;

            //if( !triedFindingReceiver ) { receiveDetected = SendCollisionEventsTo.GetComponent<IRagdollAnimatorReceiver>(); triedFindingReceiver = true; }
            //if( receiveDetected != null )
            //    receiveDetected.RagdAnim_OnCollisionEnterEvent( c );
            //else
            //    SendCollisionEventsTo.SendMessage( "ERagColl", c, SendMessageOptions.DontRequireReceiver );
        }
    }
}