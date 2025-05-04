using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DinoFractureDemo
{
    [RequireComponent(typeof(ParticleSystem))]
    public class StartParticleSystemWhenInView : MonoBehaviour
    {
        private ParticleSystem _system;
        private Coroutine _coroutine;

        void Start()
        {
            _system = GetComponent<ParticleSystem>();
            _system.Stop();
        }

        void Update()
        {
            // Check if the system is in the camera's view
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position, Camera.MonoOrStereoscopicEye.Mono);
            bool inView = (viewportPos.x > 0.0f) && (viewportPos.x < 1.0f) && (viewportPos.y > 0.0f) && (viewportPos.y < 1.0f);

            if (_coroutine != null && !inView)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;

                _system.Stop();
            }
            else if (_coroutine == null && inView)
            {
                _coroutine = StartCoroutine(StartParticleSystem());
            }
        }

        private IEnumerator StartParticleSystem()
        {
            yield return new WaitForSecondsRealtime(0.5f);

            _system.Play();
        }
    }
}