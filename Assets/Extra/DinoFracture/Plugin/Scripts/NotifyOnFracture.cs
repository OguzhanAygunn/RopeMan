using UnityEngine;
using System.Collections;

namespace DinoFracture
{
    /// <summary>
    /// When added to the same game object as the FractureGeometry, this script can be used to notify
    /// external game objects of this object’s fracture completion. The external objects need a script
    /// with the “OnFracture” callback method.
    /// </summary>
    public class NotifyOnFracture : MonoBehaviour
    {
        /// <summary>
        /// The array of game objects to notify.  They do not need to be in this object’s tree.
        /// </summary>
        [UnityEngine.Tooltip("The array of game objects to notify.  They do not need to be in this object’s tree.")]
        public GameObject[] GameObjects = new GameObject[1];

        private void OnFracture(OnFractureEventArgs args)
        {
            if (args.IsValid && args.OriginalObject.gameObject == gameObject)
            {
                for (int i = 0; i < GameObjects.Length; i++)
                {
                    if (GameObjects[i] != null)
                    {
                        GameObjects[i].SendMessage("OnFracture", args, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }
    }
}
