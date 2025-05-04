using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
        /// <summary> List of all attachables on the ragdoll dummy </summary>
        protected List<RA2AttachableObject> attachables = new List<RA2AttachableObject>();
        /// <summary> List of all attachables on the ragdoll dummy. Use WearAttachable / UnwearAttachable to change this list contains. </summary>
        public List<RA2AttachableObject> Attachables { get { return attachables; } }


        /// <summary> If this ragdoll is equipped with certain attachable object. </summary>
        public bool IsWearingAttachable( RA2AttachableObject attachable )
        {
            if( attachable == null ) return false;
            return attachables.Contains( attachable );
        }


        /// <summary> Assigning attachable object to the ragdoll dummy, generating its physics and handling collisions ignoring. </summary>
        public void WearAttachable( RA2AttachableObject attachable, Transform targetAnimatorBone )
        {
            if( attachable == null ) return;
            if( targetAnimatorBone == null ) return;

            if( ContainsAnimatorBoneTransform( targetAnimatorBone ) == false )
            {
                UnityEngine.Debug.Log( "[Ragdoll Animator 2] Ragdoll Dummy is not built with " + targetAnimatorBone.name + " source bone!\nAdd it in the Ragdoll Construct first." );
                return;
            }

            var dummyBone = DictionaryGetBoneSetupBySourceBone( targetAnimatorBone );
            if( dummyBone == null ) return;

            foreach( var coll in attachable.AttachableColliders ) IgnoreCollisionWith( coll ); // Skeleton collider should be ignored

            attachable.OnStartAttachingToRagdoll( this, dummyBone );

            attachable.transform.position = targetAnimatorBone.TransformPoint( attachable.TargetLocalPosition );
            attachable.transform.rotation = targetAnimatorBone.rotation * Quaternion.Euler( attachable.TargetLocalRotation );

            Transform physicsObject = AttachableGeneratePhysicsOn( attachable, dummyBone );
            var chain = GetChain( dummyBone );

            if( attachable.KeepColliderOnAnimator == false )
                foreach( var coll in attachable.AttachableColliders )
                    coll.enabled = false;

            attachable.transform.SetParent( targetAnimatorBone, true );

            if( attachable.AddCollisionIndicators )
            {
                attachable.gameObject.AddComponent<RagdollAnimator2BoneIndicator>().Initialize( this, dummyBone.BoneProcessor, chain, true, attachable );
                physicsObject.gameObject.AddComponent<RagdollAnimator2BoneIndicator>().Initialize( this, dummyBone.BoneProcessor, chain, false, attachable );
            }

            attachables.Add( attachable );
        }


        /// <summary> Helper dictionary to avoid generating duplicates of source attachable transforms </summary>
        private Dictionary<Transform, Transform> _helperAttachableGeneratingDictionary = null;


        /// <summary> Generating attachable item object physics, which will be parented with physical ragdoll dummy.
        /// Operation is including generating new game objects, physical dummy and skeleton bones collisions ignoring and copying settings of the ragdoll handler. </summary>
        private Transform AttachableGeneratePhysicsOn( RA2AttachableObject attachable, RagdollChainBone dummyBone )
        {
            // Generate root object for attachable object physics
            GameObject physicsObjectParent = new GameObject( attachable.name + ":Attachable Physics" );
            physicsObjectParent.transform.position = attachable.transform.position;
            physicsObjectParent.transform.rotation = attachable.transform.rotation;
            physicsObjectParent.transform.localScale = attachable.transform.lossyScale;
            physicsObjectParent.layer = attachable.ChangeObjectLayer ? RagdollDummyLayer : attachable.gameObject.layer;

            List<Collider> generatedColliders = new List<Collider>();

            foreach( var coll in attachable.AttachableColliders ) // Iterate all attachable object colliders
            {
                if( coll.transform == attachable.transform ) // I collider component's transform is the main transform of attachable
                {
                    // Generate physical object collision
                    var nColl = physicsObjectParent.AddComponent( coll.GetType() ) as Collider;
                    RagdollBonesChain.CopyColliderSettingTo( coll, nColl );
                    generatedColliders.Add( nColl );
                }
                else // Find or generate new transform, equivalent to the source attachable object hierarchy
                {
                    if( _helperAttachableGeneratingDictionary == null ) _helperAttachableGeneratingDictionary = new Dictionary<Transform, Transform>();

                    Transform targetParent;
                    _helperAttachableGeneratingDictionary.TryGetValue(coll.transform, out targetParent);

                    if (targetParent == null)
                    {
                        GameObject rootChild = new GameObject(coll.name + ":Attachable Physics");
                        rootChild.layer = attachable.ChangeObjectLayer ? RagdollDummyLayer : coll.gameObject.layer;
                        targetParent = rootChild.transform;

                        targetParent.localScale = coll.transform.lossyScale;
                        targetParent.SetParent(physicsObjectParent.transform, true);
                        targetParent.position = coll.transform.position;
                        targetParent.rotation = coll.transform.rotation;

                        if (_helperAttachableGeneratingDictionary.ContainsKey(coll.transform))
                            _helperAttachableGeneratingDictionary[coll.transform] = targetParent;
                        else
                            _helperAttachableGeneratingDictionary.Add(coll.transform, targetParent);
                    }

                    // Generate physical object collision
                    var nColl = targetParent.gameObject.AddComponent( coll.GetType() ) as Collider;
                    generatedColliders.Add( nColl );
                    RagdollBonesChain.CopyColliderSettingTo( coll, nColl );
                }
            }

            Physics.SyncTransforms(); // Ensure everything is on place physically (colliders bounds update)

            foreach( var coll in generatedColliders ) // Ignore collisions between self bones in range
                IgnoreCollisionWithUsingBounds( coll, 1.25f );

            foreach( var coll in generatedColliders ) // Ignore generated physics and base attachable model collisions
                foreach( var sColl in attachable.AttachableColliders )
                    Physics.IgnoreCollision( coll, sColl, true );

            // Trigger attach info
            attachable.OnAttachToRagdoll( physicsObjectParent, this, dummyBone, generatedColliders );

            // Move physical object to the dummy bone
            physicsObjectParent.transform.SetParent( dummyBone.PhysicalDummyBone, true );

            // Ensure the right coords
            physicsObjectParent.transform.localPosition = attachable.TargetLocalPosition;
            physicsObjectParent.transform.localRotation = Quaternion.Euler( attachable.TargetLocalRotation );

            // Include rigidbody physics if enabled
            if( attachable.Mass > 0f )
            {
                var rig = physicsObjectParent.AddComponent<Rigidbody>();
                rig.interpolation = RigidbodiesInterpolation;
                rig.collisionDetectionMode = RigidbodiesDetectionMode;
                rig.mass = attachable.Mass;

                FixedJoint joint = null;

                joint = physicsObjectParent.AddComponent<FixedJoint>();
                joint.connectedBody = dummyBone.GameRigidbody;
                joint.connectedMassScale = attachable.ConnectedMassMultiplier;
                joint.massScale = attachable.MassScale;

                attachable.OnGeneratePhysicsComponents( rig, joint );
            }

            return physicsObjectParent.transform;
        }


        /// <summary> Removing attachable object from the ragdoll dummy, restoring its collisions ignore settings and destroying the extra generated physics object. </summary>
        public void UnwearAttachable( RA2AttachableObject attachable )
        {
            if( IsWearingAttachable( attachable ) == false ) return;

            attachable.transform.SetParent( null, true );
            attachable.RemoveFromCurrentDummy();

            if( attachable.KeepColliderOnAnimator == false )
                foreach( var coll in attachable.AttachableColliders )
                    coll.enabled = true;

            foreach( var coll in attachable.AttachableColliders ) IgnoreCollisionWith( coll, false ); // Restore ignores

            RagdollAnimator2BoneIndicator indic = attachable.GetComponent<RagdollAnimator2BoneIndicator>();
            if( indic ) GameObject.Destroy( indic );

            attachables.Remove( attachable );
        }


        /// <summary> When removing bone from the initialized dummy, there is need to remove it from multiple lists/dictionaries etc. </summary>
        public void RemoveBoneFromRuntimeCalculations( RagdollChainBone b )
        {
            allBonesList.Remove( b.SourceBone );
            allBonesList.Remove( b.PhysicalDummyBone );
            animatorTransformBoneDictionary.Remove( b.SourceBone );
            nameTransformBoneDictionary.Remove( b.SourceBone.name );
            physicalTransformBoneDictionary.Remove( b.PhysicalDummyBone );
        }


        /// <summary> Useful only when restoring bone after dismemberement </summary>
        public void RestoreBoneToRuntimeCalculations( RagdollChainBone b )
        {
            if( allBonesList.Contains( b.SourceBone ) == false ) allBonesList.Add( b.SourceBone );
            if( allBonesList.Contains( b.PhysicalDummyBone ) == false ) allBonesList.Add( b.PhysicalDummyBone );
            if( animatorTransformBoneDictionary.ContainsKey( b.SourceBone ) == false ) animatorTransformBoneDictionary.Add( b.SourceBone, b );
            if( nameTransformBoneDictionary.ContainsKey( b.SourceBone.name ) == false ) nameTransformBoneDictionary.Add( b.SourceBone.name, b );
            if( physicalTransformBoneDictionary.ContainsKey( b.PhysicalDummyBone ) == false ) physicalTransformBoneDictionary.Add( b.PhysicalDummyBone, b );
        }


        /// <summary> Update local coords of attachable objects transforms </summary>
        internal void UpdateAttachables()
        {
            foreach( var attachable in attachables )
            {
                attachable.UpdateOnRagdoll();
            }
        }

        /// <summary> Update hard matching attachbles if enabled </summary>
        internal void FixedUpdateAttachables()
        {
            foreach( var attachable in attachables )
            {
                attachable.FixedUpdateTick();
            }
        }
    }
}