using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PosTrackerType { Fix,MoveToWards,Lerp}
public class PosTracker : MonoBehaviour
{
    [Title("Main")]
    [SerializeField] PosTrackerType type = PosTrackerType.Fix;
    [ShowIf("@this.type == PosTrackerType.MoveToWards || this.type == PosTrackerType.Lerp")] [SerializeField] float speed;
    [SerializeField] bool fixX;

    [Space(6)]

    [Title("Transform")]
    [SerializeField] bool active;
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset;
    private Vector3 defaultPos;

    private void Awake()
    {
        Renderer mr = GetComponent<Renderer>();
        if (mr)
        {
            if(mr is not SpriteRenderer)
            {
                mr.enabled = false;
            }
        }
            

        defaultPos = transform.position;
    }

    private void LateUpdate()
    {
        if (!active)
            return;

        Movement();
    }


    private void Movement()
    {
        if (!target)
            return;

        Vector3 targetPos = target.position + offset;
        targetPos.x = defaultPos.x;
        Vector3 pos = transform.position;
        switch (type)
        {
            case PosTrackerType.Fix:
                pos = targetPos;
                break;
            case PosTrackerType.MoveToWards:
                pos = Vector3.MoveTowards(pos, targetPos, speed * Time.deltaTime);
                break;
            case PosTrackerType.Lerp:
                pos = Vector3.Lerp(pos, targetPos, speed * Time.deltaTime);
                break;
        }

        transform.position = pos;
        
    }

    public void AssignTarget(Transform newTarget)
    {
        target = newTarget;
        active = true;
        Movement();
    }


    public void AssignTarget(Transform newTarget,Vector3 offset)
    {
        target = newTarget;
        active = true;
        this.offset = offset;
        Movement();
    }

    public void ClearTarget()
    {
        target = null;
        active = false;
        offset = Vector3.zero;
    }

    public void SetActive(bool active)
    {
        this.active = active;
    }
}
