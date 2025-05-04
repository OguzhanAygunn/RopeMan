using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartLineController : MonoBehaviour
{
    public static StartLineController instance;
    public static StartLineController Instance { get { return instance; } }

    [Title("Main")]
    [SerializeField] bool visibility;
    [SerializeField] CanvasGroup group;

    [Space(6)]

    [Title("Transform")]
    [SerializeField] Transform targetObj;
    [SerializeField] PosTracker posTracker;

    [Space(6)]

    [Title("Text")]
    [SerializeField] Transform startTrs;
    [SerializeField] TextMeshProUGUI startText;

    [Space(6)]

    [Title("Lines")]
    [SerializeField] List<Image> lineImages;
    [SerializeField] Transform lineParent;

    private Vector3 defaultPos;

    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        defaultPos = transform.position;
        targetObj = new GameObject("Start Line Target Pos").transform;
        targetObj.transform.position = transform.position;
        AssignTrackerTarget();
        TextAnim();
    }

    private void Update()
    {
        FadeController();
    }

    private void FadeController()
    {
        if (!visibility)
            return;

        float targetFade = TouchManager.IsSelectedBodyPart ? 0.2f : 1;
        float fade = group.alpha;

        fade = Mathf.Lerp(fade, targetFade, 10f * Time.deltaTime);
        group.alpha = fade;
    }

    private void TextAnim()
    {
        Vector3 startScale = startTrs.localScale;
        Vector3 endScale = startScale * 1.075f;

        AnimationCurve curve = CurveManager.GetCurve("Start Text Scale");

        startTrs.DOScale(endScale, 0.5f).SetEase(curve).OnComplete(() =>
        {
            startTrs.DOScale(startScale, 0.5f).SetEase(curve).OnComplete(() =>
            {
                TextAnim();
            });
        });
    }

    public void AssignTrackerTarget()
    {
        Vector3 pos = PlayerBodyController.SpinePart.transform.position;
        pos -= Vector3.up * 4f;
        pos.x = transform.position.x;
        pos.z = transform.position.z;

        targetObj.position = pos;
        posTracker.AssignTarget(targetObj);
    }

    public void UpdateTargetObjPos(Transform newPos)
    {
        Vector3 pos = newPos.position;
        pos.x = defaultPos.x;
        pos.z = defaultPos.z;
        pos.y -= 2f;
        targetObj.transform.position = pos;
    }


    public static async UniTaskVoid SetVisibility(bool active,float delay)
    {
        instance.posTracker.SetActive(active: active);
        Transform bodyTrs = PlayerBodyController.SpinePart.transform;
        instance.UpdateTargetObjPos(bodyTrs);
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        instance.visibility = active;

        instance.SetActiveLines(active: active);
        instance.SetActiveText(active: active);
    }

    public void SetActiveText(bool active)
    {
        float targetFadeValue = active ? 1 : 0;
        float duration = active ? 0.25f : 0.1f;
        startText.DOFade(targetFadeValue, duration).SetEase(Ease.Linear);
    }

    public async void SetActiveLines(bool active,float duration = 0.2f,float delay = 0.1f,float perDelay = 0.03f)
    {
        if (active)
        {
            lineImages.ForEach(image => image.transform.localScale = Vector3.zero);
            await UniTask.Delay(TimeSpan.FromSeconds(delay));

            lineImages.ForEach(image =>
            {
                float _delay = lineImages.IndexOf(image) * perDelay;
                AnimationCurve curve = CurveManager.GetCurve("Start Line Scale");
                image.transform.DOScale(Vector3.one, duration).SetEase(curve).SetDelay(_delay);
            });
        }
        else
        {
            lineImages.ForEach(image => image.transform.localScale = Vector3.zero);
        }


    }

}
