using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static partial class RagdollHandlerUtilities
    {
        /// <summary> Optional utility to call, only if you need colliders on the character bones for custom use, without using Ragdoll Animator 2 itself </summary>
        public static void AddCollidersOnTheCharacterBones( RagdollHandler handler )
        {
            bool fall = handler.IsFallingOrSleep;

            foreach( var chain in handler.Chains )
                foreach( var bone in chain.BoneSetups )
                    bone.RefreshCollider( chain, fall, true );
        }

        /// <summary> Optional utility to call, only if you need joints / rigidbodies on the character bones for custom use, without using Ragdoll Animator 2 itself </summary>
        public static void AddPhysicsComponentsOnTheCharacterBones( RagdollHandler handler )
        {
            bool fall = handler.IsFallingOrSleep;

            foreach( var chain in handler.Chains )
                foreach( var bone in chain.BoneSetups )
                {
                    bone.RefreshJoint( chain, fall, true, false );
                    bone.RefreshRigidbody( handler, chain, true );
                }

            var anchor = handler.GetChain( ERagdollChainType.Core ).BoneSetups[0].SourceBone;
            var anchorJoint = anchor.GetComponent<ConfigurableJoint>();
            RagdollHandler.SetConfigurableJointMotionLock( anchorJoint, ConfigurableJointMotion.Free );
            RagdollHandler.SetConfigurableJointAngularMotionLock( anchorJoint, ConfigurableJointMotion.Free );

            foreach( var chain in handler.Chains )
                foreach( var bone in chain.BoneSetups )
                {
                    if( bone.SourceBone == anchor ) continue;
                    ConfigurableJoint joint = bone.SourceBone.GetComponent<ConfigurableJoint>();
                    if( joint == null ) continue;

                    // Find parent rigidbody

                    Rigidbody pRig = null;
                    Transform parent = bone.SourceBone.parent;

                    while( parent != null && parent != anchor.parent )
                    {
                        pRig = parent.GetComponent<Rigidbody>();
                        if( pRig ) break;
                        parent = parent.parent;
                    }

                    if( pRig ) joint.connectedBody = pRig;
                }
        }

        /// <summary> Searching all source skeleton bones for joints and rigidbodies, to clear base skeleton out of old ragdoll </summary>
        public static void FindAndRemoveJointAndRigidbodyComponentsOnTheCharacterBones( RagdollHandler handler, bool log = false )
        {
            int removed = 0;
            foreach( var chain in handler.Chains )
                foreach( var bone in chain.BoneSetups )
                {
                    var joint = bone.SourceBone.GetComponent<Joint>();
                    if( joint ) { DestroyObject( joint ); removed += 1; }

                    var rigb = bone.SourceBone.GetComponent<Rigidbody>();
                    if( rigb ) { DestroyObject( rigb ); removed += 1; }
                }

            if( log )
            {
                if( removed == 0 ) Debug.Log( "[Ragdoll Animator 2] Not found any joint or rigidbody to remove." );
                else Debug.Log( "[Ragdoll Animator 2] Removed " + removed + " components on the source skeleton." );
            }
        }

        /// <summary> Searching all source skeleton bones for joints, colliders and rigidbodies, to clear base skeleton out of old ragdoll </summary>
        public static void FindAndRemoveAllPhysicalComponentsOnTheCharacterBones( RagdollHandler handler, bool log = false )
        {
            int removed = 0;
            foreach( var chain in handler.Chains )
                foreach( var bone in chain.BoneSetups )
                {
                    var coll = bone.SourceBone.GetComponent<Collider>();
                    if( coll ) { DestroyObject( coll ); removed += 1; }

                    coll = bone.SourceBone.GetComponentInChildren<Collider>();
                    if( coll ) { DestroyObject( coll ); removed += 1; }
                }

            if( log )
            {
                if( removed == 0 ) Debug.Log( "[Ragdoll Animator 2] Not found any skeleton collider to remove." );
                else Debug.Log( "[Ragdoll Animator 2] Removed " + removed + " colliders on the source skeleton." );
            }

            FindAndRemoveJointAndRigidbodyComponentsOnTheCharacterBones( handler );
        }

        /// <summary> Checking all defined bones for colliders attached to them, and assigning them as target colliders for phyiscal dummy bones if found </summary>
        public static void FindBonesCollidersInSourceBonesAndAssignAsReferenceCollidersIfFound( RagdollHandler handler, bool setAsOther, bool log = false )
        {
            int foundCount = 0;
            foreach( var chain in handler.Chains )
                foreach( var bone in chain.BoneSetups )
                {
                    var collider = bone.SourceBone.GetComponent<Collider>();
                    if( collider )
                    {
                        if( setAsOther ) bone.BaseColliderSetup.ColliderType = RagdollChainBone.EColliderType.Other;
                        bone.BaseColliderSetup.OtherReference = collider;
                        bone.BaseColliderSetup.CopySettingsFromColliderComponent( collider );

                        foundCount += 1;
                    }
                }

            if( log )
            {
                if( foundCount == 0 ) Debug.Log( "[Ragdoll Animator 2] Not found any skeleton collider to assign as target dummy bone collider." );
                else Debug.Log( "[Ragdoll Animator 2] Found " + foundCount + " colliders on the source skeleton. Assigned as target dummy bone colliders." );
            }
        }

        public static void CalculateInertiaTensor( Rigidbody rigidbody )
        {
            Vector3 size = rigidbody.transform.localScale;
            float mass = rigidbody.mass;

            float Ixx = ( mass / 12f ) * ( size.y * size.y + size.z * size.z );
            float Iyy = ( mass / 12f ) * ( size.x * size.x + size.z * size.z );
            float Izz = ( mass / 12f ) * ( size.x * size.x + size.y * size.y );

            rigidbody.inertiaTensor = new Vector3( Ixx, Iyy, Izz );
            rigidbody.inertiaTensorRotation = rigidbody.transform.rotation;
        }

        /// <summary> Calculating velocity for the rigidbody to shift it towards target position </summary>
        public static void DragRigidbodyTowards( this Rigidbody rigidbody, Vector3 worldPosition, float power )
        {
            float targetPower = power;
            float powerBoost = 0f;
            if( targetPower > 1f ) { targetPower = 1f; powerBoost = power - 1f; }

            float deltaDiv = Time.fixedDeltaTime * ( 50f - ( powerBoost * 49f ) );
            deltaDiv = Mathf.Clamp( deltaDiv, 0.005f, 1f );

            Vector3 dragToPos = rigidbody.position;
            Vector3 diff = worldPosition - dragToPos;
            Vector3 targetStableVelo = ( diff / deltaDiv ) * 10f;

            if( rigidbody.useGravity ) targetStableVelo -= Physics.gravity * Time.fixedDeltaTime;

            rigidbody.velocity = Vector3.Lerp( rigidbody.velocity, targetStableVelo, targetPower );
        }

        /// <summary> Calculating angular velocity for the rigidbody to rotate it towards target </summary>
        public static void RotateRigidbodyTowards( this Rigidbody rigidbody, Quaternion worldRotation, float power, float overallLerp = 1f )
        {
            if( overallLerp <= 0f ) return;

            float targetPower = power;
            float powerBoost = 0f;
            if( targetPower > 1f ) { targetPower = 1f; powerBoost = power - 1f; }

            float deltaDiv = Time.fixedDeltaTime * ( 50f - ( powerBoost * 49f ) );
            deltaDiv = Mathf.Clamp( deltaDiv, 0.005f, 1f );

            Vector3 velocity = FEngineering.QToAngularVelocity( rigidbody.rotation, worldRotation, ( 45f ) / deltaDiv );
            rigidbody.angularVelocity = Vector3.Slerp( rigidbody.angularVelocity, velocity, targetPower * overallLerp );
        }


        /// <summary> Using force to move rigidbody towards desired world position </summary>
        public static void AddRigidbodyForceToMoveTowards( this Rigidbody rigidbody, Vector3 worldPosition, float forceMultiply )
        {
            rigidbody.AddForce( GetVelocityToMoveTowards( rigidbody, worldPosition, forceMultiply ), ForceMode.VelocityChange );
        }


        /// <summary> Computing velocity to move rigidbody towards desired world position </summary>
        public static Vector3 GetVelocityToMoveTowards( this Rigidbody rigidbody, Vector3 worldPosition, float forceMultiply )
        {
            Vector3 posDiff = worldPosition - rigidbody.worldCenterOfMass;
            Vector3 targetVelocity = posDiff / Time.fixedDeltaTime;

            if( rigidbody.useGravity ) targetVelocity -= Physics.gravity * Time.fixedDeltaTime;
            targetVelocity *= forceMultiply;
            targetVelocity -= rigidbody.velocity;

            return targetVelocity;
        }


        /// <summary> Using acceleration to move rigidbody towards desired world position </summary>
        public static void AddAccelerationTowardsWorldPosition( Rigidbody rigidbody, Vector3 targetPosition, Vector3 lastestPositionDelta, float power, float fixedDelta )
        {
            rigidbody.AddForce( GetAccelerationToMoveTowards( rigidbody, targetPosition - rigidbody.worldCenterOfMass, lastestPositionDelta, power, fixedDelta ), ForceMode.Acceleration );
        }

        /// <summary> Using acceleration to move rigidbody towards desired world position </summary>
        public static void AddAccelerationTowardsWorldPositionDiff( Rigidbody rigidbody, Vector3 positionDifference, Vector3 lastestPositionDelta, float power, float fixedDelta, float overallMultiplier = 1f )
        {
            rigidbody.AddForce( GetAccelerationToMoveTowards( rigidbody, positionDifference, lastestPositionDelta, power, fixedDelta ) * overallMultiplier, ForceMode.Acceleration );
        }

        /// <summary> Computing acceleration to move rigidbody towards desired world position </summary>
        public static Vector3 GetAccelerationToMoveTowards( Rigidbody rigidbody, Vector3 positionDifference, Vector3 lastestPositionDelta, float power, float fixedDelta )
        {
            float factor = ( rigidbody.mass * ( 0.05f + 0.85f * power ) ) / ( fixedDelta * fixedDelta );
            float damper = ( 0.55f + 0.325f * power ) * ( 2 * Mathf.Sqrt( factor * rigidbody.mass ) );

            Vector3 veloDiff = rigidbody.velocity - lastestPositionDelta;

            return ( factor / rigidbody.mass * positionDifference ) - ( damper / rigidbody.mass * veloDiff );
        }

        /// <summary> Using torque to rotate rigidbody towards desired world rotation </summary>
        public static void AddRigidbodyTorqueToRotateTowards( this Rigidbody rigidbody, Quaternion worldRotation, float forceMultiply )
        {
            float angle = Quaternion.Angle( rigidbody.rotation, worldRotation );
            Vector3 upAxisCross = Vector3.Cross( rigidbody.rotation * Vector3.up, worldRotation * Vector3.up );
            Vector3 fwdAxisCross = Vector3.Cross( rigidbody.rotation * Vector3.forward, worldRotation * Vector3.forward );

            Vector3 torque = Vector3.Normalize( fwdAxisCross + upAxisCross ) * angle * Mathf.Deg2Rad;

            torque *= forceMultiply;
            torque /= Time.fixedDeltaTime;
            torque -= rigidbody.angularVelocity;

            rigidbody.AddTorque( torque, ForceMode.VelocityChange );
        }

        /// <summary>
        /// Adjusting size of the collider, works only for BoxCollider and CapsuleCollider
        /// </summary>
        public static void AdjustColliderBasingOnStartEndPosition( Vector3 start, Vector3 end, Transform bone, Collider collider, float radius )
        {
            // Compute reference values
            Vector3 diff = end - start;
            Vector3 dir = diff.normalized;
            float diffLocalMagn = bone.InverseTransformVector( diff ).magnitude;
            Vector3 midPoint = Vector3.LerpUnclamped( end, start, 0.5f );

            // Define collider direction and direction related values
            Vector3 colliderDir = bone.InverseTransformVector( dir );
            colliderDir = FVectorMethods.ChooseDominantAxis( colliderDir );

            if( collider is BoxCollider )
            {
                BoxCollider box = collider as BoxCollider;
                box.size = Vector3.one * ( radius * 1.5f );
                box.center = bone.InverseTransformPoint( midPoint );
            }
            else if( collider is CapsuleCollider )
            {
                CapsuleCollider caps = collider as CapsuleCollider;
                caps.height = diffLocalMagn;
                caps.center = bone.InverseTransformPoint( midPoint );
                caps.radius = Mathf.Min( diffLocalMagn, radius );
                if( caps.height / 2f < caps.radius ) caps.height = caps.radius * 2f;
            }

            AdjustColliderDirectionParams( collider, colliderDir, diffLocalMagn );
        }

        /// <summary>
        /// Collider direction parameters, works only for BoxCollider and CapsuleCollider
        /// </summary>
        public static void AdjustColliderDirectionParams( Collider collider, Vector3 colliderDir, float diffLocalMagn )
        {
            if( collider is BoxCollider )
            {
                BoxCollider box = collider as BoxCollider;

                if( colliderDir.x > 0.1f || colliderDir.x < -0.1f ) { box.size = new Vector3( diffLocalMagn, box.size.y, box.size.z ); }
                if( colliderDir.y > 0.1f || colliderDir.y < -0.1f ) { box.size = new Vector3( box.size.x, diffLocalMagn, box.size.z ); }
                if( colliderDir.z > 0.1f || colliderDir.z < -0.1f ) { box.size = new Vector3( box.size.x, box.size.y, diffLocalMagn ); }
            }
            else if( collider is CapsuleCollider )
            {
                CapsuleCollider caps = collider as CapsuleCollider;

                if( colliderDir.x > 0.1f || colliderDir.x < -0.1f ) { caps.direction = 0; }
                if( colliderDir.y > 0.1f || colliderDir.y < -0.1f ) { caps.direction = 1; }
                if( colliderDir.z > 0.1f || colliderDir.z < -0.1f ) { caps.direction = 2; }
            }
        }

        /// <summary> To avoid writing too many #if unity... since this parameter is available since unity 2022 </summary>
        public static void SetMaxLinearVelocityU2022( this Rigidbody rigidbody, float maxLinearVelocity )
        {
#if UNITY_2022_1_OR_NEWER
            rigidbody.maxLinearVelocity = maxLinearVelocity;
#endif
        }

        public static T GetOrGenerate<T>( Transform t ) where T : Component
        {
            T comp = t.GetComponent<T>();
            if( comp == null ) comp = t.gameObject.AddComponent<T>();
            return comp;
        }

        public static void DestroyComponent<T>( Transform t ) where T : Component
        {
            T comp = t.GetComponent<T>();
            if( comp != null ) DestroyObject( comp );
        }

        public static bool LayerMaskContains( LayerMask layerMask, int layer )
        {
            return layerMask == ( layerMask | ( 1 << layer ) );
        }

        /// <summary> Changing isKinematic if differs </summary>
        public static void SwitchKinematic( Rigidbody rigidbody, bool restore = false )
        {
            if( rigidbody.isKinematic != restore ) return;

            if( restore )
            {
                rigidbody.isKinematic = false;
            }
            else
            {
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rigidbody.isKinematic = true;
            }
        }

        /// <summary> Changing isKinematic, preProcessing ON and projection if differs </summary>
        public static void SwitchKinematicAndProjection( Rigidbody rigidbody, IRagdollAnimator2HandlerOwner handler, bool restore = false, ConfigurableJoint joint = null )
        {
            if( rigidbody.isKinematic != restore ) return;

            if( joint == null ) joint = rigidbody.transform.GetComponent<ConfigurableJoint>();

            if( restore )
            {
                rigidbody.isKinematic = false;

                if( joint != null )
                {
                    if( handler == null || handler.GetRagdollHandler == null ) return;
                    joint.enablePreprocessing = handler.GetRagdollHandler.PreProcessing;
                    joint.projectionMode = handler.GetRagdollHandler.ProjectionMode;
                }
            }
            else
            {
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rigidbody.isKinematic = true;

                if( joint != null )
                {
                    if( handler == null || handler.GetRagdollHandler == null ) return;
                    joint.enablePreprocessing = true;
                    joint.projectionMode = JointProjectionMode.PositionAndRotation;
                }
            }
        }

        /// <summary> Destroying objects edit/playmode </summary>
        public static void DestroyObject( UnityEngine.Object obj )
        {
            if( obj == null ) return;

#if UNITY_EDITOR
            if( Application.isPlaying == false )
            {
                UnityEditor.EditorApplication.delayCall += () => { GameObject.DestroyImmediate( obj ); };
            }
            else
                GameObject.Destroy( obj );
#else
                GameObject.Destroy(obj);
#endif
        }

    }
}