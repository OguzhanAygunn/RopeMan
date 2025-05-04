using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;
    public static TimeManager Instance { get { return instance; } }
    [SerializeField] List<TimeEffectInfo> effects;

    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }

    public void SetTimeScale(float endVal = 1,float duration=0.5f, float delay = 0)
    {
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, endVal, duration).SetDelay(delay);
    }

    [Button(size: ButtonSizes.Large)]
    public void PlayTimeAnim(string id)
    {
        TimeEffectInfo effect = instance.effects.Find(e => e.id == id);

        if (effect == null)
            return;

        effect.Play();
    }

    public async void PlayTimeAnim(string id,float delay)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        TimeEffectInfo effect = instance.effects.Find(e => e.id == id);

        if (effect == null)
            return;

        effect.Play();
    }
}


[System.Serializable]
public class TimeEffectInfo
{
    [Title("Main")]
    public string id;
    public float timeVal;
    public float duration;
    public float delay;

    [Space(6)]

    [Title("Return")]
    public bool returnTime;
    public float returnDelay;
    public float returnDuration;
    public void Play()
    {
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, timeVal, duration).SetDelay(delay).OnComplete( () =>
        {
            if (returnTime)
            {
                DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0, returnDuration).SetDelay(returnDelay);
            }
        });
    }
}

