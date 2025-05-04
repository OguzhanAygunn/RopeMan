using System;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
        [Tooltip( "Main transform of your character object. You can left it empty to treat this object as base transform. You can use it, when you want to add Ragdoll Animator to the object, which is not your character controller object (for example add it in child objects) then you can assign the character controller object here." )]
        public Transform BaseTransform;

        public Transform GetBaseTransform()
        {
            if( BaseTransform ) return BaseTransform;
            if( parentObject == null ) return null;
            return parentObject.transform;
        }

        [Tooltip( "Animator of the character (optional)" )]
        public Animator Mecanim;

        [Tooltip( "Enter on selected option to display its description as tooltip" )]
        public ERagdollLogic RagdollLogic = ERagdollLogic.ActiveRagdoll;

        [NonSerialized] public Transform HelperOwnerTransform;

        [Tooltip( "Multiplicator value for all of the colliders" )]
        [Range( 0.1f, 2f )] public float RagdollSizeMultiplier = 1f;
        [Tooltip( "Multiplicator value for colliders size excluding bone-forward axis" )]
        [Range( 0.1f, 2f )] public float RagdollThicknessMultiplier = 1f;

        [Tooltip( "Value which is distributed over ragdoll bones rigidbodies as fractional value." )]
        public float ReferenceMass = 50f;

        [Tooltip( "Target rigidbodies interpolation mode." )]
        public RigidbodyInterpolation RigidbodiesInterpolation = RigidbodyInterpolation.Interpolate;

        [Tooltip( "Target rigidbodies collision detection mode." )]
        public CollisionDetectionMode RigidbodiesDetectionMode = CollisionDetectionMode.Discrete;

        [Tooltip( "Reference value for rigidbodies Drag Parameter" )]
        public float RigidbodyDragValue = 0f;

        [Tooltip( "Reference value for rigidbodies Angular Drag Parameter" )]
        public float RigidbodyAngularDragValue = 0.2f;

        [Tooltip( "Reference value for Unity Joints rotation limit parameters. It can make change behaviour of unity physical joints." )]
        public float JointContactDistance = 0f;

        [Tooltip( "Reference value for Unity Joints rotation limit parameters. It can make change behaviour of unity physical joints." )]
        public float JointBounciness = 0f;

        [Tooltip( "Reference value for Unity Joints rotation limit parameters. It can make joint limit ranges softer." )]
        public float JointLimitSpring = 0f;

        [Tooltip( "Reference value for Unity Joints rotation limit parameters. It can make joint limit ranges softer and slower." )]
        public float JointLimitDamper = 0f;

        [Tooltip( "Joint's connected mass multiplier value, can help taming jiggle animation by lowering this value, but if bone chains are long, it can generate glitches when set too low!" )]
        [Range( 0f, 1.5f )]
        public float ConnectedMassMultiply = 0.5f;

        [Tooltip( "Gives better falling animation feeling when set around value = 1" )]
        [Range( 0f, 1.5f )]
        public float MassMultiplyOnFalling = 1f;

        [Tooltip( "Physical Material which will be applied to the generated colliders (not changing if set none)" )]
        public PhysicMaterial CollidersPhysicMaterial;

        [Tooltip( "Physical Material which will be applied to the generated colliders when switched to free fall (not changing on fall if set none)" )]
        public PhysicMaterial PhysicMaterialOnFall;

        [Tooltip( "(set zero to use deafault project value)\nIf you want to use 'Hard Matching' it's recommended to set this value higher." )]
        public float MaxAngularVelocity = 50f;

        [Tooltip( "(set zero to use deafault project value)\nUse if you want to limit max force applied on the ragdoll bones. If in your game you use extreme forces on the bones, this can help you keep bone impacts more stable." )]
        public float MaxVelocity = 0f;

        [Tooltip( "Disabling gravity on standing mode sometimes can give more stable motion." )]
        public bool NoGravityOnStanding = false;

        [Tooltip( "(set zero to use deafault project value)\nIt should make overlapping colliders push-out move smoother." )]
        public float MaxDepenetrationVelocity = 0f;

        [Tooltip( "Switching unity joint 'ProjectionMode' parameter for all ragdoll rigidbodies" )]
        public JointProjectionMode ProjectionMode = JointProjectionMode.None;

        [Tooltip( "Switching unity joint 'UsePreProcessing' parameter for all ragdoll rigidbodies" )]
        public bool PreProcessing = false;

        [Tooltip( "Blend of applied physical pose on the main character skeleton." )]
        [FPD_Suffix( 0f, 1f )] public float RagdollBlend = 1f;

        public enum EAnimatingMode
        {
            [Tooltip( "Turning off ragdoll animator calculations and turning off physical dummy so it will not react with physical objects on the scene.\nDoes the same thing as setting ragdollAnimator.enabled = false" )]
            Off,

            [Tooltip( "Ragdoll animator mode for full body animation matching, but attached with its main bone to the animated character pose." )]
            Standing,

            [Tooltip( "Unlocked main physical bone and letting it fall on the ground with the rest of the body." )]
            Falling,

            [Tooltip( "Ragdoll will fall on the ground and turn itself off (and set kinematic) when dummy stops falling and moving." )]
            Sleep,
        }

        [Tooltip( "Type of main behaviour for the component. Check tooltips of each state for description." )]
        [SerializeField] protected EAnimatingMode animatingMode = EAnimatingMode.Standing;

        private bool animatingModeChanged = false;

        public EAnimatingMode AnimatingMode
        {
            get { return animatingMode; }
            set { if( value == animatingMode ) return; animatingMode = value; animatingModeChanged = true; OnAnimatingModeChange(); }
        }

        public bool IsInStandingMode => AnimatingMode == EAnimatingMode.Standing && ( !FallOnZeroAnchor || ( FallOnZeroAnchor && AnchorBoneSpring > 0f ) );
        public bool IsInFallingMode => AnimatingMode != EAnimatingMode.Standing || ( FallOnZeroAnchor && AnchorBoneSpring <= 0f );
        public bool IsFallingOrSleep => IsInFallingMode || AnimatingMode == EAnimatingMode.Sleep;

        [Tooltip( "Can increase precision of animation matching.\nTurning on using extra joints on chain-skipped bones and dummy bones without direct connection with parent bones. It will make physics cost a bit more and can generate GC when switching AnimatingState from 'Standing' mode to other." )]
        public bool UseReconstruction = false;

        [Range( 1, 24 )]
        [Tooltip( "Quality of unity physical iterations. Unity recommends value between 10-20 for ragdolls. Don't increase it too high if you use many ragdolls." )] public int UnitySolverIterations = 12;

        [Tooltip( "It needs to be enabled if your character has no animations, otherwise character libs will fall on without muscles power.\nIf your character is animated all the time, turn it off to save some performance." )]
        public bool Calibrate = true;

        [Tooltip( "If your animator have enabled 'AnimatePhysics' update mode, you should enable it here too (switches automatically if having assigned 'Mecanim' field)" )]
        public bool AnimatePhysics = false;

        [Tooltip( "If joints position changes in case of hard collisions, should the positions be applied also to the animator bones?" )]
        public bool ApplyPositions = false;

        [Tooltip( "Turning off all calculations when ragdoll blend is set to zero. It can cause jiggle when blend is greater than zero again." )]
        public bool OptimizeOnZeroBlend = false;

        [Tooltip( "If generated ragdoll dummy object should not be visible in the scene view to make scene hierarchy cleaner" )]
        public bool HideDummyInSceneView = false;

        [Tooltip( "Checking ragdoll dummy bones colliders bounds and ignoring collisions between ones which are overlapping" )]
        public bool IgnoreBoundedColliders = true;

        [Tooltip( "Target layer for the generated ragdoll dummy object" )]
        [FPD_Layers] public int RagdollDummyLayer = 0;

        [Tooltip( "Enable if you want to update character ragdoll animation in unscaled delta time (unaffected by Time.scale)" )]
        public bool UnscaledTime = false;

        [Tooltip( "Making ragdoll initialization / re-enabling, without model jiggle. (slider = fade time in seconds)\nIt is not triggered when switching back from Sleep animating mode" )]
        [Range( 0f, 1f )] public float FadeInAnimation = 0f;

        [Tooltip( "Triggering Physics.IgnoreCollision between dummy colliders and on all colliders found in the source skeleton" )]
        public bool IgnoreSourceSkeletonColliders = true;

        [Tooltip( "To make default animation matching be precise, Ragdoll Animator needs to wait few fixed update frames, but if you use just hard matching settings, you can disable it to make first frames of ragdoll animator quicker." )]
        public bool WaitForInit = true;

        [Tooltip( "If your animations are exceeding the physical joints rotation limits, making animation not possible to reach target pose when physics are ON, you can allow joints to rotate regardless the angle limits to improve animation matching a bit. So limits should be on when falling mode to prevent rotating joints in weird angles." )]
        public ERagdollNoLimitAngles AnimationMatchLimits = ERagdollNoLimitAngles.NoLimitsOnStandingMode;

        public enum ERagdollNoLimitAngles
        { AllLimits, NoLimitsOnStandingMode, NoLimits }

        [Tooltip( "Generated ragdoll dummy will be put inside this transform as child object.\n\nAssign main character object for ragdoll to react with character movement rigidbody motion, set other for no motion reaction." )]
        public Transform TargetParentForRagdollDummy;

        [HideInInspector] public float BoundedCollidersIgnoreScaleup = 1.2f;

        [Range( 1, 6 )]
        [Tooltip( "Quality of unity rigidbody velocity iterations. 1 is default for unity projects." )] public int UnityVelocitySolverIterations = 0;

        [Tooltip("If sleep mode should automatically disable mecanim unity animator on Ragdoll Animator disable during sleep mode.")]
        public bool DisableMecanimOnSleep = true;
    }
}