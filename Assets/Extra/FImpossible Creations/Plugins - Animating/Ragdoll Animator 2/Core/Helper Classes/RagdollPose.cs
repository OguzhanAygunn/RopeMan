using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [System.Serializable]
    public class RagdollPose
    {
        [HideInInspector] public List<BonePose> BonePoses = new List<BonePose>();
        public Transform LastBaseTransform = null;

        public void ClearPose()
        {
            BonePoses.Clear();
        }

        public void UpdateBone( Transform bone, Transform baseTransform )
        {
            LastBaseTransform = baseTransform;

            for( int i = 0; i < BonePoses.Count; i++ )
            {
                if( BonePoses[i].SourceBone == bone )
                {
                    BonePoses[i].RefreshData( baseTransform );
                    return;
                }
            }

            BonePose pose = new BonePose();
            pose.SourceBone = bone;
            pose.RefreshData( baseTransform );
            BonePoses.Add( pose );
        }

        public BonePose? Contains( Transform bone )
        {
            for( int i = 0; i < BonePoses.Count; i++ )
            {
                if( BonePoses[i].SourceBone == bone ) return BonePoses[i];
            }

            return null;
        }

        public bool CheckIfAnyDiffers( Transform baseTransform )
        {
            if( LastBaseTransform != baseTransform ) return true;
            CheckForNulls();

            for( int i = 0; i < BonePoses.Count; i++ )
            {
                var pose = BonePoses[i];
                if( pose.localPosition != pose.SourceBone.localPosition ) return true;
                if( pose.localRotation != pose.SourceBone.localRotation ) return true;
            }

            return false;
        }

        public void CheckForNulls()
        {
            for( int i = BonePoses.Count - 1; i >= 0; i-- )
            {
                if( BonePoses[i].SourceBone == null ) BonePoses.RemoveAt( i );
            }
        }

        public void ApplyPose( Transform baseTransform )
        {
            if( baseTransform != null && baseTransform == LastBaseTransform )
            {
                for( int b = 0; b < BonePoses.Count; b++ ) BonePoses[b].ApplyOnScene( baseTransform );
                return;
            }

            for( int b = 0; b < BonePoses.Count; b++ ) BonePoses[b].ApplyOnScene();
        }

        [System.Serializable]
        public struct BonePose
        {
            public Transform SourceBone;

            public Vector3 localPosition;
            public Vector3 rootSpacePosition;

            public Quaternion localRotation;
            public Quaternion rootSpaceRotation;

            public void RefreshData( Transform baseTransform )
            {
                if( SourceBone == null ) return;

                localPosition = SourceBone.localPosition;
                localRotation = SourceBone.localRotation;

                rootSpacePosition = baseTransform.InverseTransformPoint( SourceBone.position );
                rootSpaceRotation = FEngineering.QToLocal( baseTransform.rotation, SourceBone.rotation );
            }

            public void ApplyOnScene()
            {
                SourceBone.localPosition = localPosition;
                SourceBone.localRotation = localRotation;
                OnChange();
            }

            public void ApplyOnScene( Transform baseTransform )
            {
                SourceBone.position = baseTransform.TransformPoint( rootSpacePosition );
                SourceBone.rotation = FEngineering.QToWorld( baseTransform.rotation, rootSpaceRotation );
                OnChange();
            }

            #region Editor Code

            /// <summary> Calling setDirty for editor use </summary>
            private void OnChange()
            {
#if UNITY_EDITOR
                if( SourceBone == null ) return;
                UnityEditor.EditorUtility.SetDirty( SourceBone );
#endif
            }

            #endregion Editor Code
        }
    }
}