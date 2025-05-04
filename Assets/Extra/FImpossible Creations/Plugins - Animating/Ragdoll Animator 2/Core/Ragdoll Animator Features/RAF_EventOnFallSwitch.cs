#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;
using UnityEngine.Events;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_EventOnFallSwitch : RagdollAnimatorFeatureBase
    {
        public override bool OnInit()
        {
            RefreshHelperEvents( InitializedWith );
            ParentRagdollHandler.AddToOnFallModeSwitchActions( OnChange );
            return base.OnInit();
        }

        public override void OnDestroyFeature()
        {
            ParentRagdollHandler.RemoveFromOnFallModeSwitchActions( OnChange );
        }

        private void OnChange()
        {
            if( InitializedWith.Enabled == false ) return;

            if( ParentRagdollHandler.AnimatingMode == RagdollHandler.EAnimatingMode.Standing )
            {
                Helper.customEventsList[1].Invoke();
            }
            else
            {
                Helper.customEventsList[0].Invoke();
            }
        }

        private bool RefreshHelperEvents( RagdollAnimatorFeatureHelper helper )
        {
            bool changed = false;
            if( helper.customEventsList == null )
            {
                helper.customEventsList = new System.Collections.Generic.List<UnityEvent>();
                changed = true;
            }

            while( helper.customEventsList.Count < 2 ) { helper.customEventsList.Add( new UnityEvent() ); changed = true; }

            return changed;
        }

#if UNITY_EDITOR

        public override string Editor_FeatureDescription => "Calling custom event when character is switching to falling state.";

        public override void Editor_InspectorGUI( SerializedProperty handlerProp, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            int featureIdx = -1;
            for( int i = 0; i < ragdollHandler.ExtraFeatures.Count; i++ ) if( ragdollHandler.ExtraFeatures[i] == helper ) { featureIdx = i; break; }

            if( featureIdx == -1 )
            {
                EditorGUILayout.HelpBox( "Something went wrong with identifying feature in ragdoll handler", UnityEditor.MessageType.None );
                return;
            }

            if( RefreshHelperEvents( helper ) )
            {
                EditorUtility.SetDirty( handlerProp.serializedObject.targetObject );
                handlerProp.serializedObject.ApplyModifiedProperties();
                handlerProp.serializedObject.Update();
            }

            var sp = handlerProp.FindPropertyRelative( "ExtraFeatures" ).GetArrayElementAtIndex( featureIdx );
            sp = sp.FindPropertyRelative( "customEventsList" );

            EditorGUILayout.LabelField( "On Start Falling:", EditorStyles.boldLabel );
            GUILayout.Space( 5 );
            EditorGUILayout.PropertyField( sp.GetArrayElementAtIndex( 0 ) );

            GUILayout.Space( 5 );
            EditorGUILayout.LabelField( "On Start Standing:", EditorStyles.boldLabel );
            GUILayout.Space( 5 );
            EditorGUILayout.PropertyField( sp.GetArrayElementAtIndex( 1 ) );

            GUILayout.Space( 3 );
        }

#endif
    }
}