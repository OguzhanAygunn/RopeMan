#if UNITY_EDITOR

using UnityEditor;

#endif

using System;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public abstract class RagdollAnimatorFeatureBase : ScriptableObject
    {
        protected Transform Transform
        { get { return Owner.BaseTransform; } }
        public RagdollHandler ParentRagdollHandler
        { get { return Owner; } }
        public RagdollAnimatorFeatureHelper Helper
        { get { return InitializedWith; } }
        [field: NonSerialized] protected RagdollHandler Owner { get; private set; } = null;
        [field: NonSerialized] protected RagdollAnimatorFeatureHelper InitializedWith { get; private set; } = null;

        public bool Initialized { get; private set; } = false;

        /// <summary> If feature script implements it, use this value to fade off module influence </summary>
        public float FeatureBlend { get; set; }

        public void Base_Init( RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            FeatureBlend = 1f;
            InitializedWith = helper;
            Owner = ragdollHandler;
            if( OnInit() ) Initialized = true;
        }

        /// <summary> [return true if initialized properly, false when initialization fails] Called when Ragdoll Animator is completing initializing physicall ragdoll dummy </summary>
        public virtual bool OnInit()
        { return true; }

        /// <summary> [Base method does nothing] Special call, to update some of the settings only when big changes are happening. (called every change in the inspector window but needs to be called manually if editing settings through code) </summary>
        //public virtual void OnValidateAfterManualChanges() { }

        /// <summary> [Base method does nothing] Called when feature is removed from the ragdoll during runtime </summary>
        //public virtual void OnRemoveFeature() { }

        /// <summary> [Base method does nothing] Called when parent ragdoll handler gets disabled during runtime </summary>
        public virtual void OnDisableRagdoll()
        { }

        /// <summary> [Base method does nothing] Called when parent ragdoll handler gets enabled after being disabled, during runtime </summary>
        public virtual void OnEnableRagdoll()
        { }

        /// <summary> [Base method does nothing] Called when parent ragdoll handler animation mode changes. For example when changing from standing mode to falling mode. </summary>
        //public virtual void OnSwitchAnimatingMode() { }

        /// <summary> [Base method does nothing] Called when feature gets removed from the ragdoll controller </summary>
        public virtual void OnDestroyFeature()
        { }

        /// <summary> [Base method does nothing] Called when toggling enabled value </summary>
        public virtual void OnEnabledSwitch()
        { }

        #region Editor Code

#if UNITY_EDITOR

        [System.NonSerialized] public SerializedObject BaseSerializedObject = null;
        [System.NonSerialized] public bool Editor_Foldout = false;
        [System.NonSerialized] public bool Editor_PlaymodeFoldout = false;
        public virtual bool Editor_DisplayEnableSwitch => true;

        public virtual string Editor_FeatureDescription => "";

        /// <summary> [Base method does nothing] </summary>
        public virtual void Editor_OnSceneGUI( RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        { }

        /// <summary> [Base method does nothing] </summary>
        public virtual void Editor_InspectorGUI( SerializedProperty handlerProp, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        { }

        /// <summary> [Base method does nothing] When removing feature by clicking remove button or setting feature reference as null </summary>
        public virtual void Editor_OnRemoveFeatureInEditorGUI( RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        { }

        static RagdollAnimatorFeatureHelper _copyFeature = null;

        /// <summary> Saving reference for copying </summary>
        public static void Editor_CopyFeature( RagdollAnimatorFeatureHelper featureHandler )
        {
            _copyFeature = featureHandler;
        }

        public static RagdollAnimatorFeatureHelper Editor_HasCopyFeatureReference()
        {
            if( _copyFeature != null && _copyFeature.FeatureReference != null ) return _copyFeature;
            return null;
        }

        public static bool Editor_HasFeatureToPasteClipboardSettings( RagdollAnimatorFeatureHelper featureHandler )
        {
            if( featureHandler == null ) return false;
            if( _copyFeature == null ) return false;
            if( featureHandler.FeatureReference == _copyFeature.FeatureReference ) return true;
            return false;
        }

        public static void Editor_PasteFeatures( RagdollAnimatorFeatureHelper targetHelper )
        {
            if( targetHelper == null ) return;
            if( _copyFeature == null ) return;
            if( _copyFeature.FeatureReference == null) return;
            if( targetHelper.FeatureReference == null ) targetHelper.FeatureReference = _copyFeature.FeatureReference;
            _copyFeature.CopySettingsFrom( targetHelper );
        }


        static RagdollHandler _copyFeaturesFrom = null;
        /// <summary> Saving reference for copying </summary>
        public static void Editor_CopyFeaturesSetup( RagdollHandler handler )
        {
            _copyFeaturesFrom = handler;
        }

        public static void Editor_PasteFeaturesSetup( RagdollHandler target )
        {
            if( _copyFeaturesFrom == null ) return;

            foreach( var feature in _copyFeaturesFrom.ExtraFeatures )
            {
                if( feature.FeatureReference == null ) continue;

                var oFeature = target.GetExtraFeatureHelper( feature.FeatureReference.GetType() ); // Get same type feature

                if( oFeature == null ) // Not found feature of same type
                {
                    // Add new feature of target type
                    target.ExtraFeatures.Add( new RagdollAnimatorFeatureHelper() );
                    oFeature.Enabled = feature.Enabled;
                    oFeature = target.ExtraFeatures[target.ExtraFeatures.Count - 1];
                    oFeature.FeatureReference = feature.FeatureReference;
                    oFeature.CustomName = feature.CustomName;
                }

                oFeature.CopySettingsFrom( feature );
            }
        }

        /// <summary> Returning reference to handler which features setup want to be copied from </summary>
        public static RagdollHandler IsPasteFeaturesSetupPossible( RagdollHandler handler )
        {
            if( _copyFeaturesFrom == handler ) return null;
            return _copyFeaturesFrom;
        }

#endif

        #endregion Editor Code
    }
}