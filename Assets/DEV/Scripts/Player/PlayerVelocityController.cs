using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerVelocityController : MonoBehaviour
{
    [SerializeField] bool active;
    [SerializeField] ClampVelocityInfo yVelocityInfo;

    private void Awake()
    {
        yVelocityInfo.AssignObj(gameObject);
    }

    private void Update()
    {
        if (!active)
            return;

        yVelocityInfo.Clamp();
    }
}


[System.Serializable]
public class ClampVelocityInfo
{
    public List<Rigidbody> rigids;
    public float min;
    public float max;
    public GameObject obj;
    public void AssignObj(GameObject obj)
    {
        this.obj = obj;
    }

    [Button(size: ButtonSizes.Large)]
    [SerializeField] void UpdateRigids() {
        rigids = obj.GetComponentsInChildren<Rigidbody>().ToList();
    }

    public void Clamp()
    {
        rigids.ForEach(rigid => rigid.velocity = new Vector3(rigid.velocity.x, Mathf.Clamp(rigid.velocity.y, min, max), rigid.velocity.z));
    }
}
