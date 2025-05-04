#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    //[CreateAssetMenu]
    public class RAF_BoundedIgnoreExtended : RagdollAnimatorFeatureBase
    {

#if UNITY_EDITOR

        public override string Editor_FeatureDescription => "Enhanced control for self body colliders ignore. (GUI feature)";
        public override void Editor_OnRemoveFeatureInEditorGUI( RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            // Restore defatults
            ragdollHandler.BoundedCollidersIgnoreScaleup = 1.2f;

            foreach( var chain in ragdollHandler.Chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    bone.BoundedIgnoreScale = 1f;
                }
            }
        }

        public override void Editor_InspectorGUI( SerializedProperty toDirty, RagdollHandler handler, RagdollAnimatorFeatureHelper helper )
        {
            if( handler.IgnoreBoundedColliders == false )
            {
                EditorGUILayout.HelpBox( "Ignore Bounded Colliders if Off", MessageType.Info );
                if( GUILayout.Button( "Turn Bounded Ignore ON" ) ) { handler.IgnoreBoundedColliders = true; UnityEditor.EditorUtility.SetDirty( toDirty.serializedObject.targetObject ); }
                return;
            }

            if( handler.WasInitialized ) GUI.enabled = false;

            var allScale = helper.RequestVariable( "All Volumes Scale:", 1.2f );
            allScale.SetMinMaxSlider( 0f, 2f );
            allScale.Editor_DisplayVariableGUI();
            handler.BoundedCollidersIgnoreScaleup = allScale.GetFloat();

            GUILayout.Space( 4 );

            if( helper.customStringList == null ) helper.customStringList = new List<string>();

            List<string> list = helper.customStringList;
            int targetCount = handler.GetAllBonesCount();

            if( list.Count < targetCount )
            { while( list.Count < targetCount ) list.Add( "1" ); }
            else while( list.Count > targetCount ) list.RemoveAt( list.Count - 1 );

            int iter = 0;
            foreach( var chain in handler.Chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    EditorGUILayout.BeginHorizontal();
                    float amount = float.Parse( list[iter] );
                    EditorGUIUtility.labelWidth = 86;
                    amount = EditorGUILayout.FloatField( "Volume Scale:", amount, GUILayout.Width( 134 ) );
                    amount = Mathf.Clamp( amount, 0f, 2f );
                    EditorGUILayout.ObjectField( bone.SourceBone, typeof( Transform ), true );
                    list[iter] = amount.ToString();

                    bone.BoundedIgnoreScale = amount;

                    EditorGUIUtility.labelWidth = 0;
                    if( amount <= 0f ) EditorGUILayout.LabelField( "OFF", GUILayout.Width( 24 ) );
                    EditorGUILayout.EndHorizontal();
                    iter += 1;
                }
            }
        }

        public override void Editor_OnSceneGUI( RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            if( ragdollHandler.WasInitialized ) return;
            if( ragdollHandler._EditorCategory != RagdollHandler.ERagdollAnimSection.Extra ) return;
            if( helper.customStringList == null ) return;

            Handles.color = Color.yellow * 0.8f;
            int iter = 0;

            var allScale = helper.RequestVariable( "All Volumes Scale:", 1.2f );

            foreach( var chain in ragdollHandler.Chains )
            {
                foreach( var bone in chain.BoneSetups )
                {

                    if( bone.SourceBone == null ) { iter += 1; return; }
                    Vector3 size = bone.BaseColliderSetup.CalculateLocalSize();
                    Handles.matrix = bone.SourceBone.localToWorldMatrix;

                    if( iter >= helper.customStringList.Count ) return;
                    float mul = 1f;
                    float.TryParse( helper.customStringList[iter], out mul );
                    if( mul > 0f ) Handles.DrawWireCube( bone.BaseColliderSetup.ColliderCenter, size * mul * allScale.GetFloat() );
                    iter += 1;
                }
            }
        }

#endif
    }
}