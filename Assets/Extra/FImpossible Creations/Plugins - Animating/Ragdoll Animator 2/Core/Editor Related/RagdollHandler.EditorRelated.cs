using System;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
        [HideInInspector] public int _Editor_SelectedChain = -1;
        [HideInInspector] public EBoneChainCategory _Editor_ChainCategory = EBoneChainCategory.Setup;

        public float CalculateScaleReferenceValue()
        {
            float len = 1f;

            Transform anchorBone = GetAnchorSourceBone();
            if( anchorBone )
            {
                Transform pRef = anchorBone;
                Transform pPar = pRef.parent;

                while( pPar != null )
                {
                    if( ( pPar.position - anchorBone.position ).sqrMagnitude > 0.05f ) { break; }
                    pPar = pPar.parent;
                }

                if( pPar != null ) return Vector3.Distance( pPar.position, anchorBone.position );
            }

            return len;
        }

#if UNITY_EDITOR

        /// <summary> EDITOR ONLY! </summary>
        public void SetTPoseFromAnimationClip()
        {
            AnimationClip tPoseClip = Resources.Load<AnimationClip>( "Ragdoll Animator/Anim Pose - TPose" );
            if( tPoseClip == null ) return;

            if( Mecanim != null )
            {
                bool preRM = Mecanim.applyRootMotion;
                Mecanim.applyRootMotion = false;
                tPoseClip.SampleAnimation( Mecanim.gameObject, 0f );
                Mecanim.applyRootMotion = preRM;
            }
            else
                if( GetBaseTransform() )
                tPoseClip.SampleAnimation( GetBaseTransform().gameObject, 0f );

            foreach( var chain in chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    if( bone.SourceBone != null ) UnityEditor.EditorUtility.SetDirty( bone.SourceBone );
                }
            }
        }

        public enum ERagdollAnimSection
        { None, Construct, Setup, Motion, Extra }

        public enum ERagdollMotionSection
        { Main, Limbs, Extra }

        public enum ERagdollSetupSection
        { Main, Physics }

        [HideInInspector] public ERagdollAnimSection _EditorCategory = ERagdollAnimSection.Construct;
        [HideInInspector] public ERagdollSetupSection _EditorMainCategory = ERagdollSetupSection.Main;
        [HideInInspector] public ERagdollMotionSection _EditorMotionCategory = ERagdollMotionSection.Extra;
        [NonSerialized] public bool _Editor_DrawMassIndicators = false;

        public void Editor_OnSceneGUI()
        {
            if( RagdollLogic == ERagdollLogic.JustBoneComponents && WasInitialized ) return;

            if( _EditorCategory == ERagdollAnimSection.Construct )
            {
                switch( _Editor_ChainCategory )
                {
                    case EBoneChainCategory.Setup: EditorHandles_RagdollConstructorSetup(); break;

                    case EBoneChainCategory.Colliders: EditorHandles_RagdollConstructorCollidersHandles(); break;

                    case EBoneChainCategory.Physics: EditorHandles_RagdollConstructorPhysics(); break;
                }
            }
            else if( _EditorCategory == ERagdollAnimSection.Setup )
            {
                if( _EditorMainCategory == ERagdollSetupSection.Main )
                    EditorHandles_RagdollFundamentalReferences();
                else
                    for( int i = 0; i < chains.Count; i++ ) EditorHandles_RagdollDrawBoneChainSceneSelector( this, chains[i] );
            }
            else if( _EditorCategory == ERagdollAnimSection.Motion )
            {
            }
            else if( _EditorCategory == ERagdollAnimSection.Extra )
            {
            }
            else
            {
                if( WasInitialized ) EditorHandles_RagdollDrawPlaymodePhysicalDummyLines();
            }
        }

        UnityEngine.Object gizmosDrawer = null;
        public void Editor_OnDrawGizmos(UnityEngine.Object toDirty)
        {
            gizmosDrawer = toDirty;

            if( WasInitialized )
            {
                EditorHandles_RagdollDrawPlaymodePhysicalDummyLines( 1f );
                if( RagdollLogic == ERagdollLogic.JustBoneComponents ) return;
            }

            if( _EditorCategory == ERagdollAnimSection.Construct )
            {
                switch( _Editor_ChainCategory )
                {
                    case EBoneChainCategory.Setup:
                        if( _Editor_DrawMassIndicators ) EditorGizmos_RagdollConstructorDrawBoneMassIndicators();
                        break;

                    case EBoneChainCategory.Colliders:
                        EditorGizmos_RagdollConstructorColliders();
                        if( _Editor_DrawMassIndicators ) EditorGizmos_RagdollConstructorDrawBoneMassIndicators();
                        break;

                    case EBoneChainCategory.Physics:
                        if( _Editor_DrawMassIndicators ) EditorGizmos_RagdollConstructorDrawBoneMassIndicators();
                        break;
                }
            }
            else if( _EditorCategory == ERagdollAnimSection.Setup )
            {
                if( WasInitialized == false )
                {
                    if( _EditorMainCategory == ERagdollSetupSection.Main )
                        for( int i = 0; i < chains.Count; i++ ) EditorGizmos_RagdollDrawBone_JustColliders( this, chains[i], 0.75f, true, false );
                    else
                        EditorGizmos_RagdollConstructorDrawJustChainsColliders( false, true );
                }
                else
                    EditorGizmos_RagdollConstructorDrawJustChainsColliders( true, false, false );

                if( _Editor_DrawMassIndicators ) EditorGizmos_RagdollConstructorDrawBoneMassIndicators();
            }
            else if( _EditorCategory == ERagdollAnimSection.Motion )
            {
            }
            else if( _EditorCategory == ERagdollAnimSection.Extra )
            {
            }
            else
            {
            }

            EditorHandles_DrawExtraFeaturesGizmos();
        }

#endif
    }
}