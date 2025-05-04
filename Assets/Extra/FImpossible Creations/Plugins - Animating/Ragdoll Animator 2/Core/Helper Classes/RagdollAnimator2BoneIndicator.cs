#if UNITY_EDITOR

using System;
using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [AddComponentMenu( "", 0 )]
    public class RagdollAnimator2BoneIndicator : MonoBehaviour
    {
        public RagdollHandler ParentHandler { get; private set; }

        /// <summary> Same as .ParentHandler </summary>
        public RagdollHandler ParentRagdollProcessor
        { get { return ParentHandler; } }

        /// <summary> If using custom ragdoll handler, it will be null </summary>
        public RagdollAnimator2 ParentRagdollAnimator
        { get { return ParentHandler.Caller as RagdollAnimator2; } }

        public RagdollBoneProcessor RagdollBoneProcessor { get; private set; }
        public Rigidbody DummyBoneRigidbody
        { get { return RagdollBoneProcessor.rigidbody; } }

        /// <summary> Physical ragdoll dummy bone</summary>
        public Transform PhysicalBone => RagdollBoneProcessor.BoneSetup.PhysicalDummyBone;

        /// <summary> Source animator skeleton bone </summary>
        public Transform SourceBone => RagdollBoneProcessor.BoneSetup.SourceBone;

#if UNITY_EDITOR
        [field: NonSerialized] 
#endif
        public RagdollChainBone BoneSettings { get; private set; }

        /// <summary> Reference to the ragdoll attachable object if this collider is related with some attachable item </summary>
        public RA2AttachableObject AttachableObject { get; private set; }

        /// <summary> Assigned only if using humanoid rig </summary>
        public ERagdollBoneID BodyBoneID { get; private set; } = ERagdollBoneID.Unknown;

#if UNITY_EDITOR
        [field: NonSerialized]
#endif
        public RagdollBonesChain ParentChain { get; private set; }
        public ERagdollChainType ChainType => ParentChain.ChainType;

        /// <summary> True when it's indicator of non physical skeleton </summary>
        public bool IsAnimatorBone { get; private set; }

        /// <summary> Only source animator bones are marked as IsAnimatorBoneReference </summary>
        public bool IsAnimatorBoneReference { get; private set; }

        internal void MarkAsAnimatorBone() => IsAnimatorBoneReference = true;

        public virtual RagdollAnimator2BoneIndicator Initialize( RagdollHandler handler, RagdollBoneProcessor boneProcessor, RagdollBonesChain parentChain, bool isAnimatorBone = false, RA2AttachableObject attachable = null )
        {
            ParentHandler = handler;
            BodyBoneID = ERagdollBoneID.Unknown;
            RagdollBoneProcessor = boneProcessor;
            if( boneProcessor != null ) BoneSettings = boneProcessor.BoneSetup;

            IsAnimatorBone = isAnimatorBone;
            AttachableObject = attachable;

            ParentChain = parentChain;

            if( boneProcessor != null )
            {
                BodyBoneID = boneProcessor.BoneSetup.BoneID;
                boneProcessor.IndicatorComponent = this;
            }

            return this;
        }

#region Editor Class

#if UNITY_EDITOR

        [UnityEditor.CanEditMultipleObjects]
        [UnityEditor.CustomEditor( typeof( RagdollAnimator2BoneIndicator ), true )]
        public class RagdollAnimator2BoneIndicatorEditor : UnityEditor.Editor
        {
            public RagdollAnimator2BoneIndicator baseGet
            { get { if( _baseget == null ) _baseget = (RagdollAnimator2BoneIndicator)target; return _baseget; } }
            private RagdollAnimator2BoneIndicator _baseget;

            public virtual string Description => "Holding reference to the animator bone and to the physical bone.";

            private void OnEnable()
            {
                FSceneIcons.SetGizmoIconEnabled( serializedObject.targetObject as MonoBehaviour, false );
            }

            public override void OnInspectorGUI()
            {
                if( Description != "" )
                {
                    GUILayout.Space( 4f );
                    EditorGUILayout.HelpBox( Description, UnityEditor.MessageType.None );
                    GUILayout.Space( 4f );
                }

                serializedObject.Update();
                DrawPropertiesExcluding( serializedObject, "m_Script" );
                serializedObject.ApplyModifiedProperties();

                if( baseGet.RagdollBoneProcessor != null )
                {
                    GUI.enabled = false;

                    if( baseGet.ParentRagdollAnimator )
                    {
                        EditorGUILayout.ObjectField( "Parent Ragdoll Animator:", baseGet.ParentRagdollAnimator, typeof( RagdollAnimator2 ), true );
                    }

                    GUILayout.Space( 4f );

                    if( baseGet.AttachableObject )
                    {
                        EditorGUILayout.ObjectField( "Attachable Reference:", baseGet.AttachableObject, typeof( RA2AttachableObject ), true );
                        GUILayout.Space( 2f );
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUIUtility.labelWidth = 64;
                    EditorGUILayout.ObjectField( "Source:", baseGet.RagdollBoneProcessor.BoneSetup.SourceBone, typeof( Transform ), true );
                    GUILayout.Space( 8f );
                    EditorGUILayout.ObjectField( "Physical:", baseGet.RagdollBoneProcessor.BoneSetup.PhysicalDummyBone, typeof( Transform ), true );
                    EditorGUIUtility.labelWidth = 0;
                    EditorGUILayout.EndHorizontal();

                    if( baseGet.BodyBoneID != ERagdollBoneID.Unknown )
                    {
                        EditorGUILayout.EnumPopup( baseGet.BodyBoneID );
                    }

                    if( baseGet.ParentChain != null )
                    {
                        EditorGUILayout.LabelField( "Parent Chain: " + baseGet.ParentChain.ChainName + " : " + baseGet.ParentChain.ChainType );
                    }

                    GUI.enabled = true;
                }
            }
        }

#endif

#endregion Editor Class
    }
}