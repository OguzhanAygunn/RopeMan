using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_RandomizeAnimationSpeed : FimpossibleComponent
    {
        public Animator Mecanim;
        public float NoiseSpeed = 0.25f;
        public float MinAnimSpeed = 0.5f;
        public float MaxAnimSpeed = 1.25f;

        private float elapsed = 0f;
        private float offsetx = 0f;
        private float offsety = 0f;

        private void Start()
        {
            offsetx = Random.Range( -100f, 100f );
            offsety = Random.Range( -100f, 100f );
        }

        private void Update()
        {
            elapsed += Time.deltaTime * NoiseSpeed;
            Mecanim.SetFloat( "Animation Speed", Mathf.Lerp( MinAnimSpeed, MaxAnimSpeed, Mathf.PerlinNoise( offsetx + elapsed, offsety + elapsed ) ) );
        }
    }
}