#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [DefaultExecutionOrder( 50 )]
    [AddComponentMenu( "FImpossible Creations/Ragdoll Animator/Set Joint Connection Body", 111 )]
    public class RA2SetJointConnectedBody : MonoBehaviour
    {
        [Tooltip( "Reading physical dummy bones out of the ragdoll animator" )]
        [HideInInspector] public GameObject ObjectWithRagdollAnimator;

        [Tooltip( "Transform with rigidbody to assign as 'ConnectedBody' of selected joint" )]
        [HideInInspector] public Transform ToAttach;

        [Tooltip( "Joint to change its 'ConnectedBody' reference" )]
        public Joint TargetJoint;

        private IRagdollAnimator2HandlerOwner handler;

        private void FixedUpdate()
        {
            if( TargetJoint == null ) { enabled = false; return; }
            if( ObjectWithRagdollAnimator == null && ToAttach == null ) { enabled = false; return; }

            if( ObjectWithRagdollAnimator != null )
            {
                handler = ObjectWithRagdollAnimator.GetComponent<IRagdollAnimator2HandlerOwner>();
                if( handler == null ) { handler = GetComponent<IRagdollAnimator2HandlerOwner>(); ObjectWithRagdollAnimator = gameObject; }
            }

            if( handler == null )
            {
                if( ToAttach == null ) { enabled = false; return; }
                else
                if( ToAttach.GetComponent<Rigidbody>() == null ) { enabled = false; return; }
            }
            else
            {
                ToAttach = RagdollHandlerUtilities.User_GetBoneSetupBySourceAnimatorBone( handler.GetRagdollHandler, ToAttach ).PhysicalDummyBone;
            }

            if( ToAttach == null ) { enabled = false; return; } // No target to attach

            Rigidbody rig = ToAttach.GetComponent<Rigidbody>();
            if( rig == null ) rig = ToAttach.GetComponentInChildren<Rigidbody>();
            if( rig == null ) { enabled = false; return; }

            TargetJoint.connectedBody = rig;
            enabled = false;
        }

        #region Editor Code

#if UNITY_EDITOR

        private bool gizmoDisableCalled = false;

        private void OnValidate()
        {
            if( !gizmoDisableCalled ) { FSceneIcons.SetGizmoIconEnabled( this, false ); gizmoDisableCalled = true; }
        }

        [CanEditMultipleObjects]
        [CustomEditor( typeof( RA2SetJointConnectedBody ), true )]
        public class RA2SetJointConnectionEditor : Editor
        {
            public RA2SetJointConnectedBody Get
            { get { if( _get == null ) _get = (RA2SetJointConnectedBody)target; return _get; } }
            private RA2SetJointConnectedBody _get;

            protected virtual string HeaderInfo => "Assigning runtime generated physical bone rigidbody to the target joint 'Connected Body'.";

            private SerializedProperty sp_ObjectWithRagdollAnimator;

            private void OnEnable()
            {
                sp_ObjectWithRagdollAnimator = serializedObject.FindProperty( "ObjectWithRagdollAnimator" );
            }

            public override void OnInspectorGUI()
            {
                UnityEditor.EditorGUILayout.HelpBox( HeaderInfo, UnityEditor.MessageType.Info );

                serializedObject.Update();

                GUILayout.Space( 4f );

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField( sp_ObjectWithRagdollAnimator );

                if( Get.ObjectWithRagdollAnimator )
                {
                    RagdollHandler.Editor_RagdollBonesSelector( Get.ObjectWithRagdollAnimator, ( Transform t ) => { Get.ToAttach = t; }, Get.ToAttach );
                }

                EditorGUILayout.EndHorizontal();

                var spc = sp_ObjectWithRagdollAnimator.Copy(); spc.Next( false );
                EditorGUILayout.PropertyField( spc );

                GUILayout.Space( 4f );
                DrawPropertiesExcluding( serializedObject, "m_Script" );

                serializedObject.ApplyModifiedProperties();
            }
        }

#endif

        #endregion Editor Code
    }
}