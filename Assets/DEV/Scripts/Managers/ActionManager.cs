using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionManager : MonoBehaviour
{
    public static ActionManager instance;
    [Title("Main")]
    [SerializeField] List<ActionInfo> actions;

    [Title("Meter Controller")]
    [SerializeField] Transform spineTrs;
    [SerializeField] float Meter { get { return spineTrs.position.y; } }
    [SerializeField] float meter;

    public static ActionManager Instance { get { return instance; } }

    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }


    private void Update()
    {
        meter = Meter;
        ActionsController();
    }


    private void ActionsController()
    {
        ActionInfo actionInfo = actions.Find(action => action.triggerMeter < Meter && !action.active);

        if(actionInfo != null)
        {
            actionInfo.PlayAction();
        }
    }
}


[System.Serializable]
public class ActionInfo
{
    public string name;
    public bool active;
    public float triggerMeter;
    public UnityEvent action;

    public void PlayAction()
    {
        active = true;
        action.Invoke();
    }
}
