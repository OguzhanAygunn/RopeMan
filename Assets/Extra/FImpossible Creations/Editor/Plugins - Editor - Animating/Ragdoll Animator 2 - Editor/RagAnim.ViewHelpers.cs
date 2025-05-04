using FIMSpace.FEditor;
using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollAnimator2Editor
    {
        [HideInInspector] public Object DemosPackage;
        [HideInInspector] public Object UserManualFile;
        [HideInInspector] public Object ExtraFeaturesDirectory;
        [HideInInspector] public Object AssemblyDefinitions;
        [HideInInspector] public Object AssemblyDefinitionsAll;

        public static Object GetExtraFeaturesDirectory;

        protected Color preC;
        protected Color preBG;
        protected float width;

        public static GUIStyle BGInBoxStyle
        { get { if( __inBoxStyle != null ) return __inBoxStyle; __inBoxStyle = new GUIStyle(); __inBoxStyle.border = new RectOffset( 4, 4, 4, 4 ); __inBoxStyle.padding = new RectOffset( 10, 10, 6, 6 ); __inBoxStyle.margin = new RectOffset( 0, 0, 0, 0 ); return __inBoxStyle; } }
        private static GUIStyle __inBoxStyle = null;
        public static Texture2D Tex_Pixel
        { get { if( __texpixl != null ) return __texpixl; __texpixl = new Texture2D( 1, 1 ); __texpixl.SetPixel( 0, 0, Color.white ); __texpixl.Apply( false, true ); return __texpixl; } }
        private static Texture2D __texpixl = null;
        public static GUIStyle StyleColorBG
        { get { if( __StlcolBG != null ) { if( __StlcolBG.normal.background != null ) return __StlcolBG; } __StlcolBG = new GUIStyle( EditorStyles.label ); Texture2D bg = Tex_Pixel; __StlcolBG.focused.background = bg; __StlcolBG.active.background = bg; __StlcolBG.normal.background = bg; __StlcolBG.border = new RectOffset( 0, 0, 0, 0 ); return __StlcolBG; } }
        private static GUIStyle __StlcolBG = null;
        protected Texture _tex_ragdoll
        { get { return RagdollHandlerEditor._tex_ragdollSmall; } }

        private void GUI_Prepare()
        {
            preC = GUI.color;
            preBG = GUI.backgroundColor;
            width = EditorGUIUtility.currentViewWidth;
        }

        /// <summary> Begin horizontal </summary>
        private void BH( GUIStyle style = null )
        {
            if( style == null )
                EditorGUILayout.BeginHorizontal();
            else
                EditorGUILayout.BeginHorizontal( style );
        }

        /// <summary> End horizontal </summary>
        private void EH()
        { EditorGUILayout.EndHorizontal(); }

        /// <summary> Begin horizontal </summary>
        private void BV( GUIStyle style = null )
        {
            if( style == null )
                EditorGUILayout.BeginHorizontal();
            else
                EditorGUILayout.BeginHorizontal( style );
        }

        /// <summary> End horizontal </summary>
        private void EV()
        { EditorGUILayout.EndVertical(); }

        private bool Button_Refresh()
        {
            _requestRepaint = true;
            return GUILayout.Button( FGUI_Resources.Tex_Refresh, FGUI_Resources.ButtonStyle, GUILayout.Height( 18 ), GUILayout.Width( 23 ) );
        }

        private void Helper_Header( string title, Texture tex )
        {
            EditorGUILayout.BeginHorizontal( FGUI_Resources.ViewBoxStyle );
            if( tex != null ) EditorGUILayout.LabelField( new GUIContent( tex ), GUILayout.Height( 18 ), GUILayout.Width( 20 ) );
            GUILayout.Space( 3 );
            EditorGUILayout.LabelField( title, FGUI_Resources.HeaderStyle );
            GUILayout.Space( 3 );
            if( tex != null ) EditorGUILayout.LabelField( new GUIContent( tex ), GUILayout.Height( 18 ), GUILayout.Width( 20 ) );
            EditorGUILayout.EndHorizontal();
        }

        public static Color selCol = new Color( 0.2f, 1f, 0.3f, 1f );
        public static readonly Color selCol1 = new Color( 0.2f, 1f, 0.3f, 1f );
        public static readonly Color selCol2 = new Color( 0.5f, 0.6f, 1.1f, 1f );
        public static readonly Color selCol3 = new Color( 0.3f, .75f, 1f, 1f );

        public void DrawCategoryButton( RagdollHandler.ERagdollAnimSection target, bool iconOnPlaymode = false )
        {
            if( target == RagdollHandler.ERagdollAnimSection.Construct ) DrawCategoryButton( target, _tex_ragdoll, ( iconOnPlaymode && Application.isPlaying ) ? "" : " Construct" );
            else if( target == RagdollHandler.ERagdollAnimSection.Setup ) DrawCategoryButton( target, FGUI_Resources.Tex_GearSetup, ( iconOnPlaymode && Application.isPlaying ) ? "" : target.ToString() );
            else if( target == RagdollHandler.ERagdollAnimSection.Motion ) DrawCategoryButton( target, FGUI_Resources.TexMotionIcon, target.ToString() );
            else if( target == RagdollHandler.ERagdollAnimSection.Extra ) DrawCategoryButton( target, FGUI_Resources.Tex_Module, target.ToString() );
        }

        protected void DrawCategoryButton( RagdollHandler.ERagdollAnimSection target, Texture icon, string lang )
        {
            if( Get._EditorCategory == target ) GUI.backgroundColor = selCol;
            int height = 28;
            int lim = Application.isPlaying ? 280 : 387;

            if( EditorGUIUtility.currentViewWidth > lim && lang != "" )
            {
                if( GUILayout.Button( new GUIContent( "  " + Lang( lang ), icon ), FGUI_Resources.ButtonStyle, GUILayout.Height( height ) ) )
                {
                    Get.Editor_HandlesUndoRecord();
                    if( Get._EditorCategory == target ) Get._EditorCategory = RagdollHandler.ERagdollAnimSection.None; else Get._EditorCategory = target;
                    if( GUI.backgroundColor == selCol && Event.current.button == 1 ) Get._EditorCategory -= 10;
                    _requestRepaint = true;
                }
            }
            else
            {
                int maxW = 500;
                if( lang == "" ) maxW = 56;

                if( GUILayout.Button( new GUIContent( icon, Lang( lang ) ), FGUI_Resources.ButtonStyle, GUILayout.Height( height ), GUILayout.MaxWidth( maxW ) ) )
                {
                    Get.Editor_HandlesUndoRecord();
                    if( Get._EditorCategory == target ) Get._EditorCategory = RagdollHandler.ERagdollAnimSection.None; else Get._EditorCategory = target;
                    if( GUI.backgroundColor == selCol && Event.current.button == 1 ) Get._EditorCategory -= 10; _requestRepaint = true;
                }
            }

            GUI.backgroundColor = preBG;
        }

        protected void SelectMainCategory( RagdollHandler.ERagdollAnimSection category )
        {
            if( category == Get._EditorCategory ) return;
            Get._EditorCategory = category;

            if( category == RagdollHandler.ERagdollAnimSection.Construct )
            {
                Get.ValidateReferencePose();
            }
        }

        protected string Lang( string s )
        {
            return s;
        }

        protected void RedrawScene()
        {
            SceneView.RepaintAll();
        }
    }
}