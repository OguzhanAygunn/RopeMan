using System.Collections.Generic;

#if UNITY_EDITOR

using FIMSpace.FEditor;
using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace
{
    [AddComponentMenu( "FImpossible Creations/Utilities/Hierarchy Shortcut" )]
    public class FHierarchyShortcut : FimpossibleComponent
    {
        [System.Serializable]
        private class SceneReference
        {
            public string Title = "Scene Object";
            public UnityEngine.Object Reference;
        }

        [SerializeField, HideInInspector] private List<SceneReference> References = new List<SceneReference>();

        #region Editor Class

#if UNITY_EDITOR

        [CanEditMultipleObjects]
        [CustomEditor( typeof( FHierarchyShortcut ) )]
        public class FHierarchyShortcutEditor : UnityEditor.Editor
        {
            public FHierarchyShortcut Get
            { get { if( _get == null ) _get = (FHierarchyShortcut)target; return _get; } }
            private FHierarchyShortcut _get;

            public override void OnInspectorGUI()
            {
                GUILayout.Space( 4f );

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox( "Holding references to helper objects on the scene for quick navigation", UnityEditor.MessageType.None );
                if( GUILayout.Button( "+", FGUI_Resources.ButtonStyle, GUILayout.Height( 18 ), GUILayout.Width( 24 ) ) ) { Get.References.Add( new SceneReference() ); }
                EditorGUILayout.EndHorizontal();

                serializedObject.Update();

                GUILayout.Space( 4f );
                DrawPropertiesExcluding( serializedObject, "m_Script" );
                int toRemove = -1;

                for( int i = 0; i < Get.References.Count; i++ )
                {
                    EditorGUILayout.BeginVertical( EditorStyles.helpBox );

                    var refr = Get.References[i];
                    EditorGUILayout.BeginHorizontal();

                    refr.Title = EditorGUILayout.TextArea( refr.Title );

                    FGUI_Inspector.RedGUIBackground();
                    if( GUILayout.Button( FGUI_Resources.GUIC_Remove, FGUI_Resources.ButtonStyle, GUILayout.Width( 24 ), GUILayout.Height( 18 ) ) ) { toRemove = i; }
                    FGUI_Inspector.RestoreGUIBackground();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    refr.Reference = (UnityEngine.Object)EditorGUILayout.ObjectField( refr.Reference, typeof( Transform ), true );

                    if( refr.Reference )
                    {
                        if( GUILayout.Button( "Ping" ) ) { EditorGUIUtility.PingObject( refr.Reference ); }
                        if( GUILayout.Button( "Select" ) ) { Selection.activeObject = refr.Reference; }
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                }

                if( toRemove > -1 ) Get.References.RemoveAt( toRemove );

                serializedObject.ApplyModifiedProperties();
            }
        }

#endif

        #endregion Editor Class
    }
}