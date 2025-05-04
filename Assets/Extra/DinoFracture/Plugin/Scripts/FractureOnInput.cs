using DinoFracture;
using UnityEngine;
using System.Collections;

namespace DinoFracture
{
    /// <summary>
    /// Apply this on the fracturing game object. When the specified
    /// key is pressed, the object will fracture.
    /// </summary>
    [RequireComponent(typeof(FractureGeometry))]
    public class FractureOnInput : MonoBehaviour
    {
        public KeyCode Key;

        void Update()
        {
            if (Input.GetKeyDown(Key))
            {
                GetComponent<FractureGeometry>().Fracture();
            }
        }
    }
}