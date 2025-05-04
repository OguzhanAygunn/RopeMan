#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [DefaultExecutionOrder( 50 )]
    [AddComponentMenu( "FImpossible Creations/Ragdoll Animator/Ragdoll Magnet Point", 11 )]
    public class RA2MagnetPoint : FimpossibleComponent
    {
        [HideInInspector] public GameObject ObjectWithRagdollAnimator;

        [Tooltip( "Transform with rigidbody to connect it with this joint" )]
        [HideInInspector] public Transform ToMove;

        [Space( 3 )]
        [Range( 0f, 2f )]
        public float DragPower = 1f;

        [Range( 0f, 2f )]
        public float RotatePower = 0f;

        [Tooltip("Set zero to compensate body physics reaction on attachement movement in world, set 1 to be affected with natural physics reaction to bones movement.")]
        [Range(0f, 1f)]
        public float MotionInfluence = 1f;

        public bool KinematicOnMax = false;

        [Space( 3 )]
        public Vector3 OriginOffset = Vector3.zero;

        public Quaternion RotationOffset = Quaternion.identity;

        private IRagdollAnimator2HandlerOwner handler;

        private void Start()
        {
            attachBone = null;

            if(ObjectWithRagdollAnimator)
            {
                handler = ObjectWithRagdollAnimator.GetComponent<IRagdollAnimator2HandlerOwner>();
            }

            if( handler == null ) { handler = GetComponent<IRagdollAnimator2HandlerOwner>(); ObjectWithRagdollAnimator = gameObject; }

            _lastFixedPosition = transform.position;

            if ( handler == null )
            {
                if( ToMove == null ) { enabled = false; return; }
                else
                if( ToMove.GetComponent<Rigidbody>() == null ) { enabled = false; return; }
            }

        }

        private void OnEnable()
        {
            wasKinematic = false;
            lastToMove = null;
        }

        private Rigidbody moveRigidbody = null;
        [NonSerialized] private RagdollChainBone attachBone = null;
        private bool wasKinematic = false;
        private Transform lastToMove = null;

        private void FixedUpdate()
        {
            if( ToMove == null ) return; // No target to attach

            if( handler != null && ( attachBone == null || attachBone.SourceBone != ToMove ) ) // Attach bone controller changed
            {
                attachBone = RagdollHandlerUtilities.User_GetBoneSetupBySourceAnimatorBone( handler.GetRagdollHandler, ToMove );
            }

            if( attachBone == null )
            {
                if( lastToMove != ToMove || moveRigidbody == null )
                {
                    moveRigidbody = ToMove.GetComponent<Rigidbody>();
                }
            }

            if( attachBone == null && moveRigidbody == null ) return; // No ragdoll bone to attach

            if( attachBone != null && ( moveRigidbody == null || moveRigidbody.transform != attachBone.PhysicalDummyBone ) ) // Rigidbody changed
            {
                moveRigidbody = attachBone.GameRigidbody;
            }

            if( moveRigidbody == null ) return; // No rigidbody to move

            lastToMove = ToMove;

            Vector3 targetPos = transform.TransformPoint( OriginOffset );
            Quaternion targetRot = transform.rotation * RotationOffset;

            bool requiresKinematic = false;

            if( DragPower > 0f )
            {
                if( DragPower > 1.99999f && KinematicOnMax )
                    requiresKinematic = true;
                else
                    RagdollHandlerUtilities.AddRigidbodyForceToMoveTowards( moveRigidbody, targetPos, DragPower );
            }

            if( RotatePower > 0f )
            {
                if( RotatePower > 1.99999f && KinematicOnMax )
                    requiresKinematic = true;
                else
                    RagdollHandlerUtilities.AddRigidbodyTorqueToRotateTowards( moveRigidbody, targetRot, RotatePower * 1.5f );
            }

            if( requiresKinematic )
            {
                if( !wasKinematic )
                {
                    if( attachBone != null ) attachBone.BypassKinematicControl = true;
                    wasKinematic = true;
                    moveRigidbody.isKinematic = true;
                }

                if( DragPower > 0f ) moveRigidbody.transform.position = targetPos;
                if( RotatePower > 0f ) moveRigidbody.transform.rotation = targetRot;
            }
            else
            {
                if( wasKinematic )
                {
                    if( attachBone != null ) attachBone.BypassKinematicControl = false;
                    wasKinematic = false;
                    moveRigidbody.isKinematic = false;
                }
            }

            UpdateMotionInfluence();
        }


        Vector3 _motionInfluenceOffset;
        Vector3 _lastFixedPosition;
        void UpdateMotionInfluence()
        {
            if (handler == null) return;

            if (MotionInfluence == 1f) { _lastFixedPosition = transform.position; return; }

            var ragd = handler.GetRagdollHandler;

            _motionInfluenceOffset = transform.position - _lastFixedPosition;
            _lastFixedPosition = transform.position;

            Vector3 offset = _motionInfluenceOffset * (1f - MotionInfluence);

            if (offset.sqrMagnitude < 0.00001f) return; // Optimize

            foreach (var chain in ragd.Chains)
            {
                foreach (var bone in chain.BoneSetups)
                {
                    bone.GameRigidbody.transform.position += offset;
                    bone.GameRigidbody.AddForce(offset, ForceMode.VelocityChange);
                }
            }

            _motionInfluenceOffset = Vector3.zero;
        }

        #region Editor Code

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            Handles.color = Color.blue;
            Handles.SphereHandleCap( 0, transform.TransformPoint( OriginOffset ), Quaternion.identity, 0.1f, EventType.Repaint );
            Handles.DrawLine( transform.TransformPoint( OriginOffset ), transform.TransformPoint( OriginOffset ) + ( transform.rotation * RotationOffset ) * Vector3.forward * 0.2f );

            if( ToMove == null ) return;
            Handles.color = Color.white;
            Handles.DrawDottedLine( ToMove.position, transform.position, 2f );
            Handles.color = Color.green;
            Handles.SphereHandleCap( 0, ToMove.position, Quaternion.identity, 0.1f, EventType.Repaint );
        }

        [CanEditMultipleObjects]
        [CustomEditor( typeof( RA2MagnetPoint ), true )]
        public class RA2MagnetPointEditor : Editor
        {
            public RA2MagnetPoint Get
            { get { if( _get == null ) _get = (RA2MagnetPoint)target; return _get; } }
            private RA2MagnetPoint _get;

            private SerializedProperty sp_ObjectWithRagdollAnimator;

            private void OnEnable()
            {
                sp_ObjectWithRagdollAnimator = serializedObject.FindProperty( "ObjectWithRagdollAnimator" );
            }

            protected virtual string HeaderInfo => "Moving and rotating rigidbody towards this object, dedicated for Ragdoll Animator 2.";

            public override void OnInspectorGUI()
            {
                UnityEditor.EditorGUILayout.HelpBox( HeaderInfo, UnityEditor.MessageType.Info );

                serializedObject.Update();

                GUILayout.Space( 4f );
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField( sp_ObjectWithRagdollAnimator );
                if( Get.ObjectWithRagdollAnimator ) RagdollHandler.Editor_RagdollBonesSelector( Get.ObjectWithRagdollAnimator, ( Transform t ) => { Get.ToMove = t; }, Get.ToMove );
                EditorGUILayout.EndHorizontal();

                var copy = sp_ObjectWithRagdollAnimator.Copy();
                copy.Next( false );
                EditorGUILayout.PropertyField( copy );

                DrawPropertiesExcluding( serializedObject, "m_Script" );

                if( Application.isPlaying )
                {
                    GUILayout.Space( 5 );
                    if( Get.moveRigidbody == null )
                    {
                        EditorGUILayout.HelpBox( "Not found rigidbody inside " + Get.ToMove + "!", UnityEditor.MessageType.None );
                    }
                    else
                    {
                        EditorGUILayout.ObjectField( "Dragging Body:", Get.moveRigidbody, typeof( Rigidbody ), true );
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }
        }

#endif

        #endregion Editor Code
    }
}