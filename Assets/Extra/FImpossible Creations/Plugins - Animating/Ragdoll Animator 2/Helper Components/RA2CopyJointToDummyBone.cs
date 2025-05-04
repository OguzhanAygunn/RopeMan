#if UNITY_EDITOR

using UnityEditor;

#endif

using System;
using System.Reflection;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [DefaultExecutionOrder( 50 )]
    [AddComponentMenu( "FImpossible Creations/Ragdoll Animator/Transfer Joint To Ragdoll Bone", 111 )]
    public class RA2CopyJointToDummyBone : MonoBehaviour
    {
        public Joint ToCopy;
        public bool DestroyObjectAfterCopying = false;

        [Space( 3 )]
        [Tooltip( "Reading physical dummy bones out of the ragdoll animator" )]
        public GameObject ObjectWithRagdollAnimator;

        [Space( 5 )]
        [Tooltip( "Transform with rigidbody to assign as 'ConnectedBody' of selected joint" )]
        [HideInInspector] public Transform TargetParent;

        private IRagdollAnimator2HandlerOwner handler;

        private void FixedUpdate()
        {
            if( ObjectWithRagdollAnimator == null && TargetParent == null ) { enabled = false; return; }

            if( ObjectWithRagdollAnimator != null )
            {
                handler = ObjectWithRagdollAnimator.GetComponent<IRagdollAnimator2HandlerOwner>();
                if( handler == null ) { handler = GetComponent<IRagdollAnimator2HandlerOwner>(); ObjectWithRagdollAnimator = gameObject; }
            }

            if( handler == null )
            {
                if( TargetParent == null ) { enabled = false; return; }
                else
                if( TargetParent.GetComponent<Rigidbody>() == null ) { enabled = false; return; }
            }
            else
            {
                TargetParent = handler.GetRagdollHandler.User_GetBoneSetupBySourceAnimatorBone( TargetParent ).PhysicalDummyBone;
            }

            if( TargetParent == null ) { enabled = false; return; } // No target to attach

            Rigidbody rig = TargetParent.GetComponent<Rigidbody>();
            if( rig == null ) rig = TargetParent.GetComponentInChildren<Rigidbody>();
            if( rig == null ) { enabled = false; return; }

            var joint = GetCopyOf( TargetParent.gameObject.AddComponent( ToCopy.GetType() ), ToCopy ) as Joint;
            joint.connectedBody = ToCopy.connectedBody;
            joint.autoConfigureConnectedAnchor = ToCopy.autoConfigureConnectedAnchor;

            enabled = false;
            if( DestroyObjectAfterCopying ) GameObject.Destroy( gameObject );
        }

        public static T GetCopyOf<T>( Component comp, T other ) where T : Component
        {
            Type type = comp.GetType();
            if( type != other.GetType() ) return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties( flags );
            foreach( var pinfo in pinfos )
            {
                if( pinfo.CanWrite )
                {
                    try
                    {
                        pinfo.SetValue( comp, pinfo.GetValue( other, null ), null );
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields( flags );
            foreach( var finfo in finfos )
            {
                finfo.SetValue( comp, finfo.GetValue( other ) );
            }
            return comp as T;
        }

        #region Editor Code

#if UNITY_EDITOR

        private bool gizmoDisableCalled = false;

        private void OnValidate()
        {
            if( !gizmoDisableCalled ) { FSceneIcons.SetGizmoIconEnabled( this, false ); gizmoDisableCalled = true; }
        }

        [CanEditMultipleObjects]
        [CustomEditor( typeof( RA2CopyJointToDummyBone ), true )]
        public class RA2CopyJointToDummyBoneEditor : Editor
        {
            public RA2CopyJointToDummyBone Get
            { get { if( _get == null ) _get = (RA2CopyJointToDummyBone)target; return _get; } }
            private RA2CopyJointToDummyBone _get;

            protected virtual string HeaderInfo => "Copying joint into ragdoll dummy bone without need of pre-generating dummy.";

            private SerializedProperty sp_TargetParent;

            private void OnEnable()
            {
                sp_TargetParent = serializedObject.FindProperty( "TargetParent" );
            }

            public override void OnInspectorGUI()
            {
                if( Get.enabled == false && UnityEngine.Application.isPlaying )
                    UnityEditor.EditorGUILayout.HelpBox( "Being disabled after doing its job (intended behaviour)", UnityEditor.MessageType.None );
                else
                    UnityEditor.EditorGUILayout.HelpBox( HeaderInfo, UnityEditor.MessageType.Info );

                serializedObject.Update();

                GUILayout.Space( 4f );
                DrawPropertiesExcluding( serializedObject, "m_Script" );

                GUILayout.Space( 4f );
                var sp = sp_TargetParent.Copy();
                EditorGUIUtility.labelWidth = 106;

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField( sp ); sp.Next( false );

                if( Get.ObjectWithRagdollAnimator )
                {
                    GUILayout.Space( 4f );
                    RagdollHandler.Editor_RagdollBonesSelector( Get.ObjectWithRagdollAnimator, ( Transform t ) => { Get.TargetParent = t; }, Get.TargetParent );
                }

                EditorGUILayout.EndHorizontal();

                serializedObject.ApplyModifiedProperties();
            }
        }

#endif

        #endregion Editor Code
    }
}