using UnityEngine;
using System.Collections;

namespace DinoFracture
{
    /// <summary>
    /// Adding this to the fracturing game object will allow
    /// other game objects to be turned off (set inactive) when
    /// this game object is fractured.
    /// </summary>
    public class DisableObjectsOnFracture : MonoBehaviour
    {
        public GameObject [] ObjectsToDisable;

        private void OnFracture(OnFractureEventArgs e)
        {
            for (int i = 0; i < ObjectsToDisable.Length; i++)
            {
                if (ObjectsToDisable[i] != null)
                {
                    ObjectsToDisable[i].SetActive(false);
                }
            }
        }
    }
}
