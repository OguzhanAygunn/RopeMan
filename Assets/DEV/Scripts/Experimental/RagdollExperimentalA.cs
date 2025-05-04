using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollExperimentalA : MonoBehaviour
{
    [SerializeField] bool active;
    [SerializeField] Vector3 pos;
    [SerializeField] private Rigidbody rigid;



    [Button(size: ButtonSizes.Large)]
    public void Push()
    {
        rigid.AddForce(pos, ForceMode.VelocityChange);
    }
}
