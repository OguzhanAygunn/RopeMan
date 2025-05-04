using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIExperimental : MonoBehaviour
{
    [Title("Main")]
    [SerializeField] bool move;
    [SerializeField] Camera cam;
    [SerializeField] Transform target;
    [SerializeField] Transform obj;
    [SerializeField] float speed;
    

    private void Update()
    {
        Move();
    }


    private void Move()
    {
        if (!move)
            return;

        
        Vector3 pos = obj.transform.position;
        Vector3 targetPos= cam.WorldToScreenPoint(target.position);

        pos = Vector3.Lerp(pos, targetPos, speed*Time.deltaTime);
        obj.transform.position = pos;
    }
}
