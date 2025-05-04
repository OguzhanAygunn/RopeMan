using FIMSpace.FProceduralAnimation;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_LODColor : FimpossibleComponent
    {
        public RagdollAnimator2 Ragdoll;
        public Material[] turnedOn;
        public Material[] turnedOff;
        public SkinnedMeshRenderer Mesh;

        private Material[] mats = new Material[3];

        private void Update()
        {
            if( Ragdoll.Handler.LODBlend > 0.5f )
            {
                if( mats.Length != turnedOn.Length ) mats = new Material[turnedOn.Length];
                for( int i = 0; i < turnedOn.Length; i++ ) mats[i] = turnedOn[i];
            }
            else
            {
                if( mats.Length != turnedOff.Length ) mats = new Material[turnedOn.Length];
                for( int i = 0; i < turnedOff.Length; i++ ) mats[i] = turnedOff[i];
            }

            Mesh.sharedMaterials = mats;
        }
    }
}