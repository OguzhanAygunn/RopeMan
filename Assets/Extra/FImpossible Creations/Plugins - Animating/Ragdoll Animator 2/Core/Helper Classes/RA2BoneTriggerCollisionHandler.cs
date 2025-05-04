using System.Collections.Generic;

#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [AddComponentMenu( "", 0 )]
    public class RA2BoneTriggerCollisionHandler : RA2BoneCollisionHandlerBase
    {
        public List<Collider> EnteredColliders { get; private set; }
        public List<Collider> EnteredSelfColliders { get; private set; }

        private bool CollectCollisions = false;

        /// <summary> Lastest enetered collier, including other and self colliders </summary>
        public Collider LatestEnterCollider { get; private set; }

        /// <summary> Used only when enabled CollectCollisions </summary>
        public Collider LatestEnterNonSelfCollider { get; private set; }

        public override void EnableSavingEnteredCollisionsList()
        {
            //if( EnteredColliders == null ) EnteredColliders = new Dictionary<Transform, Collider>();
            //if( EnteredSelfColliders == null ) EnteredSelfColliders = new Dictionary<Transform, Collider>();
            if( EnteredColliders == null ) EnteredColliders = new List<Collider>();
            if( EnteredSelfColliders == null ) EnteredSelfColliders = new List<Collider>();
            CollectCollisions = true;
        }

        public override RagdollAnimator2BoneIndicator Initialize( RagdollHandler handler, RagdollBoneProcessor boneProcessor, RagdollBonesChain parentChain, bool isAnimatorBone = false, RA2AttachableObject attachable = null )
        {
            LatestEnterCollider = null;
            LatestExitCollider = null;

            return base.Initialize( handler, boneProcessor, parentChain, isAnimatorBone, attachable );
        }

        private void OnTriggerEnter( Collider collider )
        {
            if( Ignores.Contains( collider.transform ) ) return;

            LatestEnterCollider = collider;

            if( CollectCollisions )
            {
                // Self Collision
                if( ParentRagdollProcessor.ContainsBoneTransform( collider.transform ) )
                {
                    if( !EnteredSelfColliders.Contains( collider ) ) EnteredSelfColliders.Add( collider );
                }
                else
                {
                    if( !EnteredColliders.Contains( collider ) ) EnteredColliders.Add( collider );
                }

                if( UseSelfCollisions )
                {
                    if( EnteredColliders.Count > 0 || EnteredSelfColliders.Count > 0 ) Colliding = true;
                }
                else
                {
                    if( EnteredColliders.Count > 0 ) Colliding = true;
                }
            }

            ParentHandler.OnTriggerEnterEvent( this, collider );
        }

        public Collider LatestExitCollider { get; private set; }

        private void OnTriggerExit( Collider collider )
        {
            LatestExitCollider = collider;

            if( CollectCollisions )
            {
                // Self Collision
                if( ParentRagdollProcessor.ContainsBoneTransform( collider.transform ) )
                {
                    if( EnteredSelfColliders.Contains( collider ) ) EnteredSelfColliders.Remove( collider );
                }
                else
                {
                    if( EnteredColliders.Contains( collider ) ) EnteredColliders.Remove( collider );
                }

                if( UseSelfCollisions )
                {
                    if( EnteredColliders.Count == 0 && EnteredSelfColliders.Count == 0 ) Colliding = false;
                }
                else
                {
                    if( EnteredColliders.Count == 0 ) Colliding = false;
                }
            }
        }

        public override bool IsCollidingWith( Collider collider )
        {
            if( EnteredColliders == null )
            {
                if( Colliding == false ) return false;
                if( LatestEnterCollider != null ) if( LatestEnterCollider.GetComponent<Collider>() == collider ) return true;
                return false;
            }

            if( LatestEnterNonSelfCollider.GetComponent<Collider>() == collider ) return true;

            foreach( var c in EnteredColliders )
                if( c == collider ) return true;

            return false;
        }

        public override bool CollidesWithAnything()
        {
            return Colliding;
        }

        public override Collider GetFirstCollidingCollider()
        {
            if( EnteredColliders == null ) return null;
            if( EnteredColliders.Count > 0 ) return EnteredColliders[0];
            return null;
        }

        #region Editor Class

#if UNITY_EDITOR

        [UnityEditor.CanEditMultipleObjects]
        [UnityEditor.CustomEditor( typeof( RA2BoneTriggerCollisionHandler ), true )]
        public class RA2BoneTriggerCollisionHandlerEditor : RagdollAnimator2BoneIndicatorEditor
        {
            public RA2BoneTriggerCollisionHandler Get
            { get { if( _get == null ) _get = (RA2BoneTriggerCollisionHandler)target; return _get; } }
            private RA2BoneTriggerCollisionHandler _get;

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if( Get.CollectCollisions == false )
                {
                    EditorGUILayout.HelpBox( "You need to enable collecting collisions in order to detect ' Colliding = true ' properly!", UnityEditor.MessageType.Info );
                }

                if( Get.EnteredColliders == null ) return;
                if( Get.EnteredColliders.Count == 0 ) return;
                EditorGUILayout.LabelField( "Entered: " );
                for( int i = 0; i < Get.EnteredColliders.Count; i++ )
                {
                    EditorGUILayout.ObjectField( Get.EnteredColliders[i], typeof( Collider ), true );
                }
            }
        }

#endif

        #endregion Editor Class
    }
}