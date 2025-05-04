#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [DefaultExecutionOrder( 50 )]
    [AddComponentMenu( "FImpossible Creations/Ragdoll Animator/Ragdoll Bone as Parent", 111 )]
    public class RA2DummyBoneAsParent : MonoBehaviour
    {
        [Tooltip( "Reading physical dummy bones out of the ragdoll animator" )]
        public GameObject ObjectWithRagdollAnimator;

        [Space( 5 )]
        [Tooltip( "Transform with rigidbody to assign as 'ConnectedBody' of selected joint" )]
        [HideInInspector] public Transform TargetParent;

        [HideInInspector] public Vector3 LocalPosition = Vector3.zero;
        [HideInInspector] public Vector3 LocalRotation = Vector3.zero;

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

            transform.SetParent( rig.transform, true );
            transform.localPosition = LocalPosition;
            transform.localRotation = Quaternion.Euler( LocalRotation );

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
        [CustomEditor( typeof( RA2DummyBoneAsParent ), true )]
        public class RA2DummyBoneAsParentEditor : Editor
        {
            public RA2DummyBoneAsParent Get
            { get { if( _get == null ) _get = (RA2DummyBoneAsParent)target; return _get; } }
            private RA2DummyBoneAsParent _get;

            protected virtual string HeaderInfo => "Changing this object parent to be runtime generated physical dummy bone.";

            private SerializedProperty sp_TargetParent;

            private void OnEnable()
            {
                sp_TargetParent = serializedObject.FindProperty( "TargetParent" );
            }

            public override void OnInspectorGUI()
            {
                if( Get.enabled == false && Application.isPlaying )
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

                GUILayout.Space( 6f );

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField( sp ); sp.Next( false );

                if( Get.TargetParent != null ) if( GUILayout.Button( new GUIContent( "C", "Saving current world position as target local position in neew parent space" ), GUILayout.Width( 24 ) ) )
                    {
                        Get.LocalPosition = Get.TargetParent.InverseTransformPoint( Get.transform.position );
                        Get.LocalRotation = FEngineering.QToLocal( Get.TargetParent.rotation, Get.transform.rotation ).eulerAngles;
                    }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField( sp );
                EditorGUIUtility.labelWidth = 0;

                serializedObject.ApplyModifiedProperties();
            }
        }

#endif

        #endregion Editor Code
    }
}