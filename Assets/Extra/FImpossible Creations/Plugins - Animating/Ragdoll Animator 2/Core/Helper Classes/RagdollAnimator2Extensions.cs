using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static class RagdollAnimator2Extensions
    {
        public static bool IsArm( this ERagdollChainType chain )
        {
            return chain == ERagdollChainType.LeftArm || chain == ERagdollChainType.RightArm;
        }

        public static bool IsRight( this ERagdollChainType chain )
        {
            return chain == ERagdollChainType.RightLeg || chain == ERagdollChainType.RightArm;
        }

        public static bool IsLeft( this ERagdollChainType chain )
        {
            return chain == ERagdollChainType.RightLeg || chain == ERagdollChainType.RightArm;
        }

        public static bool IsLeg( this ERagdollChainType chain )
        {
            return chain == ERagdollChainType.LeftLeg || chain == ERagdollChainType.RightLeg;
        }

        public static bool IsSameMainType( this ERagdollChainType chain, ERagdollChainType oChain )
        {
            if( chain.IsLeg() && oChain.IsLeg() ) return true;
            if( chain.IsArm() && oChain.IsArm() ) return true;
            return chain == oChain;
        }

        public static Vector3 SetAxisValue( this EJointAxis axis, Vector3 target, float value, bool inverse )
        {
            if( axis == EJointAxis.X ) target.x += inverse ? -value : value;
            else if( axis == EJointAxis.Y ) target.y += inverse ? -value : value;
            else if( axis == EJointAxis.Z ) target.z += inverse ? -value : value;
            return target;
        }

        public static Vector3 SetAxisValue( this EJointAxis axis, Vector3 target, float value, Vector3 customValue, bool inverse )
        {
            if( axis == EJointAxis.X ) target.x += inverse ? -value : value;
            else if( axis == EJointAxis.Y ) target.y += inverse ? -value : value;
            else if( axis == EJointAxis.Z ) target.z += inverse ? -value : value;
            else if( axis == EJointAxis.Custom ) target += customValue.normalized * value;
            return target;
        }

        public static Color GetIndexColor( this RagdollHandler handler, int index, float hueOffset = 0f, float alpha = 1f, float sat = 0.85f, float val = 0.85f, float stepMultiplier = 0.3f )
        {
            float h = (float)( index * stepMultiplier ) / ( (float)handler.Chains.Count - 1f );
            Color nColor = Color.HSVToRGB( ( h + hueOffset ) % 1, sat, val );
            nColor.a = alpha;
            return nColor;
        }

        // Chain Extra Operations (Copy/Paste and others)

        /// <summary>
        /// Pasting main chain settings (excluding collider settings and excluding physics settings)
        /// </summary>
        public static void PasteMainSettingsOfOtherChain( this RagdollBonesChain pasteTo, RagdollBonesChain copyFrom )
        {
            if( pasteTo == null || copyFrom == null ) return;

            //pasteTo.ConstantRagdoll = copyFrom.ConstantRagdoll;
            pasteTo.Detach = copyFrom.Detach;
        }

        public static void PasteColliderSettingsOfOtherChain( this RagdollBonesChain pasteTo, RagdollBonesChain copyFrom, bool allowDisplayDialog = true )
        {
            if( pasteTo == null || copyFrom == null ) return;

            if( pasteTo.BoneSetups.Count != copyFrom.BoneSetups.Count )
            {
                Log( "Bones count of " + pasteTo.ChainName + " is different than " + copyFrom.ChainName + " bones count!", allowDisplayDialog );
                return;
            }

            pasteTo.ChainScaleMultiplier = copyFrom.ChainScaleMultiplier;

            for( int c = 0; c < pasteTo.BoneSetups.Count; c++ )
            {
                var copyingFromBone = copyFrom.BoneSetups[c];
                var newBone = pasteTo.BoneSetups[c];
                newBone.PasteColliderSettingsOfOtherBone( copyingFromBone );
            }
        }

        private static RagdollBonesChain _copyingFrom = null;
        private static RagdollChainBone _copyingFromBone = null;
        public static RagdollBonesChain CopyingFrom => _copyingFrom;
        public static RagdollChainBone CopyingFromBone => _copyingFromBone;

        public static void SetCopyingSource( RagdollBonesChain copyFrom )
        {
            _copyingFrom = copyFrom;
        }

        public static void SetCopyingSource( RagdollChainBone copyFrom )
        {
            _copyingFromBone = copyFrom;
        }

        public static void PasteColliderSettingsOfOtherChainSymmetrical( this RagdollBonesChain pasteTo, RagdollBonesChain copyFrom, RagdollHandler handler, bool allowDisplayDialog = true )
        {
            if( pasteTo == null || copyFrom == null ) return;

            if( pasteTo.BoneSetups.Count != copyFrom.BoneSetups.Count )
            {
                Log( "Bones count of " + pasteTo.ChainName + " is different than " + copyFrom.ChainName + " bones count!", allowDisplayDialog );
                return;
            }

            pasteTo.ChainScaleMultiplier = copyFrom.ChainScaleMultiplier;

            for( int c = 0; c < pasteTo.BoneSetups.Count; c++ )
            {
                var copyingFromBone = copyFrom.BoneSetups[c];
                var newBone = pasteTo.BoneSetups[c];
                newBone.PasteColliderSettingsOfOtherBoneSymmetrical( copyingFromBone, handler );
            }

            if( handler.WasInitialized ) handler.User_UpdateAllBonesParametersAfterManualChanges();
        }

        public static void PasteExtraSettingsOfOtherChain( this RagdollBonesChain pasteTo, RagdollBonesChain copyFrom )
        {
            if( pasteTo == null || copyFrom == null ) return;

            for( int c = 0; c < pasteTo.BoneSetups.Count; c++ )
            {
                var copyingFromBone = copyFrom.BoneSetups[c];
                var newBone = pasteTo.BoneSetups[c];
                newBone.PasteExtraSettingsOfOtherBone( copyingFromBone );
            }
        }

        public static void PastePhysicsSettingsOfOtherChain( this RagdollBonesChain pasteTo, RagdollBonesChain copyFrom )
        {
            if( pasteTo == null || copyFrom == null ) return;

            pasteTo.MassMultiplier = copyFrom.MassMultiplier;
            pasteTo.MusclesForce = copyFrom.MusclesForce;
            pasteTo.AxisLimitRange = copyFrom.AxisLimitRange;
            pasteTo.UnlimitedRotations = copyFrom.UnlimitedRotations;
            pasteTo.ConnectedMassOverride = copyFrom.ConnectedMassOverride;
            pasteTo.ConnectedMassScale = copyFrom.ConnectedMassScale;
            pasteTo.AlternativeTensors = copyFrom.AlternativeTensors;
            pasteTo.AlternativeTensorsOnFall = copyFrom.AlternativeTensorsOnFall;
            pasteTo.HardMatchMultiply = copyFrom.HardMatchMultiply;

            for( int c = 0; c < pasteTo.BoneSetups.Count; c++ )
            {
                var copyingFromBone = copyFrom.BoneSetups[c];
                var newBone = pasteTo.BoneSetups[c];
                newBone.PastePhysicsSettingsOfOtherBone( copyingFromBone );
            }
        }

        public static void PastePhysics_Mass_OfOtherChain( this RagdollBonesChain pasteTo, RagdollBonesChain copyFrom )
        {
            if( pasteTo == null || copyFrom == null ) return;

            pasteTo.MassMultiplier = copyFrom.MassMultiplier;

            for( int c = 0; c < pasteTo.BoneSetups.Count; c++ )
            {
                if( c >= copyFrom.BoneSetups.Count ) return;
                var copyingFromBone = copyFrom.BoneSetups[c];
                var newBone = pasteTo.BoneSetups[c];
                newBone.MassMultiplier = copyingFromBone.MassMultiplier;
            }
        }

        public static void PastePhysicsSettingsOfOtherChainSymmetrical( this RagdollBonesChain pasteTo, RagdollBonesChain copyFrom )
        {
            if( pasteTo == null || copyFrom == null ) return;

            pasteTo.MassMultiplier = copyFrom.MassMultiplier;
            pasteTo.MusclesForce = copyFrom.MusclesForce;
            pasteTo.AxisLimitRange = -copyFrom.AxisLimitRange;
            pasteTo.UnlimitedRotations = copyFrom.UnlimitedRotations;
            pasteTo.HardMatchMultiply = copyFrom.HardMatchMultiply;

            for( int c = 0; c < pasteTo.BoneSetups.Count; c++ )
            {
                var copyingFromBone = copyFrom.BoneSetups[c];
                var newBone = pasteTo.BoneSetups[c];
                newBone.PastePhysicsSettingsOfOtherBoneSymmetrical( copyingFromBone );
            }
        }

        public static void ApplyColliderSettingsToAllBonesInChain( this RagdollChainBone settingsOf, RagdollBonesChain applyToChain )
        {
            if( settingsOf == null || applyToChain == null ) return;

            for( int b = 0; b < applyToChain.BoneSetups.Count; b++ )
            {
                var bone = applyToChain.BoneSetups[b];
                if( bone == settingsOf ) continue;
                bone.PasteColliderSettingsOfOtherBone( settingsOf );
            }
        }

        public static void PasteColliderSettingsOfOtherBone( this RagdollChainBone pasteTo, RagdollChainBone copyFrom )
        {
            if( pasteTo == null || copyFrom == null ) return;

            if( pasteTo.Colliders.Count != copyFrom.Colliders.Count ) pasteTo.Colliders.Clear();
            while( pasteTo.Colliders.Count < copyFrom.Colliders.Count ) pasteTo.AddColliderSetup();

            for( int i = 0; i < copyFrom.Colliders.Count; i++ )
            {
                pasteTo.Colliders[i].ColliderType = copyFrom.Colliders[i].ColliderType;
                pasteTo.Colliders[i].ColliderCenter = copyFrom.Colliders[i].ColliderCenter;
                pasteTo.Colliders[i].ColliderSizeMultiply = copyFrom.Colliders[i].ColliderSizeMultiply;
                pasteTo.Colliders[i].CapsuleDirection = copyFrom.Colliders[i].CapsuleDirection;
                pasteTo.Colliders[i].ColliderRadius = copyFrom.Colliders[i].ColliderRadius;
                pasteTo.Colliders[i].ColliderLength = copyFrom.Colliders[i].ColliderLength;
                pasteTo.Colliders[i].ColliderBoxSize = copyFrom.Colliders[i].ColliderBoxSize;
                pasteTo.Colliders[i].ColliderMesh = copyFrom.Colliders[i].ColliderMesh;
                pasteTo.Colliders[i].RotationCorrection = copyFrom.Colliders[i].RotationCorrection;
            }
        }

        public static void PasteColliderSizeSettingsOfOtherBone( this RagdollChainBone pasteTo, RagdollChainBone copyFrom )
        {
            if( pasteTo == null || copyFrom == null ) return;

            if( pasteTo.Colliders.Count != copyFrom.Colliders.Count ) pasteTo.Colliders.Clear();
            while( pasteTo.Colliders.Count < copyFrom.Colliders.Count ) pasteTo.AddColliderSetup();

            for( int i = 0; i < copyFrom.Colliders.Count; i++ )
            {
                pasteTo.Colliders[i].ColliderSizeMultiply = copyFrom.Colliders[i].ColliderSizeMultiply;
                pasteTo.Colliders[i].CapsuleDirection = copyFrom.Colliders[i].CapsuleDirection;
                pasteTo.Colliders[i].ColliderRadius = copyFrom.Colliders[i].ColliderRadius;
                pasteTo.Colliders[i].ColliderLength = copyFrom.Colliders[i].ColliderLength;
                pasteTo.Colliders[i].ColliderBoxSize = copyFrom.Colliders[i].ColliderBoxSize;
                pasteTo.Colliders[i].ColliderMesh = copyFrom.Colliders[i].ColliderMesh;
            }
        }

        public static void PasteColliderSettingsOfOtherBoneSymmetrical( this RagdollChainBone pasteTo, RagdollChainBone copyFrom, RagdollHandler handler )
        {
            if( pasteTo == null || copyFrom == null ) return;

            pasteTo.Colliders.Clear();
            while( pasteTo.Colliders.Count < copyFrom.Colliders.Count ) pasteTo.AddColliderSetup();

            Transform baseT = handler.GetBaseTransform();

            for( int i = 0; i < copyFrom.Colliders.Count; i++ )
            {
                Vector3 rootSpace = baseT.InverseTransformPoint( copyFrom.SourceBone.TransformPoint( copyFrom.Colliders[i].ColliderCenter ) );
                rootSpace.x *= -1f;
                pasteTo.Colliders[i].ColliderCenter = pasteTo.SourceBone.InverseTransformPoint( baseT.TransformPoint( rootSpace ) );

                pasteTo.Colliders[i].ColliderType = copyFrom.Colliders[i].ColliderType;
                pasteTo.Colliders[i].ColliderSizeMultiply = copyFrom.Colliders[i].ColliderSizeMultiply;
                pasteTo.Colliders[i].CapsuleDirection = copyFrom.Colliders[i].CapsuleDirection;
                pasteTo.Colliders[i].ColliderRadius = copyFrom.Colliders[i].ColliderRadius;
                pasteTo.Colliders[i].ColliderLength = copyFrom.Colliders[i].ColliderLength;
                pasteTo.Colliders[i].ColliderBoxSize = copyFrom.Colliders[i].ColliderBoxSize;
                pasteTo.Colliders[i].ColliderMesh = copyFrom.Colliders[i].ColliderMesh;
                pasteTo.Colliders[i].RotationCorrection = copyFrom.Colliders[i].RotationCorrection;

                //if( copyFrom.Colliders[i].RotationCorrection != Vector3.zero )
                //{
                //    Quaternion worldRot = pasteTo.SourceBone.rotation * Quaternion.Euler( pasteTo.Colliders[i].RotationCorrection );
                //    worldRot = Quaternion.AngleAxis( 180f, baseT.up ) * worldRot;
                //    pasteTo.Colliders[i].RotationCorrection = FEngineering.QToLocal( pasteTo.SourceBone.rotation, worldRot ).eulerAngles;
                //}
                //else
                //{
                //    pasteTo.Colliders[i].RotationCorrection = Vector3.zero;
                //}
            }
        }

        public static void PasteExtraSettingsOfOtherBone( this RagdollChainBone pasteTo, RagdollChainBone copyFrom )
        {
            if( pasteTo == null || copyFrom == null ) return;
            pasteTo.BoneID = copyFrom.BoneID;
        }

        public static void PastePhysicsSettingsOfOtherBone( this RagdollChainBone pasteTo, RagdollChainBone copyFrom )
        {
            if( pasteTo == null || copyFrom == null ) return;

            pasteTo.MassMultiplier = copyFrom.MassMultiplier;
            pasteTo.ForceMultiplier = copyFrom.ForceMultiplier;
            pasteTo.MainAxis = copyFrom.MainAxis;
            pasteTo.InverseMainAxis = copyFrom.InverseMainAxis;
            pasteTo.TargetMainAxis = copyFrom.TargetMainAxis;
            pasteTo.MainAxisLowLimit = copyFrom.MainAxisLowLimit;
            pasteTo.MainAxisHighLimit = copyFrom.MainAxisHighLimit;
            pasteTo.SecondaryAxis = copyFrom.SecondaryAxis;
            pasteTo.InverseSecondaryAxis = copyFrom.InverseSecondaryAxis;
            pasteTo.TargetSecondaryAxis = copyFrom.TargetSecondaryAxis;
            pasteTo.SecondaryAxisAngleLimit = copyFrom.SecondaryAxisAngleLimit;
            pasteTo.ThirdAxisAngleLimit = copyFrom.ThirdAxisAngleLimit;

            pasteTo.OverrideMaterial = copyFrom.OverrideMaterial;
            pasteTo.UseIndividualParameters = copyFrom.UseIndividualParameters;
            pasteTo.OverrideInterpolation = copyFrom.OverrideInterpolation;
            pasteTo.OverrideDetectionMode = copyFrom.OverrideDetectionMode;
            pasteTo.OverrideDragValue = copyFrom.OverrideDragValue;
            pasteTo.OverrideAngularDrag = copyFrom.OverrideAngularDrag;
            pasteTo.OverrideSpringPower = copyFrom.OverrideSpringPower;
            pasteTo.OverrideSpringDamp = copyFrom.OverrideSpringDamp;
            pasteTo.HardMatchingMultiply = copyFrom.HardMatchingMultiply;
            pasteTo.HardMatchOverride = copyFrom.HardMatchOverride;
            pasteTo.ConnectionMassOverride = copyFrom.ConnectionMassOverride;
            pasteTo.DisableCollisionEvents = copyFrom.DisableCollisionEvents;

            pasteTo.ForceKinematicOnStanding = copyFrom.ForceKinematicOnStanding;
            pasteTo.ForceLimitsAllTheTime = copyFrom.ForceLimitsAllTheTime;
        }

        public static void ApplyPhysicsSettingsToAllBonesInChain( this RagdollChainBone settingsOf, RagdollBonesChain applyToChain )
        {
            if( settingsOf == null || applyToChain == null ) return;

            for( int b = 0; b < applyToChain.BoneSetups.Count; b++ )
            {
                var bone = applyToChain.BoneSetups[b];
                if( bone == settingsOf ) continue;
                bone.PastePhysicsSettingsOfOtherBone( settingsOf );
            }
        }

        public static void PastePhysicsSettingsOfOtherBoneSymmetrical( this RagdollChainBone pasteTo, RagdollChainBone copyFrom )
        {
            if( pasteTo == null || copyFrom == null ) return;

            pasteTo.MassMultiplier = copyFrom.MassMultiplier;
            pasteTo.ForceMultiplier = copyFrom.ForceMultiplier;
            pasteTo.MainAxis = copyFrom.MainAxis;
            pasteTo.InverseMainAxis = copyFrom.InverseMainAxis;
            pasteTo.TargetMainAxis = copyFrom.TargetMainAxis;
            pasteTo.MainAxisLowLimit = copyFrom.MainAxisLowLimit;
            pasteTo.MainAxisHighLimit = copyFrom.MainAxisHighLimit;
            pasteTo.SecondaryAxis = copyFrom.SecondaryAxis;
            pasteTo.InverseSecondaryAxis = copyFrom.InverseSecondaryAxis;
            pasteTo.TargetSecondaryAxis = copyFrom.TargetSecondaryAxis;
            pasteTo.SecondaryAxisAngleLimit = copyFrom.SecondaryAxisAngleLimit;
            pasteTo.ThirdAxisAngleLimit = copyFrom.ThirdAxisAngleLimit;
            pasteTo.MusclesBoost = copyFrom.MusclesBoost;
        }

        #region Editor Code

        private static void Log( string info, bool popup = true )
        {
            Debug.Log( "[Ragdoll Animator 2] " + info );
#if UNITY_EDITOR
            if( popup ) UnityEditor.EditorUtility.DisplayDialog( "Warning! (Ragdoll Animator 2)", info, "Ok" );
#endif
        }

        #endregion Editor Code
    }
}