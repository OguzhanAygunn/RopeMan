#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [System.Serializable]
    [AddComponentMenu( "" )]
    public class RagdollAnimatorDummyReference : MonoBehaviour
    {
        public MonoBehaviour ParentComponent { get; private set; }
        public bool WasInitialized => ParentComponent != null;

        public RagdollHandler RagdollHandler { get; private set; }

        public void Initialize( MonoBehaviour creator, RagdollHandler handler )
        {
            if( ParentComponent != null ) return;
            ParentComponent = creator;
            RagdollHandler = handler;
        }

        #region Editor Class

#if UNITY_EDITOR

        private void OnEnable()
        {
        }

        private void OnValidate()
        {
        }

        [CustomEditor( typeof( RagdollAnimatorDummyReference ), true )]
        public class RagdollAnimatorDummyReferenceEditor : Editor
        {
            public RagdollAnimatorDummyReference Get
            { get { if( _get == null ) _get = (RagdollAnimatorDummyReference)target; return _get; } }
            private RagdollAnimatorDummyReference _get;

            private void OnEnable()
            {
                FSceneIcons.SetGizmoIconEnabled( Get, false );
            }

            public override void OnInspectorGUI()
            {
                EditorGUILayout.HelpBox( "This component is containing reference to the ragdoll dummy owner", UnityEditor.MessageType.Info );
                serializedObject.Update();
                GUILayout.Space( 4f );
                GUI.enabled = false;
                EditorGUILayout.ObjectField( "Parent Component:", Get.ParentComponent, typeof( MonoBehaviour ), true );
                GUI.enabled = true;
                serializedObject.ApplyModifiedProperties();

                if( Get.ParentComponent )
                {
                    if( GUILayout.Button( "Go to parent component" ) ) { Selection.activeGameObject = Get.ParentComponent.gameObject; }
                }
            }
        }

#endif

        #endregion Editor Class
    }
}