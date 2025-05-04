using FIMSpace.AnimationTools;
using FIMSpace.FEditor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public static partial class RagdollHandlerEditor
    {
        /// <summary> Returns true if found lacking bones in chain </summary>
        public static bool GUI_DrawBonesSetupTransformsList( SerializedProperty ragdollHandlerProp, RagdollHandler handler, SerializedProperty boneSetups, RagdollBonesChain chain, bool initialChains = false )
        {
            bool hasLackingBones = false;
            EditorGUILayout.BeginVertical( FGUI_Resources.BGInBoxBlankStyle );

            bool preEn = GUI.enabled;

            if( chain.BoneSetups.Count > 0 )
            {
                int toRemove = -1;

                if( chain.ChainType != ERagdollChainType.Core && chain.ChainType.IsLeg() == false && chain.BoneSetups[0].SourceBone )
                {
                    if( chain.ChainType.IsArm() && chain.BoneSetups.Count >= 4 )
                    {
                        // Shoulder Already
                    }
                    else
                    {
                        GUILayout.Space( -6 );

                        GUI.color = new Color( 1f, 1f, 1f, 0.6f );

                        var fBone = chain.BoneSetups[0];
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space( EditorGUIUtility.currentViewWidth * 0.2f );
                        EditorGUILayout.LabelField( new GUIContent( "◊", "Bone skipped in ragdoll structure" ), GUILayout.Width( 12 ), GUILayout.Height( 14 ) );
                        EditorGUILayout.ObjectField( fBone.SourceBone.parent, typeof( Transform ), true, GUILayout.Width( EditorGUIUtility.currentViewWidth * 0.3f ), GUILayout.Height( 14 ) );

                        if( GUILayout.Button( new GUIContent( "+", "Add lacking bone as physical dummy bone" ), FGUI_Resources.ButtonStyle, GUILayout.Height( 12 ) ) )
                        {
                            var nBone = new RagdollChainBone();
                            nBone.SourceBone = fBone.SourceBone.parent;
                            chain.BoneSetups.Insert( 0, nBone );

                            nBone.TryDoAutoSettings( handler, chain );
                            
                            if( chain.ChainType.IsArm() )
                            {
                                if( chain.ChainType == ERagdollChainType.LeftArm ) nBone.BoneID = ERagdollBoneID.LeftShoulder;
                                else nBone.BoneID = ERagdollBoneID.RightShoulder;
                            }

                            OnChange( ragdollHandlerProp, handler );
                        }

                        GUILayout.Space( ( EditorGUIUtility.currentViewWidth - 30 ) * 0.3f );

                        EditorGUILayout.EndHorizontal();

                        GUILayout.Space( 5 );

                        GUI.color = Color.white;
                    }
                }

                for( int i = 0; i < boneSetups.arraySize; i++ )
                {
                    var bone = chain.BoneSetups[i];
                    var boneProp = boneSetups.GetArrayElementAtIndex( i );
                    var mProp = boneProp.Copy();
                    boneProp.Next( true );

                    bool lackingParent = ( i > 0 && bone.SourceBone && chain.BoneSetups[i - 1].SourceBone != bone.SourceBone.parent );

                    if( lackingParent )
                    {
                        int depth = SkeletonRecognize.GetDepth( bone.SourceBone, chain.BoneSetups[i - 1].SourceBone ) - 1;
                        if( depth < 5 )
                        {
                            GUILayout.Space( -4 );
                            GUI.enabled = false;

                            int addAt = -1;
                            Transform addTr = null;

                            for( int j = 0; j < depth; j++ )
                            {
                                hasLackingBones = true;
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space( EditorGUIUtility.currentViewWidth * 0.2f );
                                EditorGUILayout.LabelField( new GUIContent( "◊", "Bone skipped in ragdoll structure" ), GUILayout.Width( 12 ), GUILayout.Height( 14 ) );
                                EditorGUILayout.ObjectField( SkeletonRecognize.GetParent( bone.SourceBone, ( depth ) - ( j + 1 ) ), typeof( Transform ), true, GUILayout.Width( EditorGUIUtility.currentViewWidth * 0.3f ), GUILayout.Height( 14 ) );

                                GUI.enabled = preEn;
                                if( GUILayout.Button( new GUIContent( "+", "Add lacking bone as physical dummy bone" ), FGUI_Resources.ButtonStyle, GUILayout.Height( 12 ) ) ) 
                                { 
                                    addAt = i; 
                                    addTr = SkeletonRecognize.GetParent( bone.SourceBone, ( depth ) - ( j + 1 ) );
                                }
                                GUI.enabled = false;

                                GUILayout.Space( ( EditorGUIUtility.currentViewWidth - 30 ) * 0.3f );

                                EditorGUILayout.EndHorizontal();
                            }

                            if( addAt > -1 )
                            {
                                var nBone = new RagdollChainBone();
                                nBone.SourceBone = addTr;
                                chain.BoneSetups.Insert( addAt, nBone );

                                nBone.TryDoAutoSettings(handler, chain);

                                OnChange( ragdollHandlerProp, handler );
                            }

                            GUI.enabled = preEn;
                            GUILayout.Space( 4 );
                        }
                    }

                    GUI.enabled = true;

                    EditorGUILayout.BeginHorizontal();

                    if( chain.ChainType == ERagdollChainType.Core && i == 0 )
                    {
                        GUILayout.Space( 16 );
                        EditorGUILayout.LabelField( new GUIContent( FGUI_Resources.FindIcon( "Fimp/Small Icons/Anchor" ), "This bone is treated as main-connection bone for the whole body." ), GUILayout.Width( 24 ) );
                    }
                    else
                    {
                        GUILayout.Space( 20 );

                        if( GUILayout.Button( new GUIContent( i.ToString(), "Index of the bone in the chain" ), GUILayout.Width( 20 ) ) )
                        {
                            GenericMenu_BonesSetupOperations( handler, ragdollHandlerProp, chain, i );
                        }

                        GUILayout.Space( 6 );
                    }

                    if( initialChains == false )
                        if( GUILayout.Button( FGUI_Resources.TexTargetingIcon, FGUI_Resources.ButtonStyle, GUILayout.Width( 22 ), GUILayout.Height( 18 ) ) ) { GUI_Contruct_SoloEditBone( chain, bone ); handler._Editor_ChainCategory = EBoneChainCategory.Colliders; }
                    GUILayout.Space( 4 );

                    if( GUILayout.Button( FGUI_Resources.Tex_Bone, EditorStyles.label, GUILayout.Width( 22 ), GUILayout.Height( 18 ) ) )
                    {
                        GenericMenu_BonesSetupOperations( handler, ragdollHandlerProp, chain, i );
                    }

                    GUILayout.Space( 6 );

                    if( !boneProp.objectReferenceValue ) GUI.backgroundColor = new Color( 1f, 0.6f, 0.15f, 1f );

                    if( bone.PhysicalDummyBone ) GUI.enabled = false;
                    EditorGUILayout.PropertyField( boneProp, GUIContent.none, true );
                    FGUI_Inspector.RestoreGUIBackground();

                    if( bone.PhysicalDummyBone )
                    {
                        EditorGUIUtility.labelWidth = 22;
                        EditorGUILayout.ObjectField( new GUIContent( FGUI_Resources.Tex_Physics, "Physical dummy bone reference" ), bone.PhysicalDummyBone, typeof( Transform ), true );
                        EditorGUIUtility.labelWidth = 0;
                    }

                    GUI.enabled = true;

                    EditorGUIUtility.labelWidth = 6;
                    int wdth = bone.PhysicalDummyBone == null ? 120 : 50;
                    EditorGUILayout.PropertyField( mProp.FindPropertyRelative( "BoneID" ), new GUIContent( " ", mProp.tooltip ), GUILayout.MaxWidth( wdth ) );
                    EditorGUIUtility.labelWidth = 0;

                    GUILayout.Space( 6 );

                    GUI.enabled = preEn;

                    FGUI_Inspector.RedGUIBackground();
                    if( GUILayout.Button( FGUI_Resources.GUIC_Remove, FGUI_Resources.ButtonStyle, GUILayout.Height( 18 ), GUILayout.Width( 22 ) ) ) toRemove = i;
                    FGUI_Inspector.RestoreGUIBackground();

                    GUILayout.Space( 30 );

                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space( 8 );

                    //if (lackingParent)
                    //{
                    //    EditorGUILayout.BeginHorizontal();
                    //    EditorGUILayout.EndHorizontal();
                    //}
                }

                if( toRemove > -1 )
                {
                    chain.BoneSetups.RemoveAt( toRemove );
                    OnChange( ragdollHandlerProp, handler );
                }
            }

            EditorGUILayout.EndVertical();

            return hasLackingBones;
        }

        private static void EditorUtility_MoveBone<T>( int oldIndex, int newIndex, List<T> list )
        {
            if( oldIndex == newIndex ) return;
            if( oldIndex >= list.Count ) oldIndex -= list.Count;
            if( newIndex >= list.Count ) newIndex -= list.Count;

            var pre = list[oldIndex];

            if( newIndex == 0 )
            {
                list.Remove( pre );
                list.Insert( 0, pre );
                return;
            }

            if( newIndex == list.Count - 1 )
            {
                list.Remove( pre );
                list.Insert( list.Count - 1, pre );
                return;
            }

            var toSwap = list[newIndex];

            list[newIndex] = pre;
            list[oldIndex] = toSwap;
        }
    }
}