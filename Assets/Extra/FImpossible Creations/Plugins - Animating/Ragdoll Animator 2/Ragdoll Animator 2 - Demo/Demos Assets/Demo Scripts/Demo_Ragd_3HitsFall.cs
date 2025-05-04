using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_3HitsFall : MonoBehaviour, IRagdollAnimator2Receiver
    {
        public LayerMask DetectHitsOn = 0 << 0;
        public Color FullHP = Color.green;
        public Color HP2 = Color.green;
        public Color HP1 = Color.green;
        public Color HP0 = Color.red;
        public SkinnedMeshRenderer Skin;
        public float FallImpactPower = 1f;
        public float FallImpactDuration = 0.1f;
        public float DamageAtVelocity = 4f;
        internal float lastImpulse = 0f;

        private int HP = 3;

        /// <summary> For Collisions Culldown </summary>
        private float hitTime = 0f;

        private void Start()
        {
            SetColor( FullHP );
        }

        public void RagdollAnimator2_OnCollisionEnterEvent( RA2BoneCollisionHandler hitted, Collision mainCollision )
        {
            if( Time.fixedTime - hitTime < 0.25f ) return;
            if( RagdollHandlerUtilities.LayerMaskContains( DetectHitsOn, mainCollision.collider.gameObject.layer ) == false ) return;

            float hitImpulsePower = mainCollision.impulse.magnitude;
            lastImpulse = hitImpulsePower;

            if( hitImpulsePower < DamageAtVelocity ) return;

            hitTime = Time.fixedTime;
            HP -= 1;

            if( HP >= 0 )
            {
                if( HP == 2 ) SetColor( HP2 );
                else if( HP == 1 ) SetColor( HP1 );
                else if( HP == 0 ) SetColor( HP0 );
            }

            if( HP == 0 )
            {
                var ragdollHandler = hitted.ParentHandler;
                ragdollHandler.User_SwitchFallState( RagdollHandler.EAnimatingMode.Falling );

                Vector3 impactDirection = mainCollision.relativeVelocity.normalized;

                // Push whole ragdoll with some force
                ragdollHandler.User_AddAllBonesImpact( impactDirection * FallImpactPower, FallImpactDuration, ForceMode.Acceleration );

                // Empathise hitted limb with impact
                ragdollHandler.User_AddRigidbodyImpact( hitted.DummyBoneRigidbody, impactDirection * FallImpactPower, FallImpactDuration, ForceMode.VelocityChange );
            }
        }

        public void ResetHP()
        {
            HP = 3;
            SetColor( FullHP );
        }

        private void SetColor( Color c )
        {
            var materials = Skin.materials;

            materials[0].color = c;
            materials[1].color = c * 0.55f;

            Skin.materials = materials;
        }

        #region Editor Class

#if UNITY_EDITOR

        [UnityEditor.CanEditMultipleObjects]
        [UnityEditor.CustomEditor( typeof( Demo_Ragd_3HitsFall ) )]
        public class Demo_Ragd_3HitsFallEditor : UnityEditor.Editor
        {
            public Demo_Ragd_3HitsFall Get
            { get { if( _get == null ) _get = (Demo_Ragd_3HitsFall)target; return _get; } }
            private Demo_Ragd_3HitsFall _get;

            public override bool RequiresConstantRepaint()
            { return Application.isPlaying; }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                GUILayout.Space( 4f );
                DrawPropertiesExcluding( serializedObject, "m_Script" );

                serializedObject.ApplyModifiedProperties();

                if( Get.lastImpulse != 0f ) UnityEditor.EditorGUILayout.LabelField( "Last Impulse: " + Get.lastImpulse );
            }
        }

#endif

        #endregion Editor Class
    }
}