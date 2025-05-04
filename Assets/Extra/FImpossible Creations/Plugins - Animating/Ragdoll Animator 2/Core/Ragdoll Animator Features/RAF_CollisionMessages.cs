#if UNITY_EDITOR

using UnityEditor;

#endif

using FIMSpace.FGenerating;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_CollisionMessages : RagdollAnimatorFeatureCollisions
    {
        public override bool EnableCollectCollision => collectCollisions;

        private GameObject receiver = null;
        private FUniversalVariable ignoreSelf;
        private bool collectCollisions = false;

        public override bool OnInit()
        {
            collectCollisions = InitializedWith.RequestVariable( "Collect Collisions:", false ).GetBool();

            base.OnInit();

            var receiverObject = InitializedWith.RequestVariable( "Receiver", null );

            if( receiverObject.GetUnityObject() is Transform )
            {
                Transform t = receiverObject.GetUnityObject() as Transform;
                if( t ) receiver = t.gameObject;
            }

            if( receiver == null )
            {
                UnityEngine.Debug.Log( "[Ragdoll Animator 2] Collision Messages Feature: Not assigned collision messages receiver! (" + InitializedWith.ParentRagdollHandler.BaseTransform.name + ")\nRemoving feature from the controller." );
                return false;
            }

            ignoreSelf = InitializedWith.RequestVariable( "Ignore Self Limbs:", true );

            return true;
        }

        public override void OnCollisionEnterAction( RA2BoneCollisionHandler hitted, Collision collision )
        {
            if( Helper.Enabled == false ) return;

            if( ignoreSelf.GetBool() )
            {
                if( ParentRagdollHandler.ContainsAnimatorBoneTransform( collision.transform ) ) return;
            }

            receiver.SendMessage( "RagdollAnimator2BoneCollision", hitted );
            //Debug.Log( "[Ragdoll Animator 2] Collision Messages Feature: It seems that 'RagdollAnimator2BoneCollision(RagdollAnimator2BoneCollisionHandler hitted)' method was not found! " + exc );
        }

#if UNITY_EDITOR

        public override string Editor_FeatureDescription => "Sending ragdoll bones collision messages to the receiver which has method like:";

        public override void Editor_InspectorGUI( SerializedProperty toDirty, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            EditorGUILayout.TextField( "RagdollAnimator2BoneCollision(RA2BoneCollisionHandler hitted)" );

            GUI.enabled = !ragdollHandler.WasInitialized;

            var receiverObjectV = helper.RequestVariable( "Receiver", null );
            receiverObjectV.VariableType = FGenerating.FUniversalVariable.EVariableType.UnityObject;
            var tr = EditorGUILayout.ObjectField( "Receiver:", receiverObjectV.GetUnityObject(), typeof( Transform ), true ) as Transform;

            if( tr != receiverObjectV.GetUnityObject() )
            {
                receiverObjectV.SetValue( tr );
                UnityEditor.EditorUtility.SetDirty( toDirty.serializedObject.targetObject );
            }

            GUI.enabled = true;

            GUILayout.Space( 4 );

            if( tr == null )
            {
                EditorGUILayout.HelpBox( "Receiver need to be scene object.", UnityEditor.MessageType.Warning );
            }
            //else
            //{
            //    EditorGUILayout.HelpBox( "Make sure that some script of the receiver has public method like: ", UnityEditor.MessageType.None );
            //    EditorGUILayout.TextArea( "public void RagdollAnimator2BoneCollision(RagdollAnimator2BoneCollisionHandler hitted) {}" );
            //}

            GUILayout.Space( 4 );

            var ignoreSelfV = helper.RequestVariable( "Ignore Self Limbs:", true );
            ignoreSelfV.AssignTooltip( "Not sending collision events when ragdoll dummy bone collides with its own, other bone collider. (for example arm and leg collision)" );
            ignoreSelfV.Editor_DisplayVariableGUI();
            GUILayout.Space( 2 );

            GUI.enabled = !ragdollHandler.WasInitialized;
            var coollectCollisionsV = helper.RequestVariable( "Collect Collisions:", false );
            coollectCollisionsV.AssignTooltip( "Enabling collecting collision states for each limb. It allows to define if limb is currently colliding with something or not." );
            coollectCollisionsV.Editor_DisplayVariableGUI();
            GUI.enabled = true;

            GUILayout.Space( 4 );
        }

#endif
    }
}