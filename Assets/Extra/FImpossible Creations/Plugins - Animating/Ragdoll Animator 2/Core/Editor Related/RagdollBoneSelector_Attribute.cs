using UnityEngine;

namespace FIMSpace.FProceduralAnimation
{
    public class RagdollBoneSelectorAttribute : PropertyAttribute
    {
        public string ragdollProperty = "";

        public RagdollBoneSelectorAttribute(string ragdollAnimatorVariableName)
        {
            ragdollProperty = ragdollAnimatorVariableName;
        }
    }
}