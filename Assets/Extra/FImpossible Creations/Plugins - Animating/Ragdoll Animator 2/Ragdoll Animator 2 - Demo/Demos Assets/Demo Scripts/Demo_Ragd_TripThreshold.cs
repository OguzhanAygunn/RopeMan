using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_TripThreshold : FimpossibleComponent
    {
        public override string HeaderInfo => "Info component to trigger ragdoll fall when hitted it with big enough velocity";

        public float HitApplyThreshold = 20f;
        public float HitImpact = 1f;

        internal float LastImpulsePower = 0f;

        #region Editor Class

#if UNITY_EDITOR

        [UnityEditor.CustomEditor( typeof( Demo_Ragd_TripThreshold ) )]
        public class Demo_Ragd_TripThresholdEditor : FimpossibleComponentEditor
        {
            public Demo_Ragd_TripThreshold Get
            { get { if( _get == null ) _get = (Demo_Ragd_TripThreshold)target; return _get; } }
            private Demo_Ragd_TripThreshold _get;

            public override bool RequiresConstantRepaint()
            { return Application.isPlaying; }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                GUILayout.Space( 4f );
                if( Get.LastImpulsePower != 0f ) UnityEditor.EditorGUILayout.LabelField( "Last Impulse Power: " + Get.LastImpulsePower );
            }
        }

#endif

        #endregion Editor Class
    }
}