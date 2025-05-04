using FIMSpace.FGenerating;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FIMSpace.FProceduralAnimation
{
    [System.Serializable]
    public class RagdollAnimatorFeatureHelper
    {
        [Tooltip( "Displaying this name instead of feature class name in GUI + giving possibility to get feature from the ragdoll handler using this name." )]
        [HideInInspector] public string CustomName = "";

        public bool Enabled
        { get { return enabled; } set { if( enabled != value ) { enabled = value; if( RuntimeFeature ) RuntimeFeature.OnEnabledSwitch(); } } }
        [SerializeField, HideInInspector] private bool enabled = true;
        public RagdollHandler ParentRagdollHandler => handler;

        /// <summary> Can't be serializable variables since RagdollHandler is [Serializable] instance of MonoBehaviour </summary>
        [NonSerialized] private RagdollHandler handler;

        public RagdollAnimatorFeatureBase FeatureReference = null;
        [field: NonSerialized] public RagdollAnimatorFeatureBase RuntimeFeature { get; private set; } = null;

        #region Get Module

        /// <summary> Returning runtime feature at runtime and feature reference during editor mode </summary>
        public RagdollAnimatorFeatureBase ActiveFeature
        {
            get
            {
#if UNITY_EDITOR
                if( Application.isPlaying ) return RuntimeFeature; else return FeatureReference;
#else
                    return RuntimeFeature;
#endif
            }
        }

        #endregion Get Module

        internal void Init( RagdollHandler handler )
        {
            this.handler = handler;
            if( FeatureReference == null ) return;
            PreparePlaymodeModule( handler );
        }

        /// <summary> Can be used for containing any parasable value or just strings </summary>
        [SerializeField, HideInInspector] public List<string> customStringList = null;

        /// <summary> Support for list of unity objects </summary>
        [SerializeField, HideInInspector] public List<UnityEngine.Object> customObjectList = null;

        [SerializeField, HideInInspector] public List<UnityEvent> customEventsList = null;

        public void PreparePlaymodeModule( RagdollHandler parent )
        {
            if( RuntimeFeature != null ) return; // Already initialized
            if( FeatureReference == null ) return;

            RuntimeFeature = ScriptableObject.Instantiate( FeatureReference );
            RuntimeFeature.Base_Init( parent, this );
        }

        public void DisposeRagdollFeature()
        {
            if( RuntimeFeature != null )
            {
                RuntimeFeature.OnDestroyFeature();
                ScriptableObject.Destroy( RuntimeFeature );
            }

            RuntimeFeature = null;
        }

        [SerializeField] private List<FUniversalVariable> variables = new List<FUniversalVariable>();

        public FUniversalVariable RequestVariable( string name, object defaultValue )
        {
            if( variables == null ) variables = new List<FUniversalVariable>();

            int hash = name.GetHashCode();
            for( int i = 0; i < variables.Count; i++ )
            {
                if( variables[i].GetNameHash == hash ) return variables[i];
            }

            FUniversalVariable nVar = new FUniversalVariable( name, defaultValue );
            variables.Add( nVar );
            return nVar;
        }

        public bool HasVariable( string name )
        {
            if( variables == null ) return false;
            int hash = name.GetHashCode();

            for( int i = 0; i < variables.Count; i++ )
                if( variables[i].GetNameHash == hash ) return true;

            return false;
        }

        /// <summary> Copying variables from one feature to another - only same type features </summary>
        public void CopySettingsFrom( RagdollAnimatorFeatureHelper copyFrom )
        {
            if( copyFrom == null ) return;
            if( copyFrom.FeatureReference == null ) return;
            if( FeatureReference == null ) { FeatureReference = copyFrom.FeatureReference; }
            if( FeatureReference.GetType() != copyFrom.FeatureReference.GetType() ) return;

            foreach( var variable in copyFrom.variables )
            {
                RequestVariable( variable.VariableName, variable.GetValue() );
            }
        }


        #region Editor Code

#if UNITY_EDITOR

        public void Editor_AssignHandler( RagdollHandler handler )
        {
            this.handler = handler;
        }

        public void Editor_RenamePopup()
        {
            string startName = CustomName;
            string filename = UnityEditor.EditorUtility.SaveFilePanelInProject( "Type new name (no file will be created)", startName, "", "Type new name (no file will be created)" );

            if( !string.IsNullOrEmpty( filename ) )
            {
                filename = System.IO.Path.GetFileNameWithoutExtension( filename );
                if( !string.IsNullOrEmpty( filename ) ) CustomName = filename;
            }
        }

        [NonSerialized] public string formattedName = "";//

#endif

        #endregion Editor Code
    }
}