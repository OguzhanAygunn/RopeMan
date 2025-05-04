using FIMSpace.FGenerating;

#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RAF_ReposeOnFall : RagdollAnimatorFeatureUpdate
    {
        public enum EBaseTransformRepose
        { AnchorToFootPosition = 0, AnchorBoneBottom = 1, BonesBoundsBottomCenter = 2, SkeletonCenter = 3, }

        public static Vector3 GetReposePosition( IRagdollAnimator2HandlerOwner iHandler, EBaseTransformRepose reposeMode )
        {
            if( reposeMode == EBaseTransformRepose.AnchorToFootPosition )
            {
                return iHandler.GetRagdollHandler.User_GetPosition_HipsToFoot();
            }
            else if( reposeMode == EBaseTransformRepose.AnchorBoneBottom )
            {
                return iHandler.GetRagdollHandler.User_GetPosition_AnchorBottom();
            }
            else if( reposeMode == EBaseTransformRepose.BonesBoundsBottomCenter )
            {
                return iHandler.GetRagdollHandler.User_GetPosition_BottomCenter();
            }
            else //if( reposeMode == EBaseTransformRepose.SkeletonCenter )
            {
                return iHandler.GetRagdollHandler.User_GetPosition_Center();
            }
        }

        public override bool UseLateUpdate => true;

        private FUniversalVariable reposeVar;
        private FUniversalVariable rotationVar;
        private float reposeStartAt = -100f;

        public override bool OnInit()
        {
            reposeVar = InitializedWith.RequestVariable( "Mode", 1 );
            rotationVar = InitializedWith.RequestVariable( "Apply Rotation:", false );
            return base.OnInit();
        }

        public override void LateUpdate()
        {
            if( InitializedWith.Enabled == false ) return;

            if( ParentRagdollHandler.IsFallingOrSleep )
            {
                if( reposeStartAt < 0f )
                {
                    reposeStartAt = Time.time;
                    return;
                }

                if( Time.time - reposeStartAt < 0.1f ) return; // Not allow repose start too soon to avoid transform update conflicts

                EBaseTransformRepose reposeMode = (EBaseTransformRepose)reposeVar.GetInt();

                ParentRagdollHandler.BaseTransform.position = GetReposePosition( ParentRagdollHandler, reposeMode );

                if( rotationVar.GetBool() )
                {
                    ParentRagdollHandler.BaseTransform.rotation = ParentRagdollHandler.User_GetMappedRotationHipsToLegsMiddle( Vector3.up );
                }
            }
            else
            {
                reposeStartAt = -100f;
            }
        }

#if UNITY_EDITOR

        public override string Editor_FeatureDescription => "Changing base object position (like character controller) to be aligned with ragdolled bones when falling mode.";

        public override void Editor_InspectorGUI( SerializedProperty toDirty, RagdollHandler ragdollHandler, RagdollAnimatorFeatureHelper helper )
        {
            base.Editor_InspectorGUI( toDirty, ragdollHandler, helper );

            var modeV = helper.RequestVariable( "Mode", 1 );
            modeV.AssignTooltip( "Choose option which fits to your Get Up animation origin. Some get up animations can start at hips center, some are starting at feet position." );
            EBaseTransformRepose reposeM = (EBaseTransformRepose)modeV.GetInt();
            reposeM = (EBaseTransformRepose)EditorGUILayout.EnumPopup( "Mode:", reposeM );
            modeV.SetValue( (int)reposeM );

            var rotationV = helper.RequestVariable( "Apply Rotation:", true );
            rotationV.AssignTooltip( "Apply calculated rotation to the base object" );
            rotationV.Editor_DisplayVariableGUI();
        }

#endif
    }
}