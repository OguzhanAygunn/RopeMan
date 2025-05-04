using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    /// <summary>
    /// Collection of access methods for the extension methods use cases
    /// </summary>
    public partial class RagdollHandler
    {
        internal Vector3 anchorToRootLocal { get; private set; } = Vector3.zero;
        internal Quaternion anchorToRootLocalRot { get; private set; } = Quaternion.identity;
    }
}