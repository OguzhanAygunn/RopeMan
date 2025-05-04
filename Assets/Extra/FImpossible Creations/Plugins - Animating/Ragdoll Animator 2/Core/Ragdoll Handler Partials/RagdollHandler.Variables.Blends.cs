using System;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
        /// <summary> If ragdoll is during initialization and forcing anchor to be kinematic to avoid jiggles (fixed timesteps to force kinematic) </summary>
        public int ForcingKinematicAnchor { get; private set; } = 0;

        /// <summary> Can be used to do extra fade off for ragdoll animator </summary>
        [NonSerialized] public float CustomRagdollBlendMultiplier = 1f;

        /// <summary> Internal ragdoll handler blending factor </summary>
        public float LODBlend { get; protected set; } = 1f;

        /// <summary> Internal ragdoll handler blending factor </summary>
        public float StandUpTransitionBlend { get; protected set; } = 1f;

        /// <summary> Last time switched to standing mode (unscaled time) </summary>
        public float LastStandingModeAtTime { get; protected set; } = -1f;

        /// <summary> Internal ragdoll handler blending factor </summary>
        internal float FadeInBlend { get; private set; } = 1f;

        protected float _sd_fadeIn = 0f;

        /// <summary> Calculating all ragdoll blend multiplier to give target ragdoll handler blend </summary>
        public float GetTotalBlend()
        {
            return RagdollBlend * CustomRagdollBlendMultiplier * LODBlend * StandUpTransitionBlend * FadeInBlend;
        }

        private void ResetFadeInBlend()
        {
            if( FadeInAnimation > 0f ) FadeInBlend = 0f; else FadeInBlend = 1f;
        }

        private void CalculateFadeIn()
        {
            if( FadeInBlend < 1f )
            {
                FadeInBlend = Mathf.SmoothDamp( FadeInBlend, 1.01f, ref _sd_fadeIn, FadeInAnimation, 1000000f, delta );
                if( FadeInBlend > 1f ) FadeInBlend = 1f;
                RefreshAllChainsDynamicParameters();
            }
        }

        /// <summary> Helper class which gets access to internal ragdoll handler optimization values </summary>
        public class OptimizationHandler
        {
            private RagdollHandler ragdollHandler;

            public OptimizationHandler( RagdollHandler ragdoll )
            {
                ragdollHandler = ragdoll;
            }

            public void TurnOffTick( float delta )
            {
                if( ragdollHandler.LODBlend <= 0f )
                {
                }
                else
                {
                    ragdollHandler.LODBlend = Mathf.MoveTowards( ragdollHandler.LODBlend, 0f, delta * 5f );
                }
            }

            public void TurnOnTick( float delta )
            {
                if( ragdollHandler.LODBlend < 1f )
                {
                    ragdollHandler.LODBlend = Mathf.MoveTowards( ragdollHandler.LODBlend, 1f, delta * 4f );
                }
            }
        }
    }
}