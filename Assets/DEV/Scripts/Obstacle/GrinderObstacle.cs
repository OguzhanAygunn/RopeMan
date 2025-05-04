using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrinderObstacle : Obstacle
{
    [Title("Main")]
    [SerializeField] bool active;
    [SerializeField] Transform child;
    [Space(6)]

    [Title("Move")]
    [SerializeField] Transform posA;
    [SerializeField] Transform posB;
    [SerializeField] float duration;
    [SerializeField] float delay;
    private Transform targetPos;
    private AnimationCurve curve;


    [Title("Rotate")]
    [SerializeField] Vector3 rotateSpeed;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        curve = CurveManager.GetCurve("Grinder Move");

        posA.parent = null;
        posB.parent = null;
        targetPos = posA;

        Move(_delay: 0.25f).Forget();
    }

    private void Update()
    {
        if (!active)
            return;

        RotateSpeed();
        Look();
    }


    private void RotateSpeed()
    {
        child.transform.Rotate(rotateSpeed * Time.deltaTime, Space.Self);
    }



    public async UniTaskVoid Move(float _delay = 0)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(_delay));

        transform.DOMove(targetPos.position,duration).SetDelay(delay).SetEase(curve).OnComplete(() =>
        {
            targetPos = (targetPos == posA) ? posB : posA;
            Move().Forget();
        });

        
    }

}
