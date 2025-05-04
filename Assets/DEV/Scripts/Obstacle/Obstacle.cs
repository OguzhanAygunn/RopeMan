using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Title("Look")]
    [SerializeField] bool look;
    [ShowIf("@this.look")][SerializeField] Camera cam;
    [ShowIf("@this.look")][SerializeField] Vector3 lookOffset;
    [ShowIf("@this.look")][SerializeField] float lookSpeed;

    public void Look()
    {
        if (!look)
            return;

        Vector3 camPos = cam.transform.position;
        camPos.x = transform.position.x;

        Vector3 lookPos = camPos - transform.position;
        lookPos += lookOffset;

        Quaternion lookRot = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, lookSpeed * Time.deltaTime);
    }
}
