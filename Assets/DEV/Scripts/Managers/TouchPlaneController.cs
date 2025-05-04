using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchPlaneController : MonoBehaviour
{
    public static TouchPlaneController instance;
    [Title("Main")]
    [SerializeField] bool active;
    [SerializeField] LayerMask layer;
    [SerializeField] Camera cam;
    public static Vector3 TouchPos { get { return instance.GetPos(); } }
    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }


    private void Update()
    {
        if (!active)
            return;

    }

    private Vector3 GetPos()
    {

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 pos =Vector3.zero;
        if (Physics.Raycast(ray, out hit, layer))
        {
            pos = hit.point;
            pos.z = PlayerController.instance.transform.position.z;


            return pos;
        }


        return pos;
    }
}
