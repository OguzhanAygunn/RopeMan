using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    [DefaultExecutionOrder( -100 )]
    public class Demo_Ragd_DuplicateAtAwake : FimpossibleComponent
    {
        public int CopiesX = 4;
        public int CopiesZ = 2;
        public float Separation = 2f;
        public GameObject ToCopy;

        private void Awake()
        {
            Duplicate();
        }

        public void Duplicate()
        {
            int copyX = Mathf.RoundToInt( CopiesX / 2 );
            int copyZ = Mathf.RoundToInt( CopiesZ / 2 );

            for( int x = -copyX; x <= copyX; x++ )
            {
                for( int z = -copyZ; z <= copyZ; z++ )
                {
                    GameObject newObject = Instantiate( ToCopy );
                    newObject.transform.position = transform.position + new Vector3( x * Separation, 0f, z * Separation );
                }
            }

            Physics.SyncTransforms();
        }
    }
}