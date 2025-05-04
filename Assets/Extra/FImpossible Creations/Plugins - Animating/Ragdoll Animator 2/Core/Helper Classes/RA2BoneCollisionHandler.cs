using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [AddComponentMenu( "", 0 )]
    public class RA2BoneCollisionHandler : RA2BoneCollisionHandlerBase
    {
        /// <summary> Used when enabling collecting collisions </summary>
        public Dictionary<Transform, CollisionCapture> EnteredCollisions { get; private set; }

        public Dictionary<Transform, CollisionCapture> EnteredSelfCollisions { get; private set; }
        private bool CollectCollisions = false;

        /// <summary> Lastest enetered collision, including other and self collisions </summary>
        public Collision LatestEnterCollision { get; private set; }

        /// <summary> Used only when enabled CollectCollisions </summary>
        public Collision LatestEnterNonSelfCollision { get; private set; }

        public struct CollisionCapture
        {
            public int Enters;
            public Transform Entered;
            public Collision Lastest;
        }

        public override void EnableSavingEnteredCollisionsList()
        {
            if( EnteredCollisions == null ) EnteredCollisions = new Dictionary<Transform, CollisionCapture>();
            if( EnteredSelfCollisions == null ) EnteredSelfCollisions = new Dictionary<Transform, CollisionCapture>();
            CollectCollisions = true;
        }

        public override RagdollAnimator2BoneIndicator Initialize( RagdollHandler handler, RagdollBoneProcessor boneProcessor, RagdollBonesChain parentChain, bool isAnimatorBone = false, RA2AttachableObject attachable = null )
        {
            LatestEnterCollision = null;
            LatestExitCollision = null;

            return base.Initialize( handler, boneProcessor, parentChain, isAnimatorBone, attachable );
        }

        private void OnCollisionEnter( Collision collision )
        {
            if( Ignores.Contains( collision.transform ) ) return;

            LatestEnterCollision = collision;

            if( CollectCollisions )
            {
                CollisionCapture capture;

                // Self Collision
                if( ParentRagdollProcessor.ContainsPhysicalBoneTransform( collision.transform ) )
                {
                    if( EnteredSelfCollisions.TryGetValue( collision.transform, out capture ) )
                    {
                        capture.Enters += 1;
                        capture.Lastest = collision;
                        EnteredSelfCollisions[collision.transform] = capture;
                    }
                    else
                    {
                        capture = new CollisionCapture();
                        capture.Entered = collision.transform;
                        capture.Enters = 1;
                        capture.Lastest = collision;
                        EnteredSelfCollisions.Add( collision.transform, capture );
                    }
                }
                else
                {
                    LatestEnterNonSelfCollision = collision;

                    if( EnteredCollisions.TryGetValue( collision.transform, out capture ) )
                    {
                        capture.Enters += 1;
                        capture.Lastest = collision;
                        EnteredCollisions[collision.transform] = capture;
                    }
                    else
                    {
                        capture = new CollisionCapture();
                        capture.Entered = collision.transform;
                        capture.Enters = 1;
                        capture.Lastest = collision;
                        EnteredCollisions.Add( collision.transform, capture );
                    }
                }

                Colliding = true;
            }

            ParentHandler.OnCollisionEnterEvent( this, collision );
        }

        public Collision LatestExitCollision { get; private set; }

        private void OnCollisionExit( Collision collision )
        {
            LatestExitCollision = collision;

            if( CollectCollisions )
            {
                CollisionCapture capture;

                // Self Collision
                if( ParentRagdollProcessor.ContainsPhysicalBoneTransform( collision.transform ) )
                {
                    if( EnteredSelfCollisions.TryGetValue( collision.transform, out capture ) )
                    {
                        capture.Enters -= 1;
                        capture.Lastest = collision;
                        if( capture.Enters <= 0 ) { EnteredSelfCollisions.Remove( collision.transform ); }
                        else EnteredSelfCollisions[collision.transform] = capture;
                    }
                }
                else
                {
                    if( EnteredCollisions.TryGetValue( collision.transform, out capture ) )
                    {
                        capture.Enters -= 1;
                        capture.Lastest = collision;
                        if( capture.Enters <= 0 ) { EnteredCollisions.Remove( collision.transform ); }
                        else EnteredCollisions[collision.transform] = capture;
                    }
                }

                if( UseSelfCollisions )
                {
                    if( EnteredCollisions.Count == 0 && EnteredSelfCollisions.Count == 0 ) Colliding = false;
                }
                else
                {
                    if( EnteredCollisions.Count == 0 ) Colliding = false;
                }
            }
        }

        public override bool IsCollidingWith( Collider collider )
        {
            if( EnteredCollisions == null )
            {
                if( Colliding == false ) return false;
                if( LatestEnterCollision != null ) if( LatestEnterCollision.collider == collider ) return true;
                return false;
            }

            if( LatestEnterNonSelfCollision.collider == collider ) return true;

            foreach( var c in EnteredCollisions )
                if( c.Value.Lastest.collider == collider ) return true;

            return false;
        }

        public override bool CollidesWithAnything()
        {
            if( EnteredCollisions == null ) return false;
            return EnteredCollisions.Count > 0;
        }

        public override Collider GetFirstCollidingCollider()
        {
            if( EnteredCollisions == null ) return null;
            if( EnteredCollisions.Count > 0 )
            {
                var coll = EnteredCollisions.FirstOrDefault();
                if( coll.Value.Lastest != null ) return coll.Value.Lastest.collider;
            }
            return null;
        }

        #region Editor Class

#if UNITY_EDITOR

        [UnityEditor.CanEditMultipleObjects]
        [UnityEditor.CustomEditor( typeof( RA2BoneCollisionHandler ), true )]
        public class RagdollAnimator2BoneCollisionHandlerEditor : RagdollAnimator2BoneIndicatorEditor
        {
            public RA2BoneCollisionHandler Get
            { get { if( _get == null ) _get = (RA2BoneCollisionHandler)target; return _get; } }
            private RA2BoneCollisionHandler _get;

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if( Get.CollectCollisions == false )
                {
                    EditorGUILayout.HelpBox( "You need to enable collecting collisions in order to detect ' Colliding = true ' properly!", UnityEditor.MessageType.Info );
                }

                if( Get.EnteredSelfCollisions != null && Get.EnteredSelfCollisions.Count > 0 )
                {
                    EditorGUILayout.LabelField( "Entered Self Colliders: " );

                    foreach( var item in Get.EnteredSelfCollisions )
                    {
                        if( item.Value.Lastest == null ) continue;
                        EditorGUILayout.ObjectField( item.Value.Lastest.collider, typeof( Collider ), true );
                    }

                    GUILayout.Space( 6 );
                }

                if( Get.EnteredCollisions == null ) return;
                if( Get.EnteredCollisions.Count == 0 ) return;

                EditorGUILayout.LabelField( "Entered: " );

                foreach( var item in Get.EnteredCollisions )
                {
                    if( item.Value.Lastest == null ) continue;
                    EditorGUILayout.ObjectField( item.Value.Lastest.collider, typeof( Collider ), true );
                }
            }
        }

#endif

        #endregion Editor Class
    }
}