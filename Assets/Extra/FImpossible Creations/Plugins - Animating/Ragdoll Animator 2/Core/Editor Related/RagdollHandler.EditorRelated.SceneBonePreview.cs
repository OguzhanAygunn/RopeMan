using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
#if UNITY_EDITOR

        [HideInInspector] public List<EditorBoneScenePreview> _EditorBoneScenePreviews = new List<EditorBoneScenePreview>();

        [System.Serializable]
        public struct EditorBoneScenePreview
        {
            public Transform Bone;
            public Quaternion restoreLocalRotation;

            public EditorBoneScenePreview( Transform sourceBone ) : this()
            {
                Bone = sourceBone;
                restoreLocalRotation = sourceBone.localRotation;
            }

            public void Restore()
            {
                if( Bone == null ) return;
                Bone.localRotation = restoreLocalRotation;
            }
        }

        public bool Editor_IsPreviewingBone( Transform sourceBone )
        {
            for( int i = 0; i < _EditorBoneScenePreviews.Count; i++ )
            {
                if( _EditorBoneScenePreviews[i].Bone == sourceBone ) return true;
            }

            return false;
        }

        public void Editor_EndPreviewBone( Transform sourceBone )
        {
            if( sourceBone == null ) return;

            for( int i = 0; i < _EditorBoneScenePreviews.Count; i++ )
            {
                if( _EditorBoneScenePreviews[i].Bone == sourceBone )
                {
                    _EditorBoneScenePreviews[i].Restore();
                    _EditorBoneScenePreviews.RemoveAt( i );
                    return;
                }
            }
        }

        public void Editor_StartPreviewBone( Transform sourceBone )
        {
            _EditorBoneScenePreviews.Add( new EditorBoneScenePreview( sourceBone ) );
        }

        public void Editor_CheckToStopPreviewingAll()
        {
            if( _EditorCategory != RagdollHandler.ERagdollAnimSection.Construct || _Editor_ChainCategory != EBoneChainCategory.Physics )
                Editor_StopPreviewingAll();
        }

        public void Editor_StopPreviewingAll()
        {
            for( int i = 0; i < _EditorBoneScenePreviews.Count; i++ ) _EditorBoneScenePreviews[i].Restore();
            _EditorBoneScenePreviews.Clear();
        }

#endif
    }
}