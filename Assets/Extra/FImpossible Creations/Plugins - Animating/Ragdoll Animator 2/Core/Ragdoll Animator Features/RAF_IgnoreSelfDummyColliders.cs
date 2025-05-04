using System.Collections.Generic;

#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_IgnoreSelfDummyColliders : RagdollAnimatorFeatureBase
    {
        public override bool OnInit()
        {
            if( InitializedWith.customObjectList == null ) return false;
            if( InitializedWith.customStringList == null ) return false;
            if( InitializedWith.customStringList.Count != ParentRagdollHandler.GetAllBonesCount() ) return false;

            int iter = 0;

            foreach( var chain in ParentRagdollHandler.Chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    if( InitializedWith.customStringList[iter] == "1" )
                    {
                        foreach( var ichain in ParentRagdollHandler.Chains )
                            foreach( var ibone in ichain.BoneSetups )
                            {
                                bone.IgnoreCollisionsWith( ibone, true );
                            }
                    }

                    iter += 1;
                }
            }

            return true;
        }

#if UNITY_EDITOR

        public override bool Editor_DisplayEnableSwitch => false;

        public override string Editor_FeatureDescription => "Choose bones which should not collide with all other dummy bones.";

        public override void Editor_InspectorGUI( SerializedProperty toDirty, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            base.Editor_InspectorGUI( toDirty, ragdollHandler, helper );

            if( helper.customStringList == null ) helper.customStringList = new List<string>();

            if( ragdollHandler.WasInitialized ) GUI.enabled = false;

            List<string> list = helper.customStringList;
            int targetCount = ragdollHandler.GetAllBonesCount();

            if( list.Count < targetCount )
            { while( list.Count < targetCount ) list.Add( "" ); }
            else while( list.Count > targetCount ) list.RemoveAt( list.Count - 1 );

            int iter = 0;
            foreach( var chain in ragdollHandler.Chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    EditorGUILayout.BeginHorizontal();
                    bool toggled = ( list[iter] == "1" );
                    toggled = EditorGUILayout.Toggle( toggled, GUILayout.Width( 24 ) );
                    EditorGUILayout.ObjectField( bone.SourceBone, typeof( Transform ), true );
                    list[iter] = toggled ? "1" : "0";
                    EditorGUILayout.EndHorizontal();
                    iter += 1;
                }
            }

            GUI.enabled = true;
        }

#endif
    }
}