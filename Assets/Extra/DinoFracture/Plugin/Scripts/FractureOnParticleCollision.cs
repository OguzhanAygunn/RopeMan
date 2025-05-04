using System.Collections.Generic;
using UnityEngine;

namespace DinoFracture
{
    /// <summary>
    /// This component will cause a fracture to happen at the point of impact with a particle.
    /// 
    /// Note: Particle collisions won't be activated unless "Send Collision Messages" is checked.
    /// </summary>
    [RequireComponent(typeof(FractureGeometry))]
    public class FractureOnParticleCollision : MonoBehaviour
    {
        /// <summary>
        /// The minimum velocity of incoming particles required to fracture this object.
        /// Set to 0 to have any amount of velocity cause the fracture.
        /// </summary>
        [UnityEngine.Tooltip("The minimum velocity of incoming particles required to fracture this object. Set to 0 to have any amount of velocity cause the fracture.")]
        public float VelocityThreshold;

        /// <summary>
        /// The mass of an individual particle to allow transferring of forces to the fracture pieces.
        /// If zero, no forces will be transferred.
        /// </summary>
        [UnityEngine.Tooltip("The mass of an individual particle to allow transferring of forces to the fracture pieces. If zero, no forces will be transferred.")]
        public float ParticleMass = 1e-4f;

        /// <summary>
        /// Falloff radius for transferring the force of the impact
        /// to the resulting pieces. Any piece outside of this falloff
        /// from the point of impact will have no additional impulse
        /// set on it.
        /// </summary>
        [UnityEngine.Tooltip("Falloff radius for transferring the force of the impact to the resulting pieces. Any piece outside of this falloff from the point of impact will have no additional impulse set on it.")]
        public float ForceFalloffRadius = 1.0f;

        /// <summary>
        /// The collision layers of the particle system that are allowed to cause a fracture.
        /// </summary>
        [UnityEngine.Tooltip("The collision layers of the particle system that are allowed to cause a fracture.")]
        public LayerMask CollidableLayers = (LayerMask)int.MaxValue;

        private Vector3 _impactVelocity;
        private Vector3 _impactPoint;
        private Vector3 _impactNormal;

        private FractureGeometry _fractureGeometry;
        private Rigidbody _thisBody;
        private float _thisMass;
        private float _particleMass;

        private bool _fireFracture = false;

        private List<ParticleCollisionEvent> _collisions;

        public void CopyFrom(FractureOnParticleCollision other)
        {
            VelocityThreshold = other.VelocityThreshold;
            ForceFalloffRadius = other.ForceFalloffRadius;
            CollidableLayers = other.CollidableLayers;
        }

        private void Awake()
        {
            _fractureGeometry = GetComponent<FractureGeometry>();
            _thisBody = GetComponentInParent<Rigidbody>();

            if (_thisBody != null)
            {
                _thisMass = _thisBody.mass;
            }
        }

        private void OnParticleCollision(GameObject other)
        {
            if (!_fractureGeometry.IsProcessingFracture && !_fireFracture && other.TryGetComponent(out ParticleSystem otherPS))
            {
                if ((CollidableLayers.value & (1 << other.gameObject.layer)) != 0)
                {
                    if (_collisions == null)
                    {
                        _collisions = new List<ParticleCollisionEvent>();
                    }

                    _impactPoint = Vector3.zero;
                    _impactVelocity = Vector3.zero;
                    _impactNormal = Vector3.zero;
                    _particleMass = 0.0f;

                    int colCount = otherPS.GetCollisionEvents(gameObject, _collisions);
                    if (colCount > 0)
                    {
                        for (int i = 0; i < colCount; i++)
                        {
                            _impactPoint += _collisions[i].intersection;
                            _impactVelocity += _collisions[i].velocity;
                            _impactNormal += _collisions[i].normal;
                        }

                        float invColCount = (1.0f / colCount);
                        _impactVelocity *= invColCount;

                        if (_impactVelocity.sqrMagnitude > (VelocityThreshold * VelocityThreshold))
                        {
                            // Can fracture
                            _fireFracture = true;

                            _particleMass = ParticleMass;
                            _impactPoint *= invColCount;
                            _impactNormal.Normalize();
                        }
                    }

                    _collisions.Clear();
                }
            }
        }

        private void Update()
        {
            if (_fireFracture)
            {
                _fireFracture = false;

                Vector3 localPoint = transform.worldToLocalMatrix.MultiplyPoint(_impactPoint);
                _fractureGeometry.FractureType = FractureType.Shatter;
                _fractureGeometry.Fracture(localPoint);
            }
        }

        private void OnFracture(OnFractureEventArgs args)
        {
            if (args.IsValid && args.OriginalObject.gameObject == gameObject)
            {
                if (_particleMass > 0 && _thisMass > 0)
                {
                    // Assume a completely inelastic collision
                    float reducedMass = (_particleMass * _thisMass) / (_particleMass + _thisMass);
                    Vector3 thisImpulse = _impactNormal * (Vector3.Dot(_impactVelocity, _impactNormal) * reducedMass * _thisMass);

                    for (int i = 0; i < args.FracturePiecesRootObject.transform.childCount; i++)
                    {
                        Transform piece = args.FracturePiecesRootObject.transform.GetChild(i);

                        Rigidbody rb = piece.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            float percentForce = FractureUtilities.GetFracturePieceRelativeMass(piece.gameObject);

                            if (ForceFalloffRadius > 0.0f)
                            {
                                float dist = (piece.position - _impactPoint).magnitude;
                                percentForce *= Mathf.Clamp01(1.0f - (dist / ForceFalloffRadius));
                            }

                            rb.AddForce(thisImpulse * percentForce, ForceMode.Impulse);
                        }
                    }
                }
            }
        }
    }
}
