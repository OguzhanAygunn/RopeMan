using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectController : MonoBehaviour
{
    public static PlayerEffectController instance;
    public static PlayerEffectController Instance { get { return instance; } }
    [SerializeField] Renderer mr;

    private Vector3 defaultScale;
    private Color defaultColor;

    [Title("Ready Effect")]
    [SerializeField] Transform spine;
    [SerializeField] Color readyColor;
    [SerializeField] float readyEffectDuration;
    private void Awake()
    {
        instance = (!instance) ? this : instance;

        defaultScale = spine.transform.localScale;
        defaultColor = mr.material.color;
    }



    bool readyEffect = false;
    public void ReadyEffect(bool useDelay=true)
    {
        if (readyEffect)
            return;

        readyEffect = true;
        
        //Material Effect
        mr.material.DOColor(readyColor,readyEffectDuration).SetEase(Ease.Unset).OnComplete( () =>
        {
            float delay = useDelay ? readyEffectDuration : 0;
            mr.material.DOColor(defaultColor, readyEffectDuration).SetDelay(delay).OnComplete( () =>
            {
                
            });
        });


        //Size Effect
        Vector3 scaleA = defaultScale * 1.5f;
        Vector3 scaleB = defaultScale * 0.8f;
        spine.DOScale(scaleA, readyEffectDuration).SetEase(Ease.Linear).OnComplete(() =>
        {
            spine.DOScale(scaleB, readyEffectDuration).SetEase(Ease.Linear).OnComplete(() =>
            {
                spine.DOScale(defaultScale, readyEffectDuration).SetEase(Ease.Linear).OnComplete( () =>
                {
                    readyEffect = false;
                });
            });
        });
    }


}
