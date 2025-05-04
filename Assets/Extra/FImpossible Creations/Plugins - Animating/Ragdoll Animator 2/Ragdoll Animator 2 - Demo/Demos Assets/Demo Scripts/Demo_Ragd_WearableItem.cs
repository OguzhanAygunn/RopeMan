using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_WearableItem : FimpossibleComponent
    {
        public RagdollAnimator2 RagdollAnimator;

        [RagdollBoneSelector( "RagdollAnimator" )]
        public Transform TargetParent;

        [Space( 5 )]
        public RA2AttachableObject AttachObject;

        public bool isHat = false;

        public void SwitchWearingItem()
        {
            if( RagdollAnimator.Handler.IsWearingAttachable( AttachObject ) )
                DetachFromRagdoll();
            else
                WearOnRagdoll();
        }

        public void WearOnRagdoll()
        {
            RagdollAnimator.Handler.WearAttachable( AttachObject, TargetParent );
            RagdollAnimator.Handler.Mecanim.CrossFadeInFixedTime( isHat ? "Wear Hat" : "Wear Sword", 0.35f );
        }

        public void DetachFromRagdoll()
        {
            RagdollAnimator.Handler.UnwearAttachable( AttachObject );
            AttachObject.transform.SetParent( null, true );
            AttachObject.transform.position += new Vector3( 2f, 0f, 0f );
            RagdollAnimator.Handler.Mecanim.CrossFadeInFixedTime( "Wait", 0.3f );
        }

        #region Editor Class

#if UNITY_EDITOR

        [UnityEditor.CanEditMultipleObjects]
        [UnityEditor.CustomEditor( typeof( Demo_Ragd_WearableItem ), true )]
        public class Demo_Ragd_WearableItemEditor : UnityEditor.Editor
        {
            public Demo_Ragd_WearableItem Get
            { get { if( _get == null ) _get = (Demo_Ragd_WearableItem)target; return _get; } }
            private Demo_Ragd_WearableItem _get;

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                GUILayout.Space( 4f );
                DrawPropertiesExcluding( serializedObject, "m_Script" );

                serializedObject.ApplyModifiedProperties();

                UnityEditor.EditorGUILayout.BeginHorizontal();

                if( GUILayout.Button( "Set Parent" ) )
                {
                    if( Get.transform.parent == null )
                    {
                        Get.transform.SetParent( Get.TargetParent, true );
                        Get.transform.localPosition = Get.AttachObject.TargetLocalPosition;
                        Get.transform.localRotation = Quaternion.Euler( Get.AttachObject.TargetLocalRotation );
                    }
                    else
                    {
                        Get.transform.SetParent( null, true );
                    }
                }

                UnityEditor.EditorGUILayout.EndHorizontal();

                if( Get.RagdollAnimator )
                {
                    IRagdollAnimator2HandlerOwner handler = Get.RagdollAnimator.GetComponent<IRagdollAnimator2HandlerOwner>();
                    if( handler != null )
                    {
                        if( handler.GetRagdollHandler.WasInitialized )
                        {
                            GUILayout.Space( 4 );
                            if( GUILayout.Button( "Switch Wearing Item" ) )
                            {
                                if( handler.GetRagdollHandler.IsWearingAttachable( Get.AttachObject ) )
                                {
                                    Get.DetachFromRagdoll();
                                }
                                else
                                {
                                    Get.WearOnRagdoll();
                                }
                            }
                        }
                    }
                }
            }
        }

#endif

        #endregion Editor Class
    }
}