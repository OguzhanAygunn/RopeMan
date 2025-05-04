using FIMSpace.AnimationTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [System.Serializable]
    public class RagdollChainBone
    {
        /// <summary> Source animtor bone transform </summary>
        public Transform SourceBone;

        /// <summary> Transform bone generated for physical bone </summary>
        public Transform PhysicalDummyBone;

        /// <summary> Dummy structure initial parent object </summary>
        [NonSerialized] public Transform DetachParent = null;

        /// <summary> [Playmode Only] Parent bones chain to which bone belongs to </summary>
        [field: NonSerialized] public RagdollBonesChain ParentChain { get; private set; } = null;
        [field: NonSerialized] public RagdollChainBone ParentBone { get; private set; } = null;

        /// <summary> Playmode calculations class </summary>
        public RagdollBoneProcessor BoneProcessor { get; private set; }

        /// <summary> Returning bone processor, it's reuivalent of 'Posing Bone' like it was in Ragdoll Animator 1 </summary>
        public RagdollBoneProcessor Posing => BoneProcessor;

        internal bool IsAnchor = false;

        /// <summary> Definining source bone depth index on runtime initialization </summary>
        internal int SourceBoneDepth = -1;

        /// <summary> Set true if you want to take controll over rigidbody's isKinematic switches and motion </summary>
        internal bool BypassKinematicControl = false;

        /// <summary> Scale for bounded collision ignore detection volume </summary>
        [HideInInspector] public float BoundedIgnoreScale = 1f;

        /// <summary> Information if this bone instance was dismembered from the rest of the body </summary>
        [NonSerialized] public bool WasDismembered = false;

        /// <summary> Information if some of this bone's parent was dismembered from the rest of the body </summary>
        [NonSerialized] public bool ParentDismembered = false;

        public Rigidbody InitialConnectedBody { get; internal set; }
        public Vector3 InitialJointAnchor { get; private set; } = Vector3.zero;
        public bool PlaymodeInitialized { get; private set; } = false;

        public void GenerateDummyBone( Transform transform )
        {
            if( PhysicalDummyBone != null ) return;
            PhysicalDummyBone = transform;
        }

        public void PlaymodeInitialize( RagdollBonesChain parentChain )
        {
            if( PlaymodeInitialized ) return;

            ParentChain = parentChain;
            BoneProcessor = new RagdollBoneProcessor( this );
            PlaymodeInitialized = true;

            if( SourceBone ) SourceBoneDepth = SkeletonRecognize.SkeletonInfo.GetDepth( SourceBone, parentChain.ParentHandler.GetBaseTransform() );
        }

        public Rigidbody GameRigidbody { get; private set; }

        /// <summary> Returns first collider which is set on this bone (if you use multiple colliders, access them using GetColliderSetup(1...) </summary>
        public Collider MainBoneCollider
        { get { return BaseColliderSetup.GameCollider; } }

        public ConfigurableJoint Joint { get; private set; }

        public void ApplyToAllColliders( Action<Collider> action )
        {
            foreach( var c in colliders )
            {
                if( c.GameCollider ) action.Invoke( c.GameCollider );
            }
        }

        [Tooltip( "Helper indicator value, which can be used for custom scripts hit bone indication" )]
        public ERagdollBoneID BoneID = ERagdollBoneID.Unknown;

        #region Colliders Related

        public enum EColliderType
        { Capsule, Sphere, Box, Mesh, Other }

        public enum ECapsuleDirection
        { X = 0, Y = 1, Z = 2 }

        /// <summary> Support for multiple colliders per bone </summary>
        [SerializeField, HideInInspector] private List<ColliderSetup> colliders = new List<ColliderSetup>() { new ColliderSetup() };

        /// <summary> All physical dummy colliders </summary>
        public List<ColliderSetup> Colliders
        { get { return colliders; } }

        /// <summary> First and always required collider setup on the bone </summary>
        public ColliderSetup BaseColliderSetup
        { get { return colliders[0]; } }

        [System.Serializable]
        public class ColliderSetup
        {
            public EColliderType ColliderType = EColliderType.Capsule;
            public Vector3 ColliderCenter = Vector3.zero;
            public float ColliderSizeMultiply = 1f;

            public ECapsuleDirection CapsuleDirection = ECapsuleDirection.Y;
            public float ColliderRadius = 0.1f;
            public float ColliderLength = 0.3f;

            public Vector3 ColliderBoxSize = Vector3.one;
            public Mesh ColliderMesh = null;
            public Collider OtherReference;

            public Vector3 RotationCorrection = Vector3.zero;
            public Quaternion RotationCorrectionQ => Quaternion.Euler( RotationCorrection );

            public bool UsingExtraTransform
            { get { return ( RotationCorrection != Vector3.zero || ColliderType == EColliderType.Mesh ); } }

            /// <summary> Using additional rotation correction transform </summary>
            public Transform ColliderExtraTransform = null;

            /// <summary> Runtime generated collider </summary>
            [NonSerialized] public Collider GameCollider;

            /// <summary> Runtime generated collider generated on the source skeleton bone </summary>
            [NonSerialized] public Collider GameColliderOnSource = null;

            [NonSerialized] public float BoundedIgnoreScale = 1f;

            public Vector3 GetScaleModded( RagdollBonesChain chain, RagdollChainBone bone )
            {
                if( bone.SourceBone == null ) return Vector3.one * chain.GetScaleMultiplier();
                Vector3 scale = Vector3.Scale( bone.SourceBone.lossyScale, new Vector3( ColliderSizeMultiply, ColliderSizeMultiply, ColliderSizeMultiply ) );
                //if (ColliderType == EColliderType.Box) return Vector3.Scale(scale, ColliderBoxSize) * chainMultiply;

                return scale * chain.GetScaleMultiplier();
            }

            public Vector3 ScaleUsingThickness( Vector3 scale, float thickness, RagdollBonesChain chain, RagdollChainBone bone )
            {
                if( thickness == 0f || thickness == 1f ) return scale;

                Vector3 boneDirection;

                // Ensure existance of required references
                int index = chain.GetIndex( bone );

                RagdollChainBone nextBone;
                if( index == chain.BoneSetups.Count - 1 ) { nextBone = chain.GetBone( index - 1 ); if( bone == null ) return scale; }
                else nextBone = chain.GetBone( index + 1 );

                // Required current and next transform
                if( index >= 0 && nextBone != null && bone.SourceBone != null && nextBone.SourceBone != null )
                {
                    boneDirection = ( nextBone.SourceBone.position - bone.SourceBone.position );
                    boneDirection = bone.SourceBone.InverseTransformDirection( boneDirection ).normalized;
                    Vector3 axes = FVectorMethods.ChooseDominantAxis( boneDirection );

                    if( axes.x == 0f ) axes.x = thickness; else axes.x = 1f;
                    if( axes.y == 0f ) axes.y = thickness; else axes.y = 1f;
                    if( axes.z == 0f ) axes.z = thickness; else axes.z = 1f;

                    return Vector3.Scale( scale, axes );
                }

                return scale;
            }

            public float GetAverageScale( RagdollChainBone bone, float chainMultiply = 1f )
            {
                float scale = ColliderSizeMultiply * bone.SourceBone.lossyScale.x * chainMultiply;
                if( ColliderType == EColliderType.Capsule ) return scale * ( ( ColliderRadius * 2f ) * ColliderLength );
                else if( ColliderType == EColliderType.Sphere ) return scale * ( ColliderRadius * 2f );
                else if( ColliderType == EColliderType.Box ) return scale * ( ColliderRadius * 2f );
                else if( ColliderType == EColliderType.Mesh ) if( ColliderMesh != null ) return scale * ( ColliderMesh.bounds.extents.magnitude );
                    else if( ColliderType == EColliderType.Other ) if( OtherReference != null ) return scale * ( OtherReference.bounds.extents.magnitude );
                return scale;
            }

            public Vector3 GetColliderSizeAxes()
            {
                if( ColliderType == EColliderType.Capsule )
                {
                    //if( CapsuleDirection == 0 ) return new Vector3( ColliderLength, ColliderRadius, ColliderRadius );
                    return new Vector3( ColliderRadius, ColliderLength, ColliderRadius );
                }
                else if( ColliderType == EColliderType.Sphere ) return new Vector3( ColliderRadius, ColliderRadius, ColliderRadius );
                else if( ColliderType == EColliderType.Box ) return ColliderBoxSize;
                else return new Vector3( ColliderSizeMultiply, ColliderSizeMultiply, ColliderSizeMultiply );
            }

            /// <summary> Gets or generetes target collider and refreshes its parameters </summary>
            public Collider RefreshCollider( RagdollChainBone bone, bool fallMode, int colliderIndex, RagdollBonesChain chain, bool onSource )
            {
                Transform targetT = onSource ? bone.SourceBone : bone.PhysicalDummyBone;
                Collider collider = null;
                Transform subTransform = null;

                PhysicMaterial colliderMaterial = bone.OverrideMaterial; // By default null
                if( colliderMaterial == null ) if( chain.ParentHandler != null ) if( chain.ParentHandler.CollidersPhysicMaterial )
                        {
                            if( chain.ParentHandler.PhysicMaterialOnFall && fallMode )
                                colliderMaterial = chain.ParentHandler.PhysicMaterialOnFall;
                            else
                                colliderMaterial = chain.ParentHandler.CollidersPhysicMaterial;
                        }

                if( colliderIndex > 0 )
                {
                    string name = targetT.name + ":[" + colliderIndex + "] RagdollCollider";
                    subTransform = targetT.Find( name );

                    if( subTransform == null ) subTransform = RagdollHandler.CreateTransform( name, chain.ParentHandler.RagdollDummyLayer );

                    subTransform.SetParent( targetT, false );
                    subTransform.SetAsLastSibling();
                    subTransform.localRotation = RotationCorrectionQ;
                    ColliderExtraTransform = subTransform;
                    targetT = subTransform;
                }
                else
                {
                    if( UsingExtraTransform )
                    {
                        string name = targetT.name + ":RagdollCollider";

                        subTransform = targetT.Find( name );

                        if( subTransform == null ) subTransform = RagdollHandler.CreateTransform( name, chain.ParentHandler.RagdollDummyLayer );

                        subTransform.SetParent( targetT, false );
                        subTransform.SetAsLastSibling();
                        subTransform.localRotation = RotationCorrectionQ;
                        ColliderExtraTransform = subTransform;
                        targetT = subTransform;
                    }
                    else
                    {
                        subTransform = targetT.Find( targetT.name + ":RagdollCollider" );
                        if( subTransform ) RagdollHandlerUtilities.DestroyObject( subTransform.gameObject );
                    }
                }

                float scaleMul = chain.ChainScaleMultiplier * ColliderSizeMultiply;
                float thickness = chain.GetThicknessMultiplier();

                if( chain.ParentHandler != null ) scaleMul *= chain.ParentHandler.RagdollSizeMultiplier;
                if( ColliderType == EColliderType.Capsule )
                {
                    DisposeWrongCollider( typeof( CapsuleCollider ) );
                    var caps = bone.GetOrGenerate<CapsuleCollider>( targetT );
                    caps.radius = ColliderRadius * scaleMul * thickness;
                    caps.height = ColliderLength * scaleMul;
                    caps.direction = (int)CapsuleDirection;
                    caps.center = ColliderCenter;
                    collider = caps;
                }
                else if( ColliderType == EColliderType.Sphere )
                {
                    DisposeWrongCollider( typeof( SphereCollider ) );
                    var sph = bone.GetOrGenerate<SphereCollider>( targetT );
                    sph.radius = ColliderRadius * scaleMul * thickness;
                    sph.center = ColliderCenter;
                    collider = sph;
                }
                else if( ColliderType == EColliderType.Box )
                {
                    DisposeWrongCollider( typeof( BoxCollider ) );
                    var box = bone.GetOrGenerate<BoxCollider>( targetT );
                    box.size = ScaleUsingThickness( ColliderBoxSize * scaleMul, thickness, chain, bone );
                    box.center = ColliderCenter;
                    collider = box;
                }
                else if( ColliderType == EColliderType.Mesh )
                {
                    DisposeWrongCollider( typeof( MeshCollider ) );
                    subTransform.localPosition = ColliderCenter;
                    subTransform.localScale = ScaleUsingThickness( ColliderBoxSize * scaleMul, thickness, chain, bone );
                    MeshCollider meshC = bone.GetOrGenerate<MeshCollider>( subTransform );
                    meshC.sharedMesh = ColliderMesh;
                    collider = meshC;
                }
                else if( ColliderType == EColliderType.Other )
                {
                    if( OtherReference != null )
                    {
                        DisposeWrongCollider( OtherReference.GetType() );

                        if( subTransform )
                        {
                            subTransform.localPosition = Vector3.zero;
                            subTransform.localScale = Vector3.one;
                        }

                        Collider nColl = targetT.gameObject.GetComponent( OtherReference.GetType() ) as Collider;
                        if( nColl == null ) nColl = targetT.gameObject.AddComponent( OtherReference.GetType() ) as Collider;
                        RagdollBonesChain.CopyColliderSettingTo( OtherReference, nColl );
                        collider = nColl;
                    }
                    else collider = null;
                }

                if( onSource ) GameColliderOnSource = collider; else GameCollider = collider;
                if( colliderMaterial != null ) bone.ApplyPhysicMaterial( colliderMaterial );
                if( GameCollider && GameColliderOnSource ) Physics.IgnoreCollision( GameCollider, GameColliderOnSource, true );

                return collider;
            }

            private void ProceedIgnore( Collider a, Collider b, bool ignore )
            {
                if( Physics.GetIgnoreCollision( a, b ) == ignore ) return;
                Physics.IgnoreCollision( a, b, ignore );
            }

            public void IgnoreCollisionWith( Collider coll, bool ignore )
            {
                if( GameCollider ) ProceedIgnore( coll, GameCollider, ignore );
                if( GameColliderOnSource ) ProceedIgnore( coll, GameColliderOnSource, ignore );
                if( ColliderType == RagdollChainBone.EColliderType.Other && OtherReference ) ProceedIgnore( coll, OtherReference, ignore );
            }

            public void IgnoreCollisionWith( ColliderSetup oColl, bool ignore )
            {
                if( oColl.GameCollider ) IgnoreCollisionWith( oColl.GameCollider, ignore );
                if( oColl.GameColliderOnSource ) IgnoreCollisionWith( oColl.GameColliderOnSource, ignore );
                if( oColl.ColliderType == RagdollChainBone.EColliderType.Other && OtherReference ) IgnoreCollisionWith( oColl.OtherReference, ignore );
            }

            private void DisposeWrongCollider( System.Type targetType )
            {
                if( GameCollider == null ) return;
                if( GameCollider.GetType() != targetType )
                {
                    if( GameCollider is MeshCollider ) RagdollHandlerUtilities.DestroyObject( GameCollider.gameObject );
                    else RagdollHandlerUtilities.DestroyObject( GameCollider );
                }
            }

            public float Editor_GetHandleSize( RagdollChainBone bone )
            {
                if( bone.SourceBone == null ) return 0.1f;

                float size = bone.SourceBone.lossyScale.x * ColliderSizeMultiply;
                if( ColliderType == RagdollChainBone.EColliderType.Box ) size *= ColliderBoxSize.magnitude * 0.25f;
                else if( ColliderType == RagdollChainBone.EColliderType.Capsule ) size *= ( ColliderLength + ColliderRadius * 2f ) * 0.15f;
                else if( ColliderType == RagdollChainBone.EColliderType.Sphere ) size *= ( ColliderRadius ) * 0.4f;
                else size *= 0.1f;

                return size;
            }

            public void DisposeRuntimeObjects()
            {
                if( GameCollider ) RagdollHandlerUtilities.DestroyObject( GameCollider );
                if( ColliderExtraTransform ) RagdollHandlerUtilities.DestroyObject( ColliderExtraTransform );
            }

            /// <summary> World collider size </summary>
            public Vector3 CalculateLocalSize()
            {
                Vector3 size = new Vector3();

                if( ColliderType == EColliderType.Box ) size = ColliderBoxSize;
                else if( ColliderType == EColliderType.Capsule ) size = new Vector3( ColliderRadius, ColliderLength, ColliderRadius );
                else if( ColliderType == EColliderType.Sphere ) size = new Vector3( ColliderRadius, ColliderRadius, ColliderRadius );
                else if( GameCollider ) return GameCollider.bounds.size;

                return size;
            }

            /// <summary> World collider size </summary>
            public Vector3 CalculateSize()
            {
                return Vector3.Scale( CalculateLocalSize(), GameCollider.transform.lossyScale );
            }

            public void CopySettingsFromColliderComponent( Collider collider )
            {
                if( collider is BoxCollider )
                {
                    ColliderCenter = ( (BoxCollider)collider ).center;
                    ColliderBoxSize = ( (BoxCollider)collider ).size;
                }
                else if( collider is SphereCollider )
                {
                    ColliderCenter = ( (SphereCollider)collider ).center;
                    ColliderRadius = ( (SphereCollider)collider ).radius;
                }
                else if( collider is CapsuleCollider )
                {
                    ColliderCenter = ( (CapsuleCollider)collider ).center;
                    ColliderRadius = ( (CapsuleCollider)collider ).radius;
                    ColliderLength = ( (CapsuleCollider)collider ).height;
                    CapsuleDirection = (ECapsuleDirection)( (CapsuleCollider)collider ).direction;
                }
                else if( collider is MeshCollider )
                {
                    ColliderMesh = ( (MeshCollider)collider ).sharedMesh;
                }
            }

            public void CopySettingsFromOtherSetup( ColliderSetup copyFrom )
            {
                if( copyFrom == null ) return;

                ColliderType = copyFrom.ColliderType;
                ColliderCenter = copyFrom.ColliderCenter;
                ColliderSizeMultiply = copyFrom.ColliderSizeMultiply;

                CapsuleDirection = copyFrom.CapsuleDirection;
                ColliderRadius = copyFrom.ColliderRadius;
                ColliderLength = copyFrom.ColliderLength;

                ColliderBoxSize = copyFrom.ColliderBoxSize;
                ColliderMesh = copyFrom.ColliderMesh;
                OtherReference = copyFrom.OtherReference;

                RotationCorrection = copyFrom.RotationCorrection;
            }
        }

        /// <summary> Setting joint slerp drive to zero and hard matching multiplier to zero </summary>
        public void SwitchOffJointAnimationMatching()
        {
            var drive = Joint.slerpDrive;
            drive.positionSpring = 0f;
            drive.positionDamper = 0f;
            Joint.slerpDrive = drive;
            HardMatchingMultiply = 0f;
            Joint_SetAngularMotionLock( ConfigurableJointMotion.Limited );
        }

        /// <summary> If skeleton requires more complex collider shape, you can add more colliders </summary>
        public ColliderSetup AddColliderSetup()
        {
            ColliderSetup setup = new ColliderSetup();
            ColliderSetup copyFrom = null;
            if( colliders.Count > 0 ) copyFrom = colliders[colliders.Count - 1];
            colliders.Add( setup );
            setup.CopySettingsFromOtherSetup( copyFrom );
            return setup;
        }

        public void RemoveColliderSetup( int indexToRemove )
        {
            if( indexToRemove == 0 ) return;
            if( colliders.ContainsIndex( indexToRemove ) )
            {
                colliders[indexToRemove].DisposeRuntimeObjects();
                colliders.RemoveAt( indexToRemove );
            }
        }

        public ColliderSetup GetColliderSetup( int index )
        {
            if( colliders.ContainsIndex( index ) ) return colliders[index];
            return null;
        }

        public Matrix4x4 GetMatrix( Vector3 centerOffset, Vector3 scale, Quaternion correctionRot )
        {
            if( SourceBone == null ) return Matrix4x4.identity;
            return Matrix4x4.TRS( SourceBone.TransformPoint( centerOffset ), SourceBone.rotation * correctionRot, scale );
        }

        #endregion Colliders Related

        #region Physics Related

        [Tooltip( "Multiplying target collider rigidbody mass. It's using the 'Max Mass' ragdoll value + Chain Mass Multiplier." )]
        [Range( 0f, 1f )] public float MassMultiplier = .1f;

        [Tooltip( "Controlling power of ragdoll physical forces for a single bone" )]
        public float ForceMultiplier = 1f;

        [Tooltip( "Extra power added to the bones joints springs with use of less sensitive calculations. Can be helpful for adjusting spine springs. " )]
        public float MusclesBoost = 0f;

        [Tooltip( "First rotation axis for the Unity Physical Joint component" )]
        public EJointAxis MainAxis = EJointAxis.X;

        public Vector3 TargetMainAxis = Vector3.right;
        public bool InverseMainAxis = false;

        public Vector3 GetMainAxis()
        {
            Vector3 axis;
            if( MainAxis == EJointAxis.X ) axis = InverseMainAxis ? Vector3.left : Vector3.right;
            else if( MainAxis == EJointAxis.Y ) axis = InverseMainAxis ? Vector3.down : Vector3.up;
            else if( MainAxis == EJointAxis.Z ) axis = InverseMainAxis ? Vector3.back : Vector3.forward;
            else axis = TargetMainAxis.normalized;
            return axis;
        }

        public void SetMainAxisByVector( Vector3 dir )
        {
            dir.Normalize();
            dir = FVectorMethods.ChooseDominantAxis( dir );
            if( dir.x > 0.3f ) { InverseMainAxis = false; MainAxis = EJointAxis.X; }
            else if( dir.x < -0.3f ) { InverseMainAxis = true; MainAxis = EJointAxis.X; }
            else if( dir.y > 0.3f ) { InverseMainAxis = false; MainAxis = EJointAxis.Y; }
            else if( dir.y < -0.3f ) { InverseMainAxis = true; MainAxis = EJointAxis.Y; }
            else if( dir.z > 0.3f ) { InverseMainAxis = false; MainAxis = EJointAxis.Z; }
            else if( dir.z < -0.3f ) { InverseMainAxis = true; MainAxis = EJointAxis.Z; }
        }

        public void SetSecondaryAxisByVector( Vector3 dir )
        {
            dir.Normalize();
            dir = FVectorMethods.ChooseDominantAxis( dir );

            if( dir.x > 0.3f ) { InverseSecondaryAxis = false; SecondaryAxis = EJointAxis.X; }
            else if( dir.x < -0.3f ) { InverseSecondaryAxis = true; SecondaryAxis = EJointAxis.X; }
            else if( dir.y > 0.3f ) { InverseSecondaryAxis = false; SecondaryAxis = EJointAxis.Y; }
            else if( dir.y < -0.3f ) { InverseSecondaryAxis = true; SecondaryAxis = EJointAxis.Y; }
            else if( dir.z > 0.3f ) { InverseSecondaryAxis = false; SecondaryAxis = EJointAxis.Z; }
            else if( dir.z < -0.3f ) { InverseSecondaryAxis = true; SecondaryAxis = EJointAxis.Z; }
        }

        [Tooltip( "Low Twist or lowAngularXLimit : -177 : 177 : Needs to be lower than High Twist Limit" )]
        [Range( -177f, 177f )] public float MainAxisLowLimit = -60f;

        public float GetMainAxisLowLimit( RagdollBonesChain chain )
        { return MainAxisLowLimit * chain.AxisLimitRange; }

        [Tooltip( "HighTwist or highAngularXLimit : -177 : 177" )]
        [Range( -177f, 177f )] public float MainAxisHighLimit = 60f;

        public float GetMainAxisHighLimit( RagdollBonesChain chain )
        { return MainAxisHighLimit * chain.AxisLimitRange; }

        [Tooltip( "Secondary rotation axis for the Unity Physical Joint component" )]
        public EJointAxis SecondaryAxis = EJointAxis.Y;

        public Vector3 TargetSecondaryAxis = Vector3.up;
        public bool InverseSecondaryAxis = false;

        public Vector3 GetSecondaryAxis()
        {
            Vector3 axis;
            if( SecondaryAxis == EJointAxis.X ) axis = InverseSecondaryAxis ? Vector3.left : Vector3.right;
            else if( SecondaryAxis == EJointAxis.Y ) axis = InverseSecondaryAxis ? Vector3.down : Vector3.up;
            else if( SecondaryAxis == EJointAxis.Z ) axis = InverseSecondaryAxis ? Vector3.back : Vector3.forward;
            else axis = TargetSecondaryAxis.normalized;
            return axis;
        }

        [Tooltip( "Secondary axis angle limit plus-minus 3 : 177 degrees" )]
        [Range( 3f, 177f )] public float SecondaryAxisAngleLimit = 30f;

        public float GetSecondaryAxisAngleLimit( RagdollBonesChain chain )
        { return SecondaryAxisAngleLimit * chain.AxisLimitRange; }

        [Tooltip( "Last axis angle limit plus-minus 3 : 177 degrees" )]
        [Range( 3f, 177f )] public float ThirdAxisAngleLimit = 40f;

        public float GetThirdAxisAngleLimit( RagdollBonesChain chain )
        { return ThirdAxisAngleLimit * chain.AxisLimitRange; }

        public PhysicMaterial OverrideMaterial = null;

        public bool UseIndividualParameters = false;

        [Tooltip( "Override rigidbody interpolation mode." )]
        public RigidbodyInterpolation OverrideInterpolation = RigidbodyInterpolation.Interpolate;

        [Tooltip( "Override rigidbody collision detection mode." )]
        public CollisionDetectionMode OverrideDetectionMode = CollisionDetectionMode.Discrete;

        [Tooltip( "Override Drag Parameter value for rigidbody" )]
        public float OverrideDragValue = 0f;

        [Tooltip( "Override Angular Drag Parameter value for rigidbody" )]
        public float OverrideAngularDrag = 0.2f;

        [Tooltip( "Set greater than zero, to override bone's joint animation matching spring power" )]
        public float OverrideSpringPower = 0f;

        [Tooltip( "Set greater than zero, to override bone's joint animation matching damping parameter" )]
        public float OverrideSpringDamp = 0f;

        [Range( 0f, 1f )]
        public float HardMatchingMultiply = 1f;

        [Range( 0f, 1f )]
        public float HardMatchOverride = 0f;

        [Range( 0f, 1.5f )]
        public float ConnectionMassOverride = 0f;

        [Tooltip( "Set true if you want to skip this bone in collision detection send events." )]
        public bool DisableCollisionEvents = false;

        [Tooltip( "Set true if you want to use joint limits all the time." )]
        public bool ForceLimitsAllTheTime = false;

        [Tooltip( "Setting bone kinematic during standing mode to make it better in sync with currently played animation" )]
        public bool ForceKinematicOnStanding = false;

        private bool _wasForceKinematicOnStanding = false;

        [Tooltip( "Setting configurable motion lock to limited to 0.000001f translation value : you can use linear spring limits now" )]
        public bool AllowConfigurablePosition = false;

        public float LinearSpringLimit = 10000;
        public float LinearSpringDamping = 5;

        public Vector3 GetThirdAxis()
        {
            return Vector3.Cross( GetMainAxis(), GetSecondaryAxis() );
        }

        public float GetMass( RagdollBonesChain chain )
        {
            if( chain.ParentHandler == null ) return 1f;
            return chain.ParentHandler.ReferenceMass * chain.MassMultiplier * MassMultiplier;
        }

        public void DoAutoMassSettings( RagdollHandler handler, RagdollBonesChain chain )
        {
            int index = -1;
            for( int i = 0; i < chain.BoneSetups.Count; i++ ) if( chain.BoneSetups[i] == this ) index = i;
            if( index == -1 ) return;

            MassMultiplier = chain.GetBoneMassPercentage( index, chain.GetChainTypePercentageMass() * 0.01f ) * 0.01f;
        }

        public float GetRigidbodyDrag( RagdollBonesChain chain )
        { return chain.ParentHandler.RigidbodyDragValue; }

        public float GetRigidbodyAngularDrag( RagdollBonesChain chain )
        { return chain.ParentHandler.RigidbodyAngularDragValue; }

        public float GetMainAxisLimitContactDistance( RagdollBonesChain chain )
        { return chain.ParentHandler.JointContactDistance; }

        public float GetMainAxisLimitBounciness( RagdollBonesChain chain )
        { return chain.ParentHandler.JointBounciness; }

        public float GetMainAxisLimitSpring( RagdollBonesChain chain )
        { return chain.ParentHandler.JointLimitSpring; }

        public float GetMainAxisLimitDamper( RagdollBonesChain chain )
        { return chain.ParentHandler.JointLimitDamper; }

        public float GetOtherAxesLimitSpring( RagdollBonesChain chain )
        { return chain.ParentHandler.JointLimitSpring; }

        public float GetOtherAxesLimitDamper( RagdollBonesChain chain )
        { return chain.ParentHandler.JointLimitDamper; }

        #endregion Physics Related

        [Tooltip( "Selective bone ragdoll blend multiplier" )]
        [FPD_Suffix( 0f, 1f )]
        public float BoneBlendMultiplier = 1f;

        #region Helper Reference Values

        public Vector3 LocalRight = Vector3.right;
        public Vector3 LocalUp = Vector3.up;
        public Vector3 LocalForward = Vector3.forward;
        public Vector3 ToBase = Vector3.zero;

        public void StoreHelperReferenceValues( Transform baseTransform )
        {
            LocalRight = SourceBone.InverseTransformDirection( baseTransform.right );
            LocalUp = SourceBone.InverseTransformDirection( baseTransform.up );
            LocalForward = SourceBone.InverseTransformDirection( baseTransform.forward );
            ToBase = SourceBone.InverseTransformPoint( baseTransform.position );
        }

        #endregion Helper Reference Values

        #region In between bones for lost connections between source skeleton bones parenting hierarchy

        public List<InBetweenBone> InBetweenBones { get; private set; } = null;

        public void SetInBetweenBones( List<InBetweenBone> inBetweenBones )
        {
            InBetweenBones = inBetweenBones;
        }

        /// <summary> Gets or generetes target collider and refreshes its parameters </summary>
        public Rigidbody RefreshRigidbody( RagdollHandler handler, RagdollBonesChain chain, bool onSource )
        {
            Transform targetT = onSource ? SourceBone : PhysicalDummyBone;
            Rigidbody rigid = GameRigidbody == null ? GetOrGenerate<Rigidbody>( targetT ) : GameRigidbody;
            GameRigidbody = rigid;

            if( handler.MaxAngularVelocity > 0f ) rigid.maxAngularVelocity = handler.MaxAngularVelocity;

            if( handler.MaxVelocity > 0f ) rigid.SetMaxLinearVelocityU2022( handler.MaxVelocity );
            if( handler.MaxDepenetrationVelocity > 0f ) rigid.maxDepenetrationVelocity = handler.MaxDepenetrationVelocity;

            rigid.mass = GetMass( chain );

            if( UseIndividualParameters )
            {
                rigid.interpolation = OverrideInterpolation;
                rigid.collisionDetectionMode = OverrideDetectionMode;

                rigid.drag = OverrideDragValue;
                rigid.angularDrag = OverrideAngularDrag;

                RefreshSolversCount( handler );
            }
            else
            {
                RefreshRigidbodyOptimizationParameters( handler );

                rigid.drag = GetRigidbodyDrag( chain );
                rigid.angularDrag = GetRigidbodyAngularDrag( chain );
            }

            return rigid;
        }

        /// <summary> Refreshing solver iterations count and interpolation rigidbodies settings </summary>
        public void RefreshRigidbodyOptimizationParameters( RagdollHandler handler )
        {
            RefreshSolversCount(handler);

            if( handler.disableInterpolation ) GameRigidbody.interpolation = RigidbodyInterpolation.None; else GameRigidbody.interpolation = handler.RigidbodiesInterpolation;
            if( handler.onlyDiscreteDetection ) GameRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete; else GameRigidbody.collisionDetectionMode = handler.RigidbodiesDetectionMode;
        }

        void RefreshSolversCount( RagdollHandler handler )
        {
            GameRigidbody.solverIterations = handler.UnitySolverIterations;

            if( handler.UnityVelocitySolverIterations < 1 ) GameRigidbody.solverVelocityIterations = Physics.defaultSolverVelocityIterations;
            else { GameRigidbody.solverVelocityIterations = handler.UnityVelocitySolverIterations; }

        }

        protected void RefreshRigidbodyInterpolation( RagdollHandler handler )
        {
            if( UseIndividualParameters )
                GameRigidbody.interpolation = OverrideInterpolation;
            else
                GameRigidbody.interpolation = handler.RigidbodiesInterpolation;
        }

        public bool UsingExtraTransform
        {
            get
            {
                bool any = false;
                foreach( var c in colliders ) if( c.UsingExtraTransform ) { any = true; break; }
                return any;
            }
        }

        public void ApplyPhysicMaterial( PhysicMaterial pMaterial )
        {
            ApplyToAllColliders( ( Collider c ) => c.sharedMaterial = pMaterial );
        }

        /// <summary> Generetes target colliders and refreshes its parameters </summary>
        public void RefreshCollider( RagdollBonesChain chain, bool fallMode, bool onSource )
        {
            for( int i = 0; i < colliders.Count; i++ )
            {
                var coll = colliders[i];
                coll.RefreshCollider( this, fallMode, i, chain, onSource );
            }
        }

        private T GetOrGenerate<T>( Transform from ) where T : Component
        {
            var collider = from.GetComponent<T>();
            if( collider == null ) collider = from.gameObject.AddComponent<T>();
            return collider;
        }

        public ConfigurableJoint RefreshJoint( RagdollBonesChain chain, bool fallMode, bool onSource, bool playmodeRefresh )
        {
            Transform targetT = onSource ? SourceBone : PhysicalDummyBone;
            ConfigurableJoint joint = Joint;

            if( joint == null )
            {
                joint = targetT.GetComponent<ConfigurableJoint>();
                if( joint == null ) joint = targetT.gameObject.AddComponent<ConfigurableJoint>();
                joint.rotationDriveMode = RotationDriveMode.Slerp;
            }

            Joint = joint;

            if( !playmodeRefresh )
            {
                joint.axis = GetMainAxis();
                joint.secondaryAxis = GetSecondaryAxis();
            }

            if( WasDismembered == false ) RefreshJointLimitSwitch( chain );

            Joint_UpdateAngleLimits( chain );
            Joint_UpdateAngularSpringLimits( chain );
            RefreshDynamicPhysicalParameters( chain, fallMode );

            joint.enableCollision = false;
            joint.enablePreprocessing = chain.ParentHandler.PreProcessing;
            joint.projectionMode = chain.ParentHandler.ProjectionMode;

            return joint;
        }

        public void RefreshJointLimitSwitch( RagdollBonesChain parentChain )
        {
            if( IsAnchor )
            {
                Joint_SetMotionLock( ConfigurableJointMotion.Free );
                Joint_SetAngularMotionLock( ConfigurableJointMotion.Free );
            }
            else
            {
                if( AllowConfigurablePosition )
                {
                    Joint_SetMotionLock( ConfigurableJointMotion.Limited );
                    var limit = Joint.linearLimit; limit.limit = 0.00001f; Joint.linearLimit = limit;
                    var limitSpring = Joint.linearLimitSpring; limitSpring.spring = LinearSpringLimit; limitSpring.damper = LinearSpringDamping; Joint.linearLimitSpring = limitSpring;
                }
                else
                {
                    Joint_SetMotionLock( ConfigurableJointMotion.Locked );
                }

                if( ForceLimitsAllTheTime )
                {
                    Joint_SetAngularMotionLock( ConfigurableJointMotion.Limited );
                }
                else
                {
                    bool unlimited = parentChain.UnlimitedRotations;
                    if( !unlimited ) unlimited = parentChain.ParentHandler.UnlimitedRotationOnStandingModeCheck();
                    Joint_SetAngularMotionLock( unlimited ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Limited );
                }
            }
        }

        /// <summary>
        /// (Runtime) Updating joint dynamic parameters like connected mass scale etc.
        /// </summary>
        public void RefreshDynamicPhysicalParameters( RagdollBonesChain chain, bool fallMode )
        {
            float mul = chain.ParentHandler.FadeInBlend;

            if( ConnectionMassOverride > 0f )
            {
                Joint.connectedMassScale = ConnectionMassOverride * mul;
                return;
            }

            if( fallMode )
            {
                if( Joint )
                {
                    if( chain.ConnectedMassOverride ) Joint.connectedMassScale = chain.ConnectedMassScale;
                    else
                        Joint.connectedMassScale = chain.ConnectedMassScale * chain.ParentHandler.MassMultiplyOnFalling * mul;
                }

                if( chain.ParentHandler.NoGravityOnStanding ) GameRigidbody.useGravity = true;
            }
            else
            {
                if( Joint )
                {
                    if( chain.ConnectedMassOverride ) Joint.connectedMassScale = chain.ConnectedMassScale;
                    else
                        Joint.connectedMassScale = chain.ConnectedMassScale * chain.ParentHandler.ConnectedMassMultiply * mul;
                }

                if( chain.ParentHandler.NoGravityOnStanding ) GameRigidbody.useGravity = false;
            }

            if( ForceKinematicOnStanding )
            {
                if( chain.ParentHandler.AnimatingMode == RagdollHandler.EAnimatingMode.Standing )
                {
                    if( chain.ParentHandler.Caller == null || Time.unscaledTime - chain.ParentHandler.LastStandingModeAtTime > 0.1f )
                    {
                        SwitchIsKinematic( true );
                    }
                    else
                    {
                        // Prevent bones pop to target position immedietely when starting stand up transition - wait 0.15 sec
                        chain.ParentHandler.Caller.StartCoroutine( chain.ParentHandler._IE_CallAfter( 0f,
                            () => { SwitchIsKinematic( true ); },
                            Mathf.RoundToInt( Mathf.Max( 1f, 0.15f / Time.fixedDeltaTime ) ) ) );
                    }

                    _wasForceKinematicOnStanding = true;
                }
                else
                {
                    SwitchIsKinematic( false );
                    RefreshRigidbodyInterpolation( chain.ParentHandler );
                    _wasForceKinematicOnStanding = true;
                }
            }
            else if( _wasForceKinematicOnStanding )
            {
                SwitchIsKinematic( false );
                RefreshRigidbodyInterpolation( chain.ParentHandler );
                _wasForceKinematicOnStanding = false;
            }
        }

        private void SwitchIsKinematic( bool kinematic )
        {
            if( BypassKinematicControl ) return;
            RagdollHandlerUtilities.SwitchKinematic( GameRigidbody, !kinematic );
        }

        public void ConfigureJointAnchors()
        {
            if( Joint.connectedBody == null ) return;

            Joint.autoConfigureConnectedAnchor = false;

            Transform parent;

            if( ParentBone != null ) parent = ParentBone.PhysicalDummyBone;
            else
            {
                ParentBone = ParentChain.ConnectionBone;
                parent = ParentChain.ConnectionBone.PhysicalDummyBone;
            }

            if( InitialJointAnchor == Vector3.zero )
            {

                InitialJointAnchor = Joint.connectedBody.transform.InverseTransformPoint( PhysicalDummyBone.position );
            }

            Vector3 wPos = parent.TransformPoint( InitialJointAnchor );
            Vector3 parentPos = Joint.connectedBody.transform.InverseTransformPoint( wPos );

            Joint.connectedAnchor = parentPos;
        }

        #region Joint Settings Operations

        private void Joint_UpdateAngleLimits( RagdollBonesChain chain )
        {
            if( Joint == null ) return;

            SoftJointLimit limit;
            limit = Joint.lowAngularXLimit;
            limit.limit = GetMainAxisLowLimit( chain );
            limit.contactDistance = GetMainAxisLimitContactDistance( chain );
            limit.bounciness = GetMainAxisLimitBounciness( chain );
            Joint.lowAngularXLimit = limit;

            limit = Joint.highAngularXLimit;
            limit.limit = GetMainAxisHighLimit( chain );
            limit.contactDistance = GetMainAxisLimitContactDistance( chain );
            limit.bounciness = GetMainAxisLimitBounciness( chain );
            Joint.highAngularXLimit = limit;

            limit = Joint.angularYLimit;
            limit.limit = GetSecondaryAxisAngleLimit( chain );
            limit.contactDistance = GetMainAxisLimitContactDistance( chain );
            limit.bounciness = GetMainAxisLimitBounciness( chain );
            Joint.angularYLimit = limit;

            limit = Joint.angularZLimit;
            limit.limit = GetThirdAxisAngleLimit( chain );
            limit.contactDistance = GetMainAxisLimitContactDistance( chain );
            limit.bounciness = GetMainAxisLimitBounciness( chain );
            Joint.angularZLimit = limit;
        }

        /// <summary> Used with animation matching </summary>
        private void Joint_UpdateAngularSpringLimits( RagdollBonesChain chain )
        {
            if( Joint == null ) return;

            float spring = GetMainAxisLimitSpring( chain );
            float damp = GetMainAxisLimitDamper( chain );

            SoftJointLimitSpring limit;
            limit = Joint.angularXLimitSpring;
            limit.spring = spring;
            limit.damper = damp;
            Joint.angularXLimitSpring = limit;

            limit = Joint.angularYZLimitSpring;
            limit.spring = spring;
            limit.damper = damp;
            Joint.angularYZLimitSpring = limit;
        }

        public void Joint_SetMotionLock( ConfigurableJointMotion mode )
        {
            if( Joint == null ) return;
            Joint.xMotion = mode;
            Joint.yMotion = mode;
            Joint.zMotion = mode;
        }

        public void Joint_SetAngularMotionLock( ConfigurableJointMotion mode )
        {
            if( Joint == null ) return;
            Joint.angularXMotion = mode;
            Joint.angularYMotion = mode;
            Joint.angularZMotion = mode;
        }

        public void Joint_SetPositionLimit( float limitValue )
        {
            if( Joint == null ) return;
            var lim = Joint.linearLimit;
            lim.limit = limitValue;
            Joint.linearLimit = lim;
        }

        public void SetJointMatchingParameters( float spring, float dampingValue )
        {
            if( Joint == null ) return;

            if( OverrideSpringPower > 0f ) spring = OverrideSpringPower;
            if( OverrideSpringDamp > 0f ) dampingValue = OverrideSpringDamp;

            var drive = Joint.angularXDrive;
            if( drive.positionSpring == spring && drive.positionDamper == dampingValue ) return; // Nothing changed

            if( spring <= 0f )
            {
                drive.positionSpring = 0f;
                drive.positionDamper = 0f;
            }
            else
            {
                drive.positionSpring = spring;
                drive.positionDamper = dampingValue;
            }

            //Joint.angularXDrive = drive;
            //Joint.angularYZDrive = drive;
            Joint.slerpDrive = drive;
        }

        public void SetJointMatchingParametersPosition( float spring, float dampingValue )
        {
            var drive = Joint.xDrive;
            drive.positionSpring = spring;
            drive.positionDamper = dampingValue;

            Joint.xDrive = drive;
            Joint.yDrive = drive;
            Joint.zDrive = drive;
        }

        public void SetZeroDrive()
        {
            var drive = Joint.angularXDrive;
            drive.positionSpring = 0f;
            drive.positionDamper = 0f;
            //Joint.angularXDrive = drive;
            //Joint.angularYZDrive = drive;
            Joint.slerpDrive = drive;

            var pdrive = Joint.xDrive;
            pdrive.positionSpring = 0f;
            pdrive.positionDamper = 0f;

            Joint.xDrive = pdrive;
            Joint.yDrive = pdrive;
            Joint.zDrive = pdrive;
        }

        public void SetJointMatchingMaximumForce( float maximumForce )
        {
            var drive = Joint.angularXDrive;
            drive.maximumForce = maximumForce;

            //Joint.angularXDrive = drive;
            //Joint.angularYZDrive = drive;
            Joint.slerpDrive = drive;
        }

        #endregion Joint Settings Operations

        [System.Serializable]
        public class InBetweenBone
        {
            public Transform SourceBone;
            public Transform DummyBone;

            [SerializeField] private Quaternion initLocalRotation;
            [SerializeField] private Quaternion animatorLocalRotation;
            public Quaternion InitLocalRotation { get { return initLocalRotation; } }

            internal void Initialize()
            {
                initLocalRotation = SourceBone.localRotation;
                animatorLocalRotation = initLocalRotation;
            }

            internal void AssignParent( Transform setParentIfNoParent )
            {
                if( DummyBone.parent != null ) return;
                DummyBone.SetParent( setParentIfNoParent, true );
                Initialize();
            }

            internal void Calibrate()
            {
                SourceBone.localRotation = initLocalRotation;
            }

            internal void CaptureAnimator()
            {
                animatorLocalRotation = SourceBone.localRotation;
            }

            public void SyncWithAnimator()
            {
                DummyBone.localRotation = animatorLocalRotation;
            }

            internal Rigidbody rigidbody = null;
            internal FixedJoint FixedJoint;

            internal Rigidbody GenerateRigidbody()
            {
                if( rigidbody ) return rigidbody;
                rigidbody = DummyBone.gameObject.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                return rigidbody;
            }

            internal void DestroyPhysicalComponents()
            {
                if( FixedJoint ) RagdollHandlerUtilities.DestroyObject( FixedJoint );
                if( rigidbody ) RagdollHandlerUtilities.DestroyObject( rigidbody );
            }
        }

        #endregion In between bones for lost connections between source skeleton bones parenting hierarchy

        #region Editor Code

#if UNITY_EDITOR

        public bool _EditorCollFoldout = false;
        [NonSerialized] public int _Editor_SelectedCollider = 0;
        public ColliderSetup Editor_SelectedColliderSettings
        { get { if( colliders.Count == 1 ) return colliders[0]; return colliders[_Editor_SelectedCollider]; } }

        public bool _EditorCollPosSliders = false;
        public bool _EditorBoxCollSliders = false;
        public bool _EditorMeshCollSliders = false;
        public bool _EditorRadiusSliders = true;
        public float _EditorMaxSliderValue = 0.5f;
        internal Vector3[] _editor_colliderVerts = new Vector3[0];

        public bool _EditorMainAxisPreview = true;
        public bool _EditorSecondAxisPreview = false;
        public bool _EditorThirdPreview = false;

        [NonSerialized] public bool _EditorPhysicsExtraSettings = false;

#endif

        #endregion Editor Code

        public void TryIdentifyBoneID( RagdollBonesChain chain, bool changeOnlyIfUnknown = false )
        {
            if( changeOnlyIfUnknown ) if( BoneID != ERagdollBoneID.Unknown ) return;

            if( chain.ParentHandler != null )
            {
                var mecanim = chain.ParentHandler.Mecanim;

                if( mecanim && mecanim.isHuman )
                {
                    foreach( var key in Enum.GetValues( typeof( HumanBodyBones ) ) )
                    {
                        if( (int)key < 0 ) continue;
                        HumanBodyBones bone = (HumanBodyBones)key;

                        // Identify by humanoid bones
                        if( SourceBone == mecanim.GetBoneTransform( bone ) )
                        {
                            BoneID = (ERagdollBoneID)bone;
                            return;
                        }
                    }
                }
            }

            // Identify basing on the chain type and index of bone
            int index = chain.GetIndex( this );

            if( chain.ChainType == ERagdollChainType.LeftLeg )
            {
                if( index == 0 ) BoneID = ERagdollBoneID.LeftUpperLeg;
                else if( index == 1 ) BoneID = ERagdollBoneID.LeftLowerLeg;
                else BoneID = ERagdollBoneID.LeftFoot;
            }
            else if( chain.ChainType == ERagdollChainType.RightLeg )
            {
                if( index == 0 ) BoneID = ERagdollBoneID.RightUpperLeg;
                else if( index == 1 ) BoneID = ERagdollBoneID.RightLowerLeg;
                else BoneID = ERagdollBoneID.RightFoot;
            }
            else if( chain.ChainType == ERagdollChainType.LeftArm )
            {
                if( index == 0 ) BoneID = ERagdollBoneID.LeftUpperArm;
                else if( index == 1 ) BoneID = ERagdollBoneID.LeftLowerArm;
                else BoneID = ERagdollBoneID.LeftHand;
            }
            else if( chain.ChainType == ERagdollChainType.RightArm )
            {
                if( index == 0 ) BoneID = ERagdollBoneID.RightUpperArm;
                else if( index == 1 ) BoneID = ERagdollBoneID.RightLowerArm;
                else BoneID = ERagdollBoneID.RightHand;
            }
            else if( chain.ChainType == ERagdollChainType.Core )
            {
                if( index == 0 ) BoneID = ERagdollBoneID.Hips;
                else if( index == chain.BoneSetups.Count - 1 ) BoneID = ERagdollBoneID.Head;
                else if( index == 1 ) BoneID = ERagdollBoneID.Spine;
                else if( index == 2 ) BoneID = ERagdollBoneID.Chest;
                else if( index == 3 ) BoneID = ERagdollBoneID.UpperChest;
            }
        }

        public void TryDoAutoSettings( RagdollHandler handler, RagdollBonesChain chain )
        {
            TryIdentifyBoneID( chain, true );

            if( chain.BoneSetups.Count > 1 ) // Adjust collider size
            {
                BaseColliderSetup.ColliderType = chain.BoneSetups[chain.BoneSetups.Count - 2].BaseColliderSetup.ColliderType;
                int index = chain.GetIndex( this );
                if( index < chain.BoneSetups.Count - 1 )
                {
                    chain.AdjustColliderSettingsBasingOnTheStartEndPosition( this, index, SourceBone.position, chain.GetBone( index + 1 ).SourceBone.position );
                }
            }

            DoAutoMassSettings( handler, chain );
        }

        /// <summary>
        /// Used for scene model preview for axis limits
        /// </summary>
        [System.Serializable]
        public struct ReferencePoseCoordinates
        {
            public Vector3 LocalSpacePosition;
            public Quaternion LocalSpaceRotation;

            public Vector3 RootSpacePosition;
            public Quaternion RootSpaceRotation;
        }

        /// <summary> The setup - reference pose for this bone (it should be T-Pose) </summary>
        public ReferencePoseCoordinates StoredReferencePose;

        #region Override blend implementation

        /// <summary> Override bone animator blend factors </summary>
        [NonSerialized] public float OverrideBlend = 0f;

        private Coroutine _forceBlendCoro = null;
        private float _forceBlendStartOverr = 0f;

        /// <summary> Overriding all blend factors for the bonee and blending it up or down no matter what for a short period of time </summary>
        public void User_ForceOverrideBlendFor( RagdollHandler parentHandler, float duration, float transitionTime = 0.1f, float targetOverrideBlend = 1f )
        {
            if( parentHandler.Caller == null ) return;

            if( _forceBlendCoro != null ) parentHandler.Caller.StopCoroutine( _forceBlendCoro );
            else _forceBlendStartOverr = OverrideBlend;

            _forceBlendCoro = parentHandler.Caller.StartCoroutine( IEForceOverrideBlend( parentHandler, duration, transitionTime, targetOverrideBlend ) );
        }

        public void User_ForceStopOverrideBlend( RagdollHandler parentHandler )
        {
            if( _forceBlendCoro != null ) parentHandler.Caller.StopCoroutine( _forceBlendCoro );
            OverrideBlend = 0f;
        }

        private IEnumerator IEForceOverrideBlend( RagdollHandler parentHandler, float duration, float transitionTime = 0.1f, float targetOverrideBlend = 1f )
        {
            float elapsed = 0f;
            float startBlend = _forceBlendStartOverr;

            while( elapsed < transitionTime )
            {
                elapsed += parentHandler.Delta;
                OverrideBlend = Mathf.Lerp( startBlend, targetOverrideBlend, elapsed / transitionTime );
                yield return null;
            }

            elapsed = 0f;

            while( elapsed < duration )
            {
                elapsed += parentHandler.Delta;
                OverrideBlend = targetOverrideBlend;
                yield return null;
            }

            elapsed = 0f;

            while( elapsed < transitionTime )
            {
                elapsed += parentHandler.Delta;
                OverrideBlend = Mathf.Lerp( targetOverrideBlend, startBlend, elapsed / transitionTime );
                yield return null;
            }

            OverrideBlend = startBlend;

            yield break;
        }

        #endregion Override blend implementation

        /// <summary> Bones detaching cleaning procedure </summary>
        internal void DestroyInBetweenBones( RagdollHandler parent )
        {
            if( InBetweenBones == null ) return;

            foreach( var item in InBetweenBones )
            {
                parent.skeletonFillExtraBonesList.Remove( item );
                if( item.DummyBone == null ) continue;
                RagdollHandlerUtilities.DestroyObject( item.DummyBone.gameObject );
            }

            InBetweenBones.Clear();
            InBetweenBones = null;
        }

        /// <summary>
        /// Calling Physics.IgnoreCollisions between bone colliders
        /// </summary>
        public void IgnoreCollisionsWith( RagdollChainBone otherBone, bool ignore = true )
        {
            foreach( var preC in otherBone.Colliders )
                foreach( var mColl in Colliders )
                    mColl.IgnoreCollisionWith( preC, ignore );
        }

        /// <summary>
        /// Calling Physics.IgnoreCollisions between bone colliders and provided collider
        /// </summary>
        public void IgnoreCollisionsWith( Collider coll, bool ignore = true )
        {
            foreach( var mColl in Colliders )
                mColl.IgnoreCollisionWith( coll, ignore );
        }

        /// <summary>
        /// Making confiugurable joint free in motion like broken joint
        /// </summary>
        public void SetJointFreeMotion()
        {
            Joint_SetAngularMotionLock( ConfigurableJointMotion.Free );
            Joint_SetMotionLock( ConfigurableJointMotion.Free );
            var drive = Joint.slerpDrive;
            drive.positionDamper = 0f;
            drive.positionSpring = 0f;
            Joint.slerpDrive = drive;
        }

        private bool wasPhysicsDisabled = false;
        private bool kinematicOnDisabled = false;

        public void SwitchPhysics( bool enable )
        {
            if( enable == !wasPhysicsDisabled ) return;

            if( enable == false )
            {
                if( GameRigidbody.isKinematic == false )
                {
                    GameRigidbody.velocity = Vector3.zero;
                    GameRigidbody.angularVelocity = Vector3.zero;
                }

                kinematicOnDisabled = GameRigidbody.isKinematic;
                GameRigidbody.isKinematic = true;
            }
            else
            {
                GameRigidbody.isKinematic = kinematicOnDisabled;
            }

            GameRigidbody.detectCollisions = enable;
            if( enable == false ) GameRigidbody.Sleep(); else GameRigidbody.WakeUp();

            foreach( var collider in colliders )
            {
                collider.GameCollider.enabled = enable;
            }

            wasPhysicsDisabled = !enable;
        }

        public void CheckIfShouldIgnoreByBounds( RagdollChainBone otherBone, float boundsSize )
        {
            foreach( var myC in colliders )
            {
                Bounds myBounds = myC.GameCollider.bounds;
                myBounds.size *= boundsSize;

                foreach( var othC in otherBone.colliders )
                {
                    Bounds otherBounds = othC.GameCollider.bounds;
                    otherBounds.size *= boundsSize;

                    if( myBounds.Intersects( otherBounds ) )
                    {
                        IgnoreCollisionsWith( otherBone, true );
                        break;
                    }
                }
            }
        }

        /// <summary> Storing lastest animator pose as calibration pose, useful when disabling mecanim animator </summary>
        public void StoreCalibrationPose()
        {
            BoneProcessor.StoreCalibrationPose();
        }

        /// <summary> Restoting intiial pose as calibration pose, useful when enabling back mecanim animator after disabling it </summary>
        public void RestoreCalibrationPose()
        {
            BoneProcessor.RestoreCalibrationPose();
        }

        internal void SetParentBone( RagdollChainBone parentBone )
        {
            ParentBone = parentBone;
        }
    }
}