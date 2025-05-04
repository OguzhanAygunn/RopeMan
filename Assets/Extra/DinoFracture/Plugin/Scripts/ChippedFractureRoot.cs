using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DinoFracture
{
    public class ChippedFractureRoot : MonoBehaviour
    {
        internal int Id;
        internal int SubId;

        // Forward collision events to the sub pieces
        private void OnCollisionEnter(Collision collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                var contact = collision.GetContact(i);
                if (contact.thisCollider.gameObject != gameObject)
                {
                    contact.thisCollider.gameObject.SendMessage("OnCollisionEnter", collision, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}