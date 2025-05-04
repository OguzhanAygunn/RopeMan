using FIMSpace.FGenerating;
using System.Collections.Generic;

#if UNITY_EDITOR

using FIMSpace.FEditor;
using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_BlendOnCollisions : RagdollAnimatorFeatureBase
    {
        protected List<BlendOnCollisionChain> blendChains = null;
        protected List<RagdollBonesChain> legChains = null;
        protected List<BlendOnCollisionChain> legBlendChains = null;
        protected RagdollBonesChain coreChain = null;
        protected BlendOnCollisionChain coreBlendChain = null;

        protected FUniversalVariable blendingSpeed;
        protected FUniversalVariable applyOnWholeChains;
        protected FUniversalVariable sensitiveBlend;
        protected FUniversalVariable skipFeet;
        protected FUniversalVariable ignoreSelf;
        protected FUniversalVariable coreBlendLegs;
        protected FUniversalVariable turnOffLegs;
        //protected FUniversalVariable dampOnCollision;

        public override bool OnInit()
        {
            bool initialized = base.OnInit();
            if( !initialized ) return false;

            InitIndicators();

            // Get properties
            blendingSpeed = InitializedWith.RequestVariable( "Blending Speed:", 0.75f );
            applyOnWholeChains = InitializedWith.RequestVariable( "Apply on whole chains:", true );
            sensitiveBlend = InitializedWith.RequestVariable( "Sensitive Blend:", true );
            skipFeet = InitializedWith.RequestVariable( "Skip Feet:", true );
            ignoreSelf = InitializedWith.RequestVariable( "Ignore Self Collision Blend:", true );
            coreBlendLegs = InitializedWith.RequestVariable( "Blend Legs With Core:", false );
            //dampOnCollision = InitializedWith.RequestVariable( "Damp On Collision:", true);

            blendChains = new List<BlendOnCollisionChain>();
            legBlendChains = new List<BlendOnCollisionChain>();
            turnOffLegs = InitializedWith.RequestVariable( "Turn Off Legs:", true );

            // Prepare controllers for chains and bones
            foreach( var chain in ParentRagdollHandler.Chains )
            {
                if( chain.ChainType.IsLeg() )
                {
                    if( legChains == null ) legChains = new List<RagdollBonesChain>();
                    legChains.Add( chain );
                    if( turnOffLegs.GetBool() ) continue; // If don't need leg blending
                }

                BlendOnCollisionChain bChain = new BlendOnCollisionChain( chain );

                if( chain.ChainType.IsLeg() && skipFeet.GetBool() ) bChain.SkipLastBoneCollisionCheck = true;

                foreach( RagdollChainBone bone in chain.BoneSetups )
                {
                    var handler = GetCollisionHandler( bone );
                    if( handler == null ) continue;

                    BlendOnCollisionBone nBone = new BlendOnCollisionBone( bChain, bone, handler );

                    bChain.Bones.Add( nBone );
                }

                if( chain.ChainType == ERagdollChainType.Core )
                {
                    coreChain = chain;
                    coreBlendChain = bChain;
                }
                else if( chain.ChainType.IsLeg() )
                {
                    legBlendChains.Add( bChain );
                }

                blendChains.Add( bChain );
            }

            // Finding chains parenting for sensitive blending
            foreach( var bChain in blendChains )
            {
                if( bChain.OwnerChain.ConnectionBone == null ) continue;
                var targetParentChain = ParentRagdollHandler.GetChain( bChain.OwnerChain.ConnectionBone );
                if( targetParentChain == null ) continue;

                foreach( var oChain in blendChains )
                {
                    if( oChain == bChain ) continue;
                    if( targetParentChain == oChain.OwnerChain ) { bChain.ParentOfChain = oChain; break; }
                }
            }

            // Add to update loop
            ParentRagdollHandler.AddToFixedUpdateLoop( UpdateBlending );

            return initialized;
        }

        protected virtual void InitIndicators()
        {
            ParentRagdollHandler.PrepareDummyBonesCollisionIndicators( true );
        }

        protected virtual RA2BoneCollisionHandlerBase GetCollisionHandler( RagdollChainBone bone )
        {
            return bone.MainBoneCollider.GetComponent<RA2BoneCollisionHandler>();
        }

        public override void OnDestroyFeature()
        {
            ParentRagdollHandler.RemoveFromFixedUpdateLoop( UpdateBlending );

            foreach( var chain in ParentRagdollHandler.Chains )
                foreach( var bone in chain.BoneSetups )
                    bone.BoneBlendMultiplier = 1f; // Restore parameter
        }

        private void UpdateBlending()
        {
            float dt = ParentRagdollHandler.UnscaledTime ? Time.fixedUnscaledDeltaTime : Time.fixedDeltaTime;

            if( InitializedWith.Enabled == false )
            {
                BlendInAll( ( 5f + blendingSpeed.GetFloat() * 12f ) * dt );
                return;
            }

            float delta = ( 5f + blendingSpeed.GetFloat() * 12f ) * dt;

            if( ParentRagdollHandler.AnimatingMode == RagdollHandler.EAnimatingMode.Standing && InitializedWith.Enabled )
            {
                bool applyAll = applyOnWholeChains.GetBool();
                bool sensitive = sensitiveBlend.GetBool();
                float offDelay = 0.4f - 0.3f * blendingSpeed.GetFloat();

                foreach( var chain in blendChains ) chain.Update( delta, applyAll, sensitive, offDelay );
                // TODO: Check if angle difference then blend bone too?

                // After updating all chains and their relation blendings, change dummy bones blendings
                foreach( var chain in blendChains ) chain.ApplyBoneControllerBlendsToDummyBones();

                if( turnOffLegs.GetBool() )
                {
                    if( ParentRagdollHandler.LegsBlendInRequest )
                        foreach( var chain in legChains ) { BlendLegChain( chain, 1f, delta, skipFeet.GetBool() ); }
                    else
                        foreach( var chain in legChains ) { BlendLegChain( chain, 0f, delta, false ); }
                }
                else
                {
                    if( coreBlendLegs.GetBool() )
                    {
                        bool wasColliding = false;
                        for( int i = 0; i < coreBlendChain.Bones.Count; i++ )
                        {
                            if( coreBlendChain.Bones[i].WasColliding( 0.25f ) ) { wasColliding = true; break; }
                        }

                        if( wasColliding || ParentRagdollHandler.LegsBlendInRequest )
                            foreach( var chain in legBlendChains ) foreach( var bone in chain.Bones ) { bone.BlendIn( delta ); chain.ApplyBoneControllerBlendsToDummyBones(); }
                    }
                    else if( ParentRagdollHandler.LegsBlendInRequest )
                    {
                        foreach( var chain in legBlendChains ) foreach( var bone in chain.Bones ) { bone.BlendIn( delta ); chain.ApplyBoneControllerBlendsToDummyBones(); }
                    }
                }
            }
            else // Disable blendout for non standing
            {
                BlendInAll( delta );
            }
        }

        public void BlendInAll( float delta )
        {
            foreach( var chain in blendChains )
            {
                foreach( var bone in chain.Bones ) bone.BlendIn( delta );
                chain.ApplyBoneControllerBlendsToDummyBones();
            }

            if( turnOffLegs.GetBool() ) foreach( var chain in legChains ) { BlendLegChain( chain, 1f, delta, false ); }
        }

        private void BlendLegChain( RagdollBonesChain chain, float target, float delta, bool skipFeet )
        {
            if( skipFeet )
            {
                if( target > 0f )
                {
                    if( chain.BoneSetups.Count > 2 )
                    {
                        for( int i = 0; i < chain.BoneSetups.Count - 1; i++ )
                        {
                            var bone = chain.BoneSetups[i];
                            bone.BoneBlendMultiplier = Mathf.MoveTowards( bone.BoneBlendMultiplier, target, delta );
                        }

                        var feetBone = chain.BoneSetups[chain.BoneSetups.Count - 1];
                        feetBone.BoneBlendMultiplier = Mathf.MoveTowards( feetBone.BoneBlendMultiplier, 0f, delta );

                        return;
                    }
                }
            }

            foreach( var bone in chain.BoneSetups )
            {
                bone.BoneBlendMultiplier = Mathf.MoveTowards( bone.BoneBlendMultiplier, target, delta );
            }
        }

        protected class BlendOnCollisionChain
        {
            public RagdollBonesChain OwnerChain;
            public BlendOnCollisionChain ParentOfChain;
            public List<BlendOnCollisionBone> Bones = new List<BlendOnCollisionBone>();
            public bool SkipLastBoneCollisionCheck = false;

#if UNITY_EDITOR

            /// <summary> Just for debugging </summary>
            public float LastBlend = 0f;

#endif

            public BlendOnCollisionChain( RagdollBonesChain chain )
            {
                OwnerChain = chain;
            }

            public void Update( float delta, bool applyOnWholeChain, bool sensitive, float blendOffDelay )
            {
#if UNITY_EDITOR
                LastBlend = 0f;
#endif

                BlendOnCollisionBone collidingBone = null;
                int collidingIndex = -1;

                int startI = Bones.Count - 1;
                if( SkipLastBoneCollisionCheck ) startI = Bones.Count - 2;

                for( int i = startI; i >= 0; i-- )
                {
                    if( Bones[i].CollisionHandler.CollidesWithAnything() )
                    {
                        collidingBone = Bones[i];
                        collidingIndex = i;
                        break;
                    }
                }

                if( collidingBone == null )
                {
                    foreach( var bone in Bones ) bone.BlendOff( delta * 0.65f, blendOffDelay );
                }
                else
                {
                    if( applyOnWholeChain ) // All bones in the chain blend in
                    {
                        foreach( var bone in Bones ) bone.BlendIn( delta );

                        if( sensitive ) // Chain + parent max 2 bones
                        {
                            if( ParentOfChain != null )
                            {
                                for( int i = 0; i < Mathf.Min( 1, ParentOfChain.Bones.Count ); i++ )
                                {
                                    ParentOfChain.Bones[i].BlendIn( delta * 0.6f );
                                }
                            }
                        }
                    }
                    else // Single bones blending (first bone to all parent bones blend in)
                    {
                        if( sensitive )
                        {
                            // Check if collision already happens on the main bone of the chain
                            if( Bones[0].CollisionHandler.CollidesWithAnything() ) // blend all + parent first bone
                            {
                                if( ParentOfChain != null ) if( ParentOfChain.Bones.Count > 0 ) ParentOfChain.Bones[0].BlendIn( delta * 0.75f );
                            }
                            else // Blend parent bones of colliding + first parent
                            {
                                for( int i = collidingIndex - 1; i < Bones.Count; i++ ) Bones[i].BlendIn( delta );
                                for( int i = 0; i < collidingIndex - 1; i++ ) Bones[i].BlendOff( delta * 0.6f, blendOffDelay );
                            }
                        }
                        else // Blend just colliding bone and its parent bones
                        {
                            for( int i = collidingIndex; i < Bones.Count; i++ ) Bones[i].BlendIn( delta );
                            for( int i = 0; i < collidingIndex; i++ ) Bones[i].BlendOff( delta * 0.6f, blendOffDelay );
                        }
                    }
                }
            }

            public void ApplyBoneControllerBlendsToDummyBones()
            {
                foreach( var bone in Bones )
                {
                    bone.ParentBone.BoneBlendMultiplier = bone.Blend;
                }
            }
        }

        protected class BlendOnCollisionBone
        {
            public BlendOnCollisionChain ParentChain;
            public RagdollChainBone ParentBone;
            public RA2BoneCollisionHandlerBase CollisionHandler;
            public float Blend = 0f;
            private float lastBlendIn = -100f;

            public bool WasColliding( float duration = 0.4f ) => Time.fixedTime - lastBlendIn < duration;

            public BlendOnCollisionBone( BlendOnCollisionChain chain, RagdollChainBone bone, RA2BoneCollisionHandlerBase handler )
            {
                ParentChain = chain;
                ParentBone = bone;
                CollisionHandler = handler;
            }

            private float sd = 0f;

            public void BlendIn( float delta )
            {
                lastBlendIn = Time.fixedTime;
                if( Blend == 0f ) Blend = 0.125f;
                Blend = Mathf.MoveTowards( Blend, 1f, delta );

#if UNITY_EDITOR
                ParentChain.LastBlend = Mathf.Max( ParentChain.LastBlend, Blend );
#endif
            }

            public void BlendOff( float delta, float blendOffDelay )
            {
                if( WasColliding( blendOffDelay ) ) return; // Not allow blend off when lately was colliding
                Blend = Mathf.SmoothDamp( Blend, 0f, ref sd, 1f, 10000000f, delta );
                if( Blend < .0005f ) Blend = 0f;
                //Blend = Mathf.MoveTowards( Blend, 0f, delta );

#if UNITY_EDITOR
                ParentChain.LastBlend = Mathf.Max( ParentChain.LastBlend, Blend );
#endif
            }
        }

#if UNITY_EDITOR

        public override string Editor_FeatureDescription => "During standing mode, not animating bones with physics, until some collision with dummy bone happens.\n!Generating collision handlers on the bones with enabled collisions collecting!";

        public override void Editor_InspectorGUI( SerializedProperty toDirty, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            base.Editor_InspectorGUI( toDirty, ragdollHandler, helper );
            GUILayout.Space( 4 );

            var blendingSpeedV = helper.RequestVariable( "Blending Speed:", 0.75f );
            blendingSpeedV.AssignTooltip( "How quickly physical pose should blend in when collision happens." );
            blendingSpeedV.SetMinMaxSlider( 0f, 1f );
            blendingSpeedV.Editor_DisplayVariableGUI();

            var turnOffLegsV = helper.RequestVariable( "Turn Off Legs:", true );
            GUILayout.Space( 4 );
            //var applyOnWholeChainsV = helper.RequestVariable( "Apply on whole chains:", true );
            //applyOnWholeChainsV.AssignTooltip( "Applying blending on the whole ragdoll bone chain when collision happens, instead of selective bones blending." );
            //applyOnWholeChainsV.Editor_DisplayVariableGUI();

            //if( applyOnWholeChainsV.GetBool() )
            {
                EditorGUILayout.BeginHorizontal();

                var sensitiveBlendV = helper.RequestVariable( "Sensitive Blend:", true );
                sensitiveBlendV.AssignTooltip( "When collision happens, applying blending also on the first parent bones of the chain (like arm collision -> blend upper chest too)" );
                sensitiveBlendV.Editor_DisplayVariableGUI();

                if( turnOffLegsV.GetBool() == false )
                {
                    var coreBlendLegs = helper.RequestVariable( "Blend Legs With Core:", false );
                    coreBlendLegs.AssignTooltip( "When core chain collides, blending in also the legs.\n(use only if you really need it)" );
                    coreBlendLegs.Editor_DisplayVariableGUI();
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space( 4 );
            GUI.enabled = !ragdollHandler.WasInitialized;

            EditorGUILayout.BeginHorizontal();
            turnOffLegsV.AssignTooltip( "Making legs blend zero on standing mode to make keyframe animation intact. You probably will need to disable collision events send on feet bones if you want to keep legs enabled (under Construct -> Physics -> ≡ -> Extra Settings)" );
            turnOffLegsV.Editor_DisplayVariableGUI();

            var skipFeetV = helper.RequestVariable( "Skip Feet:", true );
            skipFeetV.AssignTooltip( "Skipping checking last leg bones collision state to avoid blending legs every time feet are touching ground.\n\nWith legs turned off, this option will disable blending in feet bones on leg blend request." );
            /*if( turnOffLegsV.GetBool() == false ) */
            skipFeetV.Editor_DisplayVariableGUI();

            EditorGUILayout.EndHorizontal();

            var ignoreSelfV = helper.RequestVariable( "Ignore Self Collision Blend:", true );
            ignoreSelfV.AssignTooltip( "Ignoring self dummy colliders for collision detectors." );
            ignoreSelfV.Editor_DisplayVariableGUI();
            GUI.enabled = true;

            //GUILayout.Space( 4 );
            //var dampOnCollisionV = InitializedWith.RequestVariable( "Damp On Collision:", true );
            //dampOnCollisionV.AssignTooltip( "Making blended limb damped, so during visible ragdoll motion the bone animation is less jiggly but also less precise in comparison to played source animation" );
            //dampOnCollisionV.Editor_DisplayVariableGUI();

            if( ragdollHandler.JointLimitSpring <= 0f )
            {
                EditorGUILayout.HelpBox( "Consider enabling 'Joint Limit Springs' to minimalize chance for collision contact overlapping jitters.", UnityEditor.MessageType.None );
                if( GUILayout.Button( "Go to Setup -> Main Physics" ) ) { ragdollHandler._EditorCategory = RagdollHandler.ERagdollAnimSection.Setup; ragdollHandler._EditorMainCategory = RagdollHandler.ERagdollSetupSection.Physics; }
            }

            GUILayout.Space( 6 );
            if( ragdollHandler.WasInitialized == false )
            {
                EditorGUILayout.LabelField( "During playmode you will see debug here", EditorStyles.centeredGreyMiniLabel );
                return;
            }

            if( blendChains == null ) return;

            EditorGUILayout.LabelField( "Debug View:", EditorStyles.centeredGreyMiniLabel );
            GUI.enabled = false;

            for( int i = 0; i < blendChains.Count; i++ )
            {
                var chain = blendChains[i].OwnerChain;

                EditorGUILayout.BeginHorizontal();
                if( GUILayout.Button( new GUIContent( chain.ChainName, GetChainIcon( chain.ChainType ) ), EditorStyles.boldLabel, GUILayout.Height( 18 ), GUILayout.MaxWidth( 84 ) ) ) { }

                GUILayout.Space( 6 );
                EditorGUILayout.Slider( blendChains[i].LastBlend, 0f, 1f );
                GUILayout.Space( 6 );

                foreach( var bone in blendChains[i].Bones )
                {
                    if( bone.CollisionHandler.CollidesWithAnything() )
                    {
                        EditorGUIUtility.labelWidth = 90;
                        EditorGUILayout.ObjectField( "Collides With:", bone.CollisionHandler.GetFirstCollidingCollider(), typeof( Collider ), true );
                        EditorGUIUtility.labelWidth = 0;
                        break;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }




            for( int i = 0; i < blendChains.Count; i++ )
            {
                foreach( var bone in blendChains[i].Bones )
                {
                    EditorGUILayout.ObjectField( bone.ParentBone.SourceBone, typeof( Transform ), true );
                }
            }



            GUI.enabled = true;
        }

        private static Texture GetChainIcon( ERagdollChainType chain )
        {
            if( chain == ERagdollChainType.Core ) return FGUI_Resources.FindIcon( "SPR_BodySpine" );
            else if( chain.IsArm() ) return FGUI_Resources.FindIcon( "SPR_BodyArm" );
            else if( chain.IsLeg() ) return FGUI_Resources.FindIcon( "SPR_BodyLeg" );
            else return FGUI_Resources.FindIcon( "SPR_BodyBonesChain" );
        }

#endif
    }
}