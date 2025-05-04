using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
        [Tooltip( "Can be used to switch using all added extra features ON or OFF" )]
        public bool UseExtraFeatures = true;

        public List<RagdollAnimatorFeatureHelper> ExtraFeatures = new List<RagdollAnimatorFeatureHelper>();

        /// <summary> Adding ragdoll feature, basing on the reference to feature instance </summary>
        public void AddRagdollFeature( RagdollAnimatorFeatureBase featureReference )
        {
            RagdollAnimatorFeatureHelper handler = new RagdollAnimatorFeatureHelper();
            handler.FeatureReference = featureReference;

            if( WasInitialized ) // Runtime add feature
            {
                handler.Init( this );
                if( handler.RuntimeFeature != null && handler.RuntimeFeature.Initialized ) ExtraFeatures.Add( handler );
            }
            else // Editor add feature
            {
                ExtraFeatures.Add( handler );
            }
        }

        /// <summary> [Runtime Method] Adding ragdoll feature, basing on the type </summary>
        public void AddRagdollFeature<T>() where T : RagdollAnimatorFeatureBase
        {
            AddRagdollFeature( RagdollAnimatorFeatureBase.CreateInstance<T>() as T );
        }

        public void RemoveRagdollFeature( RagdollAnimatorFeatureHelper helper )
        {
            helper.DisposeRagdollFeature();
            ExtraFeatures.Remove( helper );
        }

        public T GetExtraFeature<T>() where T : RagdollAnimatorFeatureBase
        {
            for( int i = 0; i < ExtraFeatures.Count; i++ )
            {
                if( ExtraFeatures[i].FeatureReference == null ) continue;
                if( ExtraFeatures[i].FeatureReference is T ) return ExtraFeatures[i].ActiveFeature as T;
            }

            return null;
        }

        public RagdollAnimatorFeatureHelper GetExtraFeatureHelper<T>() where T : RagdollAnimatorFeatureBase
        {
            for( int i = 0; i < ExtraFeatures.Count; i++ )
            {
                if( ExtraFeatures[i].FeatureReference == null ) continue;
                if( ExtraFeatures[i].FeatureReference is T ) return ExtraFeatures[i];
            }

            return null;
        }

        public RagdollAnimatorFeatureHelper GetExtraFeatureHelper(Type type) 
        {
            for( int i = 0; i < ExtraFeatures.Count; i++ )
            {
                if( ExtraFeatures[i].FeatureReference == null ) continue;
                if( ExtraFeatures[i].FeatureReference.GetType() == type ) return ExtraFeatures[i];
            }

            return null;
        }

        public RagdollAnimatorFeatureHelper GetExtraFeatureHelper( string customName )
        {
            for( int i = 0; i < ExtraFeatures.Count; i++ )
                if( ExtraFeatures[i].CustomName == customName ) return ExtraFeatures[i];

            return null;
        }

        protected void CallExtraFeaturesOnInitialize()
        {
            foreach( var feature in ExtraFeatures )
            {
                if( feature == null ) continue;
                if( feature.FeatureReference == null ) continue;
                feature.Init( this );
            }

            for( int i = ExtraFeatures.Count - 1; i >= 0; i-- )
            {
                var feature = ExtraFeatures[i];
                if( feature == null ) continue;
                if( feature.RuntimeFeature == null ) { ExtraFeatures[i].DisposeRagdollFeature(); ExtraFeatures.RemoveAt( i ); continue; }
                if( feature.RuntimeFeature.Initialized == false ) { ExtraFeatures[i].DisposeRagdollFeature(); ExtraFeatures.RemoveAt( i ); }
            }
        }

        protected void CallExtraFeaturesOnEnable()
        {
            if( !UseExtraFeatures ) return;
            foreach( var feature in ExtraFeatures )
            {
                if( feature.Enabled == false ) continue;
                if( feature.FeatureReference == null ) continue;
                feature.RuntimeFeature.OnEnableRagdoll();
            }
        }

        protected void CallExtraFeaturesOnDisable()
        {
            if( !UseExtraFeatures ) return;
            foreach( var feature in ExtraFeatures )
            {
                if( feature.Enabled == false ) continue;
                if( feature.FeatureReference == null ) continue;
                feature.RuntimeFeature.OnDisableRagdoll();
            }
        }

        // ------------------------------   Update Actions

        #region Fall Mode Switch Actions

        private List<Action> OnFallModeSwitchActions = new List<Action>();

        internal void AddToOnFallModeSwitchActions( Action action )
        {
            if( OnFallModeSwitchActions.Contains( action ) == false ) OnFallModeSwitchActions.Add( action );
        }

        internal void RemoveFromOnFallModeSwitchActions( Action action )
        {
            if( OnFallModeSwitchActions.Contains( action ) ) OnFallModeSwitchActions.Remove( action );
        }

        protected void CallOnFallModeSwitchActions()
        {
            if( !UseExtraFeatures ) return; foreach( var action in OnFallModeSwitchActions ) action.Invoke();
        }

        #endregion Fall Mode Switch Actions

        #region Always Update Actions

        private List<Action> AlwaysUpdateActions = new List<Action>();

        /// <summary> Always update loop is called event when ragdoll animator is optimizing update loops </summary>
        internal void AddToAlwaysUpdateLoop( Action action )
        {
            if( AlwaysUpdateActions.Contains( action ) == false ) AlwaysUpdateActions.Add( action );
        }

        /// <summary> Always update loop is called event when ragdoll animator is optimizing update loops </summary>
        internal void RemoveFromAlwaysUpdateLoop( Action action )
        {
            if( AlwaysUpdateActions.Contains( action ) ) AlwaysUpdateActions.Remove( action );
        }

        /// <summary> Always update loop is called event when ragdoll animator is optimizing update loops </summary>
        protected void CallExtraFeaturesAlwaysUpdateLoops()
        {
            if( !UseExtraFeatures ) return;
            foreach( var action in AlwaysUpdateActions ) action.Invoke();
        }

        #endregion Always Update Actions

        #region Update Actions

        private List<Action> UpdateActions = new List<Action>();

        internal void AddToUpdateLoop( Action action )
        {
            if( UpdateActions.Contains( action ) == false ) UpdateActions.Add( action );
        }

        internal void RemoveFromUpdateLoop( Action action )
        {
            if( UpdateActions.Contains( action ) ) UpdateActions.Remove( action );
        }

        protected void CallExtraFeaturesUpdateLoops()
        {
            if( !UseExtraFeatures ) return;
            foreach( var action in UpdateActions ) action.Invoke();
        }

        #endregion Update Actions

        #region Pre Late Update Actions

        private List<Action> PreLateUpdateActions = new List<Action>();

        internal void AddToPreLateUpdateLoop( Action action )
        {
            if( PreLateUpdateActions.Contains( action ) == false ) PreLateUpdateActions.Add( action );
        }

        internal void RemoveFromPreLateUpdateLoop( Action action )
        {
            if( PreLateUpdateActions.Contains( action ) ) PreLateUpdateActions.Remove( action );
        }

        protected void CallExtraFeaturesPreLateUpdateLoops()
        {
            if( !UseExtraFeatures ) return;
            foreach( var action in PreLateUpdateActions ) action.Invoke();
        }

        #endregion Pre Late Update Actions

        #region Late Update Actions

        private List<Action> LateUpdateActions = new List<Action>();

        internal void AddToLateUpdateLoop( Action action )
        {
            if( LateUpdateActions.Contains( action ) == false ) LateUpdateActions.Add( action );
        }

        internal void RemoveFromLateUpdateLoop( Action action )
        {
            if( LateUpdateActions.Contains( action ) ) LateUpdateActions.Remove( action );
        }

        protected void CallExtraFeaturesLateUpdateLoops()
        {
            if( !UseExtraFeatures ) return;
            foreach( var action in LateUpdateActions ) action.Invoke();
        }

        #endregion Late Update Actions

        #region Post Late Update Actions

        private List<Action> PostLateUpdateActions = new List<Action>();

        internal void AddToPostLateUpdateLoop( Action action )
        {
            if( PostLateUpdateActions.Contains( action ) == false ) PostLateUpdateActions.Add( action );
        }

        internal void RemoveFromPostLateUpdateLoop( Action action )
        {
            if( PostLateUpdateActions.Contains( action ) ) PostLateUpdateActions.Remove( action );
        }

        protected void CallExtraFeaturesPostLateUpdateLoops()
        {
            if( !UseExtraFeatures ) return;
            foreach( var action in PostLateUpdateActions ) action.Invoke();
        }

        #endregion Post Late Update Actions

        #region Fixed Update Actions

        private List<Action> FixedUpdateActions = new List<Action>();

        internal void AddToFixedUpdateLoop( Action action )
        {
            if( FixedUpdateActions.Contains( action ) == false ) FixedUpdateActions.Add( action );
        }

        internal void RemoveFromFixedUpdateLoop( Action action )
        {
            if( FixedUpdateActions.Contains( action ) ) FixedUpdateActions.Remove( action );
        }

        protected void CallExtraFeaturesFixedUpdateLoops()
        {
            if( !UseExtraFeatures ) return;
            foreach( var action in FixedUpdateActions ) action.Invoke();
        }

        #endregion Fixed Update Actions

        #region On Collision Actions

        private List<Action<RA2BoneCollisionHandler, Collision>> OnCollisionEnterActions = new List<Action<RA2BoneCollisionHandler, Collision>>();

        internal void AddToDummyBoneCollisionEnterActions( Action<RA2BoneCollisionHandler, Collision> action )
        {
            if( OnCollisionEnterActions.Contains( action ) == false ) OnCollisionEnterActions.Add( action );
        }

        internal void RemoveFromDummyBoneCollisionEnterActions( Action<RA2BoneCollisionHandler, Collision> action )
        {
            if( OnCollisionEnterActions.Contains( action ) ) OnCollisionEnterActions.Remove( action );
        }

        #endregion On Collision Actions

        #region On Collision Trigger Actions

        private List<Action<RA2BoneTriggerCollisionHandler, Collider>> OnTriggerEnterActions = new List<Action<RA2BoneTriggerCollisionHandler, Collider>>();

        internal void AddToTriggerEnterActions( Action<RA2BoneTriggerCollisionHandler, Collider> action )
        {
            if( OnTriggerEnterActions.Contains( action ) == false ) OnTriggerEnterActions.Add( action );
        }

        internal void RemoveFromDummyBoneCollisionEnterActions( Action<RA2BoneTriggerCollisionHandler, Collider> action )
        {
            if( OnTriggerEnterActions.Contains( action ) ) OnTriggerEnterActions.Remove( action );
        }

        #endregion On Collision Trigger Actions
    }
}