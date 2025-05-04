using UnityEditor;
using UnityEngine;

namespace FIMSpace.FEditor
{
    [CustomPropertyDrawer(typeof(FPD_ResourcesIconAttribute))]
    public class FPD_ResourcesIconDrawer : PropertyDrawer
    {
        FPD_ResourcesIconAttribute Attribute { get { return ((FPD_ResourcesIconAttribute)base.attribute); } }

        private GUIContent gc = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (gc == null)
            {
                string spacing = "";
                for (int i = 0; i < Attribute.Spacing; i++) spacing += " ";
                gc = new GUIContent(spacing + label.text, Resources.Load<Texture2D>(Attribute.Path), label.tooltip);
            }

            EditorGUI.PropertyField(position, property, gc, true);

            EditorGUI.EndProperty();
        }

    }
}

