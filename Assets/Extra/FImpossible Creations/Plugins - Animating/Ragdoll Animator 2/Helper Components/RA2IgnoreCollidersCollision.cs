using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    [AddComponentMenu( "FImpossible Creations/Ragdoll Animator/Ignore Collision Between Colliders", 111 )]
    public class RA2IgnoreCollidersCollision : FimpossibleComponent
    {
        public List<Collider> AColliders = new List<Collider>();
        public List<Collider> BColliders = new List<Collider>();
        public List<Collider> IgnoreEachCollision = new List<Collider>();

        private void Start()
        {
            foreach( var aColl in AColliders )
            {
                foreach( var bColl in BColliders )
                {
                    Physics.IgnoreCollision( aColl, bColl, true );
                }
            }

            foreach( var aColl in AColliders )
            {
                foreach( var oColl in IgnoreEachCollision ) Physics.IgnoreCollision( aColl, oColl, true );
            }

            foreach( var bColl in BColliders )
            {
                foreach( var oColl in IgnoreEachCollision ) Physics.IgnoreCollision( bColl, oColl, true );
            }

            foreach( var oColl in IgnoreEachCollision )
            {
                foreach( var oColl2 in IgnoreEachCollision ) Physics.IgnoreCollision( oColl2, oColl, true );
            }
        }

#if UNITY_EDITOR
        public override string HeaderInfo => "Basic helper component to trigger collisions ignore on runtime";
#endif
    }
}