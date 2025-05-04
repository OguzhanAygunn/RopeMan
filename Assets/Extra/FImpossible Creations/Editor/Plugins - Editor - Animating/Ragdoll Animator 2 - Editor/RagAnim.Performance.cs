using UnityEditor;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollAnimator2Editor
    {
        private long _perf_totalT = 0;
        private long _perf_lastMin = 0;
        private long _perf_lastMax = 0;
        private dynamic _perf_totalMS = 0;
        private int _perf_totalSteps = 0;

        protected void DrawPerformance()
        {
            rGet._Editor_Perf_FixedUpdate.Editor_DisplayFoldoutButton( -9, -5 );
            if( rGet._Editor_Perf_FixedUpdate._foldout )
            {
                bool upd = rGet._Editor_Perf_Update.Editor_DisplayAlways( "Preparation:" );
                rGet._Editor_Perf_LateUpdate.Editor_DisplayAlways( "Pre-Processing:" );
                rGet._Editor_Perf_FixedUpdate.Editor_DisplayAlways( "Main Algorithm:" );

                if( upd )
                {
                    _perf_totalT = 0;
                    _perf_totalT += rGet._Editor_Perf_Update.AverageTicks;
                    _perf_totalT += rGet._Editor_Perf_LateUpdate.AverageTicks;
                    _perf_totalT += rGet._Editor_Perf_FixedUpdate.AverageTicks;
                    _perf_totalMS = 0;
                    _perf_totalMS += rGet._Editor_Perf_Update.AverageMS;
                    _perf_totalMS += rGet._Editor_Perf_LateUpdate.AverageMS;
                    _perf_totalMS += rGet._Editor_Perf_FixedUpdate.AverageMS;

                    _perf_totalSteps += 1;
                    if( _perf_totalSteps > 6 )
                    {
                        if( _perf_totalT > _perf_lastMax ) _perf_lastMax = _perf_totalT;
                        if( _perf_totalT < _perf_lastMin ) _perf_lastMin = _perf_totalT;
                    }
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField( "Total = " + _perf_totalT + " ticks  " + _perf_totalMS + "ms" );
                GUILayout.Space( 8 );
                if( _perf_lastMax != long.MinValue && _perf_lastMin != long.MaxValue ) EditorGUILayout.LabelField( "Min = " + _perf_lastMin + " ticks  :   Max = " + _perf_lastMax + " ticks" );
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}