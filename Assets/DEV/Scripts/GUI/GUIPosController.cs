using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIPosController : MonoBehaviour
{
    [Title("Main")]
    [SerializeField] bool active;
    [SerializeField] Camera cam;
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset;
    [SerializeField] float speed;


    private void Update()
    {
        Movement();
    }


    private void Movement()
    {
        if (!active)
            return;

        Vector3 pos = transform.position;
        Vector3 endPos = cam.WorldToScreenPoint(target.position);

        pos = Vector3.Lerp(pos, endPos + offset, speed * Time.deltaTime);
        transform.position = pos;
    }

    public async UniTaskVoid SetActive(bool active,float delay = 0)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        this.active = active;
    }
}
