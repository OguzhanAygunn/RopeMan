using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOutlineController : MonoBehaviour
{
    public static PlayerOutlineController instance;

    [SerializeField] List<Renderer> renderers;
    [SerializeField] Color activeColor;

    private void Awake()
    {
        instance = (!instance) ? this : instance;
    } 

    [Button(size: ButtonSizes.Large)]
    public async void SetActiveOutlines(bool active,float duration = 0.1f,float delay = 0)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        Color outlineColor = active ? activeColor : Color.black;
        
        foreach(Renderer render in renderers)
        {
            render.material.DOColor(endValue: outlineColor, property: "_OutlineColor", duration: duration);
        }

    }
}
