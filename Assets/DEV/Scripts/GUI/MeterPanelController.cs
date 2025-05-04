using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MeterPanelController : MonoBehaviour
{
    public static MeterPanelController instance;
    public static MeterPanelController Instance { get { return instance; } }

    [Title("Main")]
    [SerializeField] bool visibility;
    [SerializeField] CanvasGroup group;

    [Space(6)]


    [Title("Particle")]
    [SerializeField] bool particleActive;
    [SerializeField] ParticleSystem particle;
    [SerializeField] int particleTriggerMt;


    [Space(6)]

    [Title("Icon")]
    [SerializeField] Transform iconTrs;
    [SerializeField] Transform iconParent;
    [SerializeField] List<Image> iconImages;
    [SerializeField] Gradient activeGradient;

    

    [Space(6)]

    [Header("Icon Size Anim")]
    [SerializeField] bool iconSizeAnimActive;
    [SerializeField] Vector3 defaultIconScale;
    [SerializeField] Vector3 targetIconScale;
    [SerializeField] float speedIconScaleAnim;

    [Space(6)]

    [Title("Meter")]
    [SerializeField] Transform meterParentTransform;
    [SerializeField] Transform meterTransform;
    [SerializeField] TextMeshProUGUI meterTmpro;
    [SerializeField] Gradient meterGradient;
    [SerializeField] float gradientSpeed;
    [SerializeField] Color meterTargetColor; 

    [Space(6)]

    [Header("Meter Idle Anim")]
    [SerializeField] Vector3 defaultIdleAnimPos;
    [SerializeField] Vector3 idleAnimPos;
    [SerializeField] float idleAnimDuration;
    private Vector3 targetIdleAnimPos;

    [Space(6)]

    [Header("Meter Size Anim")]
    [SerializeField] bool meterSizeAnimActive = false;
    [SerializeField] Vector3 defaultMeterSizeAnimScale;
    [SerializeField] Vector3 targetMeterSizeAnimScale;
    [SerializeField] float meterScaleSpeed;

    private GUIPosController posController;
    private bool updateActive = true;
    private void Awake()
    {
        instance = (!instance) ? this : instance;
        posController = GetComponent<GUIPosController>();
        defaultMeterSizeAnimScale = meterTransform.localScale;
        SetVisibility(active: false, force: true, duration: 0.4f, delay: 0).Forget();
    }


    private void Start()
    {
        Init();
    }


    public void Init()
    {
        targetIdleAnimPos = idleAnimPos;
        PlayMeterIdleAnim();
        particle.Stop(withChildren: true);
        particle.Clear(withChildren: true);
    }

    private void Update()
    {
        if (!updateActive)
            return;
        MeterSizeAnimController();
        IconSizeAnimController();
        MeterTextUpdate();
        ParticleController();
        MeterColorController();
    }

    private void MeterSizeAnimController()
    {

        //Size
        Vector3 scale = iconTrs.localScale;
        Vector3 targetScale = iconSizeAnimActive ? targetIconScale: defaultIconScale;
        float speed = particleActive ? speedIconScaleAnim * 3 : speedIconScaleAnim;
        scale = Vector3.MoveTowards(scale, targetScale, speedIconScaleAnim * Time.deltaTime);
        iconTrs.localScale = scale;
        if (scale == targetScale) { iconSizeAnimActive = !iconSizeAnimActive; }
    }

    private void MeterColorController()
    {
        Color color = meterTmpro.color;
        Color targetColor = particleActive ? meterTargetColor : Color.white;

        color = Color.Lerp(color, targetColor, Time.deltaTime);

        meterTmpro.color = color;
    }

    private void IconSizeAnimController()
    {
        Vector3 scale = meterTransform.localScale;
        Vector3 targetScale = meterSizeAnimActive ? targetMeterSizeAnimScale : defaultMeterSizeAnimScale;

        float speed = particleActive ? meterScaleSpeed * 3 : meterScaleSpeed;

        scale = Vector3.MoveTowards(scale, targetScale, speed * Time.deltaTime);

        meterTransform.localScale = scale;
        if (scale == targetScale && meterSizeAnimActive)
        {
            SetActiveMeterSizeAnim(active: false);
        }
    }

    public void SetActiveMeterSizeAnim(bool active)
    {
        meterSizeAnimActive = active;
    }

    private void PlayMeterIdleAnim()
    {
        AnimationCurve startCurve = CurveManager.GetCurve("Meter Start Pos");
        AnimationCurve endCurve = CurveManager.GetCurve("Meter End Pos");


        meterTransform.DOLocalMove(idleAnimPos, idleAnimDuration).SetEase(startCurve).OnComplete( () =>
        {
            meterTransform.DOLocalMove(defaultIdleAnimPos, idleAnimDuration).SetEase(endCurve).OnComplete(() =>
            {
                PlayMeterIdleAnim();
            });
        });
    }


    [Button(size:ButtonSizes.Large)]
    public static async UniTaskVoid SetVisibility(bool active,bool force = false,float duration = 0.3f,float delay = 0)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));
        instance.visibility = active;
        instance.particle.Stop(withChildren: true);
        float targetVal = active ? 1 : 0;
        if (instance.visibility)
        {
            instance.updateActive = true;
            instance.group.DOFade(endValue: targetVal, duration: duration);
        }
        else
        {
            Transform trs = instance.transform;
            Vector3 startScale = trs.localScale;
            Vector3 endScale = startScale * 1.5f;
            trs.localScale = startScale;
            //AnimationCurve curve = CurveManager.GetCurve("Meter Panel Size");
            instance.updateActive = false;
            if (force)
            {
                instance.group.DOFade(endValue: targetVal, duration: 0).SetEase(Ease.Linear).SetDelay(0);
            }
            else
            {
                instance.meterTmpro.DOColor(Color.yellow, 0.5f);
                trs.DOScale(endScale, 0.8f).SetEase(Ease.OutElastic).OnComplete(() =>
                {
                    instance.group.DOFade(endValue: targetVal, duration: duration).SetEase(Ease.Linear).SetDelay(0.5f).OnComplete( () =>
                    {
                        instance.meterTmpro.color = Color.white;
                        trs.localScale = startScale;
                    });
                });
            }

        }
    }


    private void MeterTextUpdate()
    {
        int meter = (int)TouchManager.Meter;
        string meterText = meter.ToString() + "m";
        meterTmpro.text = meterText;
    }


    private void ParticleController()
    {
        if (!visibility)
            return;


        bool active = TouchManager.Meter > particleTriggerMt;


        if (active)
        {
            if (!particleActive)
            {
                particle.Play(withChildren: true);
                particleActive = true;
            }
        }
        else
        {
            if (particleActive)
            {
                particle.Stop(withChildren: true);
                particleActive = false;
            }
        }
    }

}
