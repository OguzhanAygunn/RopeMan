using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DinoFracture.Editor
{
    static class MeshUtilities
    {
        private static float GetTriangleVolume(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            float v123 = v1.x * v2.y * v3.z;
            float v132 = v1.x * v3.y * v2.z;

            float v213 = v2.x * v1.y * v3.z;
            float v231 = v2.x * v3.y * v1.z;

            float v321 = v3.x * v2.y * v1.z;
            float v312 = v3.x * v1.y * v2.z;

            return (v123 - v132 - v213 + v231 + v312 - v321) / 6.0f;
        }

        public static float Volume(this UnityEngine.Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;
            int[] tris = mesh.triangles;

            float volume = 0.0f;
            for (int i = 0; i < tris.Length; i += 3)
            {
                Vector3 v1 = vertices[tris[i]];
                Vector3 v2 = vertices[tris[i + 1]];
                Vector3 v3 = vertices[tris[i + 2]];
                volume += GetTriangleVolume(v1, v2, v3);
            }
            return Mathf.Abs(volume);
        }

        public static float BoundsVolume(this UnityEngine.Mesh mesh)
        {
            var bounds = mesh.bounds;
            return bounds.size.x * bounds.size.y * bounds.size.z;
        }
    }

    static class Utilities
    {
        public static void PrintStats<DataType>(string statsName, IEnumerable<DataType> items, Func<DataType, float> statFunc)
        {
            int count = 0;

            float largestVal = 0.0f;
            float smallestVal = float.MaxValue;

            float sumVals = 0.0f;
            foreach (var item in items)
            {
                if (item != null)
                {
                    float value = statFunc(item);
                    sumVals += value;
                    
                    largestVal = Mathf.Max(value, largestVal);
                    smallestVal = Mathf.Min(value, smallestVal);

                    count++;
                }
            }
            float avgVal = sumVals / count;

            float variance = 0.0f;
            foreach (var item in items)
            {
                if (item != null)
                {
                    float diff = statFunc(item) - avgVal;
                    variance += diff * diff;
                }
            }
            float stdDev = Mathf.Sqrt(variance);

            Debug.Log($"{statsName} Stats: [Diff Smallest & Largest: {largestVal - smallestVal}] [Std Dev: {stdDev}] [Avg: {avgVal}]");
        }
    }
}
