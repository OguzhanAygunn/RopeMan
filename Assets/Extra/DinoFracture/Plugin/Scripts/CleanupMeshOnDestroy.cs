using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DinoFracture
{
    /// <summary>
    /// Applying this script to the fracture template will ensure
    /// that the generated fracture mesh will be cleaned up properly
    /// when the fracture piece is destroyed.
    /// 
    /// It is always a good idea to add this to the fracture template.
    /// </summary>
    [ExecuteInEditMode]
    public class CleanupMeshOnDestroy : MonoBehaviour
    {
        private bool _isRuntimeAsset = false;

        public void SetIsRuntimeAsset()
        {
            _isRuntimeAsset = true;
        }

        private void OnDestroy()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                DestroyMesh(meshFilter.sharedMesh);
            }

            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                DestroyMesh(meshCollider.sharedMesh);
            }
        }

        private void DestroyMesh(UnityEngine.Mesh mesh)
        {
            if (mesh != null)
            {
                if (!Application.isPlaying)
                {

#if UNITY_EDITOR
                    if (!UnityEditor.AssetDatabase.IsSubAsset(mesh))
                    {
                        DestroyImmediate(mesh);
                    }
#endif
                }
                else
                {
                    if (_isRuntimeAsset)
                    {
                        Destroy(mesh);
                    }
                }
            }
        }
    }
}