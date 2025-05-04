using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DinoFractureDemo
{
    [CreateAssetMenu(fileName = "ComponentDescription", menuName = "DinoFractureDemo/PanelDescriptionAsset", order = 1)]
    public class PanelDescriptionAsset : ScriptableObject
    {
        public string Title;

        [TextArea(5, 10)]
        public string Description;

        public ComponentDescriptionAsset[] Scripts;
    }
}
