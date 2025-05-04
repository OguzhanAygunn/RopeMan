using FIMSpace.AnimationTools;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollBonesChain
    {
        public void AutoAdjustColliders(bool isHumanoid)
        {
            if (ChainType == ERagdollChainType.Core || ChainType == ERagdollChainType.Unknown)
            {
                // Adjust with specific logic
                AutoAdjustColliders_Core(isHumanoid);
            }
            else // Limb adjust, per bone reference
            {
                AutoAdjustColliders_Limb();
            }
        }

        /// <summary>
        /// Automatically adjusting colliders parameters in order to fit with child objects.
        /// </summary>
        public void AutoAdjustColliders_Limb()
        {
            for (int i = 0; i < BoneSetups.Count; i++)
            {
                // Check parenting
                Transform source = BoneSetups[i].SourceBone;
                if (source == null) continue;

                Transform next = null;
                if (i < BoneSetups.Count - 1) next = BoneSetups[i + 1].SourceBone;
                if (next == null) next = SkeletonRecognize.GetContinousChildTransform(source);

                var bone = BoneSetups[i];

                // Compute reference values
                Vector3 startPos = source.position;

                // Shifting a bit back first boen if its arm or leg
                if (ChainType.IsLeg() || ChainType.IsArm())
                {
                    if (i == 0 && source.parent) startPos = Vector3.LerpUnclamped(source.parent.position, startPos, 0.75f);
                }

                if (next != null)
                {
                    AdjustColliderSettingsBasingOnTheStartEndPosition(bone, i, startPos, next.position);
                }
                else // Ensure that bone without specific adjustements is scaled properly with the source bone lossy scale
                {
                    if (bone.SourceBone && bone.BaseColliderSetup != null)
                    {
                        if (bone.SourceBone.lossyScale.x != 0f)
                        {
                            bone.BaseColliderSetup.ColliderBoxSize = Vector3.one * (1f / bone.SourceBone.lossyScale.x);
                            bone.BaseColliderSetup.ColliderLength = 1f / bone.SourceBone.lossyScale.y;
                            bone.BaseColliderSetup.ColliderRadius = 1f / bone.SourceBone.lossyScale.x;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Automatically adjusting colliders parameters, basing on parenting and calculation of whole skeleton + found meshes, in order to fit chain with mesh/skeleton.
        /// </summary>
        public void AutoAdjustColliders_Core(bool isHumanoid)
        {
            if (BoneSetups.Count < 1) return;
            if (BoneSetups[0].SourceBone == null) return;

            Transform baseT = ParentHandler.GetBaseTransform();
            Vector3 startPosition = BoneSetups[0].SourceBone.position;
            Vector3 targetEndPosition = BoneSetups[0].SourceBone.position;
            Vector3 mainDirection = (startPosition - baseT.position).normalized;

            for (int i = 0; i < BoneSetups.Count; i++)
            {
                // Check parenting correctness
                Transform source = BoneSetups[i].SourceBone;
                if (source == null) continue;

                var bone = BoneSetups[i];

                if (i == BoneSetups.Count - 1 && isHumanoid) // Final Bone (if chain of 1 length then first bone is trated as last bone) - for humanoids defining head
                {
                    // Try find mesh for bounds reference
                    List<SkinnedMeshRenderer> skins = new List<SkinnedMeshRenderer>();

                    foreach (var t in baseT.GetComponentsInChildren<Transform>(true))
                    {
                        SkinnedMeshRenderer skin = t.GetComponent<SkinnedMeshRenderer>();
                        if (skin) skins.Add(skin);
                    }

                    Vector3 rootSpaceMainDir = FVectorMethods.ChooseDominantAxis(baseT.InverseTransformDirection(source.position - startPosition));
                    Vector3 localSourcePos = baseT.InverseTransformPoint(startPosition);

                    if (skins.Count > 0) // Find end position for collider, using skinned mesh bounds
                    {
                        float farestVal = GetAxisValue(rootSpaceMainDir, localSourcePos);
                        Vector3 farestLocalPos = localSourcePos;

                        for (int s = 0; s < skins.Count; s++)
                        {
                            var skin = skins[s];
                            Vector3 localMax = baseT.InverseTransformPoint(skin.bounds.max);
                            Vector3 localMin = baseT.InverseTransformPoint(skin.bounds.min);

                            float axisVal = GetAxisValue(rootSpaceMainDir, localMax);
                            if (axisVal > farestVal) { farestVal = axisVal; farestLocalPos = localMax; }

                            axisVal = GetAxisValue(rootSpaceMainDir, localMin);
                            if (axisVal > farestVal) { farestVal = axisVal; farestLocalPos = localMin; }
                        }

                        if (farestLocalPos == localSourcePos) farestLocalPos = baseT.InverseTransformPoint(SkeletonRecognize.GetContinousChildTransform(source).position);
                        else farestLocalPos = SetAxisValue(rootSpaceMainDir, localSourcePos, farestLocalPos);

                        targetEndPosition = baseT.TransformPoint(farestLocalPos);
                    }
                    else // Generate using farest found bone
                    {
                        // Find farest child bone in colliders direction axis
                        float farestVal = GetAxisValue(rootSpaceMainDir, localSourcePos);
                        Transform farestT = source;

                        foreach (var t in source.GetComponentsInChildren<Transform>(true))
                        {
                            Vector3 localPos = baseT.InverseTransformPoint(t.position);
                            float axisVal = GetAxisValue(rootSpaceMainDir, localPos);
                            if (axisVal > farestVal) { farestVal = axisVal; farestT = t; }
                        }

                        if (farestT == source) farestT = SkeletonRecognize.GetContinousChildTransform(source);
                        targetEndPosition = farestT.position + (farestT.position - startPosition) * 0.3f;
                    }

                    AdjustColliderSettingsBasingOnTheStartEndPosition(bone, i, startPosition, targetEndPosition);
                }
                else if (i == 0) // First Bone
                {
                    if (source.childCount > 1) // Looking for relation with leg bones
                    {
                        Vector3 midChildBones = Vector3.zero;
                        float count = 0f;

                        for (int c = 0; c < source.childCount; c++)
                        {
                            var child = source.GetChild(c);
                            if (child == source) continue;
                            //if( Vector3.Dot( mainDirection, ( child.position - source.position ).normalized ) > -0.05f ) continue;

                            if (count == 0) midChildBones = child.position;
                            else midChildBones = Vector3.LerpUnclamped(midChildBones, child.position, 0.5f);
                            count += 1f;
                        }

                        startPosition = Vector3.LerpUnclamped(midChildBones, startPosition, count == 2f ? 0.3f : 0.75f);
                        targetEndPosition = BoneSetups[i + 1].SourceBone.position;
                    }
                    else // No leg bones?
                    {
                        startPosition = Vector3.LerpUnclamped(baseT.position, startPosition, 0.9f);
                        targetEndPosition = BoneSetups[i + 1].SourceBone.position;
                    }
                }
                else // Middle Bones - same as limb adjustement
                {
                    if (i + 1 < BoneSetups.Count)
                    {
                        if (BoneSetups[i + 1].SourceBone == null) { UnityEngine.Debug.Log("[Ragdoll Animator 2] Ragdoll Generator - Null bone in " + ChainName + " chain!"); return; }
                        targetEndPosition = BoneSetups[i + 1].SourceBone.position;
                    }
                    else // Adjust for non humanoids - using length of previous bone
                    {
                        if (BoneSetups[i].SourceBone == null) return;
                        if (BoneSetups[i - 1].SourceBone == null) return;
                        targetEndPosition = BoneSetups[i].SourceBone.position + (BoneSetups[i].SourceBone.position - BoneSetups[i - 1].SourceBone.position);
                    }
                }

                // Set collider parameters
                mainDirection = AdjustColliderSettingsBasingOnTheStartEndPosition(bone, i, startPosition, targetEndPosition);

                // Align next collider accordingly to end of this collider
                startPosition = targetEndPosition;
            }
        }

        /// <summary>
        /// Adjusting size of the collider basing on the from-to position.
        /// Returns collider direction.
        /// </summary>
        public Vector3 AdjustColliderSettingsBasingOnTheStartEndPosition(RagdollChainBone bone, int boneIndex, Vector3 startPosition, Vector3 targetEndPosition)
        {
            // Compute reference values
            Vector3 diff = targetEndPosition - startPosition;
            Vector3 dir = diff.normalized;
            float diffLocalMagn = bone.SourceBone.InverseTransformVector(diff).magnitude;
            Vector3 midPoint = Vector3.LerpUnclamped(targetEndPosition, startPosition, 0.5f);

            float sourceScale = bone.SourceBone.lossyScale.x;
            if (sourceScale != 0f) sourceScale = 1f / sourceScale; else sourceScale = 1f;

            float avgRadius = GetChainAverageRadius(boneIndex) * sourceScale;

            // Define collider direction and direction related values
            Vector3 colliderDir = bone.SourceBone.InverseTransformVector(dir);
            colliderDir = FVectorMethods.ChooseDominantAxis(colliderDir);

            bone.BaseColliderSetup.ColliderBoxSize = Vector3.one * (avgRadius * 1.5f);
            bone.BaseColliderSetup.ColliderLength = diffLocalMagn;
            AdjustColliderDirectionParams(bone, colliderDir, diffLocalMagn);

            // Collider center and size
            bone.BaseColliderSetup.ColliderCenter = bone.SourceBone.InverseTransformPoint(midPoint);
            bone.BaseColliderSetup.ColliderRadius = Mathf.Min(bone.BaseColliderSetup.ColliderLength, avgRadius);
            if (bone.BaseColliderSetup.ColliderLength / 2f < bone.BaseColliderSetup.ColliderRadius) bone.BaseColliderSetup.ColliderLength = bone.BaseColliderSetup.ColliderRadius * 2f;
            if (bone.BaseColliderSetup.ColliderType == RagdollChainBone.EColliderType.Sphere) bone.BaseColliderSetup.ColliderRadius = diffLocalMagn / 2f;
            return colliderDir;
        }

        /// <summary>
        /// Collider direction parameters
        /// </summary>
        private void AdjustColliderDirectionParams(RagdollChainBone bone, Vector3 colliderDir, float diffLocalMagn)
        {
            if (colliderDir.x > 0.1f || colliderDir.x < -0.1f)
            {
                bone.BaseColliderSetup.ColliderBoxSize.x = diffLocalMagn;
                bone.BaseColliderSetup.CapsuleDirection = RagdollChainBone.ECapsuleDirection.X;
            }
            if (colliderDir.y > 0.1f || colliderDir.y < -0.1f)
            {
                bone.BaseColliderSetup.ColliderBoxSize.y = diffLocalMagn;
                bone.BaseColliderSetup.CapsuleDirection = RagdollChainBone.ECapsuleDirection.Y;
            }
            if (colliderDir.z > 0.1f || colliderDir.z < -0.1f)
            {
                bone.BaseColliderSetup.ColliderBoxSize.z = diffLocalMagn;
                bone.BaseColliderSetup.CapsuleDirection = RagdollChainBone.ECapsuleDirection.Z;
            }
        }

        private float GetAxisValue(Vector3 axis, Vector3 getFrom)
        {
            if (axis.x > 0.1f || axis.x < -0.1f) return getFrom.x;
            if (axis.y > 0.1f || axis.y < -0.1f) return getFrom.y;
            if (axis.z > 0.1f || axis.z < -0.1f) return getFrom.z;
            return 0f;
        }

        private Vector3 SetAxisValue(Vector3 axis, Vector3 baseValue, Vector3 selectFrom)
        {
            if (axis.x > 0.1f || axis.x < -0.1f) return new Vector3(selectFrom.x, baseValue.y, baseValue.z);
            if (axis.y > 0.1f || axis.y < -0.1f) return new Vector3(baseValue.x, selectFrom.y, baseValue.z);
            if (axis.z > 0.1f || axis.z < -0.1f) return new Vector3(baseValue.x, baseValue.y, selectFrom.z);
            return baseValue;
        }

        /// <summary>
        /// Get basic radius values basing on the humanoid standard scale reference
        /// </summary>
        public float GetChainAverageRadius(int boneIndex)
        {
            if (ChainType == ERagdollChainType.Core)
            {
                if (boneIndex > 1 && boneIndex == BoneSetups.Count - 1)
                    return 0.14f; // Head?
                else
                    return 0.185f; // Body
            }
            else if (ChainType.IsArm())
            {
                if (BoneSetups.Count > 2 && boneIndex == BoneSetups.Count - 1) return 0.05f; // Fist
                return 0.06f;
            }
            else if (ChainType.IsLeg()) return 0.085f;
            else return 0.04f; // Undefined chain
        }

        /// <summary>
        /// Copying all settings of two same type colliders
        /// </summary>
        public static void CopyColliderSettingTo(Collider copyFrom, Collider pasteTo)
        {
            if ((copyFrom is CapsuleCollider) && (pasteTo is CapsuleCollider))
            {
                CapsuleCollider from = copyFrom as CapsuleCollider;
                CapsuleCollider to = pasteTo as CapsuleCollider;
                CopyProvidesContacts(to, from);
                to.center = from.center;
                to.radius = from.radius;
                to.direction = from.direction;
                to.height = from.height;
            }
            else if ((copyFrom is SphereCollider) && (pasteTo is SphereCollider))
            {
                SphereCollider from = copyFrom as SphereCollider;
                SphereCollider to = pasteTo as SphereCollider;
                CopyProvidesContacts(to, from);
                to.center = from.center;
                to.radius = from.radius;
            }
            else if ((copyFrom is BoxCollider) && (pasteTo is BoxCollider))
            {
                BoxCollider from = copyFrom as BoxCollider;
                BoxCollider to = pasteTo as BoxCollider;
                CopyProvidesContacts(to, from);
                to.center = from.center;
                to.size = from.size;
            }
            else if ((copyFrom is MeshCollider) && (pasteTo is MeshCollider))
            {
                MeshCollider from = copyFrom as MeshCollider;
                MeshCollider to = pasteTo as MeshCollider;
                to.convex = from.convex;
                CopyProvidesContacts(to, from);
                to.sharedMesh = from.sharedMesh;
            }

            pasteTo.sharedMaterial = copyFrom.sharedMaterial;
        }

        static void CopyProvidesContacts(Collider to, Collider from)
        {
#if UNITY_2022_1_OR_NEWER
            to.providesContacts = from.providesContacts;
#endif
        }

    }
}