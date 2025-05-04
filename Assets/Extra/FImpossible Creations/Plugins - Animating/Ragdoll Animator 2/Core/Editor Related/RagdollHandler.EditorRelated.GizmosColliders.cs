#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;
using static FIMSpace.FProceduralAnimation.RagdollChainBone;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
#if UNITY_EDITOR

        public void EditorGizmos_RagdollConstructorColliders()
        {
            if( _Editor_SelectedChain < 0 )
            {
                if( _Editor_SelectedChain >= chains.Count ) return;
                for( int i = 0; i < chains.Count; i++ ) EditorGizmos_RagdollDrawBone_JustColliders( this, chains[i], 0.85f, true, false );
                return;
            }

            RagdollBonesChain chain = chains[_Editor_SelectedChain];
            EditorGizmos_RagdollDrawBoneColliders( chain );

            for( int i = 0; i < chains.Count; i++ )
            {
                if( chain == chains[i] ) continue;
                if( chain.IsTypeRelatedWith( chains[i] ) == false ) continue;
                EditorGizmos_RagdollDrawBone_JustColliders( this, chains[i], 0.4f, true, false, true );
            }
        }

        public void EditorGizmos_RagdollConstructorDrawJustChainsColliders( bool meshes = true, bool flat = false, bool onSource = true )
        {
            for( int i = 0; i < chains.Count; i++ ) EditorGizmos_RagdollDrawBone_JustColliders( this, chains[i], 0.85f, meshes, flat, true, onSource );
        }

        public void EditorGizmos_RagdollDrawBoneColliders( RagdollBonesChain chain, float alpha = 1f )
        {
            Handles.color = new Color( 0.1f, .75f, 0.2f, alpha );

            for( int c = 0; c < chain.BoneSetups.Count; c++ )
            {
                RagdollChainBone bone = chain.BoneSetups[c];
                if( bone.SourceBone == null ) continue;

                if( bone._EditorCollFoldout ) Gizmos.color = new Color( 0f, 1f, 0f, alpha );
                else
                {
                    Gizmos.color = new Color( 0f, 1f, 0f, 0.375f * alpha );
                }

                Handles.color = Gizmos.color;

                foreach( var setup in bone.Colliders )
                {
                    if( setup.ColliderType == RagdollChainBone.EColliderType.Capsule ) OnGizmos_DrawCapsule( bone, setup, chain );
                    else if( setup.ColliderType == RagdollChainBone.EColliderType.Sphere ) OnGizmos_DrawSphere( bone, setup, chain );
                    else if( setup.ColliderType == RagdollChainBone.EColliderType.Box ) OnGizmos_DrawBox( bone, setup, chain );
                    else if( setup.ColliderType == RagdollChainBone.EColliderType.Mesh ) OnGizmos_DrawMesh( bone, setup, chain );
                    else if( setup.ColliderType == RagdollChainBone.EColliderType.Other ) { }
                }

                //DrawCapsule(bone, 1, 0.3f, 0.1f);
            }

            Gizmos.color = Color.white;
            Handles.color = Color.white;

            Handles.matrix = Matrix4x4.identity;
        }

        public void EditorGizmos_RagdollDrawBone_JustColliders( RagdollHandler handler, RagdollBonesChain chain, float alpha = 1f, bool drawMeshes = true, bool drawFlat = true, bool colorful = true, bool onSource = true )
        {
            Handles.color = new Color( 0.1f, .85f, 0.2f, alpha );

            Color gColor = new Color( 0f, 1f, 0f, alpha );
            if( colorful ) gColor = handler.GetIndexColor( handler.GetIndexOfChain( chain ), 0f, alpha );

            for( int c = 0; c < chain.BoneSetups.Count; c++ )
            {
                RagdollChainBone bone = chain.BoneSetups[c];
                if( bone.SourceBone == null ) continue;

                Gizmos.color = gColor;

                foreach( var cSetup in bone.Colliders )
                {
                    if( cSetup.ColliderType == RagdollChainBone.EColliderType.Capsule ) OnGizmos_DrawCapsule( bone, cSetup, chain, drawMeshes, drawFlat, onSource );
                    else if( cSetup.ColliderType == RagdollChainBone.EColliderType.Sphere ) OnGizmos_DrawSphere( bone, cSetup, chain, drawMeshes, drawFlat, onSource );
                    else if( cSetup.ColliderType == RagdollChainBone.EColliderType.Box ) OnGizmos_DrawBox( bone, cSetup, chain, drawMeshes, drawFlat, onSource );
                    else if( cSetup.ColliderType == RagdollChainBone.EColliderType.Mesh ) OnGizmos_DrawMesh( bone, cSetup, chain, drawMeshes, drawFlat, onSource );
                    else if( cSetup.ColliderType == RagdollChainBone.EColliderType.Other ) { }
                }
            }

            Gizmos.color = Color.white;
            Handles.color = Color.white;
            Handles.matrix = Matrix4x4.identity;
        }

        public void EditorHandles_RagdollConstructorCollidersHandles()
        {
            if( _Editor_SelectedChain < 0 )
            {
                for( int i = 0; i < chains.Count; i++ ) EditorHandles_RagdollDrawBoneChainSceneSelector( this, chains[i] );
                return;
            }

            RagdollBonesChain chain = chains[_Editor_SelectedChain];
            EditorHandles_RagdollDrawBoneCollidersSceneHandles( chain );
        }

        private bool _editScale = false;

        public void EditorHandles_RagdollDrawBoneCollidersSceneHandles( RagdollBonesChain chain, float alpha = 1f )
        {
            RagdollChainBone selected = null;
            foreach( var bone in chain.BoneSetups ) if( bone._EditorCollFoldout ) { selected = bone; break; }

            if( selected != null )
            {
                foreach( var cSetup in selected.Colliders )
                {
                    EditorHandler_DrawColliderAdjustementsHandles( selected, cSetup );
                }

                foreach( var bone in chain.BoneSetups ) if (bone != selected) EditorHandler_DrawColliderSelectionHandle( bone, chain );

                return;
            }

            for( int c = 0; c < chain.BoneSetups.Count; c++ )
            {
                RagdollChainBone bone = chain.BoneSetups[c];
                if( bone.SourceBone == null ) continue;

                if( !bone._EditorCollFoldout )
                {
                    EditorHandler_DrawColliderSelectionHandle( bone,chain );
                    continue;
                }

                foreach( var cSetup in bone.Colliders )
                {
                    EditorHandler_DrawColliderAdjustementsHandles( bone, cSetup );
                }
            }

            Handles.matrix = Matrix4x4.identity;
        }

        private Vector3 DivideV3( Vector3 a, Vector3 divBy )
        {
            return new Vector3( a.x / divBy.x, a.y / divBy.y, a.z / divBy.z );
        }

        private void EditorHandler_DrawColliderAdjustementsHandles( RagdollChainBone bone, RagdollChainBone.ColliderSetup setup )
        {
            if( bone.SourceBone == null ) return;
            if( bone.SourceBone.lossyScale.x == 0f ) return;

            float avgSize = setup.Editor_GetHandleSize( bone ) * 1.1f / bone.SourceBone.lossyScale.x; // bone.GetAverageScale( chain.ChainScaleMultiplier );

            Handles.matrix = bone.GetMatrix( Vector3.zero, Vector3.Scale( Vector3.one, bone.SourceBone.lossyScale ), setup.RotationCorrectionQ );
            float selectSize = setup.Editor_GetHandleSize( bone ) * ( _editScale ? 6f : 3.5f ) / bone.SourceBone.lossyScale.x;
            float handleSize = setup.Editor_GetHandleSize( bone ) * ( _editScale ? 4f : 1.25f ) / bone.SourceBone.lossyScale.x;

            if( _editScale )
            {
                if( Handles.Button( setup.ColliderCenter + new Vector3( avgSize * 0.3f, 0f, avgSize * 0.3f ), Quaternion.identity, handleSize * 0.125f, selectSize * 0.125f, Handles.ArrowHandleCap ) ) { _editScale = !_editScale; }
            }
            else
            {
                if( Handles.Button( setup.ColliderCenter + new Vector3( avgSize * 0.3f, 0f, avgSize * 0.3f ), Quaternion.identity, handleSize * 0.125f, selectSize * 0.125f, Handles.CubeHandleCap ) ) { _editScale = !_editScale; }
            }

            if( Handles.Button( setup.ColliderCenter + new Vector3( -avgSize * 0.3f, 0f, -avgSize * 0.3f ), Quaternion.identity, avgSize * 0.125f, avgSize * 0.125f, Handles.CircleHandleCap ) ) { Editor_HandlesUndoRecord(); bone._EditorCollFoldout = false; }

            if( !_editScale )
            {
                Vector3 handlePos = FEditor_TransformHandles.PositionHandle( setup.ColliderCenter, Quaternion.identity, avgSize, false, false );
                if( handlePos != setup.ColliderCenter ) Editor_HandlesUndoRecord();
                setup.ColliderCenter = handlePos;
            }
            else
            {
                Vector3 size = setup.GetColliderSizeAxes();

                Color preCol = Handles.color;
                Handles.color = new Color( 1f, 1f, 1f, 0.1f ); // Add transparent hidden button since arrow handle cap is hard to select!
                if( Handles.Button( setup.ColliderCenter + new Vector3( avgSize * 0.3f, 0f, avgSize * 0.3f ), Quaternion.identity, handleSize * 0.025f, selectSize * 0.033f, Handles.CubeHandleCap ) ) { _editScale = !_editScale; }

                if( setup.ColliderType == RagdollChainBone.EColliderType.Box )
                { size = FEditor_TransformHandles.ScaleHandle( size, setup.ColliderCenter, Quaternion.identity, avgSize ); if( size != setup.ColliderBoxSize ) Editor_HandlesUndoRecord(); setup.ColliderBoxSize = size; }
                else if( setup.ColliderType == RagdollChainBone.EColliderType.Sphere )
                { size = FEditor_TransformHandles.ScaleHandle( size, setup.ColliderCenter, Quaternion.identity, avgSize, true ); if( size.x != setup.ColliderRadius ) Editor_HandlesUndoRecord(); setup.ColliderRadius = size.x; }
                else if( setup.ColliderType == RagdollChainBone.EColliderType.Capsule )
                {
                    //size = FEditor_TransformHandles.ScaleHandle( size, setup.ColliderCenter, Quaternion.identity, avgSize, false, false, true, true, false );
                    size = SizeHandlesCapsule( size, setup.ColliderCenter, Quaternion.identity, avgSize, setup.CapsuleDirection );
                    if( size.x != setup.ColliderRadius || size.y != setup.ColliderLength ) Editor_HandlesUndoRecord();
                    setup.ColliderRadius = size.x;
                    setup.ColliderLength = size.y;
                }
                else /*if( setup.ColliderType == RagdollChainBone.EColliderType.Mesh )*/
                { size = FEditor_TransformHandles.ScaleHandle( size, setup.ColliderCenter, Quaternion.identity, avgSize, true ); if( size.x != setup.ColliderSizeMultiply ) Editor_HandlesUndoRecord(); setup.ColliderSizeMultiply = size.x; }
            }
        }

        private Vector3 SizeHandlesCapsule( Vector3 scale, Vector3 position, Quaternion rotation, float size, ECapsuleDirection dir )
        {
            float handleSize = size;
            Vector3 preScale = scale;

            Vector3 axis = Vector3.up, secAxis = Vector3.right;

            if( dir == ECapsuleDirection.Y ) { axis = Vector3.forward; secAxis = Vector3.up; }
            else if( dir == ECapsuleDirection.Z ) { axis = Vector3.right; secAxis = Vector3.forward; }

            Handles.color = Handles.xAxisColor;

            EditorGUI.BeginChangeCheck();
            scale.x = Handles.ScaleSlider( scale.x, position, rotation * axis, rotation, handleSize, 0.001f );

            if( EditorGUI.EndChangeCheck() )
            {
                if( Mathf.Sign( scale.x ) != Mathf.Sign( preScale.x ) ) scale.x = preScale.x * handleSize * 0.001f;
                if( scale.x > scale.y / 2f ) scale.y = scale.x * 2f;
            }

            Handles.color = Handles.yAxisColor;

            EditorGUI.BeginChangeCheck();
            scale.y = Handles.ScaleSlider( scale.y, position, rotation * secAxis, rotation, handleSize, 0.001f );

            if( EditorGUI.EndChangeCheck() )
            {
                if( Mathf.Sign( scale.y ) != Mathf.Sign( preScale.y ) ) scale.y = preScale.y * handleSize * 0.001f;
                if( scale.y < scale.x * 2f ) scale.y = scale.x * 2f;
            }

            Handles.color = Handles.centerColor;

            EditorGUI.BeginChangeCheck();
            float num1 = Handles.ScaleValueHandle( scale.x, position, rotation, handleSize, Handles.CubeHandleCap, 0.001f );

            if( EditorGUI.EndChangeCheck() )
            {
                if( Mathf.Sign( num1 ) != Mathf.Sign( preScale.x ) )
                {
                    num1 = preScale.x * handleSize * 0.001f;
                    if( Mathf.Abs( num1 ) < 0.001f ) num1 = 0.001f * Mathf.Sign( preScale.x );
                }

                float num2 = num1 / scale.x;
                scale.x = num1;
                scale.y *= num2;
                scale.z *= num2;
            }

            return scale;
        }

        private void EditorHandler_DrawColliderSelectionHandle( RagdollChainBone bone, RagdollBonesChain chain )
        {
            if( bone.SourceBone.parent )
            {
                Handles.matrix = Matrix4x4.identity;
                Handles.color = Color.white * 0.8f;
                float size = bone.BaseColliderSetup.Editor_GetHandleSize( bone ) * 0.65f;

                if( Handles.Button( bone.SourceBone.TransformPoint( bone.BaseColliderSetup.ColliderCenter ), Quaternion.identity, size, size, Handles.CircleHandleCap ) )
                {
                    Editor_HandlesUndoRecord();

                    for( int i = 0; i < chain.BoneSetups.Count; i++ )
                    {
                        if( bone == chain.BoneSetups[i] ) { bone._EditorCollFoldout = true; }
                        else chain.BoneSetups[i]._EditorCollFoldout = false;
                    }
                }

                Handles.color = Color.white;
            }
        }

        public void EditorHandles_RagdollDrawBoneChainSceneSelector( RagdollHandler handler, RagdollBonesChain chain, float alpha = 1f )
        {
            if( chain.BoneSetups.Count == 0 ) return;

            var bone = chain.BoneSetups[0];
            if( bone.SourceBone == null ) return;
            if( bone.SourceBone.parent == null ) return;
            if( HasChain( chain ) == false ) return;

            Handles.color = handler.GetIndexColor( handler.GetIndexOfChain( chain ), 0f, alpha, 1f, 1f );

            float size = chain.GetAverageStepSizeOfTheChain() * 0.15f;
            if( Handles.Button( bone.SourceBone.TransformPoint( Vector3.zero ), Quaternion.identity, size, size, Handles.CubeHandleCap ) )
            {
                Editor_HandlesUndoRecord();
                _EditorCategory = ERagdollAnimSection.Construct;
                _Editor_SelectedChain = GetIndexOfChain( chain );
            }

            EditorHandles_DrawChainAsLines( chain );
        }

        public void EditorHandles_RagdollConstructorColliders()
        {
            if( _Editor_SelectedChain < 0 ) return;
            RagdollBonesChain chain = chains[_Editor_SelectedChain];
            EditorGizmos_RagdollDrawBoneColliders( chain );
        }

        public void Editor_HandlesUndoRecord( string actionId = "Ragdoll Animator Modify" )
        {
            if ( WasInitialized || DummyWasGenerated)
            {
                this.User_UpdateAllBonesParametersAfterManualChanges();
            }

            if( gizmosDrawer == null ) return;
            Undo.RecordObject( gizmosDrawer, actionId );
        }

#endif
    }
}