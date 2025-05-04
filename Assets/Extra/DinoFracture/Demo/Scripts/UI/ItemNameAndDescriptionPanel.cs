using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649 // Field never set

namespace DinoFractureDemo
{
    public struct ItemNameAndDescriptionData
    {
        public string ItemName;
        public string Description;
    }

    public class ItemNameAndDescriptionPanel : MonoBehaviour
    {
        [SerializeField] private Text _title;
        [SerializeField] private Transform _descriptionParagraphList;

        [SerializeField] private Text _paragraphTemplate;

        public void SetItemNameAndDescription(in ItemNameAndDescriptionData data)
        {
            _title.text = data.ItemName;

            for (int i = 0; i < _descriptionParagraphList.childCount; i++)
            {
                Destroy(_descriptionParagraphList.GetChild(i).gameObject);
            }

            var paragraphs = data.Description.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < paragraphs.Length; i++)
            {
                GameObject descGO = Instantiate(_paragraphTemplate.gameObject);
                descGO.transform.SetParent(_descriptionParagraphList, false);

                descGO.GetComponent<Text>().text = paragraphs[i];
            }
        }
    }
}