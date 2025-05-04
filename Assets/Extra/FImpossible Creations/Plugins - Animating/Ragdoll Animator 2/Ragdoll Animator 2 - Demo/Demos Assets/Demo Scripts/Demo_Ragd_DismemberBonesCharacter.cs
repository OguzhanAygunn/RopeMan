using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_DismemberBonesCharacter : FimpossibleComponent, IRagdollAnimator2Receiver
    {
        public RagdollAnimator2 Ragdoll;
        public AudioSource HitAudio;
        public EDismemberType DismemberType = EDismemberType.Disconnect;

        private RAF_DismembermentManager dismemberementManager;

        private float hitCulldown = 0f;

        private void Start()
        {
            dismemberementManager = Ragdoll.Handler.GetExtraFeature<RAF_DismembermentManager>();

            if( dismemberementManager == null )
            {
                Debug.Log( "No Dismemberement Feature in " + Ragdoll.name + "!" );
                enabled = false;
            }
        }

        public void RagdollAnimator2_OnCollisionEnterEvent( RA2BoneCollisionHandler hitted, Collision mainCollision )
        {
            if( !enabled ) return;
            if( Time.fixedTime - hitCulldown < 0.3f ) return;

            if( mainCollision.gameObject.layer != 4 ) return; // Only water layer collision

            if( HitAudio ) { HitAudio.Play(); hitCulldown = Time.fixedTime - 0.2f; }

            if( hitted.BoneSettings.WasDismembered || hitted.BoneSettings.ParentDismembered ) return; // Already disconnected

            if( mainCollision.impulse.magnitude > 7f )
            {
                dismemberementManager.DismemberBone( hitted.BoneSettings, DismemberType );

                hitted.DummyBoneRigidbody.AddForce( mainCollision.relativeVelocity.normalized, ForceMode.VelocityChange );

                OnDismember( hitted.BodyBoneID, hitted.ChainType );

                hitCulldown = Time.fixedTime;
            }
        }

        private bool llegDism = false;
        private bool rlegDism = false;

        private void OnDismember( ERagdollBoneID bone, ERagdollChainType chain )
        {
            if( chain == ERagdollChainType.LeftLeg )
            {
                llegDism = true;
                Ragdoll.Handler.Mecanim.CrossFadeInFixedTime( "Left Leg", 0.1f );
            }
            else if( chain == ERagdollChainType.RightLeg )
            {
                rlegDism = true;
                Ragdoll.Handler.Mecanim.CrossFadeInFixedTime( "Right Leg", 0.1f );
            }
            else if( chain == ERagdollChainType.LeftArm )
            {
                Ragdoll.Handler.Mecanim.CrossFadeInFixedTime( "Left Arm", 0.1f );
            }
            else if( chain == ERagdollChainType.RightArm )
            {
                Ragdoll.Handler.Mecanim.CrossFadeInFixedTime( "Right Arm", 0.1f );
            }
            else if( chain == ERagdollChainType.Core )
            {
                if( bone == ERagdollBoneID.Head )
                {
                    Ragdoll.Handler.Mecanim.CrossFadeInFixedTime( "Head", 0.1f );
                }
                else
                {
                    Ragdoll.Handler.Mecanim.CrossFadeInFixedTime( "Body", 0.1f );
                }
            }

            if( llegDism && rlegDism )
            {
                if( Ragdoll.Handler.IsFallingOrSleep == false ) Ragdoll.User_SwitchFallState();
                Ragdoll.Handler.AnchorBoneAttach = 0f;
            }
        }

        #region Editor Class

#if UNITY_EDITOR

        [UnityEditor.CanEditMultipleObjects]
        [UnityEditor.CustomEditor( typeof( Demo_Ragd_DismemberBonesCharacter ) )]
        public class Demo_Ragd_DismemberBonesCharacterEditor : UnityEditor.Editor
        {
            public Demo_Ragd_DismemberBonesCharacter Get
            { get { if( _get == null ) _get = (Demo_Ragd_DismemberBonesCharacter)target; return _get; } }
            private Demo_Ragd_DismemberBonesCharacter _get;

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                GUILayout.Space( 4f );
                DrawPropertiesExcluding( serializedObject, "m_Script" );

                serializedObject.ApplyModifiedProperties();

                if( Get.DismemberType == EDismemberType.CustomHandling || Application.isPlaying == false ) GUI.enabled = false;

                if( GUILayout.Button( "Restore" ) )
                {
                    Get.dismemberementManager.RestoreDismemberedBones();
                    Get.Ragdoll.Handler.AnimatingMode = RagdollHandler.EAnimatingMode.Standing;
                    Get.Ragdoll.Handler.AnchorBoneAttach = 0.75f;
                    Get.llegDism = false;
                    Get.rlegDism = false;
                }

                GUI.enabled = true;
            }
        }

#endif

        #endregion Editor Class
    }
}