using System.Collections.Generic;

#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_SwitchAttachable : RagdollAnimatorFeatureBase
    {
        private FGenerating.FUniversalVariable attachableV;
        private FGenerating.FUniversalVariable parentV;
        private RA2AttachableObject attached = null;

        public override bool OnInit()
        {
            if( !base.OnInit() ) return false;
            attachableV = InitializedWith.RequestVariable( "Attachable", null );
            parentV = InitializedWith.RequestVariable( "Target Parent", null );
            RefreshAttachableState( false );
            return true;
        }

        public void RefreshAttachableState( bool logIfNullParent = true )
        {
            if( attachableV.GetUnityObject() == attached ) return; // Already assigned
            Transform targetParent = parentV.GetUnityObject() as Transform;

            if( targetParent == null )
            {
                if( logIfNullParent ) UnityEngine.Debug.Log( "[Ragdoll Animator 2] Trying to attach object into null reference bone! :" + ( ParentRagdollHandler.Caller ? ParentRagdollHandler.Caller.name : "" ) + ":" );
                return; // No Parent to attach
            }

            RA2AttachableObject newAttachable = attachableV.GetUnityObject() as RA2AttachableObject;

            if( newAttachable == null )
            {
                ParentRagdollHandler.UnwearAttachable( attached );
                attached = null;
            }
            else
            {
                ParentRagdollHandler.UnwearAttachable( attached );
                ParentRagdollHandler.WearAttachable( newAttachable, parentV.GetUnityObject() as Transform );
                attached = newAttachable;
            }
        }

#if UNITY_EDITOR

        public override bool Editor_DisplayEnableSwitch => false;
        public override string Editor_FeatureDescription => "Attaching / detaching attachable object from the dummy when value changes.";

        public override void Editor_InspectorGUI( SerializedProperty toDirty, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            base.Editor_InspectorGUI( toDirty, ragdollHandler, helper );
            GUILayout.Space( 3 );

            if( helper.customObjectList == null ) helper.customObjectList = new List<Object>();

            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.labelWidth = 100;
            FGenerating.FUniversalVariable attachableRef = helper.RequestVariable( "Attachable", null );
            RA2AttachableObject attachable = EditorGUILayout.ObjectField( "Attachable:", attachableRef.GetUnityObject(), typeof( RA2AttachableObject ), true ) as RA2AttachableObject;
            attachableRef.AssignTooltip( "Attachable object setup." );
            attachableRef.SetValue( attachable );

            GUILayout.Space( 3 );

            EditorGUILayout.BeginHorizontal();
            FGenerating.FUniversalVariable targetParentRef = helper.RequestVariable( "Target Parent", null );
            targetParentRef.AssignTooltip( "Target bone to which object should be attached." );

            Transform targetParent = EditorGUILayout.ObjectField( "Target Parent:", targetParentRef.GetUnityObject(), typeof( Transform ), true ) as Transform;
            RagdollHandler.Editor_RagdollBonesSelector( ragdollHandler.Caller ? ragdollHandler.Caller.gameObject : ragdollHandler.GetBaseTransform().gameObject, ( Transform t ) => { targetParentRef.SetValue( t ); EditorUtility.SetDirty( toDirty.serializedObject.targetObject ); }, targetParentRef.GetUnityObject() as Transform );

            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0;

            if( !ragdollHandler.WasInitialized )
            {
                if( attachable != null && targetParent != null && attachable.ChangeLocalCoords )
                {
                    GUILayout.Space( 2 );
                    if( GUILayout.Button( attachable.transform.parent == targetParent ? "Detach From Parent" : "Test Set Parent Now" ) )
                    {
                        if( attachable.transform.parent == targetParent )
                        {
                            attachable.transform.SetParent( null, true );
                            attachable.transform.position += new Vector3( 1f, 0f, 0f );
                        }
                        else
                        {
                            attachable.transform.SetParent( targetParent, true );
                            attachable.transform.localPosition = attachable.TargetLocalPosition;
                            attachable.transform.localRotation = Quaternion.Euler( attachable.TargetLocalRotation );
                        }
                    }
                }
            }

            if( EditorGUI.EndChangeCheck() )
            {
                EditorUtility.SetDirty( toDirty.serializedObject.targetObject );
                if( ragdollHandler.WasInitialized ) RefreshAttachableState( true );
            }
        }

#endif
    }
}