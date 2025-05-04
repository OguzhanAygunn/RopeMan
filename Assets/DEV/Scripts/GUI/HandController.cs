using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandController : MonoBehaviour
{
    public static HandController instance;
    public static HandController Instance { get { return instance; } }

    [Title("Main")]
    [SerializeField] bool visibility=false;
    [SerializeField] Camera cam;
    [SerializeField] CanvasGroup group;
    [SerializeField] Transform handTrs;
    [SerializeField] Image handImage;
    [SerializeField] float moveSpeed;

    [Space(6)]

    [Title("Tap")]
    [SerializeField] bool onTap;
    [SerializeField] Vector3 tapScale;
    [SerializeField] float tapSpeed;


    private Vector3 defaultScale;
    private void Awake()
    {
        instance = (!instance) ? this : instance;
        defaultScale = handTrs.localScale;
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        SetVisibility(active: true, force: true).Forget();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetVisibility(active: !visibility).Forget();
        }


        Movement();
        TapScaleController();
    }

    private void TapScaleController()
    {
        if (Input.GetMouseButtonDown(0))
            onTap = true;
        else if (Input.GetMouseButtonUp(0))
            onTap = false;

        Vector3 scale = handTrs.localScale;
        Vector3 targetScale = onTap ? tapScale : Vector3.one;

        scale = Vector3.Lerp(scale, targetScale, tapSpeed * Time.deltaTime);
        handTrs.localScale = scale;
    }

    private void Movement()
    {
        if (!visibility)
            return;

        Vector3 targetPos = Input.mousePosition;
        Vector3 pos = handTrs.position;

        pos = Vector3.Lerp(pos, targetPos, moveSpeed * Time.deltaTime);
        handTrs.position = pos;
    }

    public static async UniTaskVoid SetVisibility(bool active,bool force =false,float delay=0)
    {
        if (!PlayerController.instance.IsAlive)
            return;
        instance.visibility = active;
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        float duration = force ? 0f : 0.5f;
        float alphaVal = active ? 1f : 0f;
        AnimationCurve curve = CurveManager.GetCurve("Hand Visible");
        instance.group.DOFade(alphaVal, duration).SetEase(curve);

    }
}
