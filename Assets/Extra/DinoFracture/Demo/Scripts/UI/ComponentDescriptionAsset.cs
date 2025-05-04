using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649 // Field never set

namespace DinoFractureDemo
{
    [CreateAssetMenu(fileName = "ComponentDescription", menuName = "DinoFractureDemo/ComponentDescriptionAsset", order = 2)]
    public class ComponentDescriptionAsset : ScriptableObject
    {
        public string Name;

        [TextArea(5, 10)]
        public string Description;

        public static implicit operator ItemNameAndDescriptionData(ComponentDescriptionAsset asset)
        {
            return new ItemNameAndDescriptionData() { ItemName = asset.Name, Description = asset.Description };
        }
    }
}