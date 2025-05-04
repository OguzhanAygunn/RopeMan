using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTargetController : MonoBehaviour
{
    public static CameraTargetController instance;

    public bool IsFollow { get { return follow; } set { follow = value; } }

    [Title("Main")]
    [SerializeField] bool follow;
    [SerializeField] Transform target;
    [SerializeField] float fixX;
    [SerializeField] float fixZ;
    [SerializeField] float speed;


    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }

    private void FixedUpdate()
    {
        Follow();
    }

    [Button(size: ButtonSizes.Large)]
    [SerializeField] void UpdateFixValues()
    {
        fixX = transform.position.x;
        fixZ = transform.position.z;
    }

    public void SetActiveFollow(bool active)
    {
        this.follow = active;
    }


    [Button(size: ButtonSizes.Large)]
    [SerializeField] void Follow()
    {
        if (!follow)
            return;

        Vector3 pos = target.position;
        pos.x = fixX;
        pos.z = fixZ;

        transform.position = Vector3.Lerp(transform.position, pos, speed * Time.fixedDeltaTime);
    }
}
