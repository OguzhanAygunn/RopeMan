using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum  PlayerState { Ready,Fall}

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    public bool IsAlive { get { return isAlive; } }

    [Title("Main")]
    [SerializeField] bool isAlive = true;
    [SerializeField] PlayerState state = PlayerState.Ready;
    [SerializeField] SkinnedMeshRenderer smr;
    [SerializeField] Transform spineTrs;

    
    private PlayerDefaultValues defaultValues = new PlayerDefaultValues();


    public SkinnedMeshRenderer SMR { get { return smr; } }
    public static PlayerState State { get { return instance.state; } set { instance.state = value; } }
    private void Awake()
    {
        instance = (!instance) ? this : instance;

        defaultValues.Init(this);
    }

    public void SetActiveAlive(bool active)
    {
        isAlive = active;
    }

    public void Death()
    {
        if (!isAlive)
            return;

        SetActiveAlive(active: false);
        FXManager.PlayFX("Blood Player", spineTrs.position, 3f).Forget();
        GameManager.instance.SetCompleteLevel(active: false);
    }


}


[System.Serializable]
public class PlayerDefaultValues
{
    public Vector3 pos;
    public Vector3 rotate;
    public Vector3 scale;
    public Color outlineColor;

    public void Init(PlayerController controller)
    {    
        pos = controller.transform.position;
        rotate = controller.transform.eulerAngles;
        scale = controller.transform.localScale;
        outlineColor = controller.SMR.material.GetColor("_OutlineColor");
    }
}
