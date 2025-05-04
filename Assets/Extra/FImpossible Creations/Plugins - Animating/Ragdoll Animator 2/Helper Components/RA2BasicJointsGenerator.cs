using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace FIMSpace.FProceduralAnimation
{
    [AddComponentMenu( "FImpossible Creations/Ragdoll Animator/Basic Joints Chain Generator", 111 )]
    public class RA2BasicJointsGenerator : FimpossibleComponent
    {
        public float Radius = 0.2f;

        [Space( 3 )]
        public float Mass = 1f;

        [Space( 3 )]
        public float MassScale = 1f;

        public float ConnectedMass = 1f;
        public float RigidbodyDrag = 0f;
        public float AngularDrag = 0.1f;
        public RigidbodyInterpolation Interpolation = RigidbodyInterpolation.Interpolate;

        [Space( 4 )]
        public PhysicMaterial CollidersMaterial;

        [Tooltip( "Applying alternative tensor forces for joints, in some cases it can make motion more stable" )]
        public bool LimitTensors = false;

        [FPD_Header( "Main Chain References" )]
        public Transform FirstParentBone;

        public Transform EndChildBone;

        [Space( 5 )]
        [Tooltip( "Generating rigidbody under parent bone of the first bone in chain for position control, otherwise objec will stay fixed in one position and rotation" )]
        public bool AssignAnchor = true;

        private Rigidbody dummyRigidbody = null;

        [FPD_Header( "Optional Configurable Joints Option" )]
        public bool ConfigurableJoints = false;

        public float Spring = 5000;
        public float Damping = 10f;

        [HideInInspector, SerializeField] private List<Rigidbody> rigidbodies = new List<Rigidbody>();
        [HideInInspector, SerializeField] private List<ConfigurableJoint> configurableJoints = new List<ConfigurableJoint>();

        [SerializeField, HideInInspector] private Transform generatedOn = null;

        public bool WasInitialized { get; private set; } = false;

        private void Start()
        {
            if( generatedOn != FirstParentBone )
            {
                ClearJoints();
                GenerateJoints();
            }

            UpdatePhysicalParameters();

            WasInitialized = true;
        }

        public override void OnValidate()
        {
            if( Application.isPlaying && WasInitialized == false ) return;

            UpdatePhysicalParameters();

            base.OnValidate();
        }

        private void GenerateJoints()
        {
            Transform bone = EndChildBone;

            if( AssignAnchor )
            {
                if( FirstParentBone.parent == null )
                {
                    GameObject generated = new GameObject( name + "-GeneratedParent" );
                    generated.transform.position = FirstParentBone.position;
                    generated.transform.rotation = FirstParentBone.rotation;
                    FirstParentBone.SetParent( generated.transform, true );
                }

                dummyRigidbody = RagdollHandlerUtilities.GetOrGenerate<Rigidbody>( FirstParentBone.parent );
                dummyRigidbody.isKinematic = true;
            }

            while( bone != FirstParentBone && bone != null )
            {
                Rigidbody rig = RagdollHandlerUtilities.GetOrGenerate<Rigidbody>( bone );
                Joint joint = GenerateJointOn( bone );

                Rigidbody parentRig = RagdollHandlerUtilities.GetOrGenerate<Rigidbody>( bone.parent );
                GenerateJointOn( bone.parent );

                CapsuleCollider caps = RagdollHandlerUtilities.GetOrGenerate<CapsuleCollider>( bone.parent );
                caps.material = CollidersMaterial;
                RagdollHandlerUtilities.AdjustColliderBasingOnStartEndPosition( bone.parent.position, bone.position, bone.parent, caps, Radius );
                joint.connectedBody = parentRig;

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty( bone );
#endif

                bone = bone.parent;
            }

            bone = EndChildBone;
            rigidbodies.Clear();
            configurableJoints.Clear();

            while( bone != FirstParentBone.parent && bone != null )
            {
                Rigidbody rig = RagdollHandlerUtilities.GetOrGenerate<Rigidbody>( bone );
                Joint joint = bone.GetComponent<Joint>();
                rig.mass = Mass;
                joint.connectedMassScale = ConnectedMass;
                joint.massScale = MassScale;

                rigidbodies.Add( rig );

                ConfigurableJoint cJoints = joint as ConfigurableJoint;
                if( cJoints ) configurableJoints.Add( cJoints );

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty( bone );
#endif

                bone = bone.parent;
            }

            if( AssignAnchor )
            {
                if( FirstParentBone && FirstParentBone.GetComponent<Joint>() )
                {
                    FirstParentBone.GetComponent<Joint>().connectedBody = dummyRigidbody;
                }
            }

            generatedOn = FirstParentBone;
        }

        public void UpdatePhysicalParameters()
        {
            var bone = EndChildBone;

            if (FirstParentBone == null) return;
            if( WasInitialized && FirstParentBone.GetComponent<Rigidbody>() == null ) return;

            while( bone != FirstParentBone.parent && bone != null )
            {
                Rigidbody rig = bone.GetComponent<Rigidbody>();
                if( rig == null ) { bone = bone.parent; continue; }

                rig.mass = Mass;
                rig.drag = RigidbodyDrag;
                rig.angularDrag = AngularDrag;
                rig.interpolation = Interpolation;

                Joint joint = bone.GetComponent<Joint>();
                joint.connectedMassScale = ConnectedMass;
                joint.massScale = MassScale;

                bone = bone.parent;
            }
        }

        private bool tensorSwitched = false;

        private void FixedUpdate()
        {
            foreach( var joint in configurableJoints )
            {
                var drive = joint.slerpDrive;
                drive.positionSpring = Spring;
                drive.positionDamper = Damping;
                joint.slerpDrive = drive;
            }

            if( LimitTensors )
            {
                tensorSwitched = true;
                foreach( var rigidbody in rigidbodies )
                {
                    CalculateInertiaTensor( rigidbody );
                }
            }
            else
            {
                if( tensorSwitched )
                {
                    foreach( var rigidbody in rigidbodies ) rigidbody.ResetInertiaTensor();
                    tensorSwitched = false;
                }
            }
        }

        private void CalculateInertiaTensor( Rigidbody rig )
        {
            Vector3 size = transform.localScale;
            float mass = rig.mass;

            float Ixx = ( mass / 12f ) * ( size.y * size.y + size.z * size.z );
            float Iyy = ( mass / 12f ) * ( size.x * size.x + size.z * size.z );
            float Izz = ( mass / 12f ) * ( size.x * size.x + size.y * size.y );

            rig.inertiaTensor = new Vector3( Ixx, Iyy, Izz );
            rig.inertiaTensorRotation = rig.transform.rotation;
        }

        private Joint GenerateJointOn( Transform target )
        {
            if( ConfigurableJoints )
            {
                ConfigurableJoint config = RagdollHandlerUtilities.GetOrGenerate<ConfigurableJoint>( target );
                config.xMotion = ConfigurableJointMotion.Locked;
                config.yMotion = ConfigurableJointMotion.Locked;
                config.zMotion = ConfigurableJointMotion.Locked;
                config.rotationDriveMode = RotationDriveMode.Slerp;
                return config;
            }
            else return RagdollHandlerUtilities.GetOrGenerate<FixedJoint>( target );
        }

        private void ClearJoints()
        {
            Transform bone = EndChildBone;

            while( bone != FirstParentBone && bone != null )
            {
                RagdollHandlerUtilities.DestroyComponent<Joint>( bone );
                RagdollHandlerUtilities.DestroyComponent<Rigidbody>( bone );
                RagdollHandlerUtilities.DestroyComponent<Collider>( bone );
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty( bone );
#endif
                bone = bone.parent;
            }

            if( FirstParentBone != null )
            {
                RagdollHandlerUtilities.DestroyComponent<Joint>( FirstParentBone );
                RagdollHandlerUtilities.DestroyComponent<Rigidbody>( FirstParentBone );
                RagdollHandlerUtilities.DestroyComponent<Collider>( FirstParentBone );
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty( FirstParentBone );
#endif
            }

            if( AssignAnchor )
            {
                if( FirstParentBone ) RagdollHandlerUtilities.DestroyComponent<Rigidbody>( FirstParentBone.parent );
            }

            generatedOn = null;
        }

        #region Editor Class

#if UNITY_EDITOR

        public override string HeaderInfo => "Basic component which generates joint components along provided bones chain (not animating with animator)";

        [UnityEditor.CanEditMultipleObjects]
        [UnityEditor.CustomEditor( typeof( RA2BasicJointsGenerator ), true )]
        public class RA2BasicJointsGeneratorEditor : FimpossibleComponentEditor
        {
            public RA2BasicJointsGenerator Get
            { get { if( _get == null ) _get = (RA2BasicJointsGenerator)target; return _get; } }
            private RA2BasicJointsGenerator _get;

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                GUILayout.Space( 6 );
                EditorGUILayout.BeginHorizontal();
                if( GUILayout.Button( "Generate" ) ) Get.GenerateJoints();
                if( GUILayout.Button( "Clear" ) ) Get.ClearJoints();
                EditorGUILayout.EndHorizontal();
            }
        }

#endif

        #endregion Editor Class
    }
}