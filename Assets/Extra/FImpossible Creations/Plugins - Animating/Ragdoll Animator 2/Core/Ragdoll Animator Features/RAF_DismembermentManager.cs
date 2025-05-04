#if UNITY_EDITOR

using UnityEditor;

#endif

using System.Collections.Generic;
using System;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public enum EDismemberType
    {
        /// <summary>
        /// Regardless being dismembered, bones still will be animated with physics
        /// </summary>
        AnimatedDismembered,

        /// <summary>
        /// Dismember and falling only with joint limits
        /// </summary>
        Disconnect,

        /// <summary>
        /// Removing bones from the update loop of ragdoll animator, can be used for custom handling, like adding joints on the source skeleton bones
        /// </summary>
        CustomHandling
    }

    public class RAF_DismembermentManager : RagdollAnimatorFeatureBase
    {
        public override bool OnInit()
        {
            ParentRagdollHandler.AddToPostLateUpdateLoop( LateUpdate );
            return base.OnInit();
        }

        public override void OnDestroyFeature()
        {
            ParentRagdollHandler.RemoveFromPostLateUpdateLoop( LateUpdate );
        }

        public void LateUpdate()
        {
            float finalBlend = ParentRagdollHandler.GetTotalBlend(); // Calculate ragdoll animator's blend properties

            if( update_dismemberedSync.Count > 0 )
            {
                if( finalBlend >= 1f )
                {
                    for( int i = 0; i < update_dismemberedSync.Count; i++ )
                    {
                        var bone = update_dismemberedSync[i];

                        // Just apply physical bodies pose to the source bones
                        bone.SourceBone.SetPositionAndRotation( bone.PhysicalDummyBone.position, bone.PhysicalDummyBone.rotation );
                    }
                }
                else // Support Blending
                {
                    for( int i = 0; i < update_dismemberedSync.Count; i++ )
                    {
                        var bone = update_dismemberedSync[i];

                        bone.SourceBone.SetPositionAndRotation
                            (
                            Vector3.Lerp( bone.SourceBone.position, bone.PhysicalDummyBone.position, finalBlend ),
                            Quaternion.Slerp( bone.SourceBone.rotation, bone.PhysicalDummyBone.rotation, finalBlend )
                            );
                    }
                }
            }

            if( update_dismemberedAnimated.Count > 0 )
            {
                // Ensure updating positions to the dismembered body parts
                if( ParentRagdollHandler.ApplyPositions == false )
                {
                    for( int i = 0; i < update_dismemberedAnimated.Count; i++ )
                    {
                        var bone = update_dismemberedAnimated[i];
                        bone.BoneProcessor.ApplyPhysicalPositionToTheBone( bone.ParentChain.GetBlend( finalBlend ) );
                    }
                }
            }
        }

        #region Update Lists

        /// <summary>
        /// Sort to call propert parent -> child -> child.child hierarchy coords update order
        /// </summary>
        private void SortBySourceBoneDepth( List<RagdollChainBone> bones )
        {
            bones.Sort( ( x, y ) => x.SourceBoneDepth.CompareTo( y.SourceBoneDepth ) );
        }

        private void AddBoneToDismemberedAnimatedUpdate( RagdollChainBone bone )
        { if( update_dismemberedAnimated.Contains( bone ) ) return; update_dismemberedAnimated.Add( bone ); SortBySourceBoneDepth( update_dismemberedAnimated ); }

        private void RemoveBoneFromDismemberedAnimatedUpdate( RagdollChainBone bone )
        { if( !update_dismemberedAnimated.Contains( bone ) ) return; update_dismemberedAnimated.Remove( bone ); }

        private List<RagdollChainBone> update_dismemberedAnimated = new List<RagdollChainBone>();

        private void AddBoneToDismemberedSyncUpdate( RagdollChainBone bone )
        { if( update_dismemberedSync.Contains( bone ) ) return; update_dismemberedSync.Add( bone ); SortBySourceBoneDepth( update_dismemberedSync ); }

        private void RemoveBoneFromDismemberedSyncUpdate( RagdollChainBone bone )
        { if( !update_dismemberedSync.Contains( bone ) ) return; update_dismemberedSync.Remove( bone ); }

        private List<RagdollChainBone> update_dismemberedSync = new List<RagdollChainBone>();

        #endregion Update Lists

        #region On Dismember Bone Actions

        private List<Action<RagdollChainBone>> OnBoneDismemberActions = new List<Action<RagdollChainBone>>();

        public void AddToOnDismemberBoneActions( Action<RagdollChainBone> action )
        {
            if( OnBoneDismemberActions.Contains( action ) == false ) OnBoneDismemberActions.Add( action );
        }

        public void RemoveFromOnDismemberBoneActions( Action<RagdollChainBone> action )
        {
            if( OnBoneDismemberActions.Contains( action ) ) OnBoneDismemberActions.Remove( action );
        }

        private void OnDismemberBone( RagdollChainBone bone )
        {
            foreach( var action in OnBoneDismemberActions ) action.Invoke( bone );
        }

        #endregion On Dismember Bone Actions

        #region GUI Code

#if UNITY_EDITOR

        public override string Editor_FeatureDescription => "Helping remove bones from ragdoll animator update loops and updating dismembered body parts poses.";

        public override void Editor_InspectorGUI( SerializedProperty toDirty, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            if( ragdollHandler.WasInitialized == false )
            {
                EditorGUILayout.HelpBox( "During runtime, there will be displayed dismemberement report", UnityEditor.MessageType.None );
            }
            else
            {
                EditorGUILayout.LabelField( "Dismemberement Report:", EditorStyles.boldLabel );

                GUILayout.Space( 5 );
                EditorGUILayout.LabelField( "Dismembered Bones:" );

                if( update_dismemberedSync.Count == 0 ) EditorGUILayout.LabelField( "None" );
                for( int i = 0; i < update_dismemberedSync.Count; i++ )
                {
                    EditorGUILayout.ObjectField( update_dismemberedSync[i].SourceBone, typeof( Transform ), true );
                }

                GUILayout.Space( 5 );
                EditorGUILayout.LabelField( "Dismembered Animated Bones:" );

                if( update_dismemberedAnimated.Count == 0 ) EditorGUILayout.LabelField( "None" );
                for( int i = 0; i < update_dismemberedAnimated.Count; i++ )
                {
                    EditorGUILayout.ObjectField( update_dismemberedAnimated[i].SourceBone, typeof( Transform ), true );
                }
            }
        }

#endif

        #endregion GUI Code

        /// <summary>
        /// Making bone dismembered from the rest of the body
        /// </summary>
        public void DismemberBone( RagdollChainBone bone, EDismemberType type )
        {
            if( ParentRagdollHandler.WasInitialized == false ) return;

            ApplyBoneSwitchesOnDismember( bone );

            // Break joint and continue updating it
            if( type == EDismemberType.AnimatedDismembered )
            {
                var childBones = bone.ParentChain.CollectAllConnectedBones( bone );

                foreach( var cbone in childBones )
                {
                    ApplyBoneSwitchesOnDismember( cbone );
                    ApplyFallDismemberParameters( cbone );
                    cbone.ParentDismembered = true;
                    AddBoneToDismemberedAnimatedUpdate( cbone );
                }

                bone.SetJointFreeMotion();
            }
            // Break joint, remove from updating, but apply to the source skeleton pose
            else if( type == EDismemberType.Disconnect )
            {
                var childBones = bone.ParentChain.CollectAllConnectedBones( bone );

                foreach( var fBone in bone.ParentChain.CollectAllFillBones( childBones ) )
                {
                    bone.ParentChain.ParentHandler.skeletonFillExtraBonesList.Remove( fBone );
                    bone.ParentChain.ParentHandler.skeletonFillExtraBones.Remove( fBone.SourceBone );
                }

                foreach( var cbone in childBones )
                {
                    ApplyBoneSwitchesOnDismember( cbone );
                    cbone.ParentDismembered = true;
                    cbone.ParentChain.RemoveRuntimeBoneProcessing( cbone );
                    cbone.ParentChain.ParentHandler.RemoveBoneFromRuntimeCalculations( cbone );
                    cbone.SwitchOffJointAnimationMatching();

                    AddBoneToDismemberedSyncUpdate( cbone );
                }

                bone.SetJointFreeMotion();
            }
            // Just remove bone and its child bones from updating and destroying them on the scene
            else if( type == EDismemberType.CustomHandling )
            {
                bone.ParentChain.RemoveBoneAndItsChildren( bone );
            }

            if( bone.ParentChain.Detach == false )
            {
                bone.GameRigidbody.transform.SetParent( bone.ParentChain.ParentHandler.Dummy_Container, true );
            }

            bone.WasDismembered = true;

            OnDismemberBone( bone ); // For custom on ismember events like particle effects
        }

        private void ApplyBoneSwitchesOnDismember( RagdollChainBone cbone )
        {
            cbone.BypassKinematicControl = true;
            cbone.ForceKinematicOnStanding = false;
            cbone.GameRigidbody.isKinematic = false;
        }

        /// <summary>
        /// Apply fall changes to the target dismembered bones
        /// </summary>
        private void ApplyFallDismemberParameters( RagdollChainBone bone )
        {
            if( bone.WasDismembered ) return;
            bone.RefreshJoint( bone.ParentChain, true, false, true );
            bone.RefreshDynamicPhysicalParameters( bone.ParentChain, true );
            bone.RefreshCollider( bone.ParentChain, true, false );
            bone.Joint_SetAngularMotionLock( ConfigurableJointMotion.Limited );
        }

        /// <summary>
        /// Restoring detached bones (not working for CustomHandling since CustomHandling it is destroying physical dummy bones)
        /// Restoring can work incorrectly with bone chains which are not detached.
        /// </summary>
        public void RestoreDismemberedBones()
        {
            //ParentRagdollHandler.ForceFixedReinitialization();
            //ParentRagdollHandler.ApplyTPoseOnModel( true );
            //ParentRagdollHandler.User_ForceMatchPhysicalBonesWithAnimator( true );

            var anchor = ParentRagdollHandler.GetAnchorBoneController;

            // Fix anchor position for bones restore action
            bool preKinematic = anchor.GameRigidbody.isKinematic;
            anchor.GameRigidbody.isKinematic = true;
            anchor.PhysicalDummyBone.position = anchor.BoneProcessor.AnimatorPosition;
            anchor.PhysicalDummyBone.rotation = anchor.BoneProcessor.AnimatorRotation;
            anchor.GameRigidbody.isKinematic = preKinematic;

            // Restore sync bones into ragdoll animator update process
            for( int i = 0; i < update_dismemberedSync.Count; i++ )
            {
                var bone = update_dismemberedSync[i];

                if( !bone.ParentChain.BoneSetups.Contains( bone ) ) // Not restored
                {
                    bone.ParentChain.BoneSetups.Add( bone ); // Restore add it to the chain
                    bone.ParentChain.RuntimeBoneProcessors.Add( bone.BoneProcessor );
                }

                bone.WasDismembered = false;
                bone.ParentDismembered = false;
            }

            // Apply sync disconnected bones restore
            for( int i = 0; i < update_dismemberedSync.Count; i++ )
            {
                var bone = update_dismemberedSync[i];
                ParentRagdollHandler.RestoreBoneToRuntimeCalculations( bone );
                ProceedRestoreBoneJoint( bone );
            }

            // Apply restore for animated bones
            for( int i = 0; i < update_dismemberedAnimated.Count; i++ )
            {
                var bone = update_dismemberedAnimated[i];

                if( !bone.ParentChain.BoneSetups.Contains( bone ) ) // Not restored
                {
                    bone.ParentChain.BoneSetups.Add( bone ); // Restore add it to the chain
                }

                bone.WasDismembered = false;
                bone.ParentDismembered = false;
                ProceedRestoreBoneJoint( bone );
            }

            // Refresh all ragdoll bones
            ParentRagdollHandler.User_UpdateJointsPlayParameters( false );
            ParentRagdollHandler.User_UpdateAllBonesParametersAfterManualChanges();

            update_dismemberedSync.Clear();
            update_dismemberedAnimated.Clear();
        }

        private void ProceedRestoreBoneJoint( RagdollChainBone bone )
        {
            bone.GameRigidbody.isKinematic = true;
            bone.PhysicalDummyBone.transform.position = bone.BoneProcessor.AnimatorPosition;
            bone.PhysicalDummyBone.transform.rotation = bone.BoneProcessor.AnimatorRotation;
            bone.PhysicalDummyBone.position = bone.BoneProcessor.AnimatorPosition;
            bone.PhysicalDummyBone.rotation = bone.BoneProcessor.AnimatorRotation;
            bone.GameRigidbody.isKinematic = false;

            bone.HardMatchingMultiply = 1f;

            bone.RefreshJoint( bone.ParentChain, bone.ParentChain.ParentHandler.IsFallingOrSleep, false, true );
            bone.RefreshJointLimitSwitch( bone.ParentChain );
        }
    }
}