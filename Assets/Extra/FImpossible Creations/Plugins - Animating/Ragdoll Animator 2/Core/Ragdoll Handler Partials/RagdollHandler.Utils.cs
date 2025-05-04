using System;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;




#if UNITY_EDITOR

using FIMSpace.FEditor;

#endif

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
        /// <summary> Calling provided action on all ragdoll chains bones </summary>
        public void CallOnAllRagdollBones( Action<RagdollChainBone> action )
        {
            foreach( var chain in chains )
            {
                if( chain == null ) continue;

                foreach( var bone in chain.BoneSetups )
                {
                    if( bone == null ) continue;
                    if( bone.SourceBone == null ) continue;

                    action.Invoke( bone );
                }
            }
        }

        /// <summary> Calling provided action on all in-between bones </summary>
        public void CallOnAllInBetweenBones( Action<RagdollChainBone.InBetweenBone> action )
        {
            foreach( var bone in skeletonFillExtraBonesList )
            {
                if( bone == null ) continue;
                if( bone.SourceBone == null ) continue;
                action.Invoke( bone );
            }
        }

        /// <summary> Ragdoll Handler utility transform method </summary>
        public static Transform CreateTransform( string name, int targetLayer )
        {
            GameObject obj = new GameObject( name );
            obj.layer = targetLayer;
            return obj.transform;
        }

        /// <summary> Ragdoll Handler utility transform method </summary>
        public static Transform CreateTransform( Transform copyOf )
        {
            Transform newT = CreateTransform( copyOf.name, copyOf.gameObject.layer );
            SetCoordsLike( newT, copyOf );
            return newT;
        }

        /// <summary> Ragdoll Handler utility transform method </summary>
        public static void ResetCoords( Transform transform, bool scaleToo = true )
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            if( scaleToo ) transform.localScale = Vector3.one;
        }

        /// <summary> Ragdoll Handler utility transform method </summary>
        public static void SetCoordsLike( Transform toChange, Transform coordsLike )
        {
            toChange.SetPositionAndRotation( coordsLike.position, coordsLike.rotation );
            toChange.localScale = coordsLike.lossyScale;
        }

        /// <summary> Switching configurable joint xMotion yMotion and zMotion locks </summary>
        public static void SetConfigurableJointMotionLock( ConfigurableJoint joint, ConfigurableJointMotion motion )
        {
            joint.xMotion = motion;
            joint.yMotion = motion;
            joint.zMotion = motion;
        }

        /// <summary> Switching configurable joint angularXMotion angularYMotion and angularZMotion locks </summary>
        public static void SetConfigurableJointAngularMotionLock( ConfigurableJoint joint, ConfigurableJointMotion motion )
        {
            joint.angularXMotion = motion;
            joint.angularYMotion = motion;
            joint.angularZMotion = motion;
        }

        private float ComputePositionDifferenceFactor( RagdollChainBone bone, Vector3 targetPosition )
        {
            float distanceToSource = Vector3.Distance( bone.GameRigidbody.position, targetPosition );
            distanceToSource /= bone.MainBoneCollider.bounds.size.magnitude;

            if( distanceToSource > 1f ) distanceToSource = 1f;

            float powerFactor = 1f - distanceToSource;
            powerFactor *= powerFactor;
            return powerFactor;
        }

        /// <summary>
        /// Searching through collision indicators available data, to find collision of some limb with provided collider
        /// </summary>
        public RagdollChainBone CheckIfAnyBoneCollidesWith( Collider platformCollider )
        {
            if( _dummyIndicatorsWasPrepared == false )
            {
                Debug.Log( "[Ragdoll Animator 2] Bone Collision Indicators Are Required (Extra Feature) to use 'CheckIfCollidesWith' !" );
                return null;
            }

            foreach( var chain in chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    if( bone.BoneProcessor.IndicatorComponent == null ) continue;

                    RA2BoneCollisionHandler coll = bone.BoneProcessor.IndicatorComponent as RA2BoneCollisionHandler;
                    if( coll == null ) continue;
                    if( coll.Colliding == false ) continue;
                    if( coll.IsCollidingWith( platformCollider ) ) return bone;
                }
            }

            return null;
        }

        /// <summary>
        /// Searching through collision indicators available data, to find collision of some limb with provided collider
        /// </summary>
        public bool CheckIfCollidesWith( Collider platformCollider )
        {
            return CheckIfAnyBoneCollidesWith( platformCollider ) != null;
        }

        internal void StoreAnchorHelperCoords()
        {
            Transform baseT = GetBaseTransform();
            anchorToRootLocal = GetAnchorBoneController.SourceBone.InverseTransformPoint( baseT.position );
            anchorToRootLocalRot = FEngineering.QToLocal( GetAnchorBoneController.SourceBone.rotation, baseT.rotation );

            foreach( var chain in Chains )
                foreach( var bone in chain.BoneSetups )
                    bone.StoreHelperReferenceValues( baseT );
        }

        private void Debug_DrawRagdollPoseRays()
        {
            CallOnAllRagdollBones( ( RagdollChainBone b ) => { UnityEngine.Debug.DrawLine( b.PhysicalDummyBone.position, b.PhysicalDummyBone.parent.position, Color.green, 1.01f ); } );
        }

        private void Debug_DrawAnimatorPoseRays()
        {
            CallOnAllRagdollBones( ( RagdollChainBone b ) => { UnityEngine.Debug.DrawLine( b.SourceBone.position, b.SourceBone.parent.position, Color.blue, 1.01f ); } );
        }

        public void ApplyAllPropertiesToOtherRagdoll(RagdollHandler copyTo)
        {
            PropertyInfo[] properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Type[] skipTypes = new Type[] { typeof( RagdollBonesChain ), typeof(RagdollChainBone) };
            int[] skipNames = new int[] { "DummyWasGenerated".GetHashCode(), "LODBlend".GetHashCode() };
            int hashFeatures = "ExtraFeatures".GetHashCode();
            
            foreach (PropertyInfo property in properties)
            {
                if (skipTypes.Contains(property.PropertyType) ) continue;
                if (property.PropertyType.IsAssignableFrom(typeof(Component)) ) continue;
                if (property.PropertyType.IsSubclassOf(typeof(Component)) ) continue;

                int nameHash = property.Name.GetHashCode();
                if (nameHash != hashFeatures && property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>)) continue;

                if ( skipNames.Contains(nameHash) ) continue;
                if( property.Name.StartsWith( "_Ed" ) ) continue;
                if( property.Name.StartsWith( "m_" ) ) continue;

                if( property.CanWrite)
                {
                    //UnityEngine.Debug.Log("prop: " + property.Name + " type == " + property.PropertyType);

                    object value = property.GetValue(this);
                    property.SetValue(copyTo, value);
                }
            }

            FieldInfo[] fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo field in fields)
            {
                if (skipTypes.Contains(field.FieldType) ) continue;
                if (field.FieldType.IsAssignableFrom(typeof(Component))) continue;
                if (field.FieldType.IsSubclassOf(typeof(Component))) continue;

                int nameHash = field.Name.GetHashCode();

                if (nameHash != hashFeatures && field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>)) continue;

                if (skipNames.Contains(nameHash)) continue;
                if( field.Name.StartsWith( "_Ed" ) ) continue;
                if( field.Name.StartsWith( "m_" ) ) continue;

                //UnityEngine.Debug.Log("field: " + field.Name + " type == " + field.FieldType);

                object value = field.GetValue(this);
                field.SetValue(copyTo, value);
            }
        }


        #region Editor Code

        /// <summary> Calling setDirty for editor use </summary>
        private void OnChange()
        {
#if UNITY_EDITOR
            if( GetBaseTransform() == null ) return;
            UnityEditor.EditorUtility.SetDirty( GetBaseTransform() );
            Editor_CheckToStopPreviewingAll();
#endif
        }

