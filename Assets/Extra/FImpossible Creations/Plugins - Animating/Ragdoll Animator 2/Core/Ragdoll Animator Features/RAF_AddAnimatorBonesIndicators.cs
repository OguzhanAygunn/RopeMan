using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_AddAnimatorBonesIndicators : RagdollAnimatorFeatureBase
    {
#if UNITY_EDITOR
        public override bool Editor_DisplayEnableSwitch => false;
        public override string Editor_FeatureDescription => "Adding indicator components to the animator bones in order to give reference to the physical bone object which represents a certain bone.";
#endif

        public override bool OnInit()
        {
            var ragdoll = ParentRagdollHandler;

            var addColliders = InitializedWith.RequestVariable( "Add Colliders On The Source Bones:", false );

            if( addColliders.GetBool() )
            {
                RagdollHandlerUtilities.AddCollidersOnTheCharacterBones( ragdoll );
                ragdoll.User_FindAllCollidersInsideAndIgnoreTheirCollisionWithDummyColliders( ragdoll.GetBaseTransform() );

                var triggerColliders = InitializedWith.RequestVariable( "Only Trigger Colliders:", false );

                if( triggerColliders.GetBool() )
                {
                    foreach( var chain in ParentRagdollHandler.Chains )
                    {
                        foreach( var bone in chain.BoneSetups )
                        {
                            foreach( var collS in bone.Colliders )
                            {
                                if( collS.GameColliderOnSource ) collS.GameColliderOnSource.isTrigger = true;
                            }
                        }
                    }
                }
            }

            foreach( var chain in ragdoll.Chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    // Go through all dummy bones and add indicator components to the animator (not physical dummy) bones

                    RagdollAnimator2BoneIndicator indic = bone.SourceBone.gameObject.GetComponent<RagdollAnimator2BoneIndicator>();
                    if( indic == null ) indic = bone.SourceBone.gameObject.AddComponent<RagdollAnimator2BoneIndicator>();

                    indic.Initialize( ragdoll, bone.BoneProcessor, chain, true );
                }
            }

            // Add indicator to the base transform

            RagdollAnimator2BoneIndicator baseIndic = ragdoll.BaseTransform.gameObject.GetComponent<RagdollAnimator2BoneIndicator>();
            if( baseIndic == null )
            {
                baseIndic = ragdoll.BaseTransform.gameObject.AddComponent<RagdollAnimator2BoneIndicator>();
                baseIndic.Initialize( ragdoll, null, null, true );
                baseIndic.hideFlags = HideFlags.HideInInspector;
            }

            return true;
        }

#if UNITY_EDITOR

        public override void Editor_InspectorGUI( SerializedProperty toDirty, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            GUI.enabled = !ragdollHandler.WasInitialized;
            FGenerating.FUniversalVariable addColliders = helper.RequestVariable( "Add Colliders On The Source Bones:", false );

            Transform baseT = ragdollHandler.GetBaseTransform();
            if( baseT )
            {
                if( baseT.parent ) baseT = baseT.parent;
                Rigidbody mainHasRigidbody = baseT.GetComponentInChildren<Rigidbody>();
                if ( mainHasRigidbody)
                {
                    EditorGUILayout.HelpBox( "Warning: Rigidbody character movement can conflict with skeleton bones. Add kinematic rigidbody to the parent skeleton object and ensure layers ignore.", UnityEditor.MessageType.Warning );
                }
            }

            EditorGUIUtility.labelWidth = 220;
            addColliders.Editor_DisplayVariableGUI();

            if( addColliders.GetBool() )
            {
                var triggerColliders = helper.RequestVariable( "Only Trigger Colliders:", false );
                EditorGUI.indentLevel++;
                triggerColliders.Editor_DisplayVariableGUI();
                EditorGUI.indentLevel--;
            }

            EditorGUIUtility.labelWidth = 0;
            GUI.enabled = true;
        }

#endif
    }
}