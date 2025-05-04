using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
        public bool DummyWasGenerated => Dummy_Container != null;

        /// <summary> Bottom most object of physical dummy hierarchy, containing all limbs as child objects. </summary>
        public Transform Dummy_Container
        { get { return dummyContainer; } private set { dummyContainer = value; } }

        [SerializeField, HideInInspector] private Transform dummyContainer = null;

        /// <summary> Playmode generated component on the dummy container object </summary>
        public RagdollAnimatorDummyReference DummyReference { get; private set; } = null;

        internal Dictionary<Transform, RagdollChainBone.InBetweenBone> skeletonFillExtraBones = null;
        [SerializeField, HideInInspector] internal List<RagdollChainBone.InBetweenBone> skeletonFillExtraBonesList = null;
        public List<RagdollChainBone.InBetweenBone> SkeletonFillExtraBonesList { get { return skeletonFillExtraBonesList; } }

        public void GenerateDummyHierarchy()
        {
            if( DummyWasGenerated ) return;

            // Set reference pose
            if( WaitForInit || UseReconstruction ) ApplyTPoseOnModel( true );

            // Generate main container for the ragdoll dummy
            Dummy_Container = CreateTransform( parentObject.name + "-Ragdoll", RagdollDummyLayer );
            SetCoordsLike( Dummy_Container, parentObject.transform );

            // Helper dictionary for skipped bones
            skeletonFillExtraBones = new Dictionary<Transform, RagdollChainBone.InBetweenBone>();

            // Generating ragdoll dumym hierarchy bones basing on the settings in the 'Contruct' inspector category
            for( int i = 0; i < chains.Count; i++ )
            {
                var chain = chains[i];
                if( chain.BoneSetups.Count == 0 ) continue;
                chain.GenerateDummyLimb( this );
            }

            //var coreChain = GetChain( ERagdollChainType.Core );
            //if( coreChain != null && coreChain.BoneSetups != null && coreChain.BoneSetups.Count > 1 && coreChain.BoneSetups[0] != null ) coreChain.BoneSetups[0].IsAnchor = true;
            GetChain( ERagdollChainType.Core ).BoneSetups[0].IsAnchor = true;

            // Defining extra replacement bones for better animation matching in case of skipped bones
            skeletonFillExtraBonesList = new List<RagdollChainBone.InBetweenBone>();
            foreach( var item in skeletonFillExtraBones ) skeletonFillExtraBonesList.Add( item.Value );

            // Generate components on the dummy bones
            for( int i = 0; i < chains.Count; i++ )
            {
                var chain = chains[i];
                chain.RefreshRagdollComponents( false );
            }

            // Adjust joints connected body parenting
            for( int i = 0; i < chains.Count; i++ )
            {
                var chain = chains[i];
                var parentBone = chain.ConnectionBone;
                chain.RefreshJointsParentingDefault( parentBone );
            }
        }

        /// <summary>
        /// For RagdollLogic == ERagdollLogic.JustBoneComponents
        /// </summary>
        private void GenerateJustSkeletonComponentsLogic()
        {
            RagdollHandlerUtilities.AddCollidersOnTheCharacterBones( this );
            RagdollHandlerUtilities.AddPhysicsComponentsOnTheCharacterBones( this );

            SwitchDummyPhysics( true );
        }

        public void ApplyPreGenerateDummyChanges()
        {
            // In editor OnValidate does it
            //#if UNITY_EDITOR
            this.User_UpdateAllBonesParametersAfterManualChanges();
            //#endif

            if (WaitForInit || UseReconstruction) ApplyTPoseOnModel( true );
            GetChain( ERagdollChainType.Core ).BoneSetups[0].IsAnchor = true;
        }

        private bool wasInReconstructionMode = false;

        private void GenerateInBetweenBonesPhysics()
        {
            if( wasInReconstructionMode ) return;
            wasInReconstructionMode = true;
            Caller.StartCoroutine( IEGenerateInBetweenBonesPhysics() );
        }

        private IEnumerator IEGenerateInBetweenBonesPhysics()
        {
            // Support for animating connection bones (skipped chain bones)
            foreach( var item in skeletonFillExtraBonesList )
            {
                // For some reason hips bugs out when connection bones - child rigidbodies are kinematic
                //if (item.Value.DummyBone.parent == anchor.PhysicalDummyBone) continue;
                item.GenerateRigidbody();
            }

            //foreach (var item in skeletonFillExtraBonesList)
            //{
            //    Rigidbody parentRig = item.DummyBone.parent.GetComponent<Rigidbody>();
            //    if (parentRig) if (item.FixedJoint != null) item.FixedJoint.connectedBody = parentRig;
            //}

            yield return null;

            ApplyTPoseOnModel( true );

            // Adjust joints connected body parenting
            for( int i = 0; i < chains.Count; i++ )
            {
                var chain = chains[i];
                var parentBone = chain.ConnectionBone;
                chain.RefreshJointsParentingWithInBetweenBones( parentBone );
            }

            foreach( var chain in chains )
            {
                chain.ConfigureJointsAnchors();
            }
        }

        private void DiscardInBetweenBonesPhysics()
        {
            if( !wasInReconstructionMode ) return;
            wasInReconstructionMode = false;

            Caller.StartCoroutine( IEDiscardInBetweenBonesPhysics() );
        }

        private IEnumerator IEDiscardInBetweenBonesPhysics()
        {
            foreach( var item in skeletonFillExtraBonesList )
            {
                item.DestroyPhysicalComponents();
            }

            yield return null;
            ApplyTPoseOnModel( true );

            // Adjust joints connected body parenting
            for( int i = 0; i < chains.Count; i++ )
            {
                var chain = chains[i];
                var parentBone = chain.ConnectionBone;
                chain.RefreshJointsParentingDefault( parentBone );
            }

            foreach( var chain in chains )
            {
                chain.ConfigureJointsAnchors();
            }

            this.User_SetAllKinematic( false );
        }

        /// <summary> Called during playmode initialization (pre-generate dummy is not calling this method) </summary>
        public void FinalizePhysicalDummySetup()
        {
            EnsureCollisionsIgnoreSetup();

            // Adjust joints bones parent info
            for( int i = 0; i < chains.Count; i++ )
            {
                var chain = chains[i];
                if( chain.ConnectionBone == null ) chain.DefineConnectionBone( this );

                var parentBone = chain.ConnectionBone;
                chain.RefreshBonesParentBoneVariable( parentBone );
            }

            Dummy_Container.SetParent( TargetParentForRagdollDummy, true );
            if( HideDummyInSceneView ) dummyContainer.hideFlags = HideFlags.HideInHierarchy;

            DummyReference = Dummy_Container.gameObject.AddComponent<RagdollAnimatorDummyReference>();
            DummyReference.Initialize( Caller, this );

            for( int i = 0; i < chains.Count; i++ )
            {
                var chain = chains[i];
                chain.CompletePlaymodeInitialization();
            }

            foreach( var chain in chains )
            {
                chain.ConfigureJointsAnchors();
            }

            this.User_UpdateAllBonesParametersAfterManualChanges();
            this.User_UpdateLayersAfterManualChanges();

            foreach( var chain in chains )
            {
                chain.DetachBones( this );
            }

            ResetSleepMode();

            if( AnimatingMode == EAnimatingMode.Standing )
            {
                if( UseReconstruction ) GenerateInBetweenBonesPhysics();
            }
        }

        public void EnsureRelatedCollidersIgnore()
        {
            var anchor = GetAnchorBoneController;

            foreach( var chain in chains )
            {
                chain.EnsureCollisionIgnoreBetweenChildBones();
            }
        }

        public void EnsureRelatedCollidersIgnoreUsingBounds()
        {
            foreach( var chain in chains )
            {
                chain.EnsureCollisionIgnoreBetweenBonesUsingBounds( chains, BoundedCollidersIgnoreScaleup );
            }
        }

        public void StoreReferenceTPose()
        {
            StoredReferenceTPose.ClearPose();
            Transform baseTr = GetBaseTransform();

            for( int c = 0; c < chains.Count; c++ )
            {
                for( int i = 0; i < chains[c].BoneSetups.Count; i++ )
                {
                    var bone = chains[c].BoneSetups[i];
                    StoredReferenceTPose.UpdateBone( bone.SourceBone, baseTr );

                    // Store skipped bones in chain
                    if( i >= chains[c].BoneSetups.Count - 1 ) continue; // Not Needed
                    var nextSetup = chains[c].BoneSetups[i + 1];
                    if( nextSetup.SourceBone.parent == chains[c].BoneSetups[i].SourceBone ) continue; // This chain is not lost

                    Transform child = nextSetup.SourceBone.parent;

                    while( child != null && child != chains[c].BoneSetups[i].SourceBone )
                    {
                        StoredReferenceTPose.UpdateBone( child, baseTr );
                        child = child.parent;
                    }
                }

                if( chains[c].ChainType == ERagdollChainType.Core ) continue;

                var connectionBone = DummyStructure_FindConnectionBone( chains[c] );

                // Store connection bones
                Transform parentFollow = chains[c].BoneSetups[0].SourceBone.parent;
                while( parentFollow != connectionBone.SourceBone && parentFollow != null )
                {
                    StoredReferenceTPose.UpdateBone( parentFollow, baseTr );
                    parentFollow = parentFollow.parent;
                }
            }

            OnChange();
        }

        /// <summary> For the editor event under RAHE.Construct.cs </summary>
        public void ApplyTPoseOnModel()
        {
            ApplyTPoseOnModel( true );
        }

        public void ApplyTPoseOnModel( bool syncTransforms )
        {
            if( RagdollLogic == ERagdollLogic.JustBoneComponents ) return;

            var report = ValidateReferencePose();
            if( report == EReferencePoseReport.ReferencePoseError || report == EReferencePoseReport.NoReferencePose ) return;

            //if( UseReconstruction == false ) return;

            StoredReferenceTPose.CheckForNulls();
            StoredReferenceTPose.ApplyPose( GetBaseTransform() );

            if( WasInitialized )
            {
                foreach( var chain in chains )
                {
                    foreach( var bone in chain.BoneSetups )
                    {
                        //bone.GameRigidbody.isKinematic = true;

                        if( chain.Detach || bone.IsAnchor )
                        {
                            bone.PhysicalDummyBone.position = bone.SourceBone.position;
                            bone.PhysicalDummyBone.rotation = bone.SourceBone.rotation;
                        }
                        else
                        {
                            bone.PhysicalDummyBone.localPosition = bone.SourceBone.localPosition;
                            bone.PhysicalDummyBone.localRotation = bone.SourceBone.localRotation;
                        }

                        bone.GameRigidbody.position = bone.PhysicalDummyBone.position;
                        bone.GameRigidbody.rotation = bone.PhysicalDummyBone.rotation;

                        if( bone.GameRigidbody.isKinematic == false ) bone.GameRigidbody.velocity = Vector3.zero;
                        if( bone.GameRigidbody.isKinematic == false ) bone.GameRigidbody.angularVelocity = Vector3.zero;
                    }
                }

                foreach( var item in skeletonFillExtraBonesList )
                {
                    item.DummyBone.localPosition = item.SourceBone.localPosition;
                    item.DummyBone.localRotation = item.SourceBone.localRotation;

                    if( item.rigidbody )
                    {
                        item.rigidbody.position = item.DummyBone.position;
                        item.rigidbody.velocity = Vector3.zero;
                        item.rigidbody.rotation = item.DummyBone.rotation;
                        item.rigidbody.angularVelocity = Vector3.zero;
                    }
                }
            }

            if( syncTransforms )
            {
                Physics.SyncTransforms();
            }

            OnChange();
        }

        public void SwitchPreGeneratedDummy()
        {
            if( DummyWasGenerated )
            {
                GameObject.DestroyImmediate( Dummy_Container.gameObject ); // Executable only in editmode
            }
            else
            {
                GenerateDummyHierarchy();

                dummyContainer.SetParent( parentObject.transform, true );
            }
        }

        internal RagdollChainBone.InBetweenBone GetParentConnectionBoneTo( Transform physicalDummyBone )
        {
            for( int i = 0; i < skeletonFillExtraBonesList.Count; i++ )
            {
                if( skeletonFillExtraBonesList[i].DummyBone == physicalDummyBone.parent )
                {
                    return skeletonFillExtraBonesList[i];
                }
            }

            return null;
        }

        protected bool _dummyIndicatorsWasPrepared = false;

        /// <summary> Checking if all dummy bones has attached collision indicator for OnCollisionEnter type of events </summary>
        internal void PrepareDummyBonesCollisionIndicators( bool collectCollisions, bool useSelfCollision = true )
        {
            foreach( var chain in chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    for( int c = 0; c < bone.Colliders.Count; c++ )
                    {
                        RagdollAnimator2BoneIndicator indic = bone.Colliders[c].GameCollider.GetComponent<RagdollAnimator2BoneIndicator>();

                        if( bone.DisableCollisionEvents ) // If we want to skip collision events in this bone, lets add just indicator
                        {
                            if( indic == null )
                            {
                                indic = bone.Colliders[c].GameCollider.gameObject.AddComponent<RagdollAnimator2BoneIndicator>();
                                indic.Initialize( this, bone.BoneProcessor, chain, false );
                                continue;
                            }
                            else continue;
                        }

                        if( indic ) if( ( indic is RA2BoneCollisionHandler ) == false ) { GameObject.Destroy( indic ); indic = null; }

                        RA2BoneCollisionHandler collisionHandler;
                        if( indic == null )
                        {
                            collisionHandler = bone.Colliders[c].GameCollider.gameObject.AddComponent<RA2BoneCollisionHandler>();
                            collisionHandler.Initialize( this, bone.BoneProcessor, chain, false );
                        }
                        else
                        {
                            collisionHandler = bone.Colliders[c].GameCollider.GetComponent<RA2BoneCollisionHandler>();
                        }

                        if( collectCollisions ) collisionHandler.EnableSavingEnteredCollisionsList();

                        collisionHandler.UseSelfCollisions = useSelfCollision;
                    }
                }
            }

            _dummyIndicatorsWasPrepared = true;
        }

        protected bool _sourceIndicatorsWasPrepared = false;

        /// <summary> Checking if all source bones has attached collision indicator for OnCollisionEnter type of events </summary>
        internal void PrepareSourceBonesCollisionIndicators( bool triggerHandlers, bool enableCollisionCollecting = false, bool useSelfCollision = false )
        {
            foreach( var chain in chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    RagdollAnimator2BoneIndicator indic = bone.SourceBone.GetComponent<RagdollAnimator2BoneIndicator>();

                    bone.RefreshCollider( chain, IsFallingOrSleep, true );

                    if( bone.DisableCollisionEvents ) // If we want to skip collision events in this bone, lets add just indicator
                    {
                        if( indic == null )
                        {
                            indic = bone.SourceBone.gameObject.AddComponent<RagdollAnimator2BoneIndicator>();
                            indic.Initialize( this, bone.BoneProcessor, chain, true );
                            continue;
                        }
                        else continue;
                    }

                    if( triggerHandlers )
                    {
                        if( indic ) if( ( indic is RA2BoneTriggerCollisionHandler ) == false ) { GameObject.Destroy( indic ); indic = null; }

                        RA2BoneTriggerCollisionHandler triggerCollisionHandler;
                        if( indic == null )
                        {
                            triggerCollisionHandler = bone.SourceBone.gameObject.AddComponent<RA2BoneTriggerCollisionHandler>();
                            triggerCollisionHandler.Initialize( this, bone.BoneProcessor, chain, true );
                        }
                        else
                        {
                            triggerCollisionHandler = bone.SourceBone.GetComponent<RA2BoneTriggerCollisionHandler>();
                        }

                        if( enableCollisionCollecting ) triggerCollisionHandler.EnableSavingEnteredCollisionsList();

                        triggerCollisionHandler.UseSelfCollisions = useSelfCollision;
                    }
                    else
                    {
                        if( indic ) if( ( indic is RA2BoneCollisionHandler ) == false ) { GameObject.Destroy( indic ); indic = null; }

                        RA2BoneCollisionHandler collisionHandler;
                        if( indic == null )
                        {
                            collisionHandler = bone.SourceBone.gameObject.AddComponent<RA2BoneCollisionHandler>();
                            collisionHandler.Initialize( this, bone.BoneProcessor, chain, true );
                        }
                        else
                        {
                            collisionHandler = bone.SourceBone.GetComponent<RA2BoneCollisionHandler>();
                        }

                        collisionHandler.UseSelfCollisions = useSelfCollision;

                        if( enableCollisionCollecting ) collisionHandler.EnableSavingEnteredCollisionsList();
                    }
                }
            }

            _sourceIndicatorsWasPrepared = true;
        }

        public void User_ResetOverrideBlends()
        {
            foreach( var chain in chains )
            {
                chain.User_ResetOverrideBlends();
            }
        }

        /// <summary> Storing lastest animator pose as calibration pose, useful when disabling mecanim animator </summary>
        public void StoreCalibrationPose()
        {
            foreach( var chain in chains ) chain.StoreCalibrationPose();
        }

        /// <summary> Restoting intiial pose as calibration pose, useful when enabling back mecanim animator after disabling it </summary>
        public void RestoreCalibrationPose()
        {
            foreach( var chain in chains ) chain.RestoreCalibrationPose();
        }
    }
}