using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirPlaneRotater : MonoBehaviour
{
    [Title("Main")]
    [SerializeField] bool active;
    [SerializeField] float rotateSpeed;
    [SerializeField] Transform airPlane;


    private void Update()
    {
        Rotate();
    }

    private void Rotate()
    {
        if (!active)
            return;
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    private void AirPlaneRotate()
    {

    }

}
