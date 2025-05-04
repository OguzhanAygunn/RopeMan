using System;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
        [Tooltip( "Multiplier for springs, springs damping, hard matching and few other forces responsible for physical animation matching.\nCall User_UpdateJointsPlayParameters() after changing this variable." )]
        [Range( 0f, 1f )]
        public float MusclesPower = 1f;

        /// <summary> Used by Extra Features </summary>
        internal float musclesPowerMultiplier = 1f;

        /// <summary> Calculated muscles power with additional multipliers </summary>
        public float targetMusclesPower { get; private set; } = 1f;

        [Tooltip( "Main unity's joints spring drive value towards desired pose when using animation matching" )]
        public float SpringsValue = 1500f;
        [Tooltip("Value for springs power when switching to Fall Mode from Standing Mode. Zero or lower means springs on fall will use main Springs Power Value.")]
        public float SpringsOnFall = 0f;

        /// <summary> Returning SpringsValue on standing mode, if fall mode and if SpringOnFall is <=0 still returning SpringsValue, else return SpringOnFall </summary>
        internal float GetCurrentMainSpringsValue => IsInFallingMode ? ( SpringsOnFall <= 0 ? SpringsValue : SpringsOnFall ) : SpringsValue;

        /// <summary> Spring value for fall mode, used by Extra Features, can be used in custom way if not conflicts with used Extra Features </summary>
        public float? OverrideSpringsValueOnFall = null;

        [Tooltip( "Main unity's joints damping value for animation matching springs" )]
        public float DampingValue = 40f;

        [Tooltip( "Damping Value when switching to fall mode" )]
        public float DampingValueOnFall = 0f;

        [Tooltip( "Forcing limbs to match with animator pose, can nicely help out animation matching. (It adds a bit cpu cost to the overall component performance, you can try debug button for insight)" )]
        [Range( 0f, 1f )]
        public float HardMatching = 0f;

        [Tooltip("Applying hard animation matching for ragdoll bone positions")]
        public bool HardMatchPositions = false;
        [Tooltip("Applying hard animation matching for ragdoll bone positions also during fall mode")]
        public bool HardMatchPositionsOnFall = false;
        [Tooltip("Use if you want to keep rotation hard matching stronger but position hard matching weaker")]
        public float PositionHardMatchingMultiplier = 1f;

        [Tooltip( "Hard matching during falling mode is usually not needed, so you can switch it off or make it weaker then." )]
        [Range( 0f, 1f )]
        public float HardMatchingOnFalling = 0f;

        [Tooltip("[Only for standing mode] Set zero to compensate body physics reaction on character body movement in world, set 1 to be affected with natural physics reaction to bones movement.")]
        [Range(0f, 1f)]
        public float MotionInfluence = 1f;

        internal bool disableHardMatching = false;
        internal bool disableInterpolation = false;
        internal bool onlyDiscreteDetection = false;

        [Range( 0f, 1f )]
        [Tooltip( "How strictly anchor bone (pelvis) should stick to its animator position when using Standing Animating mode" )]
        public float AnchorBoneSpring = 1f;

        /// <summary> Used for getup transition fade in standing mode </summary>
        [NonSerialized] public float AnchorBoneSpringMultiplier = 1f;

        /// <summary> It's AnchorBoneSpring variable (multiplied by AnchorBoneSpringMultiplier), but with variable name as you see it in the inspector window </summary>
        public float AnchorBoneAttach
        { get { return AnchorBoneSpring * AnchorBoneSpringMultiplier; } set { AnchorBoneSpring = value; } }

        [Tooltip( "When Anchor Bone Spring is set to the max, allowing to switch anchor rigidbody kinematic on standing mode, for max stability.\nIs Kinematic disables velocity memory on the rigidody, you can use anchor limit to maintain similar effect on the anchor but keep the velocity." )]
        public bool MakeAnchorKinematicOnMaxSpring = false;

        [Tooltip("With kinematic anchor, you can make character movement unaffected by physical forces")]
        public bool UnaffectedMovement = false;

        [Tooltip( "(Standing mode) If anchor will get stuck far away from the main object, it will be teleported towards desired controlled position." )]
        public bool AutoUnstuck = false;

        [Tooltip( "(Standing Mode) Freezing hips rigidbody rotation, so it will not be rotated due to collisions. It is actually solving anchor collision jitter in many cases." )]
        public bool LockAnchorRotation = true;

        [Tooltip( "(has effect only with 'No Limits On Standing Mode') Enabling joint rotation limits for anchor bone. Can be used instead of kinematic anchor bone for more controlled stability." )]
        public bool AnchorJointLimits = false;

        [Tooltip( "If Anchor Attach set to zero should be treated as fall mode" )]
        public bool FallOnZeroAnchor = true;

        /// <summary> Anchor bone position power multiplier for extra features </summary>
        internal float anchorBoneSpringPositionMultiplier = 1f;

    }
}