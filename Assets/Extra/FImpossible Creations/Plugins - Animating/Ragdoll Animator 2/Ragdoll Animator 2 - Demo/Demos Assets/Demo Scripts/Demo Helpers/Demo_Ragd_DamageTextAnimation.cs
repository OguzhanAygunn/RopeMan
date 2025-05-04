using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_DamageTextAnimation : MonoBehaviour
    {
        public Vector3 OffsetPos = new Vector3( 0, 20f, 0f );
        public float Duration = 0.6f;
        public float DestroyAfter = 1.4f;
        private Vector3 StartPos;
        private float elapsed = 0f;

        private void Start()
        {
            StartPos = transform.position;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.LerpUnclamped( StartPos, StartPos + OffsetPos, EaseOutElastic( 0f, 1f, Mathf.Min( 1f, elapsed / Duration ) ) );

            if( elapsed > DestroyAfter ) Destroy( gameObject );
        }

        public static float EaseOutElastic( float start, float end, float value )
        {
            end -= start;

            float d = 1f;
            float p = d * .3f;
            float s;
            float a = 0;

            if( value == 0 ) return start;

            if( ( value /= d ) == 1 ) return start + end;

            if( a == 0f || a < Mathf.Abs( end ) )
            {
                a = end;
                s = p * 0.25f;
            }
            else
            {
                s = p / ( 2 * Mathf.PI ) * Mathf.Asin( end / a );
            }

            return ( a * Mathf.Pow( 2, -10 * value ) * Mathf.Sin( ( value * d - s ) * ( 2 * Mathf.PI ) / p ) + end + start );
        }
    }
}