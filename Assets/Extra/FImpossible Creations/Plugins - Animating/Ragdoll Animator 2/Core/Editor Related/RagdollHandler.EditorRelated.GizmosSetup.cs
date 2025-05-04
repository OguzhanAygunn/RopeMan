#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
#if UNITY_EDITOR

        public void EditorHandles_RagdollFundamentalReferences()
        {
            float len = CalculateScaleReferenceValue();

            Transform anchorBone = GetAnchorSourceBone();
            if(/*SkeletonRootBone &&*/ anchorBone )
            {
                Handles.color = new Color( 0.2f, 1f, 0.4f, 1f );
                Handles.DrawAAPolyLine( 3f, anchorBone.position, GetAnchorSourceBone().position );
            }

            if( GetAnchorSourceBone() )
            {
                Handles.color = new Color( 0.2f, 1f, 0.4f, .2f );
                Handles.CircleHandleCap( 0, GetAnchorSourceBone().position, Quaternion.Euler( 90f, 0f, 0f ), len * 0.1f, EventType.Repaint );
            }

            Handles.color = Color.white;
        }

        public void EditorHandles_RagdollConstructorSetup()
        {
            if( _Editor_SelectedChain < 0 )
            {
                if( _Editor_SelectedChain >= chains.Count ) return;
                for( int i = 0; i < chains.Count; i++ ) EditorHandles_RagdollDrawBoneChain( chains[i], 0.5f );
                return;
            }
            RagdollBonesChain chain = chains[_Editor_SelectedChain];
            EditorHandles_RagdollDrawBoneChain( chain );
        }

#endif
    }
}