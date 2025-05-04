using FIMSpace.AnimationTools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [System.Serializable]
    public partial class RagdollBonesChain
    {
        public void SetParentHandler( RagdollHandler handler )
        { _prentHandler = handler; }

        /// <summary> Cant be serialized since it's parent and also Serializable class </summary>
        public RagdollHandler ParentHandler => _prentHandler;

        [NonSerialized] private RagdollHandler _prentHandler = null;

        public string ChainName = "Bones Chain";
        public ERagdollChainType ChainType = ERagdollChainType.Unknown;
        public List<RagdollChainBone> BoneSetups = new List<RagdollChainBone>();
        public RagdollChainBone LastBone => BoneSetups[BoneSetups.Count - 1];

        /// <summary> All bone controllers to execute every frame (excluded anchor bone) </summary>
        [field: NonSerialized] public List<RagdollBoneProcessor> RuntimeBoneProcessors { get; private set; }

        /// <summary> Parent bone of the ragdoll bones chain </summary>
        [field: NonSerialized] public RagdollChainBone ConnectionBone { get; private set; }

        /// <summary> Summed distance between all bones, calculated during intialization </summary>
        public float ChainBonesLength { get; private set; }

        [Tooltip( "Multiplicator value for colliders size excluding bone-forward axis" )]
        [Range( 0.1f, 2f )] public float ChainThicknessMultiplier = 1f;
        [Tooltip( "Multiplicator value for colliders size" )]
        [Range( 0f, 2f )] public float ChainScaleMultiplier = 1f;

        public float GetScaleMultiplier()
        {
            if( ParentHandler != null ) return ParentHandler.RagdollSizeMultiplier * ChainScaleMultiplier;
            else return ChainScaleMultiplier;
        }

        public float GetThicknessMultiplier()
        {
            if( ChainThicknessMultiplier == 0f ) ChainThicknessMultiplier = 1f;

            if( ParentHandler != null )
            {
                if( ParentHandler.RagdollThicknessMultiplier == 0f ) ParentHandler.RagdollThicknessMultiplier = 1f;

                return ParentHandler.RagdollThicknessMultiplier * ChainThicknessMultiplier;
            }

            else return ChainThicknessMultiplier;
        }

        [Tooltip( "Multiplying target mass value for all bones in the chain" )]
        [Range( 0f, 1f )] public float MassMultiplier = 1f;

        [Tooltip( "Multiplying target joint force value for all bones in the chain" )]
        [Range( 0f, 2f )] public float MusclesForce = 1f;

        [Tooltip( "Multiplying target joint angle limit ranges for all bones in the chain" )]
        [Range( 0.1f, 2f )] public float AxisLimitRange = 1f;

        [Tooltip( "Bypassing configurable joint limits" )]
        public bool UnlimitedRotations = false;

        [Tooltip( "Joints connected mass scale - Can help out calming down too much sensitive bones" )]
        public float ConnectedMassScale = 1f;

        [Tooltip( "If this connected mass value should be used all the time, and not serve as multiplier" )]
        public bool ConnectedMassOverride = false;

        [Tooltip( "Detaching limb bones hierarchy. It can help animating tails and fixes handling kinematic bones, but it is not working with reconstruction mode." )]
        public bool Detach = true;

        [Tooltip( "Selective limb Ragdoll Blend multiplier" )]
        [FPD_Suffix( 0f, 1f )] public float ChainBlend = 1f;

        [Tooltip( "Override all ragdoll blend parameters (not including internal per bone blend multipliers) with this value (set 0 to not use it, set 0.0000001 to override blend chain being off)" )]
        [FPD_Suffix( 0f, 1f )] public float OverrideBlend = 0f;

        [Tooltip( "Applying alternative interia tensors for the chain's rigidbodies. It will make motion smooth and slower, dedicated to be used just in long chains made out of many bones or chains which are unstable." )]
        public bool AlternativeTensors = false;
        [Tooltip( "Alternative tensors switch on fall mode." )]
        public bool AlternativeTensorsOnFall = false;
        /// <summary> Flag to help resetting alternative tensors </summary>
        internal bool tensorsSwitched = false;

        [Tooltip( "Multiplying hard matching parameter over whole chain" )]
        [Range( 0f, 1f )] public float HardMatchMultiply = 1f;

        public bool PlaymodeInitialized { get; private set; } = false;

        public void CompletePlaymodeInitialization()
        {
            if( PlaymodeInitialized ) return;

            RuntimeBoneProcessors = new List<RagdollBoneProcessor>();
            ChainBonesLength = 0f;

            RagdollChainBone parentBone = null;

            foreach( var bone in BoneSetups )
            {
                bone.PlaymodeInitialize( this );
                if( bone.IsAnchor == false ) RuntimeBoneProcessors.Add( bone.BoneProcessor );
                if( parentBone != null ) ChainBonesLength += Vector3.Distance( parentBone.SourceBone.position, bone.SourceBone.position );
                bone.SetParentBone( parentBone );
                parentBone = bone;
            }

            PlaymodeInitialized = true;
        }

        /// <summary> Generated connection hierarchy to lost connection bones under ragdoll dummy hierarchy structure </summary>
        public List<RagdollChainBone.InBetweenBone> ParentConnectionBones { get; private set; } = null;

        #region Dynamic Calculations

        [NonSerialized] public float blendOnCollisionCulldown = 0f;
        [NonSerialized] public float blendOnCollisionMin = 0f;

        private bool playmodeDetached = false;

        public RagdollBonesChain( RagdollHandler ragdollHandler )
        {
            SetParentHandler( ragdollHandler );
        }

        public RagdollChainBone AddNewBone( Transform sceneBone )
        {
            if( sceneBone != null ) for( int b = 0; b < BoneSetups.Count; b++ ) if( BoneSetups[b].SourceBone == sceneBone ) return BoneSetups[b]; // Already added

            var bone = new RagdollChainBone();
            bone.SourceBone = sceneBone;
            BoneSetups.Add( bone );

            return bone;
        }

        public RagdollChainBone AddNewBone( ERagdollBoneID boneID, RagdollChainBone.EColliderType colliderType = RagdollChainBone.EColliderType.Capsule )
        {
            var bone = AddNewBone( ParentHandler.Mecanim.GetBoneTransform( (HumanBodyBones)boneID ) );
            bone.BaseColliderSetup.ColliderType = colliderType;
            bone.BoneID = boneID;
            return bone;
        }

        public RagdollChainBone AddNewBone( bool assignSuggestion = true, RagdollChainBone.EColliderType colliderType = RagdollChainBone.EColliderType.Capsule )
        {
            Transform t = null;

            if( assignSuggestion )
            {
                if( BoneSetups.Count > 0 )
                {
                    Transform lastBone = BoneSetups[BoneSetups.Count - 1].SourceBone;
                    if( lastBone != null ) t = SkeletonRecognize.GetContinousChildTransform( lastBone );
                }
            }

            return AddNewBone( t, colliderType );
        }

        public RagdollChainBone AddNewBone( Transform sourceBone, RagdollChainBone.EColliderType colliderType, ERagdollBoneID boneID = ERagdollBoneID.Unknown )
        {
            if( sourceBone != null ) for( int b = 0; b < BoneSetups.Count; b++ ) if( BoneSetups[b].SourceBone == sourceBone ) return BoneSetups[b]; // Already added

            var bone = new RagdollChainBone();

            bone.BoneID = boneID;
            bone.SourceBone = sourceBone;
            bone.BaseColliderSetup.ColliderType = colliderType;

            BoneSetups.Add( bone );
            return bone;
        }

        #endregion Dynamic Calculations

        #region Additional Features

        public void Setup_GatherChildBones()
        {
            if( BoneSetups.Count <= 0 ) return;
            if( BoneSetups[0].SourceBone == null ) return;

            Transform next = BoneSetups[0].SourceBone;
            while( next != null )
            {
                Transform continous = SkeletonRecognize.GetContinousChildTransform( next );
                if( continous == null ) break;
                AddNewBone( continous );
                next = continous;
            }
        }

        #endregion Additional Features

        /// <summary>
        /// Returning reference to the bone setup.
        /// If index is below 0, returns [0] bone, when greater than bones list count, returning last bone in the chain.
        /// </summary>
        public RagdollChainBone GetBone( int index )
        {
            if( BoneSetups.Count == 0 ) return null;
            if( index >= BoneSetups.Count ) return LastBone;
            return BoneSetups[index];
        }

        /// <summary>
        /// Checking index of the bone reference, if the bone is present in the chain.
        /// If not, method will return value = -1
        /// </summary>
        public int GetIndex( RagdollChainBone bone )
        {
            for( int i = 0; i < BoneSetups.Count; i++ ) if( BoneSetups[i] == bone ) return i;
            return -1;
        }

        public RagdollChainBone GetParent( RagdollChainBone bone )
        {
            RagdollChainBone parent = null;
            for( int i = 0; i < BoneSetups.Count - 1; i++ ) if( BoneSetups[i + 1] == bone ) { parent = BoneSetups[i]; break; }
            return parent;
        }

        public bool ContainsAnimatorBoneTransform( Transform checkBone )
        {
            foreach( var bone in BoneSetups ) { if( bone.SourceBone == checkBone ) return true; }
            return false;
        }

        public bool ContainsAnimatorBoneTransform( string boneName )
        {
            foreach( var bone in BoneSetups ) { if( bone.SourceBone.name == boneName ) return true; }
            return false;
        }

        public bool ContainsDummyBoneTransform( Transform checkBone )
        {
            foreach( var bone in BoneSetups ) { if( bone.PhysicalDummyBone == checkBone ) return true; }
            return false;
        }

        public float CalculateLength()
        {
            float distance = 0f;
            Transform preBone = null;
            for( int i = 0; i < BoneSetups.Count; i++ )
            {
                if( BoneSetups[i].SourceBone == null ) continue;
                if( preBone ) distance += Vector3.Distance( preBone.position, BoneSetups[i].SourceBone.position );
                preBone = BoneSetups[i].SourceBone;
            }

            return distance;
        }

        public Transform DummyParentObject { get; private set; } = null;

        public Transform GenerateDummyLimb( RagdollHandler handler, bool generateLostParents = true )
        {
            if( DummyParentObject != null ) return DummyParentObject;

            Transform parent = null;

            // Generate basic limb hierarchy for physical dummy hierarchy
            for( int i = 0; i < BoneSetups.Count; i++ )
            {
                BoneSetups[i].GenerateDummyBone( RagdollHandler.CreateTransform( BoneSetups[i].SourceBone.name, handler.RagdollDummyLayer ) );
                RagdollHandler.SetCoordsLike( BoneSetups[i].PhysicalDummyBone, BoneSetups[i].SourceBone );
                BoneSetups[i].PhysicalDummyBone.SetParent( parent, true );
                parent = BoneSetups[i].PhysicalDummyBone;

                #region Check for chain continuity with source skeleton

                if( generateLostParents == false ) continue; // Skipping calculations below
                if( i >= BoneSetups.Count - 1 ) continue; // Not Needed

                var nextSetup = BoneSetups[i + 1];
                if( nextSetup.SourceBone.parent == BoneSetups[i].SourceBone ) continue; // This chain is not lost

                Transform child = nextSetup.SourceBone.parent;

                //if( Detach == false )
                {
                    // Generate connection transforms to keep ragdoll dummy hierarchy as source skeleton hierarchy
                    List<RagdollChainBone.InBetweenBone> inBetweenBones = new List<RagdollChainBone.InBetweenBone>();

                    while( child != null && child != BoneSetups[i].SourceBone )
                    {
                        RagdollChainBone.InBetweenBone inBetween;

                        if( handler.skeletonFillExtraBones.TryGetValue( child, out inBetween ) == false )
                        {
                            inBetween = new RagdollChainBone.InBetweenBone();
                            inBetween.SourceBone = child;
                            inBetween.DummyBone = RagdollHandler.CreateTransform( child );
                            inBetween.DummyBone.gameObject.layer = handler.RagdollDummyLayer;
                            inBetween.DummyBone.name += ":InBetween";
                            handler.skeletonFillExtraBones.Add( child, inBetween );
                        }

                        inBetweenBones.Add( inBetween );
                        child = child.parent;
                    }

                    inBetweenBones[inBetweenBones.Count - 1].AssignParent( BoneSetups[i].PhysicalDummyBone );

                    for( int b = inBetweenBones.Count - 2; b >= 0; b-- )
                    {
                        inBetweenBones[b].AssignParent( inBetweenBones[b + 1].DummyBone );
                    }

                    BoneSetups[i].SetInBetweenBones( inBetweenBones );

                    parent = inBetweenBones[0].DummyBone;
                }

                #endregion Check for chain continuity with source skeleton
            }

            DummyParentObject = BoneSetups[0].PhysicalDummyBone;

            // Core should be child of the anchor like pelvis ! Discarded Approach !
            // New Approach : Core is child of dummy object. Core first bone is anchor.
            if( ChainType == ERagdollChainType.Core )
            {
                BoneSetups[0].PhysicalDummyBone.SetParent( handler.Dummy_Container, true );
                return DummyParentObject;
            }

            var connectionBone = handler.DummyStructure_FindConnectionBone( this );
            var coreBone = handler.GetChain( ERagdollChainType.Core, null ).BoneSetups[0];

            if( connectionBone == null )
            {
                UnityEngine.Debug.Log( "[Ragdoll Animator] Can't find connection bone for " + ChainName + " in the " + handler.ParentObject.name + " Ragdoll Dummy! (" + ChainType + ")" );
            }
            else
            {
                ConnectionBone = connectionBone;

                // Direct parent of limb start bone
                if( connectionBone.SourceBone == BoneSetups[0].SourceBone.parent || connectionBone == coreBone /*|| Detach*/ )
                {
                    DummyParentObject.SetParent( connectionBone.PhysicalDummyBone, true );
                }
                else // Generate lacking hierarchy
                {
                    List<RagdollChainBone.InBetweenBone> parentConnectionBones = new List<RagdollChainBone.InBetweenBone>();

                    // Check for lost parent connection
                    Transform parentFollow = BoneSetups[0].SourceBone.parent;

                    while( parentFollow != connectionBone.SourceBone && parentFollow != null )
                    {
                        RagdollChainBone.InBetweenBone connBone;

                        if( handler.skeletonFillExtraBones.TryGetValue( parentFollow, out connBone ) == false )
                        {
                            connBone = new RagdollChainBone.InBetweenBone();
                            connBone.SourceBone = parentFollow;
                            connBone.DummyBone = RagdollHandler.CreateTransform( parentFollow );
                            connBone.DummyBone.gameObject.layer = handler.RagdollDummyLayer;
                            connBone.DummyBone.name += ":Connection";
                            handler.skeletonFillExtraBones.Add( parentFollow, connBone );
                        }

                        parentConnectionBones.Add( connBone );
                        parentFollow = parentFollow.parent;
                    }

                    parentConnectionBones.Reverse();

                    // Set parent of dummy bone which is child of the found connection bone
                    parentConnectionBones[0].AssignParent( connectionBone.PhysicalDummyBone );

                    // Parent the generated connection hierarchy
                    for( int i = 1; i < parentConnectionBones.Count; i++ )
                        parentConnectionBones[i].AssignParent( parentConnectionBones[i - 1].DummyBone );

                    // Finally assing parent for the first dummy limb bone
                    DummyParentObject.SetParent( parentConnectionBones[parentConnectionBones.Count - 1].DummyBone, true );

                    // Assign connection bones list for extra calculations
                    ParentConnectionBones = parentConnectionBones;
                }
            }

            return DummyParentObject;
        }

        /// <summary> Compares all bones distances and returning average of the results </summary>
        internal float GetAverageStepSizeOfTheChain()
        {
            float size = 0f;
            float iterations = 0f;
            for( int i = 1; i < BoneSetups.Count; i++ )
            {
                if( BoneSetups[i - 1].SourceBone == null ) continue;
                if( BoneSetups[i].SourceBone == null ) continue;

                float distance = Vector3.Distance( BoneSetups[i - 1].SourceBone.position, BoneSetups[i].SourceBone.position );
                size += distance;
                iterations += 1f;
            }

            if( size < 0.001f ) return 0.05f;
            return size / iterations;
        }

        public void RefreshRagdollComponents( bool addOnSource = false )
        {
            bool fall = ParentHandler.IsFallingOrSleep;

            for( int b = 0; b < BoneSetups.Count; b++ )
            {
                var bone = BoneSetups[b];
                bone.RefreshRigidbody( ParentHandler, this, addOnSource );
                bone.RefreshCollider( this, fall, addOnSource );
                bone.RefreshJoint( this, fall, addOnSource, false );
            }
        }

        public void RefreshJointsParentingDefault( RagdollChainBone parentBone )
        {
            foreach( var bone in BoneSetups )
            {
                if( bone.Joint == null ) return;

                if( parentBone != null )
                {
                    bone.Joint.connectedBody = parentBone.GameRigidbody;
                    bone.InitialConnectedBody = parentBone.GameRigidbody;
                }

                parentBone = bone;
            }
        }

        public void RefreshBonesParentBoneVariable( RagdollChainBone parentBone )
        {
            foreach( var bone in BoneSetups )
            {
                bone.SetParentBone( parentBone );
                parentBone = bone;
            }
        }

        public void DetachBones( RagdollHandler handler )
        {
            foreach( var bone in BoneSetups ) // Detach parent needs to be assigned in all cases
            {
                if( bone.Joint == null ) return;
                bone.DetachParent = bone.PhysicalDummyBone.parent;
            }

            if( !Detach ) return;

#if UNITY_EDITOR
            if( Application.isPlaying == false ) return;
#endif
            if( playmodeDetached ) return;

            foreach( var bone in BoneSetups )
            {
                if( bone.Joint == null ) return;
                bone.PhysicalDummyBone.transform.SetParent( handler.Dummy_Container, true );
            }

            playmodeDetached = true;
        }

        public void RefreshJointsParentingWithInBetweenBones( RagdollChainBone parentBone )
        {
            foreach( var bone in BoneSetups )
            {
                if( bone.Joint == null ) return;

                Rigidbody parentRig = bone.PhysicalDummyBone.parent.GetComponent<Rigidbody>();
                if( parentRig )
                {
                    bone.Joint.connectedBody = parentRig;
                }

                if( bone.Joint.connectedBody == null )
                {
                    if( parentBone != null )
                    {
                        bone.Joint.connectedBody = parentBone.GameRigidbody;
                    }
                }

                parentBone = bone;
            }
        }

        /// <summary> Getting symmetry chain just by using ChainType </summary>
        public RagdollBonesChain GetSymmetryChainByType()
        {
            if( ParentHandler == null ) return null;

            RagdollBonesChain symmetryChain = null;

            if( ChainType == ERagdollChainType.RightArm )
                symmetryChain = ParentHandler.GetChain( ERagdollChainType.LeftArm );
            else if( ChainType == ERagdollChainType.LeftArm )
                symmetryChain = ParentHandler.GetChain( ERagdollChainType.RightArm );
            else if( ChainType == ERagdollChainType.RightLeg )
                symmetryChain = ParentHandler.GetChain( ERagdollChainType.LeftLeg );
            else if( ChainType == ERagdollChainType.LeftLeg )
                symmetryChain = ParentHandler.GetChain( ERagdollChainType.RightLeg );

            return symmetryChain;
        }

        public RagdollChainBone GetSymmetryTo( RagdollChainBone bone )
        {
            if( ParentHandler == null ) return null;

            var boneChain = ParentHandler.GetChain( bone );
            if( boneChain == null ) return null;

            int boneIndex = boneChain.GetIndex( bone );

            var oChain = FindSymmetryChainTo( ParentHandler, boneChain );

            if( oChain == null ) return null;

            if( oChain.BoneSetups.ContainsIndex( boneIndex ) ) return oChain.BoneSetups[boneIndex]; return null;
        }

        /// <summary> Finding symmetry chain basing on the base transform and side positions of bones </summary>
        public static RagdollBonesChain FindSymmetryChainTo( RagdollHandler handler, RagdollBonesChain chain )
        {
            if( chain == null ) return null;
            if( handler == null ) return null;
            if( chain.BoneSetups.Count == 0 ) return null;

            Transform baseT = handler.GetBaseTransform();
            if( baseT == null ) return null;

            Transform refBone = chain.GetBone( 0 ).SourceBone;
            if( refBone == null ) return null;

            RagdollBonesChain sChain = null;

            float nearest = float.MaxValue;
            foreach( var oChain in handler.Chains )
            {
                if( oChain == chain ) continue;
                if( oChain.ChainType.IsSameMainType( chain.ChainType ) == false ) continue;
                if( oChain.BoneSetups.Count == 0 ) continue;
                if( oChain.GetBone( 0 ) == null ) continue;

                var oBone = oChain.GetBone( 0 ).SourceBone;
                if( oBone == null ) continue;

                float diff = Vector3.Distance( oBone.position, refBone.position );
                if( diff < nearest )
                {
                    if( Mathf.Sign( baseT.InverseTransformPoint( oBone.position ).x ) == Mathf.Sign( baseT.InverseTransformPoint( refBone.position ).x ) ) continue; // Same Side
                    nearest = diff;
                    sChain = oChain;
                }
            }

            return sChain;
        }

        public bool HasSymmetryTo( RagdollChainBone bone ) => GetSymmetryTo( bone ) != null;

        public bool IsTypeRelatedWith( RagdollBonesChain ragdollBonesChain )
        {
            if( ChainType.IsLeg() ) if( ragdollBonesChain.ChainType.IsLeg() ) return true;
            if( ChainType.IsArm() ) if( ragdollBonesChain.ChainType.IsArm() ) return true;
            if( ChainType == ragdollBonesChain.ChainType ) return true;
            return false;
        }

        /// <summary> Fixing dummy bones and in-between bones </summary>
        public void Calibrate()
        {
            foreach( var bone in BoneSetups )
            {
                bone.BoneProcessor.Calibrate();
            }
        }

        public void CalibrateJustRotation()
        {
            foreach( var bone in BoneSetups )
            {
                bone.BoneProcessor.CalibrateRotation();
            }
        }

        public void ApplyPhysicalRotationsToTheSkeleton( float finalBlend )
        {
            finalBlend = GetBlend( finalBlend );

            foreach( var bone in RuntimeBoneProcessors ) bone.ApplyPhysicalRotationToTheBone( finalBlend );
        }

        public float GetBlend( float baseBlend )
        {
            if( OverrideBlend > 0f ) return OverrideBlend;
            return baseBlend * ChainBlend;
        }

        public void ApplyPhysicalPositionToTheSkeleton( float finalBlend )
        {
            finalBlend = GetBlend( finalBlend );
            foreach( var bone in RuntimeBoneProcessors ) bone.ApplyPhysicalPositionToTheBone( finalBlend );
        }

        public void CaptureAnimator()
        {
            foreach( var bone in BoneSetups )
            {
                bone.BoneProcessor.CaptureAnimatorPose();
            }
        }

        public void ConfigureJointsAnchors()
        {
            foreach( var bone in BoneSetups )
            {
                bone.ConfigureJointAnchors();
            }
        }

        /// <summary> Overriding all blend factors for the bones in the chain (override is including per bone multipliers) and blending it up or down no matter what for a short period of time </summary>
        public void User_ForceOverrideAllBonesBlendFor( float duration, float transitionTime = 0.1f, float targetOverrideBlend = 1f )
        {
            foreach( var bone in BoneSetups )
            {
                bone.User_ForceOverrideBlendFor( ParentHandler, duration, transitionTime, targetOverrideBlend );
            }
        }

        public void User_ResetOverrideBlends()
        {
            foreach( var bone in BoneSetups )
            {
                bone.User_ForceStopOverrideBlend( ParentHandler );
            }
        }

        public void TryIdentifyBoneIDs( bool changeOnlyUnknowns = false )
        {
            foreach( var bone in BoneSetups )
            {
                bone.TryIdentifyBoneID( this, changeOnlyUnknowns );
            }
        }

        /// <summary> Storing lastest animator pose as calibration pose, useful when disabling mecanim animator </summary>
        public void StoreCalibrationPose()
        {
            foreach( var bone in BoneSetups ) bone.StoreCalibrationPose();
        }

        /// <summary> Restoting intiial pose as calibration pose, useful when enabling back mecanim animator after disabling it </summary>
        public void RestoreCalibrationPose()
        {
            foreach( var bone in BoneSetups ) bone.RestoreCalibrationPose();
        }

        /// <summary> Making ragdoll chain bones ignoring physical collisions with provided collider </summary>
        public void IgnoreCollisionsWith( Collider coll, bool ignore )
        {
            foreach( var bone in BoneSetups )
            {
                foreach( var collS in bone.Colliders )
                {
                    collS.IgnoreCollisionWith( coll, ignore );
                }
            }
        }

        internal void DefineConnectionBone( RagdollHandler ragdollHandler )
        {
            ConnectionBone = ragdollHandler.DummyStructure_FindConnectionBone( this );
        }

    }
}