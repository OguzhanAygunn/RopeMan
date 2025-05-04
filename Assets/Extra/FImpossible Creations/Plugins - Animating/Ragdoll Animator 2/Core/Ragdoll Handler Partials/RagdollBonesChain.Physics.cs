using FIMSpace.AnimationTools;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollBonesChain
    {
        /// <summary>
        /// Adjusting bones mass values, joints axes and limits
        /// </summary>
        public void AutoAdjustPhysics()
        {
            if( BoneSetups.Count == 0 ) return;

            float limbMul = GetChainTypePercentageMass() * 0.01f;

            for( int b = 0; b < BoneSetups.Count; b++ )
            {
                BoneSetups[b].MassMultiplier = GetBoneMassPercentage( b, limbMul ) * 0.01f * 2f; // * 2 for chain mass 0.5
            }

            MassMultiplier = 0.5f;

            AutoAdjustJointsAxes();
            AutoAdjustJointsLimits();
        }

        /// <summary>
        /// Automatically joints axis parameters in order to allow rotations in certain directions.
        /// </summary>
        public void AutoAdjustJointsAxes()
        {
            for( int i = 0; i < BoneSetups.Count; i++ )
            {
                // Check parenting
                Transform source = BoneSetups[i].SourceBone;
                if( source == null ) continue;

                Transform next = null;
                if( i < BoneSetups.Count - 1 ) next = BoneSetups[i + 1].SourceBone;
                if( next == null ) next = SkeletonRecognize.GetContinousChildTransform( source );

                var bone = BoneSetups[i];

                // Compute reference values
                Vector3 startPos = source.position;

                if( next == null ) continue;

                AdjustJointAxesBasingOnTheStartEndPosition( bone, i, startPos, next.position );
            }

            if( ChainType == ERagdollChainType.Core )
            {
                var lastBone = BoneSetups[BoneSetups.Count - 1];
                if( BoneSetups.Count > 2 )
                {
                    if( ParentHandler == null || ParentHandler.GetBaseTransform() == null )
                        lastBone.SetMainAxisByVector( Vector3.Cross( lastBone.GetMainAxis(), lastBone.GetSecondaryAxis() ) );
                    else
                    {
                        lastBone.SetMainAxisByVector( lastBone.SourceBone.InverseTransformDirection( ParentHandler.GetBaseTransform().right ) ); // Pitch rotation
                        lastBone.SetSecondaryAxisByVector( lastBone.SourceBone.InverseTransformDirection( ParentHandler.GetBaseTransform().forward ) ); // Roll rotation
                    }

                    if( lastBone.MainAxis != EJointAxis.Custom && lastBone.MainAxis == lastBone.SecondaryAxis ) // Prevent equal axes
                    {
                        lastBone.SetMainAxisByVector( FVectorMethods.GetCounterAxis( lastBone.GetMainAxis() ) );
                    }
                }
            }
        }

        /// <summary>
        /// Adjusting joints limti axes basing on the from-to position.
        /// </summary>
        private void AdjustJointAxesBasingOnTheStartEndPosition( RagdollChainBone bone, int boneIndex, Vector3 startPosition, Vector3 targetEndPosition )
        {
            // Compute reference values
            Vector3 diff = targetEndPosition - startPosition;
            Vector3 dir = diff.normalized;

            // Define collider direction and direction related values
            Vector3 upDir = bone.SourceBone.InverseTransformVector( dir );

            bone.SetSecondaryAxisByVector( upDir );
            upDir = bone.GetSecondaryAxis();

            bone.SetMainAxisByVector( Vector3.Cross( upDir, -bone.SourceBone.InverseTransformVector( ParentHandler.GetBaseTransform().forward ) ) );

            if( bone.MainAxis != EJointAxis.Custom && bone.MainAxis == bone.SecondaryAxis ) // Prevent equal axes
            {
                bone.SetMainAxisByVector( FVectorMethods.GetCounterAxis( bone.GetMainAxis() ) );
            }
        }

        /// <summary>
        /// Automatically setting joints axis limit parameters in order to allow rotations in certain directions to the specific ranges.
        /// </summary>
        public void AutoAdjustJointsLimits()
        {
            #region First bone adjusting

            var firstBone = BoneSetups[0];

            if( ChainType == ERagdollChainType.Core )
            {
                firstBone.MainAxisLowLimit = -35f;
                firstBone.MainAxisHighLimit = 35f;
                firstBone.SecondaryAxisAngleLimit = 20f;
                firstBone.ThirdAxisAngleLimit = 20f;
            }
            else if( ChainType.IsLeg() )
            {
                firstBone.MainAxisLowLimit = -55f;
                firstBone.MainAxisHighLimit = 45f;
                firstBone.SecondaryAxisAngleLimit = 15f;
                firstBone.ThirdAxisAngleLimit = 45f;
            }
            else if( ChainType.IsArm() )
            {
                firstBone.MainAxisLowLimit = -50f;
                firstBone.MainAxisHighLimit = 75f;
                firstBone.SecondaryAxisAngleLimit = 35f;
                firstBone.ThirdAxisAngleLimit = 55f;
            }
            else
            {
                firstBone.MainAxisLowLimit = -35f;
                firstBone.MainAxisHighLimit = 35f;
                firstBone.SecondaryAxisAngleLimit = 35f;
                firstBone.ThirdAxisAngleLimit = 35f;
            }

            #endregion First bone adjusting

            if( BoneSetups.Count < 2 ) return;

            #region Middle Bones Adjust

            for( int b = 1; b < BoneSetups.Count - 1; b++ )
            {
                var bone = BoneSetups[b];

                if( ChainType == ERagdollChainType.Core )
                {
                    float div = BoneSetups.Count - 2;
                    if( div < 1f ) div = 1f;
                    bone.MainAxisLowLimit = -22f / div;
                    bone.MainAxisHighLimit = 70f / div;
                    bone.SecondaryAxisAngleLimit = 30f;
                    bone.ThirdAxisAngleLimit = 20f;
                }
                else if( ChainType.IsLeg() )
                {
                    bone.MainAxisLowLimit = -60f;
                    bone.MainAxisHighLimit = 10f;
                    bone.SecondaryAxisAngleLimit = 10f;
                    bone.ThirdAxisAngleLimit = 15f;
                }
                else if( ChainType.IsArm() )
                {
                    bone.MainAxisLowLimit = -8f;
                    bone.MainAxisHighLimit = 55f;
                    bone.SecondaryAxisAngleLimit = 10f;
                    bone.ThirdAxisAngleLimit = 10f;
                }
                else
                {
                    bone.MainAxisLowLimit = -30f;
                    bone.MainAxisHighLimit = 30f;
                    bone.SecondaryAxisAngleLimit = 30f;
                    bone.ThirdAxisAngleLimit = 30f;
                }
            }

            #endregion Middle Bones Adjust

            #region Last Bone Adjust

            var lastBone = BoneSetups[BoneSetups.Count - 1];

            if( ChainType == ERagdollChainType.Core )
            {
                lastBone.MainAxisLowLimit = -45f;
                lastBone.MainAxisHighLimit = 30f;
                lastBone.SecondaryAxisAngleLimit = 20f;
                lastBone.ThirdAxisAngleLimit = 55f;
            }
            else if( ChainType.IsLeg() )
            {
                lastBone.MainAxisLowLimit = -40f;
                lastBone.MainAxisHighLimit = 40f;
                lastBone.SecondaryAxisAngleLimit = 15f;
                lastBone.ThirdAxisAngleLimit = 40f;
            }
            else if( ChainType.IsArm() )
            {
                lastBone.MainAxisLowLimit = -75f;
                lastBone.MainAxisHighLimit = 50f;
                lastBone.SecondaryAxisAngleLimit = 90f;
                lastBone.ThirdAxisAngleLimit = 30f;
            }
            else
            {
                lastBone.MainAxisLowLimit = -30f;
                lastBone.MainAxisHighLimit = 30f;
                lastBone.SecondaryAxisAngleLimit = 30f;
                lastBone.ThirdAxisAngleLimit = 30f;
            }

            #endregion Last Bone Adjust
        }

        /// <summary>
        /// Almost like human anatomy (used for auto-settings)
        /// </summary>
        public float GetChainTypePercentageMass()
        {
            float percentage;
            if( ChainType == ERagdollChainType.Core ) percentage = 50f;
            else if( ChainType.IsLeg() ) percentage = 16f;
            else if( ChainType.IsArm() ) percentage = 6f;
            else if( ChainType == ERagdollChainType.OtherLimb ) percentage = 20f;
            else percentage = 16f;

            return percentage;
        }

        /// <summary>
        /// Almost like human anatomy (used for auto-settings)
        /// </summary>
        public float GetBoneMassPercentage( int index, float totalLimbMul )
        {
            if( index == BoneSetups.Count - 1 && BoneSetups.Count > 2 ) // Last bone like hand / foot / head
            {
                if( ChainType == ERagdollChainType.Core ) return totalLimbMul * 12f;
                else if( ChainType.IsLeg() ) return totalLimbMul * 14f;
                else if( ChainType.IsArm() ) return totalLimbMul * 14f;
                else if( ChainType == ERagdollChainType.OtherLimb ) return ( totalLimbMul / (float)BoneSetups.Count ) * 16f;
                else return ( totalLimbMul / (float)BoneSetups.Count ) * 14f;
            }
            else if( index == 0 ) // First bone, in most cases most heavy
            {
                float startBoneDiv = ( BoneSetups.Count - 3 );
                if( startBoneDiv < 1f ) startBoneDiv = 1f;

                if( ChainType == ERagdollChainType.Core ) return ( totalLimbMul * 26f ) / startBoneDiv;
                else if( ChainType.IsLeg() ) return ( totalLimbMul * 50f ) / startBoneDiv;
                else if( ChainType.IsArm() ) return ( totalLimbMul * 45f ) / startBoneDiv;
                else if( ChainType == ERagdollChainType.OtherLimb ) return ( ( totalLimbMul / (float)BoneSetups.Count ) * 18f ) / startBoneDiv;
                else return ( ( totalLimbMul / (float)BoneSetups.Count ) * 18f ) / startBoneDiv;
            }
            else // Chain's middle bones
            {
                float middleCountDiv = ( BoneSetups.Count - 2 );
                if( middleCountDiv < 1f ) middleCountDiv = 1f;

                if( ChainType == ERagdollChainType.Core ) return ( totalLimbMul * 24f ) / middleCountDiv;
                else if( ChainType.IsLeg() ) return ( totalLimbMul * 25f ) / middleCountDiv;
                else if( ChainType.IsArm() ) return ( totalLimbMul * 28f ) / middleCountDiv;
                else if( ChainType == ERagdollChainType.OtherLimb ) return ( ( totalLimbMul / (float)BoneSetups.Count ) * 16f ) / middleCountDiv;
                else return ( ( totalLimbMul / (float)BoneSetups.Count ) * 18f ) / middleCountDiv;
            }
        }

        /// <summary>
        /// Basing on the human anatomy (used for tooltips)
        /// </summary>
        public float GetChainTypePercentageMassReal()
        {
            float percentage;
            if( ChainType == ERagdollChainType.Core ) percentage = 50f + 8f; // + Head 8 %
            else if( ChainType.IsLeg() ) percentage = 16f;
            else if( ChainType.IsArm() ) percentage = 5f;
            else if( ChainType == ERagdollChainType.OtherLimb ) percentage = 5f;
            else percentage = 8f;

            return percentage;
        }

        /// <summary>
        /// Basing on the human anatomy (used for tooltips)
        /// </summary>
        public float GetBoneMassPercentageReal( int index, float totalLimbMul )
        {
            if( index == BoneSetups.Count - 1 && BoneSetups.Count > 2 ) // Last bone like hand / foot / head
            {
                if( ChainType == ERagdollChainType.Core ) return totalLimbMul * 13.79f;
                else if( ChainType.IsLeg() ) return totalLimbMul * 9.375f;
                else if( ChainType.IsArm() ) return totalLimbMul * 14f;
                else if( ChainType == ERagdollChainType.OtherLimb ) return ( totalLimbMul / (float)BoneSetups.Count ) * 0.55f;
                else return ( totalLimbMul / (float)BoneSetups.Count ) * 0.65f;
            }
            else if( index == 0 ) // First bone, in most cases most heavy
            {
                float startBoneDiv = ( BoneSetups.Count - 3 );
                if( startBoneDiv < 1f ) startBoneDiv = 1f;

                if( ChainType == ERagdollChainType.Core ) return ( totalLimbMul * 29.3f ) / startBoneDiv;
                else if( ChainType.IsLeg() ) return ( totalLimbMul * 63f ) / startBoneDiv;
                else if( ChainType.IsArm() ) return ( totalLimbMul * 54f ) / startBoneDiv;
                else if( ChainType == ERagdollChainType.OtherLimb ) return ( ( totalLimbMul / (float)BoneSetups.Count ) * 1f ) / startBoneDiv;
                else return ( ( totalLimbMul / (float)BoneSetups.Count ) * 1f ) / startBoneDiv;
            }
            else // Chain's middle bones
            {
                float middleCountDiv = ( BoneSetups.Count - 2 );
                if( middleCountDiv < 1f ) middleCountDiv = 1f;

                if( ChainType == ERagdollChainType.Core ) return ( totalLimbMul * 26.3f ) / middleCountDiv;
                else if( ChainType.IsLeg() ) return ( totalLimbMul * 27.5f ) / middleCountDiv;
                else if( ChainType.IsArm() ) return ( totalLimbMul * 32f ) / middleCountDiv;
                else if( ChainType == ERagdollChainType.OtherLimb ) return ( ( totalLimbMul / (float)BoneSetups.Count ) * .8f ) / middleCountDiv;
                else return ( ( totalLimbMul / (float)BoneSetups.Count ) * .8f ) / middleCountDiv;
            }
        }

        //void IgnoreCollisionsBetweenBones( RagdollChainBone a, RagdollChainBone b )
        //{
        //    if( b.MainBoneCollider == null ) return;

        //    a.ApplyToAllColliders( ( Collider boneCollider ) =>
        //    {
        //        foreach( var sColl in b.Colliders )
        //        {
        //            Physics.IgnoreCollision( boneCollider, sColl.GameCollider, true );
        //        }
        //    }
        //    );
        //}

        public void EnsureCollisionIgnoreBetweenChildBones()
        {
            if( BoneSetups.Count > 1 )
            {
                var preBone = BoneSetups[0];

                for( int i = 1; i < BoneSetups.Count; i++ )
                {
                    BoneSetups[i].IgnoreCollisionsWith( preBone );
                    //foreach( var preC in preBone.Colliders )
                    //    foreach( var mColl in BoneSetups[i].Colliders )
                    //        mColl.IgnoreCollisionWith( preC, true );

                    preBone = BoneSetups[i];
                }
            }

            if( BoneSetups.Count > 0 && ConnectionBone != null )
            {
                BoneSetups[0].IgnoreCollisionsWith( ConnectionBone );
                //foreach( var preC in BoneSetups[0].Colliders )
                //    foreach( var connC in ConnectionBone.Colliders )
                //        preC.IgnoreCollisionWith( connC, true );
            }
        }

        public void CheckIfShouldIgnoreByBounds( RagdollChainBone otherBone, float boundsSize = 1.1f )
        {
            foreach( var bone in BoneSetups )
            {
                if( bone == otherBone ) continue;
                bone.CheckIfShouldIgnoreByBounds( otherBone, boundsSize );
            }
        }

        /// <summary>
        /// Proceed overall collider scale up/down for Physics.ComputePenetration calculations
        /// </summary>
        private void ScaleCollider( Collider c, float scale )
        {
            if( c is BoxCollider )
            {
                var b = c as BoxCollider;
                b.size *= scale;
                b.center *= scale;
            }
            else if( c is SphereCollider )
            {
                var s = c as SphereCollider;
                s.radius *= scale;
                s.center *= scale;
            }
            else if( c is CapsuleCollider )
            {
                var cps = c as CapsuleCollider;
                cps.height *= scale;
                cps.radius *= scale;
                cps.center *= scale;
            }
        }

        public void EnsureCollisionIgnoreBetweenBonesUsingBounds( List<RagdollBonesChain> chains, float scaleUpFactor = 1.2f )
        {
            foreach( var chain in chains )
            {
                foreach( var myBone in BoneSetups )
                {
                    if( myBone.MainBoneCollider == null ) continue;
                    if( myBone.BoundedIgnoreScale <= 0f ) continue;

                    foreach( var myC in myBone.Colliders )
                    {
                        Collider coll = myC.GameCollider;

                        if( coll.transform.lossyScale.x == 0f )
                        {
                            UnityEngine.Debug.Log( "[Ragdoll Animator 2] Detected zero scale object! It is not supported! (" + coll.transform.name + ")" );
                            continue;
                        }

                        // Bounds for mesh colliders
                        Bounds myBounds = coll.bounds;
                        myBounds.size *= scaleUpFactor * myC.BoundedIgnoreScale * myBone.BoundedIgnoreScale;

                        ScaleCollider( coll, coll.transform.lossyScale.x * scaleUpFactor * myBone.BoundedIgnoreScale ); // Extra scale for further collision check
                        // Unfortunately, Physics.ComputePenetration is not supporting colliders transforms scaling
                        // so you need to do scale manually ¯\_(ツ)_/¯

                        foreach( var otherBone in chain.BoneSetups )
                        {
                            if( myBone == otherBone ) continue;
                            if( otherBone.MainBoneCollider == null ) continue;
                            if( otherBone.BoundedIgnoreScale <= 0f ) continue;

                            foreach( var oCollSet in otherBone.Colliders )
                            {
                                var oCollider = oCollSet.GameCollider;

                                if( oCollider.transform.lossyScale.x == 0f )
                                {
                                    UnityEngine.Debug.Log( "[Ragdoll Animator 2] Detected zero scale object! It is not supported! (" + oCollider.transform.name + ")" );
                                    continue;
                                }

                                // Do basic bounds ignore in every case
                                Bounds oBounds = oCollSet.GameCollider.bounds;
                                oBounds.size *= scaleUpFactor * oCollSet.BoundedIgnoreScale * otherBone.BoundedIgnoreScale;
                                if( myBounds.Intersects( oBounds ) )
                                {
                                    oCollSet.IgnoreCollisionWith( myC, true );
                                }

                                // Mesh colliders works only with bounds check
                                if( ( myC.GameCollider is MeshCollider ) || ( oCollider is MeshCollider ) )
                                {
                                    //Bounds oBounds = oCollSet.GameCollider.bounds;
                                    //oBounds.size *= scaleUpFactor;
                                    //if( myBounds.Intersects( oBounds ) ) oCollSet.IgnoreCollisionWith( myC, true );
                                }
                                else // Use penetration check for other colliders
                                {
                                    ScaleCollider( oCollider, oCollider.transform.lossyScale.x * scaleUpFactor * otherBone.BoundedIgnoreScale );

                                    Vector3 penetrationDirection;
                                    float penetrationDistance;
                                    bool penetrationDetected = Physics.ComputePenetration( coll, coll.transform.position, coll.transform.rotation, oCollider, oCollider.transform.position, oCollider.transform.rotation, out penetrationDirection, out penetrationDistance );

                                    if( penetrationDetected )
                                    {
                                        oCollSet.IgnoreCollisionWith( myC, true );
                                    }

                                    ScaleCollider( oCollider, 1f / ( oCollider.transform.lossyScale.x * scaleUpFactor * otherBone.BoundedIgnoreScale ) );
                                }
                            }
                        }

                        // Restore scale
                        ScaleCollider( coll, 1f / ( coll.transform.lossyScale.x * scaleUpFactor * myBone.BoundedIgnoreScale ) );
                    }
                }
            }

            bool fall = ParentHandler.IsFallingOrSleep;

            // Ensure that all colliders stays in the right scales after operations done above
            foreach( var chain in chains )
            {
                foreach( var myBone in BoneSetups )
                {
                    myBone.RefreshCollider( chain, fall, false );
                }
            }
        }

        /// <summary>
        /// [Runtime] Removing bone and its child bones from the update loop and destroying them on the scene
        /// In addition, removing connection bones if exists
        /// </summary>
        public void RemoveBoneAndItsChildren( RagdollChainBone parentBone )
        {
            // Remove this bone and its child bones from the ragdoll animator update loop
            var childBones = CollectAllConnectedBones( parentBone );
            var childFillBones = CollectAllFillBones( childBones );

            // Remove fill bones to avoid null reference exceptions
            for( int i = childFillBones.Count - 1; i >= 0; i-- )
            {
                var fillbone = childFillBones[i];
                ParentHandler.skeletonFillExtraBonesList.Remove( fillbone );
                if( fillbone.DummyBone ) GameObject.Destroy( fillbone.DummyBone.gameObject );
            }

            // Destroy physical bones
            foreach( var bone in childBones )
            {
                bone.ParentDismembered = true;
                if( bone.GameRigidbody ) GameObject.Destroy( bone.GameRigidbody.gameObject );
            }

            // Removing bone from list and its processor
            foreach( var b in childBones )
            {
                b.ParentChain.RemoveRuntimeBoneProcessing( b );
                b.ParentChain.ParentHandler.RemoveBoneFromRuntimeCalculations( b );
            }
        }

        /// <summary>
        /// Remove bone info stored in the chain - not destroying scene objects
        /// </summary>
        public void RemoveRuntimeBoneProcessing( RagdollChainBone ragdollChainBone )
        {
            RuntimeBoneProcessors.Remove( ragdollChainBone.BoneProcessor );
            BoneSetups.Remove( ragdollChainBone );
        }

        /// <summary>
        /// Collecting all child bones connected with this bone
        /// </summary>
        public List<RagdollChainBone> CollectAllConnectedBones( RagdollChainBone bone, bool includeSelf = true )
        {
            List<RagdollChainBone> bones = new List<RagdollChainBone>();

            int index = bone.ParentChain.GetIndex( bone );
            if( index == -1 ) return bones;

            if( includeSelf ) bones.Add( bone );
            for( int i = index; i < bone.ParentChain.BoneSetups.Count; i++ ) bones.Add( bone.ParentChain.BoneSetups[i] );

            foreach( var chain in ParentHandler.Chains )
            {
                if( chain == this ) continue;

                if( bones.Contains( chain.ConnectionBone ) )
                {
                    foreach( var cBone in chain.BoneSetups )
                    {
                        bones.Add( cBone );
                    }
                }
            }

            return bones;
        }

        /// <summary>
        /// Collecting all child bones connected with this bone
        /// </summary>
        public List<RagdollChainBone.InBetweenBone> CollectAllFillBones( List<RagdollChainBone> bones )
        {
            List<RagdollChainBone.InBetweenBone> fills = new List<RagdollChainBone.InBetweenBone>();

            // Remove fill bones to avoid null reference exceptions
            for( int i = ParentHandler.skeletonFillExtraBonesList.Count - 1; i >= 0; i-- )
            {
                var fillbone = ParentHandler.skeletonFillExtraBonesList[i];

                foreach( var bone in bones )
                {
                    if( SkeletonRecognize.IsChildOf( fillbone.DummyBone, bone.PhysicalDummyBone ) )
                    {
                        fills.Add( fillbone );
                        break; // Stop checking for this fill bone
                    }
                }
            }

            return fills;
        }

        public void SwitchPhysics( bool enable )
        {
            foreach( var bone in BoneSetups )
            {
                bone.SwitchPhysics( enable );
            }
        }
    }
}