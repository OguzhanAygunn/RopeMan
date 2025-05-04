#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace FIMSpace.FProceduralAnimation
{
    [AddComponentMenu("FImpossible Creations/Ragdoll Animator/Ragdoll Attachable (Equipable)", 111)]
    public class RA2AttachableObject : MonoBehaviour
    {
        [Space(2)]
        [HideInInspector] public bool ChangeLocalCoords = true;

        [HideInInspector] public Vector3 TargetLocalPosition = Vector3.zero;
        [HideInInspector] public Vector3 TargetLocalRotation = Vector3.zero;

        [Space(5)]
        [Tooltip("If collider should be present on the animator and on the physical dummy")]
        public bool KeepColliderOnAnimator = false;

        [Tooltip("Changing attachable object layer to be same as animator bones and dummmy bones layers")]
        public bool ChangeObjectLayer = true;

        [FPD_SingleLineTwoProps("DetectCollisions")]
        [Tooltip("Add collision indicator component to this model attached on the source animator bone and on the generated physics object")]
        public bool AddCollisionIndicators = true;
        [Tooltip("Adding collision detector component. To use it, you need to call myAttachable.AddEventToCallOnCollision()")]
        [HideInInspector] public bool DetectCollisions = false;

        [Space(5)]
        public List<Collider> AttachableColliders = new List<Collider>();
        [Tooltip("Optional reference to item source rigidbody")]
        public Rigidbody OptionalRigidbody;

        [Space(5)]
        [Tooltip("Set mass above zero, to generate fixed joint connection between attachable item and attachement bone, affecting weight putted on the bone.")]
        public float Mass = 0f;

        [HideInInspector]
        [Tooltip("Making connected mass multiplier lower, will produce lighter motion for the item.")]
        [Range(0f, 1f)] public float ConnectedMassMultiplier = 0.25f;
        [HideInInspector]
        [Range(0f, 5f)] public float MassScale = 1.5f;

        [HideInInspector] public int IgnoreChainsCollisions = 0;

        [Tooltip("Making item more stiff but hold more precisely.")]
        [HideInInspector][Range(0f, 1f)] public float HardMatching = 0f;
        [Tooltip("Making hard matching less powerful when item gets pushed away from the desired coordinates.")]
        [HideInInspector][Range(0f, 1f)] public float SoftLimit = 0f;

        public RagdollHandler AttachedTo { get; private set; }
        public RagdollChainBone AttachedToBone { get; private set; }
        public GameObject GeneratedPhysicsObject { get; private set; }
        public List<Collider> GeneratedPhysicsColliders { get; private set; }

        public Rigidbody lastRigidbody { get; private set; }
        public FixedJoint lastJoint { get; private set; }

        bool wasOriginalRigidbodyKinematic = false;
        Vector3 unwearVelocity = Vector3.zero;
        Vector3 unwearAngularVelocity = Vector3.zero;

        private void Reset()
        {
            GatherAllChildColliders();
            OptionalRigidbody = GetComponentInChildren<Rigidbody>();
            if (OptionalRigidbody) Mass = OptionalRigidbody.mass;
        }

        public void GatherAllChildColliders()
        {
            List<Collider> alloc = new List<Collider>();
            foreach (var t in gameObject.GetComponentsInChildren<Transform>())
            {
                alloc.Clear();
                t.GetComponents<Collider>(alloc);
                foreach (var c in alloc) if (AttachableColliders.Contains(c) == false) AttachableColliders.Add(c);
            }
        }

        public void GetCurrentLocalCoords()
        {
            TargetLocalPosition = transform.localPosition;
            TargetLocalRotation = transform.localRotation.eulerAngles;
        }


        /// <summary> Initial attachement operations </summary>
        internal void OnStartAttachingToRagdoll(RagdollHandler ragdollHandler, RagdollChainBone dummyBone)
        {
            if (OptionalRigidbody)
            {
                wasOriginalRigidbodyKinematic = OptionalRigidbody.isKinematic;
                OptionalRigidbody.isKinematic = true;
                OptionalRigidbody.detectCollisions = false;
            }
        }

        /// <summary> When ragdoll animator completes attaching operations </summary>
        public void OnAttachToRagdoll(GameObject root, RagdollHandler ragdoll, RagdollChainBone bone, List<Collider> colliders)
        {
            AttachedTo = ragdoll;
            AttachedToBone = bone;
            GeneratedPhysicsColliders = colliders;
            if (GeneratedPhysicsObject != null && GeneratedPhysicsObject != root) GameObject.Destroy(GeneratedPhysicsObject);
            GeneratedPhysicsObject = root;

            if (DetectCollisions)
            {
                collisionsDetector = GeneratedPhysicsObject.AddComponent<AttachableCollisionDetector>();
                collisionsDetector.Parent = this;
            }

            // Ignore selected chains collisions
            IgnoreChainCollisionsWith(ragdoll, true);
        }

        public void RemoveFromCurrentDummy()
        {
            if (lastRigidbody)
            {
                unwearVelocity = lastRigidbody.velocity;
                unwearAngularVelocity = lastRigidbody.angularVelocity;
            }

            if (GeneratedPhysicsObject) GameObject.Destroy(GeneratedPhysicsObject);
            collisionsDetector = null;
            GeneratedPhysicsColliders = null;
            if (AttachedTo != null) IgnoreChainCollisionsWith(AttachedTo, false);
            AttachedTo = null;
            AttachedToBone = null;

            if (OptionalRigidbody)
            {
                OptionalRigidbody.isKinematic = wasOriginalRigidbodyKinematic;
                OptionalRigidbody.detectCollisions = true;

                if (OptionalRigidbody.isKinematic == false)
                {
                    StartCoroutine(IECallAfterFixedFrame(() =>
                    {
                        OptionalRigidbody.velocity = unwearVelocity;
                        OptionalRigidbody.angularVelocity = unwearAngularVelocity;
                    }));
                }
            }

        }

        IEnumerator IECallAfterFixedFrame(Action action)
        {
            yield return new WaitForFixedUpdate();
            action.Invoke();
        }

        /// <summary> Using chain selector mask to ignore ragdoll collisions </summary>
        void IgnoreChainCollisionsWith(RagdollHandler ragdoll, bool ignore)
        {
            ERagdollChainType chainMask = (ERagdollChainType)IgnoreChainsCollisions;

            foreach (var chain in ragdoll.Chains)
            {
                if ((chainMask & chain.ChainType) != 0)
                {
                    foreach (var sColl in AttachableColliders) chain.IgnoreCollisionsWith(sColl, ignore);
                    if (GeneratedPhysicsColliders != null) foreach (var sColl in GeneratedPhysicsColliders) chain.IgnoreCollisionsWith(sColl, ignore);
                }
            }
        }

        /// <summary> Applying relevant rotation of physics object's local space rotation </summary>
        internal void UpdateOnRagdoll()
        {
            transform.localPosition = GeneratedPhysicsObject.transform.localPosition;
            transform.localRotation = GeneratedPhysicsObject.transform.localRotation;
        }

        /// <summary> Called from ragdoll handler </summary>
        internal void FixedUpdateTick()
        {
            if (HardMatching <= 0f) return;
            if (lastRigidbody == null) return;

            Vector3 rigidbodyRelevantPosition = AttachedToBone.BoneProcessor.AnimatorPosition;
            rigidbodyRelevantPosition += (AttachedToBone.BoneProcessor.AnimatorRotation * Quaternion.Euler(TargetLocalRotation)) * (lastRigidbody.centerOfMass + TargetLocalPosition);

            float mul = 1f;

            if (SoftLimit > 0f)
            {
                float diff = (rigidbodyRelevantPosition - lastRigidbody.worldCenterOfMass).sqrMagnitude;
                mul = 1f / ((diff * SoftLimit * 50f) + 1f);
            }

            RagdollHandlerUtilities.AddRigidbodyForceToMoveTowards(lastRigidbody, rigidbodyRelevantPosition, HardMatching * mul);

        }

        /// <summary>
        /// Can be used for custom rigidbody and joint handling
        /// </summary>
        internal virtual void OnGeneratePhysicsComponents(Rigidbody rig, FixedJoint joint)
        {
            lastRigidbody = rig;
            lastJoint = joint;
        }

        #region Attachables collision detection implementation

        AttachableCollisionDetector collisionsDetector = null;

        List<Action<Collision>> CollisionEvents = null;
        List<Action<Collision>> CollisionExitEvents = null;
        public void AddEventToCallOnCollision(Action<Collision> action)
        {
            if (CollisionEvents == null) CollisionEvents = new List<Action<Collision>>();
            if (CollisionEvents.Contains(action)) return;
            CollisionEvents.Add(action);
        }
        public void RemoveEventToCallOnCollision(Action<Collision> action)
        {
            if (CollisionEvents == null) return;
            if (CollisionEvents.Contains(action) == false) return;
            CollisionEvents.Remove(action);
        }
        public void AddEventToCallOnCollisionExit(Action<Collision> action)
        {
            if (CollisionExitEvents == null) CollisionExitEvents = new List<Action<Collision>>();
            if (CollisionExitEvents.Contains(action)) return;
            CollisionExitEvents.Add(action);
        }
        public void RemoveEventToCallOnCollisionExit(Action<Collision> action)
        {
            if (CollisionExitEvents == null) return;
            if (CollisionExitEvents.Contains(action) == false) return;
            CollisionExitEvents.Remove(action);
        }

        void CallOnCollisionEnter(Collision collision)
        {
            if (CollisionEvents == null) return;
            for (int i = 0; i < CollisionEvents.Count; i++) CollisionEvents[i].Invoke(collision);
        }

        void CallOnCollisionExit(Collision collision)
        {
            if (CollisionExitEvents == null) return;
            for (int i = 0; i < CollisionExitEvents.Count; i++) CollisionExitEvents[i].Invoke(collision);
        }

        class AttachableCollisionDetector : MonoBehaviour
        {
            public RA2AttachableObject Parent;

            private void OnCollisionEnter(Collision collision)
            {
                if (Parent == null) return;
                Parent.CallOnCollisionEnter(collision);
            }

            private void OnCollisionExit(Collision collision)
            {
                if (Parent == null) return;
                Parent.CallOnCollisionExit(collision);
            }
        }

        #endregion


        #region Editor Code

#if UNITY_EDITOR

        private bool gizmoDisableCalled = false;

        private void OnValidate()
        {
            if (!gizmoDisableCalled) { FSceneIcons.SetGizmoIconEnabled(this, false); gizmoDisableCalled = true; }

            if (AttachedTo != null && AttachedTo.WasInitialized)
            {
                if (lastRigidbody) lastRigidbody.mass = Mass;
                if (lastJoint)
                {
                    lastJoint.connectedMassScale = ConnectedMassMultiplier;
                    lastJoint.massScale = MassScale;
                }
            }

            //if (DetectCollisions)
            //{
            //    AddCollisionIndicators = true;
            //    UnityEditor.EditorUtility.SetDirty(this);
            //}
        }

        [CanEditMultipleObjects]
        [CustomEditor(typeof(RA2AttachableObject), true)]
        public class RA2AttachObjectEditor : Editor
        {
            public RA2AttachableObject Get
            { get { if (_get == null) _get = (RA2AttachableObject)target; return _get; } }
            private RA2AttachableObject _get;

            protected string HeaderInfo => "Setup for object to attach on the Ragdoll Animator as physical part of the body.";

            private SerializedProperty sp_ChangeLocalCoords;
            private SerializedProperty sp_Mass;

            private string[] ignores = new string[] { "m_Script" };

            private void OnEnable()
            {
                sp_ChangeLocalCoords = serializedObject.FindProperty("ChangeLocalCoords");
                sp_Mass = serializedObject.FindProperty("Mass");

                //if (Application.isPlaying)
                //{
                //    ignores = new string[] { "m_Script", "AddCollisionIndicators" };
                //}
            }

            public override void OnInspectorGUI()
            {
                GUILayout.Space(2);
                UnityEditor.EditorGUILayout.HelpBox(HeaderInfo, UnityEditor.MessageType.None);

                serializedObject.Update();

                EditorGUILayout.PropertyField(sp_ChangeLocalCoords);

                if (Get.ChangeLocalCoords)
                {
                    var sp = sp_ChangeLocalCoords.Copy(); sp.Next(false);
                    EditorGUILayout.PropertyField(sp); sp.Next(false);
                    EditorGUILayout.PropertyField(sp);

                    if (Get.transform.parent != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10);

                        if (GUILayout.Button("Read Current Local Coords"))
                        {
                            Get.GetCurrentLocalCoords();
                        }

                        GUILayout.Space(10);
                        EditorGUILayout.EndHorizontal();
                    }
                }

                DrawPropertiesExcluding(serializedObject, ignores);

                if (Get.Mass > 0f)
                {
                    GUILayout.Space(4);

                    var sp = sp_Mass.Copy();
                    sp.Next(false);
                    sp.Next(false);
                    sp.Next(false);
                    sp.Next(false);
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(sp, new GUIContent(" Hard Matching:", EditorGUIUtility.IconContent("Animator Icon").image, sp.tooltip), true);
                    EditorGUILayout.EndHorizontal();

                    if (Get.HardMatching > 0f)
                    {
                        EditorGUI.indentLevel++;
                        sp.Next(false);
                        EditorGUILayout.PropertyField(sp, true);
                        EditorGUI.indentLevel--;
                    }
                }

                GUILayout.Space(4);
                EditorGUIUtility.labelWidth = 200;
                ERagdollChainType chTypes = (ERagdollChainType)Get.IgnoreChainsCollisions;
                chTypes = (ERagdollChainType)EditorGUILayout.EnumFlagsField("Ignore Collisions With Chains:", chTypes);
                Get.IgnoreChainsCollisions = (int)chTypes;
                EditorGUIUtility.labelWidth = 0;
                GUILayout.Space(4);

                if (Get.Mass < 0f) Get.Mass = 0f;
                if (Get.Mass > 0f)
                {
                    EditorGUI.indentLevel++;
                    var sp = sp_Mass.Copy(); sp.Next(false);
                    EditorGUILayout.PropertyField(sp); sp.Next(false);
                    EditorGUILayout.PropertyField(sp); sp.Next(false);
                    EditorGUI.indentLevel--;
                }

                GUILayout.Space(5);

                if (Application.isPlaying == false && Get.ChangeLocalCoords)
                {
                    EditorGUILayout.LabelField("Attaching helper for Local Coords setup", EditorStyles.centeredGreyMiniLabel);
                    EditorGUILayout.BeginHorizontal();
                    Get.helperRagdoll = EditorGUILayout.ObjectField(Get.helperRagdoll, typeof(RagdollAnimator2), true) as RagdollAnimator2;
                    if (Get.helperRagdoll) RagdollHandler.Editor_RagdollBonesSelector(Get.helperRagdoll.gameObject, (Transform t) => { Get.helperTransform = t; }, Get.helperTransform);

                    Get.helperTransform = EditorGUILayout.ObjectField(Get.helperTransform, typeof(Transform), true) as Transform;
                    EditorGUILayout.EndHorizontal();

                    if (Get.helperRagdoll && Get.helperTransform)
                    {
                        if (Get.transform.parent == Get.helperTransform)
                        {
                            if (GUILayout.Button("Detach")) { Get.transform.parent = null; }
                            //if( GUILayout.Button( "Read current coords" ) ) { Get.GetCurrentLocalCoords(); }
                        }
                        else
                        {
                            if (GUILayout.Button("Attach"))
                            {
                                Get.transform.parent = Get.helperTransform;
                                Get.transform.localPosition = Get.TargetLocalPosition;
                                Get.transform.localRotation = Quaternion.Euler(Get.TargetLocalRotation);
                            }
                        }
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }
        }

        private RagdollAnimator2 helperRagdoll;
        private Transform helperTransform;

#endif

        #endregion Editor Code

    }
}