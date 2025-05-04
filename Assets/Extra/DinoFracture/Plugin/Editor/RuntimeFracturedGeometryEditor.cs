using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DinoFracture.Editor
{
    [CustomEditor(typeof(RuntimeFracturedGeometry))]
    [CanEditMultipleObjects()]
    public class RuntimeFracturedGeometryEditor : FractureGeometryEditor
    {
        private static Dictionary<string, PropertyName> _sRuntimeFractureProperties;

        static RuntimeFracturedGeometryEditor()
        {
            _sRuntimeFractureProperties = new Dictionary<string, PropertyName>();
            AddPropertyName(_sRuntimeFractureProperties, "Asynchronous");
        }

        public override void OnInspectorGUI()
        {
            DrawCommonFractureProperties();

            Space(10);

            DrawFractureProperties(_sRuntimeFractureProperties);

            Space(10);

            DrawFractureEventProperties();
        }
    }
}