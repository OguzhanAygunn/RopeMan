using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_SetParent : FimpossibleComponent
    {
        public Transform TargetParent;

        public bool InitPositionIsLocal = false;
        public Vector3 LocalPosition;
        public Vector3 LocalRotation;

        public override string HeaderInfo => "Simply assigning new parent to this object. It's purpose is to show this object on the scene for quick check what kind of components will be used in playmode.";

        private void Start()
        {
            if( TargetParent == null ) return;
            transform.SetParent( TargetParent, true );

            if( InitPositionIsLocal ) ReadCoords();

            transform.localPosition = LocalPosition;
            transform.localRotation = Quaternion.Euler( LocalRotation );
        }

        private void ReadCoords()
        {
            LocalPosition = TargetParent.InverseTransformPoint( transform.position );
            LocalRotation = FEngineering.QToLocal( TargetParent.rotation, transform.rotation ).eulerAngles;
        }

        #region Editor Class

#if UNITY_EDITOR

        [UnityEditor.CustomEditor( typeof( Demo_Ragd_SetParent ), true )]
        public class Demo_Ragd_SetParentEditor : FimpossibleComponentEditor
        {
            public Demo_Ragd_SetParent Get
            { get { if( _get == null ) _get = (Demo_Ragd_SetParent)target; return _get; } }
            private Demo_Ragd_SetParent _get;

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                GUILayout.Space( 4f );
                if( GUILayout.Button( "Read Current" ) ) Get.ReadCoords();
            }
        }

#endif

        #endregion Editor Class
    }
}