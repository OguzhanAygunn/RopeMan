using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
        #region Anchor Bone

        private RagdollChainBone _playmodeAnchorBone = null;
        public RagdollChainBone GetAnchorBoneController => WasInitialized ? _playmodeAnchorBone : GetChain( ERagdollChainType.Core ).BoneSetups[0];
        //[Tooltip("Generated copy of anchor bone as parent of anchor joint")]
        //[HideInInspector] public Rigidbody AnchorHandlerRigidbody;

        public Transform GetAnchorSourceBone()
        {
            if( WasInitialized ) return _playmodeAnchorBone.SourceBone;

            var coreChain = GetChain( ERagdollChainType.Core );
            if( coreChain == null ) return null;
            if( coreChain.BoneSetups.Count == 0 ) return null;
            return coreChain.BoneSetups[0].SourceBone;
        }

        #endregion Anchor Bone

        #region Validation

        /// <summary>
        /// TODO: Remove Reference T Pose controls, it most likely will not be required!
        /// </summary>
        public RagdollPose StoredReferenceTPose = new RagdollPose();

        public enum EReferencePoseReport
        { NoReferencePose, ReferencePoseOK, ReferencePoseChanged, ReferencePoseError }

        public EReferencePoseReport ValidateReferencePose()
        {
            try
            {
                if( StoredReferenceTPose.BonePoses.Count < 2 || StoredReferenceTPose.LastBaseTransform == null ) return EReferencePoseReport.NoReferencePose;
                if( StoredReferenceTPose.CheckIfAnyDiffers( GetBaseTransform() ) ) return EReferencePoseReport.ReferencePoseChanged;
                return EReferencePoseReport.ReferencePoseOK;
            }
            catch( Exception ext )
            {
                UnityEngine.Debug.Log( "[Ragdoll Animator 2] Reference Pose Error! Check Error Log!" );
                UnityEngine.Debug.LogException( ext );
                return EReferencePoseReport.ReferencePoseError;
            }
        }

        public bool IsBaseSetupValid()
        {
            return GetAnchorSourceBone() != null;
        }

        public bool IsRagdollConstructionValid()
        {
            if( Chains.Count > 1 ) return true;
            else
            {
                if( chains.Count == 1 )
                {
                    for( int i = 0; i < chains[0].BoneSetups.Count; i += 1 ) if( chains[0].BoneSetups[i].SourceBone == null ) return false;
                    return true;
                }
            }
            return false;
        }

        #endregion Validation

        public void EnsureChainsHasParentHandler()
        {
            for( int c = 0; c < chains.Count; c++ ) chains[c].SetParentHandler( this );
        }

        public RagdollBonesChain AddNewBonesChain( string targetName, ERagdollChainType targetType )
        {
            RagdollBonesChain chain = new RagdollBonesChain( this );
            chain.SetParentHandler( this );
            chain.ChainName = targetName;
            chain.ChainType = targetType;
            Chains.Add( chain );
            return chain;
        }

        public int GetIndexOfChain( RagdollBonesChain chain )
        {
            for( int i = 0; i < chains.Count; i++ ) if( chains[i] == chain ) return i;
            return -1;
        }

        public bool HasChain( RagdollBonesChain chain )
        {
            return GetIndexOfChain( chain ) > -1;
        }

        public RagdollBonesChain GetChain( ERagdollChainType type )
        {
            for( int i = 0; i < chains.Count; i++ )
            {
                if( chains[i].ChainType != type ) continue;
                return chains[i];
            }

            return null;
        }

        public RagdollBonesChain GetChain( int index )
        {
            if( index < 0 || index >= chains.Count ) return null;
            return chains[index];
        }

        public RagdollBonesChain GetChain( ERagdollChainType type, RagdollBonesChain restrictedTo )
        {
            Transform baseTransform = GetBaseTransform();

            for( int i = 0; i < chains.Count; i++ )
            {
                if( chains[i].ChainType != type ) continue;

                if( restrictedTo != null && BaseTransform != null )
                {
                    if( chains[i] == restrictedTo ) continue;
                    if( chains[i].BoneSetups.Count != restrictedTo.BoneSetups.Count ) continue;
                    if( chains[i].BoneSetups.Count == 0 ) continue;
                    if( chains[i].BoneSetups[0].SourceBone == null ) continue;
                    if( restrictedTo.BoneSetups[0].SourceBone == null ) continue;
                    if( baseTransform == null ) continue;

                    Vector3 checkedLocalPos = BaseTransform.InverseTransformPoint( chains[i].BoneSetups[0].SourceBone.position );
                    Vector3 restrLocalPos = BaseTransform.InverseTransformPoint( restrictedTo.BoneSetups[0].SourceBone.position );

                    float zDiff = Mathf.Abs( checkedLocalPos.z - restrLocalPos.z );
                    if( zDiff > chains[i].CalculateLength() * 0.11f ) continue; // Too big difference in Z axis for symmetry
                }

                return chains[i];
            }

            return null;
        }

        public RagdollBonesChain GetChain( RagdollChainBone member )
        {
            for( int c = 0; c < chains.Count; c++ )
            {
                for( int b = 0; b < chains[c].BoneSetups.Count; b++ )
                {
                    var bone = chains[c].BoneSetups[b];
                    if( bone == member ) return chains[c];
                }
            }

            return null;
        }

        public RagdollChainBone FindAnimatorBoneTransformChainBone( Transform bone )
        {
            foreach( var chain in chains )
            {
                if( chain.ContainsAnimatorBoneTransform( bone ) )
                {
                    for( var i = 0; i < chain.BoneSetups.Count; i++ )
                    { if( chain.BoneSetups[i].SourceBone == bone ) return chain.BoneSetups[i]; }
                }
            }

            return null;
        }

        public RagdollBonesChain FindAnimatorBoneTransformOwnerChain( Transform bone )
        {
            foreach( var chain in chains ) if( chain.ContainsAnimatorBoneTransform( bone ) ) return chain;
            return null;
        }

        public bool ContainsAnimatorBoneTransform( Transform bone )
        {
            if( !WasInitialized )
            {
                foreach( var chain in chains ) if( chain.ContainsAnimatorBoneTransform( bone ) ) return true;
            }
            else
            {
                return DictionaryContainsAnimatorBone( bone );
            }

            return false;
        }

        public bool ContainsPhysicalBoneTransform( Transform bone )
        {
            if( !WasInitialized )
            {
                foreach( var chain in chains ) if( chain.ContainsDummyBoneTransform( bone ) ) return true;
            }
            else
            {
                return DictionaryContainsDummyBone( bone );
            }

            return false;
        }

        public bool ContainsAnimatorBoneTransform( string name )
        {
            if( !WasInitialized )
            {
                foreach( var chain in chains ) if( chain.ContainsAnimatorBoneTransform( name ) ) return true;
            }
            else
            {
                return DictionaryContainsAnimatorBone( name );
            }

            return false;
        }

        /// <summary> Checking all physical bones and source bones </summary>
        public bool ContainsBoneTransform( Transform bone )
        {
            if( !WasInitialized )
            {
                foreach( var chain in chains ) if( chain.ContainsDummyBoneTransform( bone ) ) return true;
                foreach( var chain in chains ) if( chain.ContainsAnimatorBoneTransform( bone ) ) return true;
            }
            else
            {
                return DictionaryContainsBone( bone );
            }

            return false;
        }

        public RagdollChainBone DummyStructure_FindConnectionBone( RagdollBonesChain childChain )
        {
            if( childChain.ChainType == ERagdollChainType.Core ) return null;
            if( childChain.BoneSetups[0].SourceBone == null ) return null;

            Transform startBone = childChain.BoneSetups[0].SourceBone;

            while( startBone != null )
            {
                startBone = startBone.parent;
                var parentChainBone = FindAnimatorBoneTransformChainBone( startBone );
                if( parentChainBone != null ) return parentChainBone;
            }

            if( childChain.ChainType != ERagdollChainType.Core )
            {
                var coreChain = GetChain( ERagdollChainType.Core, null );
                if( coreChain.BoneSetups.Count > 0 ) return coreChain.BoneSetups[0];
                UnityEngine.Debug.Log( "[Ragdoll Animator Setup] Can't define right Core bone chain!" );
            }

            return null;
        }

        internal readonly Dictionary<string, RagdollChainBone> nameTransformBoneDictionary = new Dictionary<string, RagdollChainBone>();
        internal readonly Dictionary<Transform, RagdollChainBone> physicalTransformBoneDictionary = new Dictionary<Transform, RagdollChainBone>();
        internal readonly Dictionary<Transform, RagdollChainBone> animatorTransformBoneDictionary = new Dictionary<Transform, RagdollChainBone>();
        internal readonly List<Transform> allBonesList = new List<Transform>();
        internal readonly Dictionary<ERagdollBoneID, RagdollChainBone> boneIDDictionary = new Dictionary<ERagdollBoneID, RagdollChainBone>();

        public void PrepareBonesDicationaries()
        {
            foreach( var chain in chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    if( nameTransformBoneDictionary.ContainsKey( bone.SourceBone.name ) == false ) nameTransformBoneDictionary.Add( bone.SourceBone.name, bone );
                    physicalTransformBoneDictionary.Add( bone.PhysicalDummyBone, bone );

                    foreach( var collS in bone.Colliders )
                    {
                        if( collS.UsingExtraTransform && collS.ColliderExtraTransform )
                        {
                            if( physicalTransformBoneDictionary.ContainsKey( collS.ColliderExtraTransform ) == false )
                                physicalTransformBoneDictionary.Add( collS.ColliderExtraTransform, bone );
                        }
                    }

                    animatorTransformBoneDictionary.Add( bone.SourceBone, bone );
                    if( boneIDDictionary.ContainsKey( bone.BoneID ) == false ) boneIDDictionary.Add( bone.BoneID, bone );
                    allBonesList.Add( bone.SourceBone );
                    allBonesList.Add( bone.PhysicalDummyBone );
                }
            }
        }

        private bool DictionaryContainsAnimatorBone( Transform sourceSkeletonBone )
        {
            return animatorTransformBoneDictionary.ContainsKey( sourceSkeletonBone );
        }

        private bool DictionaryContainsAnimatorBone( string transformName )
        {
            return nameTransformBoneDictionary.ContainsKey( transformName );
        }

        private bool DictionaryContainsDummyBone( Transform sceneBone )
        {
            return physicalTransformBoneDictionary.ContainsKey( sceneBone );
        }

        private bool DictionaryContainsBone( Transform sceneBone )
        {
            return allBonesList.Contains( sceneBone );
        }

        internal RagdollChainBone DictionaryGetBoneControllerBySourceBoneName( string boneName )
        {
            RagdollChainBone get;
            if( nameTransformBoneDictionary.TryGetValue( boneName, out get ) ) return get;
            return null;
        }

        internal RagdollChainBone DictionaryGetBoneSetupByBoneID( ERagdollBoneID id )
        {
            RagdollChainBone get;

            if( id == ERagdollBoneID.Chest )
            {
                if( boneIDDictionary.ContainsKey( id ) == false ) id = ERagdollBoneID.UpperChest;
            }
            else if( id == ERagdollBoneID.UpperChest )
            {
                if( boneIDDictionary.ContainsKey( id ) == false ) id = ERagdollBoneID.Chest;
            }

            if( boneIDDictionary.TryGetValue( id, out get ) ) return get;
            return null;
        }

        internal RagdollChainBone DictionaryGetBoneSetupBySourceBone( Transform sourceSkeletonBone )
        {
            RagdollChainBone get;
            if( animatorTransformBoneDictionary.TryGetValue( sourceSkeletonBone, out get ) ) return get;
            return null;
        }

        internal RagdollChainBone DictionaryGetBoneControllerByRagdollBone( Transform sceneBone )
        {
            RagdollChainBone get;
            if( physicalTransformBoneDictionary.TryGetValue( sceneBone, out get ) ) return get;
            return null;
        }

        /// <summary> Making ragdoll dummy bones ignoring physical collisions with provided collider </summary>
        public void IgnoreCollisionWith( Collider coll, bool ignore = true )
        {
            foreach( var chain in chains )
            {
                chain.IgnoreCollisionsWith( coll, ignore );
            }
        }

        /// <summary> Making ragdoll dummy bones ignoring physical collisions with provided collider </summary>
        public void IgnoreCollisionWith( List<Collider> coll, bool ignore = true )
        {
            foreach( var chain in chains )
                foreach( var bone in chain.BoneSetups )
                {
                    foreach( var collS in bone.Colliders )
                    {
                        foreach( var c in coll )
                        {
                            collS.IgnoreCollisionWith( c, ignore );
                        }
                    }
                }
        }

        /// <summary> Making ragdoll dummy bones ignoring physical collisions with provided collider </summary>
        public void IgnoreCollisionWithUsingBounds( Collider coll, float boundsScale = 1.2f, bool ignore = true )
        {
            Bounds mBounds = coll.bounds;
            mBounds.size *= boundsScale;

            foreach( var chain in chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    foreach( var collS in bone.Colliders )
                    {
                        if( collS.GameCollider )
                        {
                            if( collS.GameCollider.bounds.Intersects( mBounds ) )
                            {
                                Physics.IgnoreCollision( coll, collS.GameCollider, ignore );
                                if( collS.ColliderType == RagdollChainBone.EColliderType.Other && collS.OtherReference ) Physics.IgnoreCollision( coll, collS.OtherReference, ignore );
                            }
                        }
                    }
                }
            }
        }

        bool wasEnsuredCollisionsIgnore = false;
        internal void EnsureCollisionsIgnoreSetup()
        {
            if( wasEnsuredCollisionsIgnore ) return;
#if UNITY_EDITOR
            if( Application.isPlaying )
#endif
                wasEnsuredCollisionsIgnore = true;


            EnsureRelatedCollidersIgnore();

            if( IgnoreSourceSkeletonColliders ) User_FindAllCollidersInsideAndIgnoreTheirCollisionWithDummyColliders( GetBaseTransform() );
            if( IgnoreBoundedColliders ) EnsureRelatedCollidersIgnoreUsingBounds();
        }

        /// <summary> If both ragdoll handlers has same types of chains and same bones count, then all chains settings will be copied (colliders and physics settings) </summary>
        public void CopyChainsSettingsOf( RagdollHandler copyChainsSetupOf )
        {
            for( int i = 0; i < copyChainsSetupOf.chains.Count; i++ )
            {
                var copyChain = copyChainsSetupOf.chains[i];
                if( i >= chains.Count ) return;
                var myChain = chains[i];
                if( myChain.ChainType != copyChain.ChainType ) continue;
                myChain.PasteExtraSettingsOfOtherChain( copyChain );
                myChain.PastePhysicsSettingsOfOtherChain( copyChain );
                myChain.PasteColliderSettingsOfOtherChain( copyChain );
            }
        }

        /// <summary> Calculating count of all bone slots added in the bone chains </summary>
        public int GetAllBonesCount()
        {
            int bones = 0;

            foreach( var chain in chains ) bones += chain.BoneSetups.Count;

            return bones;
        }

        public bool CheckIfBoneDuplicatesExistsInTheBoneSetups()
        {
            foreach( var chain in chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    foreach( var chChain in chains )
                    {
                        if( chChain == chain ) continue;

                        foreach( var chBone in chChain.BoneSetups )
                        {
                            if( bone.SourceBone == chBone.SourceBone ) return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}