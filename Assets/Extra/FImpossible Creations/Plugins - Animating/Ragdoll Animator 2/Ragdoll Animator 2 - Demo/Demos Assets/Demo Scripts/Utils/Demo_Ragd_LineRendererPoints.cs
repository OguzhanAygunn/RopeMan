using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    [DefaultExecutionOrder( 100 )]
    public class Demo_Ragd_LineRendererPoints : FimpossibleComponent
    {
        public LineRenderer Line;
        public Transform A;
        public Transform B;
        public Transform Mid;

        [Space( 6 )]
        public Vector3 AOffset = Vector3.zero;

        public Vector3 BOffset = Vector3.zero;
        public Vector3 MidOffset = Vector3.zero;

        private void LateUpdate()
        {
            RefreshLine();
        }

        private void RefreshLine()
        {
            if( Line == null ) return;
            if( A == null ) return;
            if( B == null ) return;

            Line.SetPosition( 0, A.TransformPoint( AOffset ) );

            if( Mid && Line.positionCount > 2 ) Line.SetPosition( 1, Mid.TransformPoint( MidOffset ) );

            Line.SetPosition( Line.positionCount - 1, B.TransformPoint( BOffset ) );
        }

        public override void OnValidate()
        {
            RefreshLine();
            base.OnValidate();
        }
    }
}