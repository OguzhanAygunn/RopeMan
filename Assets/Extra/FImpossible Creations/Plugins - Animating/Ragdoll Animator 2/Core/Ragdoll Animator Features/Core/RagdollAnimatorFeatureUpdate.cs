namespace FIMSpace.FProceduralAnimation
{
    /// <summary>
    /// Base for ragdoll feature which requires update loops to work
    /// </summary>
    public abstract class RagdollAnimatorFeatureUpdate : RagdollAnimatorFeatureBase
    {
        /// <summary> Used during initialization, adding this feature Update loop call to the ragdoll handler </summary>
        public virtual bool UseUpdate => false;

        /// <summary> Used during initialization, adding this feature LateUpdate loop call to the ragdoll handler </summary>
        public virtual bool UseLateUpdate => false;

        /// <summary> Used during initialization, adding this feature FixedUpdate loop call to the ragdoll handler </summary>
        public virtual bool UseFixedUpdate => false;

        /// <summary>
        /// You need to call base.OnInit( helper ) in order to make RagdollAnimatorFeatureUpdate work properly!
        /// It is initializing update events to call on the ragdoll handler.
        /// </summary>
        public override bool OnInit()
        {
            if( UseUpdate ) ParentRagdollHandler.AddToUpdateLoop( Update );
            if( UseLateUpdate ) ParentRagdollHandler.AddToLateUpdateLoop( LateUpdate );
            if( UseFixedUpdate ) ParentRagdollHandler.AddToFixedUpdateLoop( FixedUpdate );
            return true;
        }

        /// <summary> [Base method does nothing] Requires override for UseUpdate = true to call this method </summary>
        public virtual void Update()
        {
        }

        /// <summary> [Base method does nothing] Requires override for UseLateUpdate = true to call this method </summary>
        public virtual void LateUpdate()
        {
        }

        /// <summary> [Base method does nothing] Requires override for UseFixedUpdate = true to call this method </summary>
        public virtual void FixedUpdate()
        {
        }

        /// <summary> Removing used loops from the parent ragdoll handler </summary>
        public override void OnDestroyFeature()
        {
            if( UseUpdate ) ParentRagdollHandler.RemoveFromUpdateLoop( Update );
            if( UseLateUpdate ) ParentRagdollHandler.RemoveFromLateUpdateLoop( LateUpdate );
            if( UseFixedUpdate ) ParentRagdollHandler.RemoveFromFixedUpdateLoop( FixedUpdate );
        }
    }
}