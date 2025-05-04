using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RengeGames.HealthBars;
using Sirenix.OdinInspector;
using DG.Tweening;

public class TensionUIController : MonoBehaviour
{
    public static TensionUIController instance;
    public static TensionUIController Instance { get { return instance; } }

    [Title("Main")]
    [SerializeField] PosTracker posTracker;
    [SerializeField] RadialSegmentedHealthBar tensionBar;
    [SerializeField] float targetTension;
    [SerializeField] float tensionSpeed;

    [Space(6)]

    [Title("Parent")]
    [SerializeField] Transform parent;
    [SerializeField] float yPos;
    [SerializeField] float idleDuration;
    private AnimationCurve idleCurve;
    private Vector3 defaultScale;

    private void Awake()
    {
        instance = (!instance) ? this : instance;
        defaultScale = transform.localScale;
        SetVisibility(active: false, force: true);
    }
    private void Start()
    {
        idleCurve = CurveManager.GetCurve("Tension Idle Anim");
        IdleAnim();
    }
    public void Update()
    {
        TensionController();
    }

    private void IdleAnim()
    {
        parent.DOMoveY(yPos, idleDuration).SetEase(idleCurve).OnComplete( () =>
        {
            parent.DOMoveY(-yPos, idleDuration).SetEase(idleCurve).OnComplete(() =>
            {
                IdleAnim();
            });
        });
    }


    private void TensionController()
    {
        float tension = tensionBar.RemoveSegments.Value;
        float target = (targetTension + TouchManager.Meter);
        

        tension = Mathf.Lerp(tension, target, tensionSpeed * Time.deltaTime);
        tensionBar.RemoveSegments.Value = tension;
    }


    [Button(size: ButtonSizes.Large)]
    public void SetVisibility(bool active,bool force=false)
    {
        float duration = force ? 0 : 0.4f;
        Vector3 targetScale = active ? defaultScale : Vector3.zero;
        transform.DOScale(targetScale, duration).SetEase(Ease.Linear);
        posTracker.SetActive(active: !active);
    }
}
