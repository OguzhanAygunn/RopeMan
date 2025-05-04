using DinoFracture;
using UnityEngine;
using System.Collections;

namespace DinoFractureDemo
{
    public class TurnOffLightOnFracture : MonoBehaviour
    {
        public void OnFracture(OnFractureEventArgs fractureRoot)
        {
            var mat = GetComponent<Renderer>().material;
            mat.color = new Color(0.3f, 0.3f, 0.3f, mat.color.a);
            mat.SetColor("_EmissionColor", Color.black);
        }
    }
}