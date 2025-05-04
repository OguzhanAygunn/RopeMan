using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_ObstacleSceneReceiver : MonoBehaviour, IRagdollAnimator2Receiver
    {
        private float ResetOnDelay = 0.15f;
        private float accumulatedVelocity = 0f;
        private float lastHitTime = -100f;

        private float lastAppliedImpulse = 0f;

        public void RagdollAnimator2_OnCollisionEnterEvent( RA2BoneCollisionHandler hitted, Collision mainCollision )
        {
            var obstacle = mainCollision.GetContact( 0 ).otherCollider;

            Demo_Ragd_TripThreshold trip = obstacle.GetComponent<Demo_Ragd_TripThreshold>();

            if( trip )
            {
                float hitImpulse = mainCollision.impulse.magnitude;
                trip.LastImpulsePower = hitImpulse;

                if( Time.fixedUnscaledTime - lastHitTime > ResetOnDelay ) accumulatedVelocity = 0f;
                lastHitTime = Time.fixedUnscaledTime;
                accumulatedVelocity += hitImpulse;

                if( hitImpulse >= trip.HitApplyThreshold )
                {
                    lastAppliedImpulse = hitImpulse;

                    hitted.ParentRagdollProcessor.User_SwitchFallState( RagdollHandler.EAnimatingMode.Falling );

                    if( trip.HitImpact != 0f )
                    {
                        // Apply counter impact
                        hitted.DummyBoneRigidbody.AddForce( mainCollision.impulse * trip.HitImpact, ForceMode.Impulse );
                    }
                }
            }
        }

        #region Editor Class

#if UNITY_EDITOR

        [UnityEditor.CustomEditor( typeof( Demo_Ragd_ObstacleSceneReceiver ) )]
        public class Demo_Ragd_ObstacleSceneReceiverEditor : UnityEditor.Editor
        {
            public Demo_Ragd_ObstacleSceneReceiver Get
            { get { if( _get == null ) _get = (Demo_Ragd_ObstacleSceneReceiver)target; return _get; } }
            private Demo_Ragd_ObstacleSceneReceiver _get;

            public override bool RequiresConstantRepaint()
            { return Application.isPlaying; }

            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                if( Get.lastAppliedImpulse != 0f ) UnityEditor.EditorGUILayout.LabelField( "Last Applied Impulse: " + Get.lastAppliedImpulse );
            }
        }

#endif

        #endregion Editor Class
    }
}