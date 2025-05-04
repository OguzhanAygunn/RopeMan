using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    /// <summary>
    /// Ragdoll Animator 2 bones chain type
    /// </summary>
    public enum ERagdollChainType : int
    {
        Unknown = 128, Core = 2, LeftArm = 4, RightArm = 8, LeftLeg = 16, RightLeg = 32, OtherLimb = 64
    }


    /// <summary>
    /// Ragdoll Animator 2 configurable joint axis type
    /// </summary>
    public enum EJointAxis
    {
        X, Y, Z, Custom
    }

    /// <summary>
    /// Ragdoll Animator 2 enum
    /// </summary>
    public enum ERagdollLogic
    {
        [Tooltip("Main Ragdoll Animator purpose, using all features for active and animated ragdoll")]
        ActiveRagdoll,

        [Tooltip("Very limited features, no animation matching, just letting unity physics control over character ragdoll source bones. DISABLE mecanim animator to let character fall on the ground.")]
        JustBoneComponents
    }

    /// <summary>
    /// Ragdoll Animator 2 indicator for get up animation type to play
    /// </summary>
    public enum ERagdollGetUpType
    {
        None, FromBack, FromFacedown, FromLeftSide, FromRightSide
    }

    /// <summary>
    /// Ragdoll Animator 2 enum.
    /// Same as Unity's HumanBodyBones but excluding finger and other no ragdoll related bones
    /// </summary>
    public enum ERagdollBoneID
    {
        Unknown = -1,
        Hips = 0,
        LeftUpperLeg = 1,
        RightUpperLeg = 2,
        LeftLowerLeg = 3,
        RightLowerLeg = 4,
        LeftFoot = 5,
        RightFoot = 6,
        Spine = 7,
        Chest = 8,
        UpperChest = 54,
        Neck = 9,
        Head = 10,
        LeftShoulder = 11,
        RightShoulder = 12,
        LeftUpperArm = 13,
        RightUpperArm = 14,
        LeftLowerArm = 15,
        RightLowerArm = 16,
        LeftHand = 17,
        RightHand = 18,
        LeftToes = 19,
        RightToes = 20,
        Tail = 21,
        Item = 22
    }

    /// <summary>
    /// Ragdoll Animator 2 category for GUI
    /// </summary>
    public enum EBoneChainCategory
    {
        Setup, Colliders, Physics
    }
}