#if UNITY_EDITOR

        private static GUIContent _gc_selector = new GUIContent();

        public static void Editor_RagdollBonesSelector( GameObject ObjectWithRagdollAnimator, Action<Transform> selectAction, Transform selected = null, bool drawIcon = true )
        {
            IRagdollAnimator2HandlerOwner handler = ObjectWithRagdollAnimator.GetComponent<IRagdollAnimator2HandlerOwner>();
            if( handler != null )
            {
                _gc_selector.text = "  Select Bone";
                _gc_selector.tooltip = "Select bone, which will choose physical dummy bone during runtime";

                if( drawIcon ) _gc_selector.image = FGUI_Resources.FindIcon( "Ragdoll Animator/SPR_RagdollAnim2s" ); else _gc_selector.image = null;

                if( GUILayout.Button( _gc_selector, GUILayout.Height( 20 ) ) )
                {
                    UnityEditor.GenericMenu menu = new UnityEditor.GenericMenu();

                    foreach( var chain in handler.GetRagdollHandler.Chains )
                    {
                        string prefix = chain.ChainName + " (" + chain.ChainType + ")" + "/";

                        foreach( var bone in chain.BoneSetups )
                        {
                            if( bone.SourceBone == null ) continue;
                            Transform target = bone.SourceBone;
                            menu.AddItem( new GUIContent( prefix + target.name ), target == selected, () => { selectAction.Invoke( target ); UnityEditor.EditorUtility.SetDirty( ObjectWithRagdollAnimator ); } );
                        }
                    }

                    menu.ShowAsContext();
                }
            }
        }

#endif

        #endregion Editor Code
    }
}