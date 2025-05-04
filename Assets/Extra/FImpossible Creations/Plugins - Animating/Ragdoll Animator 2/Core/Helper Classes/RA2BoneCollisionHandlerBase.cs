using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    /// <summary> Extension for bone indicator, which adds collision detection for ragdoll physical bones </summary>
    public abstract class RA2BoneCollisionHandlerBase : RagdollAnimator2BoneIndicator
    {
        [Tooltip( "Can be computed only when using collisions collecting (EnableSavingEnteredCollisionsList)\nTrue when any collision happening (including self collision)" )]
        public bool Colliding = false;

        [Tooltip( "If self collisions count should be used to define 'Colliding' state" )]
        public bool UseSelfCollisions = true;

        public readonly List<Transform> Ignores = new List<Transform>();

        public abstract void EnableSavingEnteredCollisionsList();

        public abstract bool IsCollidingWith( Collider collider );

        public abstract bool CollidesWithAnything();

        public abstract Collider GetFirstCollidingCollider();
    }
}