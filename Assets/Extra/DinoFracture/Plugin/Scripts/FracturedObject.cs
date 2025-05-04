using UnityEngine;

namespace DinoFracture
{
    /// <summary>
    /// This class is automatically added by the engine and
    /// is not meant to be added by users.
    /// </summary>
    public class FracturedObject : MonoBehaviour
    {
        public float TotalMass;
        public float TotalVolume;

        public float ThisMass;
        public float ThisVolume;

        public float GetRelativeSize()
        {
            return ThisVolume / TotalVolume;
        }

        public float GetRelativeMass()
        {
            if (TotalMass <= 0.0f)
            {
                return 1.0f;
            }
            return ThisMass / TotalMass;
        }
    }
}