using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBodyPart : MonoBehaviour
{
    [SerializeField] Rigidbody rigid;
    [SerializeField] bool isSelected;
    [SerializeField] bool defaultKinematic;
    [SerializeField] RigidbodyConstraints defaultConstraints;

    private Vector3 defaultPos;
    private Vector3 defaultScale;
    private Vector3 defaultRotation;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();

        defaultConstraints = rigid.constraints;
        defaultKinematic = rigid.isKinematic;

        defaultPos = transform.position;
        defaultScale = transform.localScale;
        defaultRotation = transform.localEulerAngles;
    }


    private void Update()
    {
        Move();
    }


    private void Move()
    {
        if (!isSelected)
            return;


        Vector3 startPos = transform.position;
        Vector3 endPos = TouchPlaneController.TouchPos;
        float speed = PlayerBodyController.BodySpeed;


        transform.position = Vector3.Lerp(startPos, endPos, speed * Time.deltaTime);
    }


    public void SetFreezeConstraints(bool active)
    {
        RigidbodyConstraints constraints = active ? RigidbodyConstraints.FreezeAll : defaultConstraints;
        bool kinematic = active ? true : defaultKinematic;


        rigid.constraints = constraints;
        rigid.isKinematic = kinematic;
    }

    public void SetSelected(bool active)
    {
        isSelected = active;
        transform.DOLocalRotate(defaultRotation, 0.2f);
    }

    public void ToDefaultX()
    {
        transform.DOMoveX(defaultPos.x, 0.15f);
    }


    public async UniTaskVoid ResetRotate(float duration = 0.5f,float delay=0)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        RigidbodyConstraints freezeRotate = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezePositionZ;

        rigid.constraints = freezeRotate;
        rigid.angularVelocity = Vector3.zero;
        transform.DOLocalRotate(defaultRotation, duration).OnComplete( () =>
        {
            rigid.constraints = defaultConstraints;
        });
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle") || other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            PlayerController.instance.Death();
        }
    }


}
