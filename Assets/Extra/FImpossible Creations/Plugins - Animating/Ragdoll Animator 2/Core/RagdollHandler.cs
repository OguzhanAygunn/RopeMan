using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [System.Serializable]
    public partial class RagdollHandler : IRagdollAnimator2HandlerOwner
    {
        RagdollHandler IRagdollAnimator2HandlerOwner.GetRagdollHandler => this;

        public List<RagdollBonesChain> Chains
        { get { return chains; } }
        [SerializeField] private List<RagdollBonesChain> chains = new List<RagdollBonesChain>();

        public bool WasInitialized { get; private set; } = false;

        /// <summary> How many fixed frames ragdoll handler is being initialized </summary>
        private int fixedFramesElapsed = 0;

        /// <summary> Behaviour which initialized the ragdoll handler (by default it is RagdollAnimato2 unless using RagdollHandler in custom way) </summary>
        public MonoBehaviour Caller { get; private set; } = null;

        /// <summary> Object to which handler belongs </summary>
        public GameObject ParentObject
        { get { return ParentObject; } }

        [SerializeField, HideInInspector] private GameObject parentObject = null;

        public void HandledBy( GameObject gameObject )
        { if( WasInitialized ) return; parentObject = gameObject; }

        public bool WasPreGeneratedDummy { get; private set; }

        public void Initialize( MonoBehaviour caller, GameObject creator )
        {
            if( WasInitialized ) return;



            BaseTransform = GetBaseTransform();
            Caller = caller;
            parentObject = creator;

            if( IsBaseSetupValid() == false || IsRagdollConstructionValid() == false )
            {
                Debug.Log( "[Ragdoll Animator 2] The Ragdoll Setup for " + creator.name + " is not valid! Component will be disabled." );
                animatingMode = EAnimatingMode.Off;
                return;
            }

            EnsureChainsHasParentHandler();

            if( RagdollLogic == ERagdollLogic.JustBoneComponents )
            {
                if( animatingMode != EAnimatingMode.Sleep ) animatingMode = EAnimatingMode.Falling;
                GenerateJustSkeletonComponentsLogic();
                disableUpdating = true;
                WasInitialized = true;
                return;
            }

            WasPreGeneratedDummy = DummyWasGenerated;

            if( DummyWasGenerated ) ApplyPreGenerateDummyChanges();
            else GenerateDummyHierarchy();

            _playmodeAnchorBone = GetAnchorBoneController;
            ForcingKinematicAnchor = 2;

            if( AnimatingMode == EAnimatingMode.Standing ) LastStandingModeAtTime = Time.unscaledTime;
            else
            {
                animatingModeChanged = true;
            }

            _motionInfluenceOffset = Vector3.zero;
            _lastFixedPosition = GetAnchorSourceBone().position;
            _lastAnimatingMode = animatingMode;

            CalculateRagdollBlend();
            FinalizePhysicalDummySetup();
            ResetFadeInBlend();

            User_UpdateJointsPlayParameters( true );

            PrepareBonesDicationaries();
            StoreAnchorHelperCoords();

            CallExtraFeaturesOnInitialize(); // Extra Features -----

            if( Mecanim ) if( !IsHumanoid ) IsHumanoid = Mecanim.isHuman;

            #region Editor Utils

#if UNITY_EDITOR

            if( RagdollLogic == ERagdollLogic.ActiveRagdoll && _EditorCategory != ERagdollAnimSection.Extra )
            {
                _EditorCategory = RagdollHandler.ERagdollAnimSection.Motion;
                _EditorMotionCategory = RagdollHandler.ERagdollMotionSection.Main;
            }

#endif

            #endregion Editor Utils

            WasInitialized = true;
        }

        /// <summary> playmode handler value for animate physics </summary>
        private bool animatePhysics = false;

        private bool scheduledFixedUpdate = true;

        private void UpdateAnimatePhysicsVariable()
        {
            if( Mecanim )
            {
                animatePhysics = Mecanim.updateMode == AnimatorUpdateMode.AnimatePhysics;
            }
            else
            {
                animatePhysics = AnimatePhysics;
            }
        }
    }
}