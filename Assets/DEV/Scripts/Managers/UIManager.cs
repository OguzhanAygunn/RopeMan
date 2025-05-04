using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    [SerializeField] GameObject failPanel;
    [SerializeField] GameObject winPanel;
    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }



    public async UniTaskVoid Complete(bool active,float delay = 0)
    {

        if (delay > 0)
            await UniTask.Delay(TimeSpan.FromSeconds(delay));

        if (active)
            Win();
        else
            Fail();
    }


    public void Fail()
    {
        failPanel.SetActive(true);
    }

    public void Win()
    {
        winPanel.SetActive(true);
    }
}
