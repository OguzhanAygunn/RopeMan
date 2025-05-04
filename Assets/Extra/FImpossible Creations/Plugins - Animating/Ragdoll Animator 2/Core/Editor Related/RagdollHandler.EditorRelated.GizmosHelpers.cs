using FIMSpace.AnimationTools;

#if UNITY_EDITOR

using FIMSpace.FEditor;
using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
#if UNITY_EDITOR

        [HideInInspector] public Mesh m_hemi = null;
        [HideInInspector] public Mesh m_cyli = null;
        [HideInInspector] public Mesh m_sphere = null;
        [HideInInspector] public Mesh m_box = null;

        public static bool WireframeMode = false;
        public static bool MeshMode = true;
        public static bool DrawFlatColliders = false;

        private void TryLoadEditorMeshResources()
        {
            if( m_hemi != null ) return;

            GameObject loadHemi = Resources.Load<GameObject>( "Ragdoll Animator/Hemisphere" );
            if( loadHemi ) { MeshFilter filter = loadHemi.GetComponent<MeshFilter>(); if( filter != null ) m_hemi = filter.sharedMesh; }

            GameObject loadCyl = Resources.Load<GameObject>( "Ragdoll Animator/Cylinder" );
            if( loadCyl ) { MeshFilter filter = loadCyl.GetComponent<MeshFilter>(); if( filter != null ) m_cyli = filter.sharedMesh; }

            GameObject loadSphere = Resources.Load<GameObject>( "Ragdoll Animator/Sphere" );
            if( loadSphere ) { MeshFilter filter = loadSphere.GetComponent<MeshFilter>(); if( filter != null ) m_sphere = filter.sharedMesh; }

            GameObject loadBox = Resources.Load<GameObject>( "Ragdoll Animator/Cube" );
            if( loadBox ) { MeshFilter filter = loadBox.GetComponent<MeshFilter>(); if( filter != null ) m_box = filter.sharedMesh; }
        }

        private void OnGizmos_DrawCapsule( RagdollChainBone bone, RagdollChainBone.ColliderSetup setup, RagdollBonesChain chain, bool drawMeshes = true, bool drawFlat = true, bool onSource = true )
        {
            Transform tgt = onSource ? bone.SourceBone : bone.PhysicalDummyBone;
            Handles.matrix = Matrix4x4.TRS( tgt.TransformPoint( setup.RotationCorrectionQ * setup.ColliderCenter ), tgt.rotation * setup.RotationCorrectionQ, setup.GetScaleModded( chain, bone ) );
            TryLoadEditorMeshResources();

            if( (int)setup.CapsuleDirection == 0 ) Handles.matrix *= Matrix4x4.Rotate( Quaternion.Euler( 0f, 90f, 0f ) );
            else if( (int)setup.CapsuleDirection == 1 ) Handles.matrix *= Matrix4x4.Rotate( Quaternion.Euler( 90f, 90f, 0f ) );

            float radius = setup.ColliderRadius * chain.GetThicknessMultiplier();
            float length = setup.ColliderLength;

            if( DrawFlatColliders ) if( drawFlat ) _Handles_DrawCapsulePoly( bone._editor_colliderVerts, radius, length );

            if( !drawMeshes )
            {
                Gizmos.color = Color.white;
                Gizmos.matrix = Matrix4x4.identity;
                return;
            }

            if( WireframeMode )
            {
                Gizmos.matrix = Handles.matrix;
                Vector3 c1 = new Vector3( 0f, 0f, ( length / 2f ) - radius );
                Vector3 c2 = new Vector3( 0f, 0f, ( -length / 2f ) + radius );
                Gizmos.DrawWireSphere( c1, radius );
                Gizmos.DrawWireSphere( c2, radius );

                Gizmos.DrawLine( c1 + new Vector3( 0f, radius, 0f ), c2 + new Vector3( 0f, radius, 0f ) );
                Gizmos.DrawLine( c1 - new Vector3( 0f, radius, 0f ), c2 - new Vector3( 0f, radius, 0f ) );
                Gizmos.DrawLine( c1 + new Vector3( radius, 0f, 0f ), c2 + new Vector3( radius, 0f, 0f ) );
                Gizmos.DrawLine( c1 - new Vector3( radius, 0f, 0f ), c2 - new Vector3( radius, 0f, 0f ) );
            }

            if( MeshMode )
            {
                Matrix4x4 baseMx = Handles.matrix;
                Gizmos.matrix = baseMx * Matrix4x4.Scale( new Vector3( radius, radius, radius ) );
                Gizmos.DrawMesh( m_hemi, 0, new Vector3( 0f, 0f, ( length / 2f - radius ) / radius ), Quaternion.Euler( 0f, 180f, 0f ) );
                Gizmos.DrawMesh( m_hemi, 0, new Vector3( 0f, 0f, ( -length / 2f + radius ) / radius ), Quaternion.identity );

                Gizmos.matrix = baseMx * Matrix4x4.Scale( new Vector3( radius, radius, length - radius * 2f ) );
                Gizmos.DrawMesh( m_cyli, 0, Vector3.zero, Quaternion.identity );
            }

            Gizmos.color = Color.white;
            Gizmos.matrix = Matrix4x4.identity;
        }

        private void OnGizmos_DrawBox( RagdollChainBone bone, RagdollChainBone.ColliderSetup setup, RagdollBonesChain chain, bool drawMeshes = true, bool drawFlat = true, bool onSource = true )
        {
            Transform tgt = onSource ? bone.SourceBone : bone.PhysicalDummyBone;
            Handles.matrix = Matrix4x4.TRS( tgt.TransformPoint( setup.RotationCorrectionQ * setup.ColliderCenter ), tgt.rotation * setup.RotationCorrectionQ, setup.GetScaleModded( chain, bone ) );
            TryLoadEditorMeshResources();

            Vector3 boxSize = setup.ScaleUsingThickness( setup.ColliderBoxSize, chain.GetThicknessMultiplier(), chain, bone );

            if( DrawFlatColliders ) if( drawFlat ) _Handles_DrawBoxPoly( bone._editor_colliderVerts, boxSize.x / 2f, boxSize.y / 2f );

            if( !drawMeshes )
            {
                Gizmos.color = Color.white;
                Gizmos.matrix = Matrix4x4.identity;
                return;
            }

            Matrix4x4 baseMx = Handles.matrix;

            Gizmos.matrix = baseMx * Matrix4x4.Scale( boxSize );
            if( WireframeMode ) Gizmos.DrawWireMesh( m_box, 0, Vector3.zero, Quaternion.identity );
            if( MeshMode ) Gizmos.DrawMesh( m_box, 0, Vector3.zero, Quaternion.identity );

            Gizmos.color = Color.white;
            Gizmos.matrix = Matrix4x4.identity;
        }

        private void OnGizmos_DrawSphere( RagdollChainBone bone, RagdollChainBone.ColliderSetup setup, RagdollBonesChain chain, bool drawMeshes = true, bool drawFlat = true, bool onSource = true )
        {
            Transform tgt = onSource ? bone.SourceBone : bone.PhysicalDummyBone;
            Handles.matrix = Matrix4x4.TRS( tgt.TransformPoint( setup.ColliderCenter ), tgt.rotation * setup.RotationCorrectionQ, setup.GetScaleModded( chain, bone ) );
            TryLoadEditorMeshResources();

            float radius = setup.ColliderRadius * chain.GetThicknessMultiplier();

            if( DrawFlatColliders ) if( drawFlat ) _Handles_DrawSpherePoly( bone._editor_colliderVerts, radius );
            if( !drawMeshes )
            {
                Gizmos.color = Color.white;
                Gizmos.matrix = Matrix4x4.identity;
                return;
            }

            Matrix4x4 baseMx = Handles.matrix;

            Gizmos.matrix = baseMx * Matrix4x4.Scale( Vector3.one * radius );
            if( WireframeMode ) Gizmos.DrawWireMesh( m_sphere, 0, Vector3.zero, Quaternion.identity );
            if( MeshMode ) Gizmos.DrawMesh( m_sphere, 0, Vector3.zero, Quaternion.identity );

            Gizmos.color = Color.white;
            Gizmos.matrix = Matrix4x4.identity;
        }

        private void OnGizmos_DrawMesh( RagdollChainBone bone, RagdollChainBone.ColliderSetup setup, RagdollBonesChain chain, bool drawMeshes = true, bool drawFlat = true, bool onSource = true )
        {
            if( setup.ColliderMesh == null )
            {
                if( drawFlat ) OnGizmos_DrawUnknownCollider( bone, chain );
                return;
            }

            Transform tgt = onSource ? bone.SourceBone : bone.PhysicalDummyBone;
            Handles.matrix = Matrix4x4.TRS( tgt.TransformPoint( setup.ColliderCenter ), tgt.rotation * setup.RotationCorrectionQ, setup.GetScaleModded( chain, bone ) );
            TryLoadEditorMeshResources();

            if( !drawMeshes )
            {
                Gizmos.color = Color.white;
                Gizmos.matrix = Matrix4x4.identity;
                return;
            }

            Matrix4x4 baseMx = Handles.matrix;
            Vector3 boxSize = setup.ScaleUsingThickness( setup.ColliderBoxSize, chain.GetThicknessMultiplier(), chain, bone ); 
            Gizmos.matrix = baseMx * Matrix4x4.Scale( boxSize );

            if( WireframeMode ) Gizmos.DrawWireMesh( setup.ColliderMesh, 0, Vector3.zero, Quaternion.identity );
            if( MeshMode ) Gizmos.DrawMesh( setup.ColliderMesh, 0, Vector3.zero, Quaternion.identity );

            Gizmos.color = Color.white;
            Gizmos.matrix = Matrix4x4.identity;
        }

        private void OnGizmos_DrawUnknownCollider( RagdollChainBone bone, RagdollBonesChain chain, bool drawMeshes = true, bool drawFlat = true, bool onSource = true )
        {
            Transform tgt = onSource ? bone.SourceBone : bone.PhysicalDummyBone;
            Handles.matrix = tgt.localToWorldMatrix;
            if( DrawFlatColliders ) if( drawFlat ) _Handles_DrawSpherePoly( bone._editor_colliderVerts, 0.05f * chain.ChainScaleMultiplier, 3, 20 );
            Gizmos.matrix = Matrix4x4.identity;
        }

        public void EditorHandles_RagdollDrawPlaymodePhysicalDummyLines( float alpha = 0.5f )
        {
            Handles.color = new Color( 1f, 1f, 1f, alpha );

            for( int i = 0; i < chains.Count; i++ )
            {
                float hue = (float)i / (float)chains.Count;
                Color hsv = Color.HSVToRGB( ( hue + 0.2f ) % 1f, 0.7f, 0.8f );
                hsv.a = alpha;
                Handles.color = hsv;
                EditorHandles_DrawChainAsLines( chains[i], 3f );
            }

            Handles.color = new Color( 1f, 1f, 1f, alpha );

            for( int i = 0; i < chains.Count; i++ )
            {
                EditorHandles_DrawConnectionBonesAsLines( chains[i], 3f );
            }

            Handles.color = Color.white;
        }

        public void EditorHandles_DrawChainAsLines( RagdollBonesChain chain, float width = 3f )
        {
            if( chain.PlaymodeInitialized )
            {
                for( int i = 0; i < chain.BoneSetups.Count; i++ )
                {
                    var bone = chain.BoneSetups[i];
                    Handles.SphereHandleCap( 0, bone.PhysicalDummyBone.position, Quaternion.identity, 0.025f, EventType.Repaint );

                    if( i == chain.BoneSetups.Count - 1 ) return;
                    Handles.DrawAAPolyLine( width, bone.PhysicalDummyBone.position, chain.BoneSetups[i + 1].PhysicalDummyBone.position );
                }
            }
            else
            {
                for( int i = 0; i < chain.BoneSetups.Count; i++ )
                {
                    var bone = chain.BoneSetups[i];
                    if( bone.SourceBone == null ) continue;

                    Handles.SphereHandleCap( 0, bone.SourceBone.position, Quaternion.identity, 0.025f, EventType.Repaint );

                    if( i == chain.BoneSetups.Count - 1 ) return;
                    Handles.DrawAAPolyLine( width, bone.SourceBone.position, chain.BoneSetups[i + 1].SourceBone.position );
                }
            }
        }

        public void EditorHandles_DrawConnectionBonesAsLines( RagdollBonesChain chain, float width = 3f )
        {
            if( chain.BoneSetups.Count == 0 ) return;
            if( chain.BoneSetups[0].PhysicalDummyBone == null ) return;
            if( chain.BoneSetups[0].PhysicalDummyBone.parent == null ) return;

            if( chain.BoneSetups[0].PhysicalDummyBone.parent.position != Vector3.zero && chain.BoneSetups[0].PhysicalDummyBone.parent != Dummy_Container )
                Handles.DrawAAPolyLine( width, chain.BoneSetups[0].PhysicalDummyBone.position, chain.BoneSetups[0].PhysicalDummyBone.parent.position );

            if( chain.ParentConnectionBones == null ) return;

            for( int i = 0; i < chain.ParentConnectionBones.Count; i++ )
            {
                var bone = chain.ParentConnectionBones[i];
                if( bone.DummyBone == null ) continue;
                if( bone.DummyBone.parent == null ) break;
                Handles.DrawAAPolyLine( width, bone.DummyBone.position, bone.DummyBone.parent.position );
            }
        }

        public void EditorHandles_RagdollDrawBoneChain( RagdollBonesChain chain, float alpha = 0.6f )
        {
            float len = CalculateScaleReferenceValue();

            if( chain.BoneSetups.Count == 0 ) return;

            float boneWdth = 1f;

            if( chain.ChainType == ERagdollChainType.Unknown ) Handles.color = new Color( 1f, 1f, 0.2f, alpha );
            else if( chain.ChainType == ERagdollChainType.Core ) Handles.color = new Color( 1f, 0.4f, 0.2f, alpha );
            else if( chain.ChainType.IsArm() ) Handles.color = new Color( 0.4f, 1f, 0.2f, alpha );
            else if( chain.ChainType.IsLeg() ) { Handles.color = new Color( 0.2f, 0.4f, 1f, alpha ); boneWdth = 0.6f; }
            else if( chain.ChainType == ERagdollChainType.OtherLimb ) Handles.color = new Color( 0.2f, 1f, 1f, alpha );

            // Draw main bones
            for( int i = 0; i < chain.BoneSetups.Count - 1; i++ )
            {
                if( chain.BoneSetups[i].SourceBone == null ) continue;
                if( chain.BoneSetups[i].SourceBone.parent == null ) continue;
                if( chain.BoneSetups[i + 1].SourceBone == null ) continue;

                FGUI_Handles.DrawBoneHandle( chain.BoneSetups[i].SourceBone.position, chain.BoneSetups[i + 1].SourceBone.position, .6f * boneWdth, false, 3, .8f, 4f, 0.4f );
                Handles.SphereHandleCap( 0, chain.BoneSetups[i].SourceBone.position, Quaternion.identity, len * 0.04f, EventType.Repaint );
            }

            // Draw Last Bone
            var lastBone = chain.BoneSetups[chain.BoneSetups.Count - 1];
            if( lastBone.SourceBone == null ) return;

            Transform furtherBone = SkeletonRecognize.GetContinousChildTransform( lastBone.SourceBone );
            Handles.SphereHandleCap( 0, lastBone.SourceBone.position, Quaternion.identity, len * 0.04f, EventType.Repaint );

            if( furtherBone != null )
                FGUI_Handles.DrawBoneHandle( lastBone.SourceBone.position, furtherBone.position, .6f * boneWdth, false, 3, .8f, 4f, 0.4f );

            Handles.color = new Color( 1f, 0.7f, 0.7f, 0.3f );

            // Draw connection ragdoll bone
            if( chain.BoneSetups[0].SourceBone != null )
            {
                var conn = DummyStructure_FindConnectionBone( chain );
                if( conn != null )
                {
                    EditorHandles_DrawAALineFromTo( chain.BoneSetups[0].SourceBone, conn.SourceBone, 4f, 1f );
                    Handles.SphereHandleCap( 0, conn.SourceBone.position, Quaternion.identity, len * 0.03f, EventType.Repaint );
                }
            }

            // Check Missing Bones
            if( chain.BoneSetups[0].SourceBone == null ) return;

            Transform parentCheck = lastBone.SourceBone;
            Transform childCheck;

            while( parentCheck != null /*|| ( parentCheck.parent == chain.BoneSetups[0].SourceBone)*/)
            {
                childCheck = parentCheck;
                parentCheck = parentCheck.parent;

                var rBoneParent = chain.GetParent( lastBone );
                if( rBoneParent == null ) break;
                if( parentCheck != rBoneParent.SourceBone )
                {
                    if( parentCheck && childCheck )
                    {
                        FGUI_Handles.DrawBoneHandle( parentCheck.position, childCheck.position, 1f, false, 0.5f * boneWdth, 0.8f );
                        Handles.SphereHandleCap( 0, parentCheck.position, Quaternion.identity, len * 0.01f, EventType.Repaint );
                    }
                }
                else lastBone = rBoneParent;
            }
        }

        public void EditorHandles_DrawBonesFromTo( Transform startChild, Transform endParent, float alpha = 0.3f )
        {
            if( SkeletonRecognize.IsChildOf( startChild, endParent ) == false ) return;

            Transform ch = startChild;
            Transform p;

            while( ch != null )
            {
                p = ch;
                ch = ch.parent;

                if( p == null ) break;
                if( ch == null ) break;
                FGUI_Handles.DrawBoneHandle( ch.position, p.position, 1f, false, 0.5f, 0.8f );

                if( ch == endParent ) return;
            }
        }

        public void EditorHandles_DrawAALineFromTo( Transform startChild, Transform endParent, float width = 3f, float alpha = 0.3f )
        {
            if( startChild == null || endParent == null ) return;

            if( SkeletonRecognize.IsChildOf( startChild, endParent ) == false )
            {
                Handles.DrawAAPolyLine( width, startChild.position, endParent.position );
                return;
            }

            Transform ch = startChild; Transform p;

            while( ch != null )
            {
                p = ch; ch = ch.parent;
                if( p == null ) break; if( ch == null ) break;
                Handles.DrawAAPolyLine( width, ch.position, p.position );
                if( ch == endParent ) return;
            }
        }

        public void EditorHandles_DrawBoneMassIndicator( RagdollChainBone bone, RagdollBonesChain chain, Vector3 direction, Vector3 sideDir )
        {
            if( bone.SourceBone == null ) return;
            if( chain.ParentHandler == null ) return;

            Vector3 sPos = bone.SourceBone.position;
            Vector3 ePos = sPos + direction * bone.BaseColliderSetup.GetAverageScale( bone ) * 4f;

            float maxMass = chain.ParentHandler.ReferenceMass;
            float mass = (float)System.Math.Round( bone.GetMass( chain ), 1 );

            GUIContent title = new GUIContent(/*bone.SourceBone.name + " - " +*/ mass + " = " + Mathf.Round( ( mass / maxMass ) * 100f ) + "%", "Rigidbody mass for the bone." );

            Handles.DrawLine( sPos, ePos );

            if( chain.ChainType.IsLeft() )
            {
                ePos += sideDir * ( EditorStyles.label.CalcSize( title ).x * 0.0125f * HandleUtility.GetHandleSize( ePos ) );
            }

            Handles.Label( ePos, title );
        }

        public static int _Editor_selectedModuleIndex = -1;

        private void EditorHandles_DrawExtraFeaturesGizmos()
        {
            if( ExtraFeatures.ContainsIndex( _Editor_selectedModuleIndex ) )
            {
                if( ExtraFeatures[_Editor_selectedModuleIndex].FeatureReference == null ) return;
                if( _Editor_selectedModuleIndex >= ExtraFeatures.Count ) return;
                if( ExtraFeatures[_Editor_selectedModuleIndex].ActiveFeature == null ) return;
                ExtraFeatures[_Editor_selectedModuleIndex].ActiveFeature.Editor_OnSceneGUI( this, ExtraFeatures[_Editor_selectedModuleIndex] );
            }
        }

#endif
    }
}