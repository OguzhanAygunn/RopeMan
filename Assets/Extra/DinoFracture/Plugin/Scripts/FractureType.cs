using UnityEngine;

namespace DinoFracture
{
    /// <summary>
    /// The type of fracture to perform
    /// </summary>
    public enum FractureType
    {
        /// <summary>
        /// Traditional fracture. Divide the
        /// mesh into many random sized pieces.
        /// </summary>
        Shatter,

        /// <summary>
        /// Use one or more user-defined planes
        /// to cut the mesh
        /// </summary>
        Slice
    }
}