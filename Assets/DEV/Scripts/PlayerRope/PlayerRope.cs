using Cysharp.Threading.Tasks;
using DG.Tweening;
using Obi;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRope : MonoBehaviour
{
    public PlayerRopeBall BallController { get { return ballController; } }

    [Title("Main")]
    [SerializeField] RopeDirection ropeDirection;
    public RopeDirection RopeDirection { get { return ropeDirection; } }
    [SerializeField] bool active;
    [SerializeField] bool visibility;
    [SerializeField] ObiRope rope;
    [SerializeField] ObiRopeExtrudedRenderer ropeRenderer;
    [SerializeField] Transform connectTransform;
    [SerializeField] PlayerRopeBall ballController;
    [SerializeField] Renderer myRenderer;

    [Space(6)]

    [Title("Effect")]
    [SerializeField] Vector2 defaultUVScale; 
    [SerializeField] Vector2 targetUVScale;
    [SerializeField] float scaleDuration;


    private float defaultScale;
    // Start is called before the first frame update


    private void Awake()
    {
        defaultScale = ropeRenderer.thicknessScale;


        defaultUVScale = ropeRenderer.uvScale;
    }

    void Start()
    {
    }


    bool uvScaleEffect = false;
    public async UniTaskVoid RopeUVScaleEffect(float delay = 0)
    {
        if (uvScaleEffect)
            return;

        uvScaleEffect = true;
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        DOTween.To(() => ropeRenderer.uvScale, x => ropeRenderer.uvScale = x, targetUVScale, scaleDuration).SetEase(Ease.Linear).OnComplete(() => {
            DOTween.To(() => ropeRenderer.uvScale, x => ropeRenderer.uvScale = x, defaultUVScale, scaleDuration).SetEase(Ease.Linear).OnComplete( () =>
            {
                uvScaleEffect = false;
            });
        });
    }

    public async UniTaskVoid SetVisibility(bool active, float duration = 0.3f, bool force = false, float delay = 0)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));
        visibility = active;

        duration = force ? 0 : duration;
        float targetScale = active ? 1 : 0;

        if (!active)
        {
            ballController.SetFreeBall(active: true, delay: 0).Forget();
        }
        else
        {
        }


        DOTween.To(() => rope.stretchingScale, x => rope.stretchingScale = x, targetScale, duration).OnComplete( () => { 
                myRenderer.enabled = active;
        });
        ballController.SetVisibility(active: active, duration: duration).Forget();
    }

    public void HitRope()
    {
        ballController.Hit();
    }

}
