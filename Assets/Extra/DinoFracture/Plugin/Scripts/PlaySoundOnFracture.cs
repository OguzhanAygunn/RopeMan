using UnityEngine;
using System.Collections;

namespace DinoFracture
{
    /// <summary>
    /// An object with this component will play the audio source when fractured.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class PlaySoundOnFracture : MonoBehaviour
    {
        private void OnFracture(OnFractureEventArgs args)
        {
            if (args.IsValid && args.OriginalObject.gameObject == gameObject)
            {
                AudioSource ourSrc = GetComponent<AudioSource>();
                if (ourSrc.clip != null)
                {
                    AudioSource.PlayClipAtPoint(ourSrc.clip, transform.position, ourSrc.volume);
                }
            }
        }
    }
}