using Cysharp.Threading.Tasks.Triggers;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerBodyController : MonoBehaviour
{
    private static PlayerBodyController instance;
    public static PlayerBodyController Instance { get { return instance; } }


    [Title("Main")]
    [SerializeField] List<PlayerBodyPart> bodyParts;
    [SerializeField] float bodySpeed;
    [SerializeField] PlayerBodyPart spinePart;



    [Title("Jump")]
    [SerializeField] Rigidbody jumpBody;
    
    [SerializeField] float jumpPower;
    [SerializeField] float jumpTriggerDistance;
    public static float JumpTriggerDistance { get { return Instance.jumpTriggerDistance; } }
    public static PlayerBodyPart SpinePart { get { return instance.spinePart; } }
    public static float BodySpeed { get { return Instance.bodySpeed; } }
    public static PlayerBodyPart JumpBody { get { return Instance.jumpBody.GetComponent<PlayerBodyPart>(); } }
    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }


    [Button(size: ButtonSizes.Large)]
    public void BodyPartsUpdate()
    {
        bodyParts = GetComponentsInChildren<PlayerBodyPart>().ToList();
    }


    public static void Jump(Vector3 power)
    {
        power *= instance.jumpPower;
        power.z = 0;
        instance.jumpBody.AddForce(power, ForceMode.VelocityChange);
        

    }
}
