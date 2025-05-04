using UnityEngine;

namespace DinoFracture
{
    /// <summary>
    /// This component will cause a fracture to happen at the point of impact.
    /// </summary>
    [RequireComponent(typeof(FractureGeometry))]
    public class FractureOnCollision : MonoBehaviour
    {
        /// <summary>
        /// The minimum amount of force required to fracture this object.
        /// Set to 0 to have any amount of force cause the fracture.
        /// </summary>
        [UnityEngine.Tooltip("The minimum amount of force required to fracture this object. Set to 0 to have any amount of force cause the fracture.")]
        public float ForceThreshold;

        /// <summary>
        /// Falloff radius for transferring the force of the impact
        /// to the resulting pieces. Any piece outside of this falloff
        /// from the point of impact will have no additional impulse
        /// set on it.
        /// </summary>
        [UnityEngine.Tooltip("Falloff radius for transferring the force of the impact to the resulting pieces. Any piece outside of this falloff from the point of impact will have no additional impulse set on it.")]
        public float ForceFalloffRadius = 1.0f;

        /// <summary>
        /// If true and this is a kinematic body, an impulse will be
        /// applied to the colliding body to counter the effects of
        /// hitting a kinematic body. If false and this is a kinematic
        /// body, the colliding body will bounce off as if this were an
        /// unmovable wall.
        /// </summary>
        [UnityEngine.Tooltip("If true and this is a kinematic body, an impulse will be applied to the colliding body to counter the effects of' hitting a kinematic body. If false and this is a kinematic body, the colliding body will bounce off as if this were an unmovable wall.")]
        public bool AdjustForKinematic = true;

        /// <summary>
        /// The collision layers that are allowed to cause a fracture.
        /// </summary>
        [UnityEngine.Tooltip("The collision layers that are allowed to cause a fracture.")]
        public LayerMask CollidableLayers = (LayerMask)int.MaxValue;

        private Vector3 _impactImpulse;
        private float _impactMass;
        private Vector3 _impactPoint;
        private Rigidbody _impactBody;

        private FractureGeometry _fractureGeometry;
        private Rigidbody _thisBody;
        private float _thisMass;

        private bool _fireFracture = false;

        public void CopyFrom(FractureOnCollision other)
        {
            ForceThreshold = other.ForceThreshold;
            ForceFalloffRadius = other.ForceFalloffRadius;
            AdjustForKinematic = other.AdjustForKinematic;
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

        private void OnCollisionEnter(Collision col)
        {
            if (!_fractureGeometry.IsProcessingFracture && !_fireFracture && col.contactCount > 0)
            {
                if ((CollidableLayers.value & (1 << col.gameObject.layer)) != 0)
                {
                    _impactBody = col.rigidbody;
                    _impactMass = (col.rigidbody != null) ? col.rigidbody.mass : 0.0f;

                    _impactPoint = Vector3.zero;

                    float sumSeparation = 0.0f;
                    Vector3 avgNormal = Vector3.zero;
                    for (int i = 0; i < col.contactCount; i++)
                    {
                        var contact = col.GetContact(i);
                        if (contact.thisCollider.gameObject == gameObject)
                        {
                            float separation = Mathf.Max(1e-3f, contact.separation);

                            _impactPoint += contact.point * separation;
                            avgNormal -= contact.normal * separation;
                            sumSeparation += separation;
                        }
                    }
                    _impactPoint *= 1.0f / sumSeparation;
                    avgNormal = avgNormal.normalized;

                    _impactImpulse = -avgNormal * col.impulse.magnitude;

                    float forceMag = 0.5f * _impactImpulse.sqrMagnitude;
                    if (forceMag >= ForceThreshold)
                    {
                        _fireFracture = true;
                    }
                    else
                    {
                        _impactMass = 0.0f;
                    }
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
            if (args.IsValid && args.OriginalObject.gameObject == gameObject && _impactMass > 0.0f)
            {
                Vector3 thisImpulse = _impactImpulse * _thisMass / (_thisMass + _impactMass);

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

                if (AdjustForKinematic)
                {
                    // If the fractured body is kinematic, the collision for the colliding body will
                    // be as if it hit an unmovable wall.  Try to correct for that by adding the same
                    // force to colliding body.
                    if (_thisBody != null && _thisBody.isKinematic && _impactBody != null)
                    {
                        Vector3 impactBodyImpulse = _impactImpulse * _impactMass / (_thisMass + _impactMass);
                        _impactBody.AddForceAtPosition(impactBodyImpulse, _impactPoint, ForceMode.Impulse);
                    }
                }
            }
        }
    }
}
