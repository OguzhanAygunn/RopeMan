#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
#if UNITY_EDITOR

        private void _Handles_DrawCapsulePoly( Vector3[] verts, float radius, float length, float lineWidth = 4, int vertsPerCap = 20 )
        {
            if( verts == null ) return;

            if( verts.Length != vertsPerCap * 2 )
            {
                verts = new Vector3[vertsPerCap * 2 + 1];
                for( int v = 0; v < verts.Length; v++ ) verts[v] = Vector3.zero;
            }

            // Right Cap

            float step = 180f / (float)( vertsPerCap - 1 );

            for( int v = 0; v < vertsPerCap; v++ )
            {
                float angle = v * step;
                float rad = Mathf.Deg2Rad * angle;

                Vector3 capPos = Vector3.zero;
                capPos.y += Mathf.Cos( rad ) * radius;
                capPos.z += Mathf.Sin( rad ) * radius + length * 0.5f - radius;

                verts[v] = capPos;
            }

            for( int v = vertsPerCap - 1; v >= 0; v-- )
            {
                Vector3 capPos = Vector3.zero;
                capPos.y -= verts[v].y;
                capPos.z -= verts[v].z;

                verts[v + vertsPerCap] = capPos;
            }

            verts[verts.Length - 1] = verts[0];

            Handles.DrawAAPolyLine( lineWidth, verts );

            Color preCol = Handles.color;
            Handles.color *= 0.7f;
            Handles.DrawAAConvexPolygon( verts );
            Handles.color = preCol;
        }

        private void _Handles_DrawBoxPoly( Vector3[] verts, float width, float height, float lineWidth = 4 )
        {
            if( verts == null ) return;

            if( verts.Length != 5 )
            {
                verts = new Vector3[5];
                for( int v = 0; v < verts.Length; v++ ) verts[v] = Vector3.zero;
            }

            verts[0] = new Vector3( -width, -height, 0f );
            verts[1] = new Vector3( width, -height, 0f );
            verts[2] = new Vector3( width, height, 0f );
            verts[3] = new Vector3( -width, height, 0f );
            verts[4] = verts[3];

            verts[verts.Length - 1] = verts[0];

            Handles.DrawAAPolyLine( lineWidth, verts );

            Color preCol = Handles.color;
            Handles.color *= 0.7f;
            Handles.DrawAAConvexPolygon( verts );
            Handles.color = preCol;
        }

        private void _Handles_DrawSpherePoly( Vector3[] verts, float radius, float lineWidth = 4, int sphereVerts = 32 )
        {
            if( verts == null ) return;

            if( verts.Length != sphereVerts )
            {
                verts = new Vector3[sphereVerts + 1];
                for( int v = 0; v < verts.Length; v++ ) verts[v] = Vector3.zero;
            }

            float step = 360f / (float)( sphereVerts - 1 );

            for( int v = 0; v < sphereVerts; v++ )
            {
                float angle = v * step;
                float rad = Mathf.Deg2Rad * angle;

                Vector3 capPos = Vector3.zero;
                capPos.y += Mathf.Cos( rad ) * radius;
                capPos.z += Mathf.Sin( rad ) * radius;

                verts[v] = capPos;
            }

            verts[verts.Length - 1] = verts[0];
            Handles.DrawAAPolyLine( lineWidth, verts );

            Color preCol = Handles.color;
            Handles.color *= 0.7f;
            Handles.DrawAAConvexPolygon( verts );
            Handles.color = preCol;
        }

#endif
    }
}