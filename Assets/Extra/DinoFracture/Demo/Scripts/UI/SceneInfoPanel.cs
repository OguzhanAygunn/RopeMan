using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649 // Field never set

namespace DinoFractureDemo
{
    public class SceneInfoPanel : MonoBehaviour
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Transform _descriptionParagraphList;
        [SerializeField] private Transform _scriptList;

        [SerializeField] private Text _descriptionTemplate;
        [SerializeField] private ItemNameAndDescriptionPanel _compDescriptionTemplate;

        public void SetPanelDescription(PanelDescriptionAsset desc)
        {
            if (_titleText != null)
            {
                _titleText.text = desc.Title;
            }

            if (_descriptionParagraphList != null)
            {
                for (int i = 0; i < _descriptionParagraphList.childCount; i++)
                {
                    Destroy(_descriptionParagraphList.GetChild(i).gameObject);
                }

                var paragraphs = desc.Description.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < paragraphs.Length; i++)
                {
                    GameObject descGO = Instantiate(_descriptionTemplate.gameObject);
                    descGO.transform.SetParent(_descriptionParagraphList, false);

                    descGO.GetComponent<Text>().text = paragraphs[i];
                }
            }

            if (_scriptList != null)
            {
                for (int i = 0; i < _scriptList.childCount; i++)
                {
                    Destroy(_scriptList.GetChild(i).gameObject);
                }

                for (int i = 0; i < desc.Scripts.Length; i++)
                {
                    GameObject scriptGO = Instantiate(_compDescriptionTemplate.gameObject);
                    scriptGO.transform.SetParent(_scriptList, false);

                    scriptGO.GetComponent<ItemNameAndDescriptionPanel>().SetItemNameAndDescription(desc.Scripts[i]);
                }
            }
        }
    }
}