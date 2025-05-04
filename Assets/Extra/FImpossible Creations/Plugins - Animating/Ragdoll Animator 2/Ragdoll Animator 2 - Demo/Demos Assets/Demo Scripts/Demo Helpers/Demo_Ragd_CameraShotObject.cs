using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_CameraShotObject : FimpossibleComponent
    {
        public GameObject ToShot;
        public float Velocity = 10f;

        private void Update()
        {
            if( Input.GetMouseButtonDown( 0 ) )
            {
                Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
                RaycastHit hit;

                if( Physics.Raycast( ray.origin, ray.direction, out hit ) )
                {
                    Vector3 targetPos = hit.point;

                    GameObject b = Instantiate( ToShot );
                    Rigidbody r = b.GetComponent<Rigidbody>();

                    Vector3 dir = targetPos - transform.position; dir.Normalize();
                    r.position = transform.position + dir;

                    r.AddForce( dir * Velocity, ForceMode.VelocityChange );
                }
            }
        }
    }
}