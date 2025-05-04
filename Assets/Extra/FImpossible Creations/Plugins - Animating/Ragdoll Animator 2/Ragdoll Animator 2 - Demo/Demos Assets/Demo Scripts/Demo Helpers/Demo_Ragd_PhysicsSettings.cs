using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_PhysicsSettings : FimpossibleComponent
    {
        public float SetFixedTimeStep = 0.01f;
        public float TimeScale = 1f;
        public float GravityY = -9.81f;

        [Tooltip( "Ignore TransparentFX vs TransparentFX collision" )]
        public bool TransparentFXFullIgnore = false;

        private void Start()
        {
            Time.timeScale = TimeScale;
            Physics.gravity = new Vector3( Physics.gravity.x, GravityY, Physics.gravity.z );
            Time.fixedDeltaTime = SetFixedTimeStep;

            if( TransparentFXFullIgnore ) Physics.IgnoreLayerCollision( 1, 1, true );
            else // RESTORE FOR DEMO SCENES ON BUILD
            {
                Physics.IgnoreLayerCollision( 1, 1, false );
            }

            Physics.IgnoreLayerCollision( 1, 4, true );
        }

#if UNITY_EDITOR

        [UnityEditor.CustomEditor( typeof( Demo_Ragd_PhysicsSettings ) )]
        private class Demo_Ragd_PhysicsSettingsEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                UnityEditor.EditorGUILayout.HelpBox( "Setting fixed timestep for more precise impact detection on camera bullets", UnityEditor.MessageType.Info );
                DrawDefaultInspector();
                GUILayout.Space( 4f );
                UnityEditor.EditorGUILayout.HelpBox( "Disabling collision between TransparentFX and Water layer for playmode only!", UnityEditor.MessageType.Warning );
            }
        }

#endif
    }
}