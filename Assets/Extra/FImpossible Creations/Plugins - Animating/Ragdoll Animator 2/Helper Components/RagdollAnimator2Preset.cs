#if UNITY_EDITOR
using FIMSpace.FEditor;
using UnityEditor;
#endif
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [CreateAssetMenu(fileName = "Ragdoll Animator 2 Preset", menuName = "FImpossible Creations/Ragdoll Animator 2 Preset", order = 10)]
    public class RagdollAnimator2Preset : ScriptableObject
    {
        public RagdollHandler Settings = new RagdollHandler();

        #region Editor Class

#if UNITY_EDITOR

        [CanEditMultipleObjects]
        [CustomEditor(typeof(RagdollAnimator2Preset), true)]
        public class RagdollAnimator2PresetEditor : Editor
        {
            public RagdollAnimator2Preset Get { get { if (_get == null) _get = (RagdollAnimator2Preset)target; return _get; } }
            private RagdollAnimator2Preset _get;
            bool? loadSettings = false;

            public override void OnInspectorGUI()
            {
                EditorGUILayout.HelpBox("Ragdoll Animator 2 Settings which can be applied on any other ragdoll animator 2 component", UnityEditor.MessageType.Info);

                #region Load Settings Button
                
                var mRect = GUILayoutUtility.GetLastRect();

                mRect.y += mRect.height + 6;
                mRect.height = 18;
                mRect.x += mRect.width - 140;
                mRect.width = 130;

                if (loadSettings == false)
                {
                    if (GUI.Button(mRect, new GUIContent("Overwrite Settings", "Click to enable field with ragdoll animator 2 reference to drag to change settings of this preset file as dragged ragdoll animator object."))) loadSettings = true;
                }
                else if (loadSettings == true)
                {
                    RagdollAnimator2 ragd = null;
                    EditorGUIUtility.labelWidth = 34;
                    ragd = EditorGUI.ObjectField(mRect, new GUIContent("Get:", "Select ragdoll animator 2 reference to change settings of this preset file as selected ragdoll animator."), ragd, typeof(RagdollAnimator2), true) as RagdollAnimator2;
                    EditorGUIUtility.labelWidth = 0;
                    
                    if (ragd)
                    {
                        loadSettings = null;
                        ragd.Settings.ApplyAllPropertiesToOtherRagdoll(Get.Settings);
                        EditorUtility.SetDirty(Get);
                    }
                }
                else
                {
                    GUI.backgroundColor = new Color(0.3f, 1f, 0.4f, 1f);
                    if (GUI.Button(mRect, new GUIContent("Settings Assigned"))) loadSettings = false;
                    GUI.backgroundColor = Color.white;
                }

                #endregion

                serializedObject.Update();

                GUILayout.Space(6f);
                DrawPropertiesExcluding(serializedObject, "m_Script");

                GUILayout.Space(4);
                GUILayout.Button("", EditorStyles.label, GUILayout.Height(6));

                var pRect = GUILayoutUtility.GetLastRect();
                var rect = GUILayoutUtility.GetRect(pRect.width, 58);
                rect.width -= 10;

                GUI.color = Color.green;
                GUI.Box(rect, new GUIContent(""), FGUI_Resources.BGInBoxStyle);
                GUI.color = Color.white;
                GUI.Label(rect, new GUIContent("  Drag & Drop Objects with Ragdoll Animator 2\n to apply this file settings on them\n (not changing construct settings)", FGUI_Resources.Tex_Drag), EditorStyles.centeredGreyMiniLabel);

                #region Drag and Drop

                var dropEvent = UnityEngine.Event.current;

                if (dropEvent != null)
                {
                    if (dropEvent.type == UnityEngine.EventType.DragPerform || dropEvent.type == UnityEngine.EventType.DragUpdated)
                        if (rect.Contains(dropEvent.mousePosition))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                            if (dropEvent.type == UnityEngine.EventType.DragPerform)
                            {
                                DragAndDrop.AcceptDrag();
                                foreach (var dragged in DragAndDrop.objectReferences)
                                {
                                    GameObject draggedObject = dragged as GameObject;
                                    if (draggedObject != null)
                                    {
                                        RagdollAnimator2 rag = draggedObject.GetComponent<RagdollAnimator2>();

                                        if (rag)
                                        {
                                            Get.Settings.ApplyAllPropertiesToOtherRagdoll( rag.Settings );
                                            UnityEditor.EditorUtility.SetDirty( rag );
                                        }
                                    }
                                }
                            }

                            UnityEngine.Event.current.Use();
                        }
                }

                #endregion Drag and Drop

                serializedObject.ApplyModifiedProperties();
            }
        }

#endif

        #endregion

    }
}