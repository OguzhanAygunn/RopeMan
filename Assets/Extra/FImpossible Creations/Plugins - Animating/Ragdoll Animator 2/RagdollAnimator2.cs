using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    /// <summary>
    /// Ragdoll Animator 2 component is containing RagdollHandler which is responsible for all of the calculations
    /// </summary>
    [DefaultExecutionOrder(-1)]
    [HelpURL("https://assetstore.unity.com/packages/tools/physics/ragdoll-animator-2-285638")]
    [AddComponentMenu("FImpossible Creations/Ragdoll Animator 2", 1)]
    public class RagdollAnimator2 : FimpossibleComponent, IRagdollAnimator2HandlerOwner
    {
        /// <summary> The main Ragdoll processing class </summary>
        [SerializeField] private RagdollHandler handler = new RagdollHandler();

        /// <summary> Returns RagdollHandler (It was called Processor/Parameters in Ragdoll Animator 1) </summary>
        public RagdollHandler Settings
        { get { return handler; } }

        /// <summary> Returns RagdollHandler (It was called Processor/Parameters in Ragdoll Animator 1) </summary>
        public RagdollHandler Actions
        { get { return handler; } }

        /// <summary> Returns RagdollHandler (It was called Processor/Parameters in Ragdoll Animator 1) </summary>
        public RagdollHandler Handler
        { get { return handler; } }

        /// <summary> Interface implementation </summary>
        public RagdollHandler GetRagdollHandler => handler;

        public RagdollHandler.EAnimatingMode AnimatingMode => Handler.AnimatingMode;
        public Animator Mecanim => Handler.Mecanim;

        /// <summary> Shortcut for ragdoll motion blend (ragdollAnimator.Handler.RagdollBlend) </summary>
        public float RagdollBlend
        {
            get { return handler.RagdollBlend; }
            set { handler.RagdollBlend = value; }
        }

        public bool IsInFallingOrSleepMode => Handler.IsFallingOrSleep;

        #region Base Transform Get

        public Transform GetBaseTransform
        {
            get
            {
                if (handler.BaseTransform == null) return transform;
                return handler.BaseTransform;
            }
        }

        #endregion Base Transform Get



        private void Reset()
        {
            handler.EnsureChainsHasParentHandler();
            handler.Mecanim = GetComponentInChildren<Animator>();
            if (!handler.Mecanim) if (transform.parent) handler.Mecanim = transform.parent.GetComponent<Animator>();
        }


        private void Start()
        {
            handler.Initialize(this, gameObject);
        }


        private void Update()
        {
            #region Debug Performance Measure Start

#if UNITY_EDITOR
            _Debug_Perf_MeasureUpdate(true);
#endif

            #endregion Debug Performance Measure Start

            Handler.UpdateTick();

            #region Debug Performance Measure End

#if UNITY_EDITOR
            _Debug_Perf_MeasureUpdate(false);
#endif

            #endregion Debug Performance Measure End
        }


        private void LateUpdate()
        {
            #region Debug Performance Measure Start

#if UNITY_EDITOR
            _Debug_Perf_MeasureLateUpdate(true);
#endif

            #endregion Debug Performance Measure Start

            Handler.LateUpdateTick();

            #region Debug Performance Measure End

#if UNITY_EDITOR
            _Debug_Perf_MeasureLateUpdate(false);
#endif

            #endregion Debug Performance Measure End
        }


        private void FixedUpdate()
        {
            #region Debug Performance Measure Start

#if UNITY_EDITOR
            _Debug_Perf_MeasureFixedUpdate(true);
#endif

            #endregion Debug Performance Measure Start

            Handler.FixedUpdateTick();

            #region Debug Performance Measure End

#if UNITY_EDITOR
            _Debug_Perf_MeasureFixedUpdate(false);
#endif

            #endregion Debug Performance Measure End
        }


        private void OnEnable()
        {
            handler.OnEnable();
        }


        private void OnDisable()
        {
            #region Editor Application Quitting Check
#if UNITY_EDITOR
            if (_isQuitting) return;
#endif
            #endregion

            handler.OnDisable();
        }


        private void OnDestroy()
        {
            handler.OnCreatorDestroy();
        }




        public override void OnValidate()
        {
#if UNITY_EDITOR
            if (UnityEditor.BuildPipeline.isBuildingPlayer) { return; }
            if (UnityEditor.Selection.activeGameObject == gameObject) // Avoid calling 'UpdateAllAfterManualChanges' on build
#endif

            UpdateAllAfterManualChanges();

            base.OnValidate();
        }


        public void UpdateAllAfterManualChanges()
        {
            if (handler.Chains.Count <= 0) return;

            if (handler.Chains[0].ParentHandler == null) { handler.EnsureChainsHasParentHandler(); }

            if (handler.DummyWasGenerated) handler.User_UpdateAllBonesParametersAfterManualChanges();

            if (handler.WasInitialized == false) return;

            handler.User_UpdateJointsPlayParameters(true);
        }


        /// <summary>
        /// Auto-finding references to body bones and applying collider and physics settings.
        /// It is calling handler.TryFindBones()  handler chains -> AutoAdjustColliders   handler chains -> AutoAdjustPhysics   hander.StoreReferenceTPose()
        /// </summary>
        public void TryFindBonesAndDoFullSetup()
        {
            if (handler.Mecanim == null)
            {
                handler.Mecanim = GetComponentInChildren<Animator>();
                if (handler.Mecanim == null) handler.Mecanim = GetComponentInParent<Animator>();
            }

            handler.HelperOwnerTransform = GetBaseTransform;
            handler.TryFindBones();

            foreach (var chain in handler.Chains)
            {
                chain.AutoAdjustColliders(handler.IsHumanoid);
                chain.AutoAdjustPhysics();
            }

            handler.StoreReferenceTPose();
        }

        #region Public Events for Unity Events or SendMessages

        /// <summary> For Unity Events use or SendMessage </summary>
        public void RA2Event_SwitchToFall()
        {
            this.User_SwitchFallState();
        }

        /// <summary> For Unity Events use or SendMessage </summary>
        public void RA2Event_SwitchToStand()
        {
            this.User_SwitchFallState(true);
        }

        /// <summary> For Unity Events use or SendMessage </summary>
        public void RA2Event_TransitionStand()
        {
            this.User_TransitionToStandingMode();
        }

        /// <summary> For Unity Events use or SendMessage </summary>
        public void RA2Event_TransitionStand(float duration)
        {
            this.User_TransitionToStandingMode(duration, 0f);
        }

        /// <summary> For Unity Events use or SendMessage </summary>
        public void RA2Event_SwitchToSleep()
        {
            handler.User_SwitchFallState(RagdollHandler.EAnimatingMode.Sleep);
            handler.User_ResetOverrideBlends();
            handler.User_DisableMecanimAfter(2.5f);
        }

        /// <summary> For Unity Events use or SendMessage </summary>
        public void RA2Event_SwitchToSleep(float disableMecanimAfter)
        {
            handler.User_SwitchFallState(RagdollHandler.EAnimatingMode.Sleep);
            handler.User_ResetOverrideBlends();
            handler.User_DisableMecanimAfter(disableMecanimAfter);
        }

        /// <summary> For Unity Events use or SendMessage </summary>
        public void RA2Event_AddFullImpact(Vector3 impact)
        {
            handler.User_AddAllBonesImpact(impact);
        }

        /// <summary> For Unity Events use or SendMessage </summary>
        public void RA2Event_AddLeftLegImpact(Vector3 impact)
        {
            handler.User_AddChainImpact(handler.GetChain(ERagdollChainType.LeftLeg), impact, 0f);
        }

        /// <summary> For Unity Events use or SendMessage </summary>
        public void RA2Event_AddRightLegImpact(Vector3 impact)
        {
            handler.User_AddChainImpact(handler.GetChain(ERagdollChainType.RightLeg), impact, 0f);
        }

        /// <summary> For Unity Events use or SendMessage </summary>
        public void RA2Event_AddLeftArmImpact(Vector3 impact)
        {
            handler.User_AddChainImpact(handler.GetChain(ERagdollChainType.LeftArm), impact, 0f);
        }

        /// <summary> For Unity Events use or SendMessage </summary>
        public void RA2Event_AddRightArmImpact(Vector3 impact)
        {
            handler.User_AddChainImpact(handler.GetChain(ERagdollChainType.RightArm), impact, 0f);
        }

        /// <summary> For Unity Events use or SendMessage </summary>
        public void RA2Event_AddCoreImpact(Vector3 impact)
        {
            handler.User_AddChainImpact(handler.GetChain(ERagdollChainType.Core), impact, 0f);
        }

        /// <summary> For Unity Events use or SendMessage </summary>
        public void RA2Event_AddHeadImpact(Vector3 impact)
        {
            var core = handler.GetChain(ERagdollChainType.Core);
            if (core.BoneSetups.Count == 0) return;
            var bone = core.GetBone(10000);
            if (bone == null) return;
            handler.User_AddBoneImpact(bone, impact, 0f);
        }

        #endregion


        #region Editor Code

        public void USER__ENTER_Settings_VARIABLE_FOR_MORE_METHODS() { }
        public void INFO__ENTER_Settings_VARIABLE_FOR_MORE_METHODS() { }

        #region Extra Debugging Classes

#if UNITY_EDITOR

        public FDebug_PerformanceTest _Editor_Perf_Update = new FDebug_PerformanceTest();
        public FDebug_PerformanceTest _Editor_Perf_LateUpdate = new FDebug_PerformanceTest();
        public FDebug_PerformanceTest _Editor_Perf_FixedUpdate = new FDebug_PerformanceTest();

        private void _Debug_Perf_MeasureUpdate(bool start)
        { _Debug_Perf_DoMeasure(_Editor_Perf_Update, start); }

        private void _Debug_Perf_MeasureLateUpdate(bool start)
        { _Debug_Perf_DoMeasure(_Editor_Perf_LateUpdate, start); }

        private void _Debug_Perf_MeasureFixedUpdate(bool start)
        { _Debug_Perf_DoMeasure(_Editor_Perf_FixedUpdate, start); }

        private void _Debug_Perf_DoMeasure(FDebug_PerformanceTest test, bool start)
        { if (start) test.Start(gameObject, false); else test.Finish(); }

#endif

        #endregion Extra Debugging Classes

#if UNITY_EDITOR

        private bool _isQuitting = false;

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDrawGizmosSelected()
        {
            if (UnityEditor.Selection.activeGameObject != gameObject) return;
            handler.Editor_OnDrawGizmos(this);
        }

#endif

        #region Helper utility backup

        //[UnityEditor.MenuItem( "CONTEXT/RagdollAnimator2/Go Refresh Types", false, 10000 )]
        //private static void OpenFimpossibleStorePage( UnityEditor.MenuCommand menuCommand )
        //{
        //    RagdollAnimator2 ra = menuCommand.context as RagdollAnimator2;
        //    if( ra )
        //    {
        //        foreach( var chain in ra.handler.Chains )
        //        {
        //            //string name = chain.ChainName.ToLower();
        //            //if( name == "core" ) chain.ChainType = ERagdollChainType.Core;
        //            //else if( name == "left arm" ) chain.ChainType = ERagdollChainType.LeftArm;
        //            //else if( name == "right arm" ) chain.ChainType = ERagdollChainType.RightArm;
        //            //else if( name == "left leg" ) chain.ChainType = ERagdollChainType.LeftLeg;
        //            //else if( name == "right leg" ) chain.ChainType = ERagdollChainType.RightLeg;
        //            //else chain.ChainType = ERagdollChainType.OtherLimb;
        //            chain.HardMatchMultiply = 1f;
        //        }

        //        UnityEditor.EditorUtility.SetDirty( ra );
        //    }
        //}

        #endregion

        #endregion Editor Code


    }

}