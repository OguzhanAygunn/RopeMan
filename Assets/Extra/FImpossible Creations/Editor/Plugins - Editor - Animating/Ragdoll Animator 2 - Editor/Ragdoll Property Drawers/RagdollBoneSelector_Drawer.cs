using FIMSpace.FEditor;
using FIMSpace.FProceduralAnimation;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [CustomPropertyDrawer(typeof(RagdollBoneSelectorAttribute))]
    public class RagdollBoneSelector_Drawer : PropertyDrawer
    {
        private SerializedProperty sp = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RagdollBoneSelectorAttribute att = (RagdollBoneSelectorAttribute)base.attribute;

            if (sp == null) sp = property.serializedObject.FindProperty(att.ragdollProperty);
            IRagdollAnimator2HandlerOwner rHandler = null;
            if (sp != null) rHandler = sp.objectReferenceValue as IRagdollAnimator2HandlerOwner;

            var pos = position;

            if (rHandler != null) position.width -= 114;

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PrefixLabel(position, label);
            EditorGUI.PropertyField(position, property);

            if (rHandler != null)
            {
                var button = pos;
                button.x += pos.width - 110;
                button.width = 108;
                if (GUI.Button(button, new GUIContent("  Select Bone", FGUI_Resources.FindIcon("Ragdoll Animator/SPR_RagdollAnim2s"))))
                {
                    UnityEditor.GenericMenu menu = new UnityEditor.GenericMenu();

                    foreach (var chain in rHandler.GetRagdollHandler.Chains)
                    {
                        string prefix = chain.ChainName + " (" + chain.ChainType + ")" + "/";

                        foreach (var bone in chain.BoneSetups)
                        {
                            if (bone.SourceBone == null) continue;

                            Transform target = bone.SourceBone;

                            menu.AddItem(new GUIContent(prefix + target.name), target == property.objectReferenceValue, () =>
                            {
                                property.objectReferenceValue = target;
                                UnityEditor.EditorUtility.SetDirty(property.serializedObject.targetObject);
                                property.serializedObject.ApplyModifiedProperties();
                            });
                        }
                    }

                    menu.ShowAsContext();
                }
            }

            EditorGUI.EndProperty();
        }
    }
}