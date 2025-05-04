using FIMSpace.FProceduralAnimation;
using System.Collections;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_OptimalDeathClicker : MonoBehaviour
    {
        public float Impact = 4f;
        public LayerMask RaycastMask;

        private void Update()
        {
            if( Input.GetMouseButtonDown( 0 ) )
            {
                Ray r = Camera.main.ScreenPointToRay( Input.mousePosition );
                RaycastHit hit;

                if( Physics.Raycast( r, out hit, Mathf.Infinity, RaycastMask, QueryTriggerInteraction.Collide ) )
                {
                    var rag = hit.transform.GetComponent<RagdollAnimator2>();
                    if( rag )
                    {
                        rag.Handler.Mecanim.enabled = false;
                        rag.enabled = true;

                        // Wait for components to generate
                        StartCoroutine( CallImpulse( rag ) );
                    }
                }
            }
        }

        private IEnumerator CallImpulse( RagdollAnimator2 rag )
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            foreach( var chain in rag.Handler.Chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    Rigidbody rig = bone.SourceBone.GetComponent<Rigidbody>();
                    if( rig == null ) continue;
                    RagdollHandlerUtilities.ApplyLimbImpact( rig, Camera.main.transform.forward * Impact, ForceMode.Force );
                }
            }
        }
    }
}