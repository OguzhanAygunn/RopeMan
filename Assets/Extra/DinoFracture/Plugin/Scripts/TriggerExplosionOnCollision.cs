using DinoFracture;
using UnityEngine;
using System.Threading;
using System.Collections;

namespace DinoFracture
{
    /// <summary>
    /// Triggers a fracture + explosion when this game object is
    /// collided with.
    /// 
    /// This script does not need to be applied on a fracturing game object.
    /// </summary>
    public class TriggerExplosionOnCollision : MonoBehaviour
    {
        /// <summary>
        /// List of explosions to trigger
        /// </summary>
        [UnityEngine.Tooltip("List of explosions to trigger")]
        public FractureGeometry[] Explosives;

        /// <summary>
        /// The force behind the explosions
        /// </summary>
        [UnityEngine.Tooltip("The force behind the explosions")]
        public float Force;

        /// <summary>
        /// The radius of the explosions
        /// </summary>
        [UnityEngine.Tooltip("The radius of the explosions")]
        public float Radius;

        private void OnCollisionEnter(Collision col)
        {
            for (int i = 0; i < Explosives.Length; i++)
            {
                if (Explosives[i] != null && Explosives[i].gameObject.activeSelf)
                {
                    // This ensures OnFracture() will be called on us
                    Explosives[i].Fracture().SetCallbackObject(this);
                }
            }
        }

        /// <summary>
        /// Automatically called by FractureEngine when fracturing is complete
        /// </summary>
        /// <param name="args"></param>
        private void OnFracture(OnFractureEventArgs args)
        {
            if (args.IsValid)
            {
                Explode(args.FracturePiecesRootObject, args.OriginalMeshBounds, args.OriginalObject.transform.localScale);
            }
        }

        private void Explode(GameObject root, Bounds bounds, Vector3 scale)
        {
            Vector3 adjLocalCenter = new Vector3(bounds.center.x * scale.x, bounds.center.y * scale.y, bounds.center.z * scale.z);

            Vector3 center = root.transform.localToWorldMatrix.MultiplyPoint(adjLocalCenter);
            Transform rootTrans = root.transform;
            for (int i = 0; i < rootTrans.childCount; i++)
            {
                Transform pieceTrans = rootTrans.GetChild(i);
                Rigidbody body = pieceTrans.GetComponent<Rigidbody>();
                if (body != null)
                {
                    Vector3 forceVector = (pieceTrans.position - center);
                    float dist = forceVector.magnitude;

                    // Normalize the vector and scale it by the explosion radius
                    forceVector *= Mathf.Max(0.0f, Radius - dist) / (Radius * dist);
                    
                    // Scale by the force amount
                    forceVector *= Force;

                    body.AddForceAtPosition(forceVector, center);
                }
            }
        }
    }
}