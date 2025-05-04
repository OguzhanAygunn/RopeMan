using FIMSpace.AnimationTools;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
        [Tooltip( "Helper information for a few algorithms, to call methods with humanoid / quadruped in mind" )]
        [HideInInspector] public bool IsHumanoid = true;

        /// <summary>
        /// Using auto-find algorithms to define ragdoll dummy skeleton.
        /// Humanoid rigs should work without any problems, but generic rig auto-finding is based on predicition algoritms,
        /// so you need to tweak it after auto finding defining skeleton structure.
        /// </summary>
        public void TryFindBones( bool logResultReport = true )
        {
            if( Mecanim && Mecanim.isHuman )
            {
                IsHumanoid = true;

                chains.Clear();
                AddNewBonesChain( "Core", ERagdollChainType.Core );
                chains[0].BoneSetups = new System.Collections.Generic.List<RagdollChainBone>();
                chains[0].AddNewBone( ERagdollBoneID.Hips, RagdollChainBone.EColliderType.Box );

                if( Mecanim.GetBoneTransform( HumanBodyBones.Chest ) ) { chains[0].AddNewBone( ERagdollBoneID.Chest, RagdollChainBone.EColliderType.Box ); }
                else if( Mecanim.GetBoneTransform( HumanBodyBones.Spine ) ) { chains[0].AddNewBone( ERagdollBoneID.Spine, RagdollChainBone.EColliderType.Box ); }

                if( Mecanim.GetBoneTransform( HumanBodyBones.UpperChest ) ) { chains[0].AddNewBone( ERagdollBoneID.Chest, RagdollChainBone.EColliderType.Box ); }
                if( Mecanim.GetBoneTransform( HumanBodyBones.Head ) ) { chains[0].AddNewBone( ERagdollBoneID.Head, RagdollChainBone.EColliderType.Capsule ); }

                AddNewBonesChain( "Left Arm", ERagdollChainType.LeftArm );
                chains[1].BoneSetups = new System.Collections.Generic.List<RagdollChainBone>();
                chains[1].AddNewBone( ERagdollBoneID.LeftUpperArm );
                chains[1].AddNewBone( ERagdollBoneID.LeftLowerArm );
                if( Mecanim.GetBoneTransform( HumanBodyBones.LeftHand ) ) chains[1].AddNewBone( ERagdollBoneID.LeftHand, RagdollChainBone.EColliderType.Box );

                AddNewBonesChain( "Right Arm", ERagdollChainType.RightArm );
                chains[2].BoneSetups = new System.Collections.Generic.List<RagdollChainBone>();
                chains[2].AddNewBone( ERagdollBoneID.RightUpperArm );
                chains[2].AddNewBone( ERagdollBoneID.RightLowerArm );
                if( Mecanim.GetBoneTransform( HumanBodyBones.RightHand ) ) chains[2].AddNewBone( ERagdollBoneID.RightHand, RagdollChainBone.EColliderType.Box );

                AddNewBonesChain( "Left Leg", ERagdollChainType.LeftLeg );
                chains[3].BoneSetups = new System.Collections.Generic.List<RagdollChainBone>();
                chains[3].AddNewBone( ERagdollBoneID.LeftUpperLeg );
                chains[3].AddNewBone( ERagdollBoneID.LeftLowerLeg );
                if( Mecanim.GetBoneTransform( HumanBodyBones.LeftFoot ) ) chains[3].AddNewBone( ERagdollBoneID.LeftFoot, RagdollChainBone.EColliderType.Box );

                AddNewBonesChain( "Right Leg", ERagdollChainType.RightLeg );
                chains[4].BoneSetups = new System.Collections.Generic.List<RagdollChainBone>();
                chains[4].AddNewBone( ERagdollBoneID.RightUpperLeg );
                chains[4].AddNewBone( ERagdollBoneID.RightLowerLeg );
                if( Mecanim.GetBoneTransform( HumanBodyBones.RightFoot ) ) chains[4].AddNewBone( ERagdollBoneID.RightFoot, RagdollChainBone.EColliderType.Box );

                if( logResultReport ) _EditorDisplayDialog( "Generated Dummy Structure", "Automatically generated dummy structure, using Mecanim Humanoid references.\n\nThat doesn't mean all is ready. You will need to do few corrections to the colliders and adjust bone physics settings for best results." );
            }
            else
            {
                chains.Clear();
                AddNewBonesChain( "Core", ERagdollChainType.Core );
                chains[0].BoneSetups = new System.Collections.Generic.List<RagdollChainBone>();
                chains[0].AddNewBone( null, RagdollChainBone.EColliderType.Box );

                SkeletonRecognize.SkeletonInfo skeletonInfo = new SkeletonRecognize.SkeletonInfo( GetBaseTransform(), null, chains[0].BoneSetups[0].SourceBone );
                chains.Clear();

                #region Core Chain

                var spineCh = AddNewBonesChain( "Core", ERagdollChainType.Core );
                spineCh.BoneSetups = new System.Collections.Generic.List<RagdollChainBone>();
                spineCh.AddNewBone( false );
                spineCh.BoneSetups[0].BaseColliderSetup.ColliderType = RagdollChainBone.EColliderType.Box;
                spineCh.BoneSetups[0].SourceBone = skeletonInfo.ProbablyHips;

                for( int i = 0; i < skeletonInfo.ProbablySpineChainShort.Count; i++ )
                {
                    spineCh.AddNewBone( false );
                    spineCh.BoneSetups[spineCh.BoneSetups.Count - 1].BaseColliderSetup.ColliderType = RagdollChainBone.EColliderType.Box;
                    spineCh.BoneSetups[spineCh.BoneSetups.Count - 1].SourceBone = skeletonInfo.ProbablySpineChainShort[i];
                }

                if( skeletonInfo.ProbablyHead && spineCh.ContainsAnimatorBoneTransform( skeletonInfo.ProbablyHead ) == false )
                {
                    spineCh.AddNewBone( false );
                    spineCh.BoneSetups[spineCh.BoneSetups.Count - 1].BaseColliderSetup.ColliderType = RagdollChainBone.EColliderType.Capsule;
                    spineCh.BoneSetups[spineCh.BoneSetups.Count - 1].SourceBone = skeletonInfo.ProbablyHead;
                }

                #endregion Core Chain

                string suffix = "";
                if( logResultReport )
                {
                    // Hardly suggesting to check auto-found limbs on unsure skeleton info results
                    //if( skeletonInfo.WhatIsIt == SkeletonRecognize.EWhatIsIt.Creature ) suffix = "?";
                    //else
                    if( skeletonInfo.WhatIsIt == SkeletonRecognize.EWhatIsIt.Unknown ) suffix = "?";
                }

                IsHumanoid = skeletonInfo.WhatIsIt == SkeletonRecognize.EWhatIsIt.Humanoidal;

                #region Legs

                for( int l = 0; l < skeletonInfo.ProbablyRightLegs.Count; l++ )
                {
                    var probablyLegs = skeletonInfo.ProbablyRightLegs[l];

                    for( int s = 0; s < skeletonInfo.ProbablySpineChain.Count; s++ )
                    {
                        if( probablyLegs.Contains( skeletonInfo.ProbablySpineChain[s] ) ) probablyLegs.Remove( skeletonInfo.ProbablySpineChain[s] );
                    }

                    var legCh = AddNewBonesChain( "Right Leg" + suffix, ERagdollChainType.RightLeg );
                    legCh.BoneSetups = new System.Collections.Generic.List<RagdollChainBone>();

                    for( int b = 0; b < probablyLegs.Count; b++ )
                    {
                        legCh.AddNewBone( false );
                        legCh.BoneSetups[b].BaseColliderSetup.ColliderType = ( b == probablyLegs.Count - 1 && b > 1 ) ? RagdollChainBone.EColliderType.Box : RagdollChainBone.EColliderType.Capsule;
                        legCh.BoneSetups[b].SourceBone = probablyLegs[b];
                    }
                }

                for( int l = 0; l < skeletonInfo.ProbablyLeftLegs.Count; l++ )
                {
                    var probablyLegs = skeletonInfo.ProbablyLeftLegs[l];

                    for( int s = 0; s < skeletonInfo.ProbablySpineChain.Count; s++ )
                    {
                        if( probablyLegs.Contains( skeletonInfo.ProbablySpineChain[s] ) ) probablyLegs.Remove( skeletonInfo.ProbablySpineChain[s] );
                    }

                    var legCh = AddNewBonesChain( "Left Leg" + suffix, ERagdollChainType.LeftLeg );
                    legCh.BoneSetups = new System.Collections.Generic.List<RagdollChainBone>();

                    for( int b = 0; b < probablyLegs.Count; b++ )
                    {
                        legCh.AddNewBone( false );
                        legCh.BoneSetups[b].BaseColliderSetup.ColliderType = ( b == probablyLegs.Count - 1 && b > 1 ) ? RagdollChainBone.EColliderType.Box : RagdollChainBone.EColliderType.Capsule;
                        legCh.BoneSetups[b].SourceBone = probablyLegs[b];
                    }
                }

                #endregion Legs

                #region Arms

                for( int l = 0; l < skeletonInfo.ProbablyRightArms.Count; l++ )
                {
                    var probablyArms = skeletonInfo.ProbablyRightArms[l];

                    for( int s = 0; s < skeletonInfo.ProbablySpineChain.Count; s++ )
                    {
                        if( probablyArms.Contains( skeletonInfo.ProbablySpineChain[s] ) ) probablyArms.Remove( skeletonInfo.ProbablySpineChain[s] );
                    }

                    var armChain = AddNewBonesChain( "Right Arm" + suffix, ERagdollChainType.RightArm );
                    armChain.BoneSetups = new System.Collections.Generic.List<RagdollChainBone>();

                    for( int b = 0; b < probablyArms.Count; b++ )
                    {
                        armChain.AddNewBone( false );
                        armChain.BoneSetups[b].BaseColliderSetup.ColliderType = ( b == probablyArms.Count - 1 && b > 1 ) ? RagdollChainBone.EColliderType.Box : RagdollChainBone.EColliderType.Capsule;
                        armChain.BoneSetups[b].SourceBone = probablyArms[b];
                    }
                }

                for( int l = 0; l < skeletonInfo.ProbablyLeftArms.Count; l++ )
                {
                    var probablyArms = skeletonInfo.ProbablyLeftArms[l];

                    for( int s = 0; s < skeletonInfo.ProbablySpineChain.Count; s++ )
                    {
                        if( probablyArms.Contains( skeletonInfo.ProbablySpineChain[s] ) ) probablyArms.Remove( skeletonInfo.ProbablySpineChain[s] );
                    }

                    var armChain = AddNewBonesChain( "Left Arm" + suffix, ERagdollChainType.LeftArm );
                    armChain.BoneSetups = new System.Collections.Generic.List<RagdollChainBone>();

                    for( int b = 0; b < probablyArms.Count; b++ )
                    {
                        armChain.AddNewBone( false );
                        armChain.BoneSetups[b].BaseColliderSetup.ColliderType = ( b == probablyArms.Count - 1 && b > 1 ) ? RagdollChainBone.EColliderType.Box : RagdollChainBone.EColliderType.Capsule;
                        armChain.BoneSetups[b].SourceBone = probablyArms[b];
                    }
                }

                #endregion Arms

                foreach( var chain in chains )
                {
                    chain.TryIdentifyBoneIDs();
                }

                if( logResultReport )
                {
                    if( skeletonInfo.WhatIsIt == SkeletonRecognize.EWhatIsIt.Humanoidal ) _EditorDisplayDialog( "Generated Dummy Structure", "Automatically generated dummy structure, using predicted transforms.\nAlgorithm detected skeleton structure matching with Humanoid.\nYou probably will need to do adjustements!\n\nCheck if limbs and it's bone references are right!\nYou will need to do colliders corrections and adjust bone physics settings.\n\n" + skeletonInfo.GetLog() );
                    else if( skeletonInfo.WhatIsIt == SkeletonRecognize.EWhatIsIt.Quadroped ) _EditorDisplayDialog( "Generated Dummy Structure", "Automatically generated dummy structure, using predicted transforms.\nAlgorithm detected skeleton structure matching with Quadroped.\nYou will need to do adjustements!\n\nChecking if right limbs and bones was added IS REQUIRED. You will need to do colliders corrections and adjust bone physics settings.\n\n" + skeletonInfo.GetLog() );
                    else _EditorDisplayDialog( "Generated Dummy Structure", "Automatically generated dummy structure, using predicted transforms.\nAlgorithm couldn't specify type of the skeleton.\nPredicted limbs are added, BUT MOST LIKELY THEY'RE WRONG, and you should define ragdoll skeleton manually!\n\n" + skeletonInfo.GetLog() );
                }
            }

#if UNITY_EDITOR

            _EditorCategory = ERagdollAnimSection.Construct;
            _Editor_ChainCategory = EBoneChainCategory.Colliders;

#endif

        }

        /// <summary>
        /// Executed only when no bone is defined in the chain.
        /// Basing on the chain type, trying to find first bone for the chain.
        /// Only for core/leg/arm chains.
        /// </summary>
        public void TryAutoFindChainFirstBone( RagdollBonesChain chain )
        {
            if( chain.BoneSetups.Count == 0 )
            {
                chain.AddNewBone( false, RagdollChainBone.EColliderType.Box ); // Define first bone
            }
            else if( chain.BoneSetups[0].SourceBone != null ) return; // Already assigned

            if( chain.ChainType == ERagdollChainType.Core ) // Find pelvis/hips
            {
                if( Mecanim && Mecanim.isHuman ) chain.BoneSetups[0].SourceBone = Mecanim.GetBoneTransform( HumanBodyBones.Hips );
                else
                {
                    SkeletonRecognize.SkeletonInfo skeletonInfo = new SkeletonRecognize.SkeletonInfo( GetBaseTransform(), null, null );
                    if( skeletonInfo.ProbablyHips ) chain.BoneSetups[0].SourceBone = skeletonInfo.ProbablyHips;
                }
            }
            else if( chain.ChainType.IsLeg() )
            {
                if( Mecanim && Mecanim.isHuman )
                {
                    if( chain.ChainType == ERagdollChainType.LeftLeg )
                        chain.BoneSetups[0].SourceBone = Mecanim.GetBoneTransform( HumanBodyBones.LeftUpperLeg );
                    else
                        chain.BoneSetups[0].SourceBone = Mecanim.GetBoneTransform( HumanBodyBones.RightUpperLeg );
                }
                else
                {
                    SkeletonRecognize.SkeletonInfo skeletonInfo = new SkeletonRecognize.SkeletonInfo( GetBaseTransform(), null, null );

                    if( chain.ChainType == ERagdollChainType.LeftLeg )
                    { if( skeletonInfo.LeftLegs > 0 && skeletonInfo.ProbablyLeftLegs.Count > 0 ) chain.BoneSetups[0].SourceBone = skeletonInfo.ProbablyLeftLegs[0][0]; }
                    else
                    { if( skeletonInfo.RightLegs > 0 && skeletonInfo.ProbablyRightLegs.Count > 0 ) chain.BoneSetups[0].SourceBone = skeletonInfo.ProbablyRightLegs[0][0]; }
                }
            }
            else if( chain.ChainType.IsArm() )
            {
                if( Mecanim && Mecanim.isHuman )
                {
                    if( chain.ChainType == ERagdollChainType.LeftArm )
                        chain.BoneSetups[0].SourceBone = Mecanim.GetBoneTransform( HumanBodyBones.LeftUpperArm );
                    else
                        chain.BoneSetups[0].SourceBone = Mecanim.GetBoneTransform( HumanBodyBones.RightUpperArm );
                }
                else
                {
                    SkeletonRecognize.SkeletonInfo skeletonInfo = new SkeletonRecognize.SkeletonInfo( GetBaseTransform(), null, null );

                    if( chain.ChainType == ERagdollChainType.LeftArm )
                    { if( skeletonInfo.LeftArms > 0 && skeletonInfo.ProbablyLeftArms.Count > 0 ) chain.BoneSetups[0].SourceBone = skeletonInfo.ProbablyLeftArms[0][0]; }
                    else
                    { if( skeletonInfo.RightArms > 0 && skeletonInfo.ProbablyRightArms.Count > 0 ) chain.BoneSetups[0].SourceBone = skeletonInfo.ProbablyRightArms[0][0]; }
                }
            }
        }

        private void _EditorDisplayDialog( string title, string description )
        {
#if UNITY_EDITOR
            if( Application.isPlaying )
                Debug.Log( "[" + title + "] " + description );
            else
                UnityEditor.EditorUtility.DisplayDialog( title, description, "Ok" );
#endif
        }
    }
}