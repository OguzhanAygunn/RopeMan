using System.Collections.Generic;

#if UNITY_EDITOR

using FIMSpace.FEditor;
using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_IgnoreCollisionWith : RagdollAnimatorFeatureBase
    {
        public override bool OnInit()
        {
            if( InitializedWith.customObjectList == null ) return false;

            for( int i = 0; i < InitializedWith.customObjectList.Count; i++ )
            {
                var coll = InitializedWith.customObjectList[i] as Collider;
                if( coll == null ) continue;
                ParentRagdollHandler.IgnoreCollisionWith( coll, true );
            }

            return true;
        }

#if UNITY_EDITOR

        public override bool Editor_DisplayEnableSwitch => false;

        public override string Editor_FeatureDescription => "Making ragdoll dummy colliders ignore selected other colliders.";

        public override void Editor_InspectorGUI( SerializedProperty toDirty, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            base.Editor_InspectorGUI( toDirty, ragdollHandler, helper );

            if( helper.customObjectList == null ) helper.customObjectList = new List<Object>();

            GUI.enabled = !ragdollHandler.WasInitialized;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField( "Colliders To Ignore:", EditorStyles.boldLabel );
            GUILayout.FlexibleSpace();
            if( GUILayout.Button( "+", FGUI_Resources.ButtonStyle, GUILayout.Width( 24 ) ) ) helper.customObjectList.Add( null );

            EditorGUILayout.EndHorizontal();

            int toRemove = -1;
            for( int i = 0; i < helper.customObjectList.Count; i++ )
            {
                EditorGUILayout.BeginHorizontal();

                helper.customObjectList[i] = EditorGUILayout.ObjectField( helper.customObjectList[i], typeof( Collider ), true ) as Collider;

                FGUI_Inspector.RedGUIBackground();
                GUILayout.FlexibleSpace();
                if( GUILayout.Button( FGUI_Resources.GUIC_Remove, FGUI_Resources.ButtonStyle, GUILayout.Height( 18 ) ) ) toRemove = i;
                FGUI_Inspector.RestoreGUIBackground();
                EditorGUILayout.EndHorizontal();
            }

            if( toRemove > -1 )
            {
                helper.customObjectList.RemoveAt( toRemove );
                EditorUtility.SetDirty( toDirty.serializedObject.targetObject );
            }

            GUI.enabled = true;
        }

#endif
    }
}