using FIMSpace.AnimationTools;

#if UNITY_EDITOR

using FIMSpace.FEditor;
using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
#if UNITY_EDITOR

        public void EditorHandles_RagdollConstructorPhysics()
        {
            if( _Editor_SelectedChain < 0 )
            {
                if( _Editor_SelectedChain >= chains.Count ) return;
                for( int i = 0; i < chains.Count; i++ ) EditorHandles_RagdollDrawBonePhysics( chains[i], 0.4f );
                return;
            }
            RagdollBonesChain chain = chains[_Editor_SelectedChain];
            EditorHandles_RagdollDrawBonePhysics( chain );
        }

        public void EditorGizmos_RagdollConstructorDrawBoneMassIndicators()
        {
            Transform refTransform = GetBaseTransform();

            foreach( var chain in chains )
            {
                Vector3 dir = Vector3.one;
                Vector3 sideDir = Vector3.zero;

                if( chain.ChainType == ERagdollChainType.RightLeg ) { dir = Vector3.right; sideDir = Vector3.right; }
                else if( chain.ChainType == ERagdollChainType.LeftLeg ) { dir = Vector3.left; sideDir = Vector3.left; }
                else if( chain.ChainType == ERagdollChainType.Core )
                {
                    dir = new Vector3( -.3f, 0.6f, 0f );
                }
                else if( chain.ChainType == ERagdollChainType.LeftArm ) { dir = new Vector3( -0.7f, 0.7f, 0f ); sideDir = Vector3.left; }
                else if( chain.ChainType == ERagdollChainType.RightArm ) { dir = new Vector3( 0.7f, 0.7f, 0f ); sideDir = Vector3.right; }
                else if( chain.ChainType == ERagdollChainType.OtherLimb ) { dir = Vector3.down; }

                if( chain.ChainType == ERagdollChainType.Core )
                {
                    for( int b = 0; b < chain.BoneSetups.Count; b++ )
                    {
                        float progr = (float)b / (float)( chain.BoneSetups.Count - 1 );
                        Vector3 nDir = Vector3.Lerp( new Vector3( -0.1f, .2f, 0f ), new Vector3( -0.1f, 1f, 0f ), progr );

                        if( refTransform ) nDir = refTransform.TransformDirection( nDir );
                        EditorHandles_DrawBoneMassIndicator( chain.BoneSetups[b], chain, nDir, sideDir );
                    }
                }
                else
                {
                    if( refTransform )
                    {
                        dir = refTransform.TransformDirection( dir );
                        sideDir = refTransform.TransformDirection( sideDir );
                    }

                    foreach( var bone in chain.BoneSetups )
                    {
                        EditorHandles_DrawBoneMassIndicator( bone, chain, dir, sideDir );
                    }
                }
            }
        }

        public void EditorHandles_RagdollDrawBonePhysics( RagdollBonesChain chain, float alpha = 0.75f )
        {
            Handles.color = Gizmos.color;

            for( int b = 0; b < chain.BoneSetups.Count; b++ )
            {
                var bone = chain.BoneSetups[b];
                if( bone.SourceBone == null ) return;

                Transform nextBone;
                if( b < chain.BoneSetups.Count - 1 ) nextBone = chain.BoneSetups[b + 1].SourceBone;
                else nextBone = SkeletonRecognize.GetContinousChildTransform( bone.SourceBone );

                Vector3 nextPos = Vector3.zero;
                if( nextBone == null ) { nextPos = bone.SourceBone.position + ( bone.SourceBone.position - bone.SourceBone.parent.position ) * 0.65f; }
                else nextPos = nextBone.position;
                if( bone._EditorCollFoldout == false ) continue;

                Vector3 continousPointLocal = bone.SourceBone.InverseTransformPoint( nextPos );

                float waveTime = (float)EditorApplication.timeSinceStartup * 5f;
                float sin = Mathf.Sin( waveTime );

                if( sin < 0f )
                {
                    float pSin = -sin;
                    sin = Mathf.Lerp( sin, -( pSin * pSin * pSin ), pSin );
                }
                else
                {
                    sin = Mathf.Lerp( sin, sin * sin * sin, sin );
                }

                float waveFactor = Mathf.InverseLerp( -1f, 1f, sin );

                float twistValue = -Mathf.Lerp( bone.MainAxisLowLimit, bone.MainAxisHighLimit, waveFactor );
                float swingValue = Mathf.Lerp( -bone.SecondaryAxisAngleLimit, bone.SecondaryAxisAngleLimit, waveFactor );
                float thirdValue = Mathf.Lerp( -bone.ThirdAxisAngleLimit, bone.ThirdAxisAngleLimit, waveFactor );
                Vector3 anglesOffset = Vector3.zero;

                if( bone._EditorMainAxisPreview ) anglesOffset = bone.MainAxis.SetAxisValue( anglesOffset, twistValue, bone.TargetMainAxis, bone.InverseMainAxis );
                if( bone._EditorSecondAxisPreview ) anglesOffset = bone.SecondaryAxis.SetAxisValue( anglesOffset, swingValue, bone.TargetSecondaryAxis, bone.InverseSecondaryAxis );
                if( bone._EditorThirdPreview ) anglesOffset += bone.GetThirdAxis() * thirdValue;

                anglesOffset *= chain.AxisLimitRange;

                Quaternion rotationOffset = Quaternion.Euler( anglesOffset );

                bool scenePreview = false;
                for( int i = 0; i < _EditorBoneScenePreviews.Count; i++ )
                {
                    if( _EditorBoneScenePreviews[i].Bone != bone.SourceBone ) continue;
                    _EditorBoneScenePreviews[i].Bone.localRotation = _EditorBoneScenePreviews[i].restoreLocalRotation;
                    _EditorBoneScenePreviews[i].Bone.rotation *= rotationOffset;
                    scenePreview = true;
                    break;
                }

                if( !scenePreview )
                {
                    Matrix4x4 boneMx = Matrix4x4.TRS( bone.SourceBone.position, bone.SourceBone.rotation * rotationOffset, bone.SourceBone.lossyScale );

                    Handles.matrix = boneMx;
                    FGUI_Handles.DrawBoneHandle( Vector3.zero, continousPointLocal );
                    Handles.matrix = Matrix4x4.identity;
                }
            }
        }

#endif
    }
}