using DinoFracture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DinoFractureDemo
{
    [Serializable]
    public class ImportantGameObjectData
    {
        public GameObject GameObject;
        private string _gameObjectPath;

        [TextArea(2, 6)]
        public string Description;

        public string GameObjectPath => _gameObjectPath;

        public void ResolveGameObjectPath()
        {
            _gameObjectPath = GetGameObjectPath(GameObject);
        }

        private static string GetGameObjectPath(GameObject gameObject)
        {
            List<string> parts = new List<string>();

            var loopGO = gameObject;
            while (loopGO != null)
            {
                parts.Add(loopGO.name);

                var parent = loopGO.transform.parent;
                loopGO = (parent != null) ? parent.gameObject : null;
            }

            if (string.IsNullOrEmpty(gameObject.scene.name))
            {
                parts.Add("<Prefab>");
            }
            else
            {
                parts.Add("<Scene>");
            }

            parts.Reverse();
            return string.Join(" / ", parts);
        }
    }

    public class ImportantGameObjectsPanel : MonoBehaviour
    {
        [SerializeField] private Transform _objectList;
        [SerializeField] private ItemNameAndDescriptionPanel _objDescriptionTemplate;

        public void SetImportantGameObjects(ImportantGameObjectData[] data)
        {
            for (int i = 0; i < _objectList.childCount; i++)
            {
                Destroy(_objectList.GetChild(i).gameObject);
            }

            for (int i = 0; i < data.Length; i++)
            {
                AddItem(data[i]);
            }
        }

        private void AddItem(ImportantGameObjectData data)
        {
            GameObject objGO = Instantiate(_objDescriptionTemplate.gameObject);
            objGO.transform.SetParent(_objectList, false);

            ItemNameAndDescriptionData dataItem = new ItemNameAndDescriptionData();
            dataItem.ItemName = data.GameObjectPath;
            dataItem.Description = data.Description;

            objGO.GetComponent<ItemNameAndDescriptionPanel>().SetItemNameAndDescription(dataItem);
        }
    }
}
