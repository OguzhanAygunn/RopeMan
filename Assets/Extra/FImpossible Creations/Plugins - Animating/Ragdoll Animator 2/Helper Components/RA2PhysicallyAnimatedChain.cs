using System.Collections.Generic;

#if UNITY_EDITOR

using FIMSpace.FEditor;
using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [AddComponentMenu( "FImpossible Creations/Ragdoll Animator/Ragdoll Animated Chain", 111 )]
    public class RA2PhysicallyAnimatedChain : FimpossibleComponent
    {
        [FPD_Header( "Animating Properties", 1 )]
        public float SpringsPower = 1000;

        [FPD_FixedCurveWindow] public AnimationCurve SpringOverChain = AnimationCurve.EaseInOut( 0f, 1f, 1f, 1f );
        public float Damping = 5;

        [Space( 6 )]
        [Range( 0f, 1f )] public float PositionHardMatching = 0f;
        [FPD_FixedCurveWindow] public AnimationCurve HardMatchOverChain = AnimationCurve.EaseInOut( 0f, 1f, 1f, 1f );

        [Space( 8 )]
        public float RigidbodiesMass = 1f;

        [Tooltip( "Use curve to multiply RigidbodiesMass value over the chain. Value on the left is first parent bone mass, on the right - last child bone mass." )]
        [FPD_FixedCurveWindow] public AnimationCurve MassOverChain = AnimationCurve.EaseInOut( 0f, 1f, 1f, 1f );

        public float RigidbodyDrag = 0f;
        public float AngularDrag = 1f;
        public RigidbodyInterpolation Interpolation = RigidbodyInterpolation.Interpolate;
        public bool KinematicAnchor = true;

        [Space( 8 )]
        [Tooltip( "Optional, use for animate physics sync" )]
        public Animator Mecanim;

        public bool Calibrate = true;

        [FPD_Header( "Main References for chain generating" )]
        public Transform FirstParentBone;

        public Transform EndChildBone;

        [Space( 3 )]
        [Tooltip( "Optional. If you generating chain for parented bone, like arm, you should assign there FirstParentBone." )]
        public Transform TargetParent = null;

        [FPD_Header( "Joints Generator Settings" )]
        [FPD_Layers] public int DummyLayer = 0;

        public float MassScale = 1f;
        public float ConnectedMass = 1f;

        [Space( 3 )]
        public float Radius = 0.2f;

        [Tooltip( "Use curve to multiply colliders radius value over the chain. Value on the left is first parent bone collider radius multiplier, on the right - last child bone collider multiplier." )]
        [FPD_FixedCurveWindow] public AnimationCurve RadiusOverChain = AnimationCurve.EaseInOut( 0f, 1f, 1f, 1f );

        [Space( 3 )]
        public PhysicMaterial CollidersMaterial;

        public bool HideGeneratedDummy = false;

        [SerializeField, HideInInspector] private GameObject generatedDummy = null;
        public GameObject GeneratedDummy => generatedDummy;
        private Rigidbody dummyRigidbody = null;

        private Vector3 targetAnchorPosition;
        private Quaternion targetAnchorRotation;

        [SerializeField, HideInInspector] public List<BoneReference> joints = new List<BoneReference>();
        private List<JointHelper> jointControllers = new List<JointHelper>();
        private JointHelper FirstBone => jointControllers[0];

        public bool WasInitialized { get; private set; } = false;

        #region Animate Physics Update Handling

        private bool fixedInitialized = false;

        /// <summary> How many fixed frames ragdoll handler is being initialized </summary>
        private int fixedFramesElapsed = 0;

        /// <summary> playmode handler value for animate physics </summary>
        private bool animatePhysics = false;

        private bool unscaledTime = false;
        private bool scheduledFixedUpdate = true;

        private void UpdateAnimatePhysicsVariable()
        {
            if( Mecanim )
            {
                animatePhysics = Mecanim.updateMode == AnimatorUpdateMode.AnimatePhysics;
                unscaledTime = Mecanim.updateMode == AnimatorUpdateMode.UnscaledTime;
            }
        }

        #endregion Animate Physics Update Handling

        /// <summary>
        /// Initialization process
        /// </summary>
        private void Awake()
        {
            if( FirstParentBone == null || EndChildBone == null )
            {
                UnityEngine.Debug.Log( "[Ragdoll Animator 2 Helper] Not Assigned bone reference in " + name + "!" );
                GameObject.Destroy( this );
                return;
            }

            // Generate joints if not using pre-generated dummy
            if( !generatedDummy )
            {
                GenerateJoints();
                Physics.SyncTransforms();
            }

            // Proceed base parenting
            if( TargetParent == null )
            {
                if( FirstParentBone.parent )
                {
                    generatedDummy.transform.SetParent( FirstParentBone.parent, true );
                }
            }
            else
            {
                generatedDummy.transform.SetParent( TargetParent, true );
            }

            // Hide if wanted
            if( HideGeneratedDummy )
            {
                generatedDummy.gameObject.hideFlags = HideFlags.HideInHierarchy;
            }

            // Initialize all joint helpers
            for( int j = 0; j < joints.Count; j++ )
            {
                joints[j].joint.gameObject.layer = DummyLayer; // Refresh
                joints[j].joint.connectedMassScale = ConnectedMass;
                joints[j].joint.massScale = MassScale;

                Collider coll = joints[j].joint.GetComponent<Collider>();
                if( coll ) coll.sharedMaterial = CollidersMaterial; // Refresh

                var nHelper = new JointHelper( joints[j].sourceBone, joints[j].joint, coll );
                jointControllers.Add( nHelper );
                nHelper.processor.CaptureAnimatorPose();

                joints[j].joint.transform.SetParent( generatedDummy.transform, true );
            }

            jointControllers.Reverse();
            dummyRigidbody = generatedDummy.GetComponent<Rigidbody>();
            dummyRigidbody.mass = RigidbodiesMass;
            dummyRigidbody.interpolation = Interpolation;

            if( jointControllers.Count == 0 || jointControllers[0] == null )
            {
                UnityEngine.Debug.Log( "[Ragdoll Animator 2 - Animated Chain] Couldn't generate any joint! Check this object setup : " + name );
                enabled = false;
                return;
            }

            targetAnchorPosition = FirstBone.sourceBone.parent.position;
            targetAnchorRotation = FirstBone.sourceBone.parent.rotation;

            //FirstBone.joint.xMotion = ConfigurableJointMotion.Free;
            //FirstBone.joint.yMotion = ConfigurableJointMotion.Free;
            //FirstBone.joint.zMotion = ConfigurableJointMotion.Free;

            // Final refresh components
            UpdateComponentsParameters();

            WasInitialized = true;
        }

        /// <summary>
        /// Refreshing all important physical parameters
        /// </summary>
        public void UpdateComponentsParameters()
        {
            if( jointControllers == null ) return;

            for( int j = 0; j < jointControllers.Count; j++ )
            {
                jointControllers[j].joint.gameObject.layer = DummyLayer; // Refresh
                jointControllers[j].joint.connectedMassScale = ConnectedMass;
                jointControllers[j].joint.massScale = MassScale;

                jointControllers[j].rigidbody.mass = RigidbodiesMass;
                jointControllers[j].rigidbody.drag = RigidbodyDrag;
                jointControllers[j].rigidbody.angularDrag = AngularDrag;

                jointControllers[j].collider.sharedMaterial = CollidersMaterial;
            }
        }

        /// <summary>
        /// Prepare for procedural animation
        /// </summary>
        private void Update()
        {
            if( Mecanim )
            {
                animatePhysics = Mecanim.updateMode == AnimatorUpdateMode.AnimatePhysics;
            }

            if( animatePhysics ) return;
            if( Calibrate == false ) return;
            foreach( var controller in jointControllers ) controller.processor.CalibrateRotation();
        }

        /// <summary>
        /// Apply physical operations and support for Animate physics
        /// </summary>
        private void FixedUpdate()
        {
            #region Update Handling

            // Ensuring correct initialization for physical components like unity joints - one tick is not enough for unknown reason -_-
            if( fixedFramesElapsed < 2 )
            {
                fixedInitialized = false;
                fixedFramesElapsed += 1;
                return;
            }
            else
            {
                if( !fixedInitialized )
                {
                    fixedInitialized = true;
                }
            }

            // Animate physics related
            scheduledFixedUpdate = true;

            #endregion Update Handling

            if( animatePhysics && Calibrate )
            {
                foreach( var controller in jointControllers ) controller.processor.CalibrateRotation();
            }

            float chainCountDiv = (float)jointControllers.Count - 1;
            if( chainCountDiv == 0f ) chainCountDiv = 1f;

            for( int j = 0; j < jointControllers.Count; j++ )
            {
                var controller = jointControllers[j];

                float chainProgress = (float)j / ( chainCountDiv );

                var drive = controller.joint.slerpDrive;
                drive.positionSpring = SpringOverChain.Evaluate( chainProgress ) * SpringsPower;
                drive.positionDamper = Damping;
                controller.joint.slerpDrive = drive;

                controller.rigidbody.mass = MassOverChain.Evaluate( chainProgress ) * RigidbodiesMass;

                controller.processor.ApplyJointRotation();
            }

            if( PositionHardMatching > 0f )
            {
                for( int j = 0; j < jointControllers.Count; j++ )
                {
                    var controller = jointControllers[j];
                    float chainProgress = (float)j / ( chainCountDiv );
                    controller.processor.HardMatchBonePosition( HardMatchOverChain.Evaluate( chainProgress ) * PositionHardMatching );
                }
            }

            if( KinematicAnchor )
            {
                dummyRigidbody.isKinematic = true;
                generatedDummy.transform.position = targetAnchorPosition;
                generatedDummy.transform.rotation = targetAnchorRotation;
            }
            else
            {
                dummyRigidbody.isKinematic = false;
                RagdollHandlerUtilities.AddRigidbodyForceToMoveTowards( dummyRigidbody, targetAnchorPosition, 1f );
                RagdollHandlerUtilities.AddRigidbodyTorqueToRotateTowards( dummyRigidbody, targetAnchorRotation, 1f );
            }
        }

        /// <summary>
        /// Capture animator and apply physical pose to the skeleton
        /// </summary>
        private void LateUpdate()
        {
            #region Animate Physics

            if( animatePhysics )
            {
                if( scheduledFixedUpdate == false ) return;
                scheduledFixedUpdate = false;
            }

            if( !fixedInitialized ) return;

            #endregion Animate Physics

            foreach( var controller in jointControllers ) controller.processor.CaptureAnimatorPose();

            targetAnchorPosition = FirstBone.sourceBone.parent.position;
            targetAnchorRotation = FirstBone.sourceBone.parent.rotation;

            foreach( var controller in jointControllers ) controller.sourceBone.rotation = controller.joint.transform.rotation;
        }

        #region Handling Helpers

        /// <summary>
        /// Setup and pre-generated dummy helper
        /// </summary>
        [System.Serializable]
        public struct BoneReference
        {
            public Transform sourceBone;
            public Transform physicalBone;
            public ConfigurableJoint joint;

            public BoneReference( Transform src, ConfigurableJoint jnt )
            {
                sourceBone = src;
                joint = jnt;
                physicalBone = joint.transform;
            }
        }

        /// <summary>
        /// Runtime calculations helper
        /// </summary>
        private class JointHelper
        {
            public ConfigurableJoint joint;
            public RagdollBoneProcessor processor;
            public Quaternion lastFixedRotation;
            public Rigidbody rigidbody;
            public Collider collider;
            public Transform sourceBone;

            public JointHelper( Transform src, ConfigurableJoint jnt, Collider coll )
            {
                joint = jnt;
                sourceBone = src;
                collider = coll;
                rigidbody = jnt.GetComponent<Rigidbody>();
                processor = new RagdollBoneProcessor( jnt, sourceBone, jnt.gameObject.GetComponent<Rigidbody>() );
                lastFixedRotation = jnt.transform.localRotation;
            }
        }

        #endregion Handling Helpers

        #region Utility Operations

        public override void OnValidate()
        {
#if UNITY_EDITOR
            if( Application.isPlaying )
            {
                // Update during inspector view editing
                UpdateComponentsParameters();
            }
#endif

            base.OnValidate();
        }

        private bool _wasDisabled = false;

        private void OnEnable()
        {
            if( WasInitialized == false ) return;
            if( _wasDisabled == false ) return;
            SwitchAllPhysics( true );
        }

        private void OnDisable()
        {
            if( WasInitialized == false ) return;
            if( _wasDisabled ) return;
            SwitchAllPhysics( false );
        }

        /// <summary>
        /// Setting all rigidbodies sleep and disabling colliders
        /// </summary>
        public void SwitchAllPhysics( bool enabled )
        {
            if( FirstBone.rigidbody == null ) return;

            for( int j = 0; j < jointControllers.Count; j++ )
            {
                jointControllers[j].rigidbody.detectCollisions = enabled;
                jointControllers[j].rigidbody.isKinematic = enabled;
                jointControllers[j].collider.enabled = enabled;
                if( enabled == false ) jointControllers[j].rigidbody.Sleep(); else jointControllers[j].rigidbody.WakeUp();
            }

            _wasDisabled = !enabled;
        }

        #endregion Utility Operations

        #region Generating Helpers

        private void GenerateJoints()
        {
            if( generatedDummy ) RagdollHandlerUtilities.DestroyObject( generatedDummy );
            joints.Clear();

            // Generate Hierarchy
            Transform bone = EndChildBone;

            int count = 0;
            while( bone != FirstParentBone && bone != null ) { count += 1; bone = bone.parent; }

            bone = EndChildBone;

            float chainCountDiv = (float)count - 1;
            if( chainCountDiv == 0f ) chainCountDiv = 1f;
            int j = 0;

            while( bone != FirstParentBone && bone != null )
            {
                GameObject dummyBone = new GameObject( bone.parent.name );
                Transform dBone = dummyBone.transform;
                dummyBone.layer = DummyLayer;
                dummyBone.transform.position = bone.parent.position;
                dummyBone.transform.rotation = bone.parent.rotation;

                RagdollHandlerUtilities.GetOrGenerate<Rigidbody>( dBone );
                ConfigurableJoint joint = RagdollHandlerUtilities.GetOrGenerate<ConfigurableJoint>( dBone );
                if( ContainsJoint( joint ) == false ) joints.Add( new BoneReference( bone.parent, joint ) );

                CapsuleCollider caps = RagdollHandlerUtilities.GetOrGenerate<CapsuleCollider>( dBone );
                caps.material = CollidersMaterial;

                float chainProgress = (float)j / ( chainCountDiv );
                float radius = RadiusOverChain.Evaluate( 1f - chainProgress ) * Radius;

                RagdollHandlerUtilities.AdjustColliderBasingOnStartEndPosition( bone.parent.position, bone.position, bone.parent, caps, radius );

                j += 1;
                bone = bone.parent;
            }

            generatedDummy = new GameObject( name + "-GeneratedDummy" );
            generatedDummy.layer = DummyLayer;
            generatedDummy.transform.position = FirstParentBone.parent.position;
            generatedDummy.transform.rotation = FirstParentBone.parent.rotation;
            generatedDummy.transform.SetParent( transform, true );

            dummyRigidbody = generatedDummy.AddComponent<Rigidbody>();
            dummyRigidbody.isKinematic = true;

            joints[joints.Count - 1].joint.transform.SetParent( generatedDummy.transform, true );

            for( int i = 0; i < joints.Count - 1; i++ )
            {
                joints[i].joint.transform.SetParent( joints[i + 1].joint.transform, true );
            }

            for( int i = 0; i < joints.Count; i++ )
            {
                var joint = joints[i].joint;
                Rigidbody rig = joint.GetComponent<Rigidbody>();
                rig.mass = RigidbodiesMass;

                joint.connectedBody = joint.transform.parent.GetComponent<Rigidbody>();
                joint.connectedMassScale = ConnectedMass;
                joint.xMotion = ConfigurableJointMotion.Locked;
                joint.yMotion = ConfigurableJointMotion.Locked;
                joint.zMotion = ConfigurableJointMotion.Locked;
                joint.rotationDriveMode = RotationDriveMode.Slerp;
            }
        }

        private void ClearJoints()
        {
            if( generatedDummy ) RagdollHandlerUtilities.DestroyObject( generatedDummy );
        }

        private bool ContainsJoint( ConfigurableJoint joint )
        {
            for( int i = 0; i < joints.Count; i++ )
            {
                if( joints[i].joint == joint ) return true;
            }

            return false;
        }

        #endregion Generating Helpers

        #region Editor Class

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if( WasInitialized )
            {
                Handles.color = Color.green * 0.9f;

                var preJc = jointControllers[0];
                Handles.SphereHandleCap( 0, preJc.rigidbody.position, Quaternion.identity, Vector3.Distance( preJc.rigidbody.position, dummyRigidbody.position ) * 0.1f, EventType.Repaint );

                for( int j = 1; j < jointControllers.Count; j++ )
                {
                    var jc = jointControllers[j];
                    Handles.DrawLine( jc.rigidbody.position, preJc.rigidbody.position );
                    Handles.SphereHandleCap( 0, jc.rigidbody.position, Quaternion.identity, Vector3.Distance( jc.rigidbody.position, preJc.rigidbody.position ) * 0.1f, EventType.Repaint );
                    preJc = jc;
                }

                Handles.color = Color.cyan * 0.7f;

                preJc = jointControllers[0];
                Handles.SphereHandleCap( 0, preJc.processor.AnimatorPosition, Quaternion.identity, Vector3.Distance( preJc.rigidbody.position, dummyRigidbody.position ) * 0.1f, EventType.Repaint );
                for( int j = 1; j < jointControllers.Count; j++ )
                {
                    var jc = jointControllers[j];
                    Handles.DrawLine( jc.processor.AnimatorPosition, preJc.processor.AnimatorPosition );
                    Handles.SphereHandleCap( 0, jc.processor.AnimatorPosition, Quaternion.identity, Vector3.Distance( jc.rigidbody.position, preJc.rigidbody.position ) * 0.1f, EventType.Repaint );
                    preJc = jc;
                }
            }
            else
            {
                Transform start = EndChildBone;
                if( start == null ) return;
                Transform end = FirstParentBone;
                if( end == null ) return;
                Transform b = start;

                Handles.color = Color.green * 0.9f;

                while( b != FirstParentBone && b != null )
                {
                    if( b.parent == null ) break;

                    Handles.DrawLine( b.position, b.parent.position );
                    Handles.SphereHandleCap( 0, b.position, Quaternion.identity, Vector3.Distance( b.position, b.parent.position ) * 0.1f, EventType.Repaint );

                    if( b.parent == FirstParentBone ) break;
                    b = b.parent;
                }

                Handles.SphereHandleCap( 0, end.position, Quaternion.identity, Vector3.Distance( end.position, b.position ) * 0.1f, EventType.Repaint );
            }
        }

        public override string HeaderInfo => "Helper component which will animate single bone chains along with source bones animation";

        [UnityEditor.CanEditMultipleObjects]
        [UnityEditor.CustomEditor( typeof( RA2PhysicallyAnimatedChain ), true )]
        public class RA2PhysicallyAnimatedChainEditor : FimpossibleComponentEditor
        {
            public RA2PhysicallyAnimatedChain Get
            { get { if( _get == null ) _get = (RA2PhysicallyAnimatedChain)target; return _get; } }
            private RA2PhysicallyAnimatedChain _get;

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if( !Get.WasInitialized )
                {
                    GUILayout.Space( 6 );
                    EditorGUILayout.BeginHorizontal();
                    if( GUILayout.Button( Get.GeneratedDummy ? "Refresh" : "Pre - Generate" ) ) Get.GenerateJoints();
                    if( Get.GeneratedDummy ) if( GUILayout.Button( "Clear" ) ) Get.ClearJoints();
                    EditorGUILayout.EndHorizontal();
                }

                if( Get.GeneratedDummy )
                {
                    GUILayout.Space( 4 );
                    if( GUILayout.Button( new GUIContent( "  Select Dummy", FGUI_Resources.FindIcon( "Ragdoll Animator/SPR_RagdollAnimatedJoint" ) ), GUILayout.Height( 25 ) ) )
                    {
                        UnityEditor.Selection.activeGameObject = Get.GeneratedDummy;
                    }
                }
            }
        }

#endif

        #endregion Editor Class
    }
}