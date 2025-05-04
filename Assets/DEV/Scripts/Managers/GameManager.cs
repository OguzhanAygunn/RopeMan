using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] int targetFPS;


    private void Awake()
    {
        instance = (!instance) ? this : instance;
        ChangeFPS();
    }

    private void Start()
    {
        Screen.SetResolution(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2, true);
    }

    [Button(size: ButtonSizes.Large)]
    [SerializeField] void ChangeFPS()
    {
        Application.targetFrameRate = targetFPS;
    }

    public void RestartLevel()
    {
        DOTween.KillAll();
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(sceneIndex);
    }

    public void SetCompleteLevel(bool active)
    {

        if (!active)
            Fail();
        else
            Win();
    }

    public void Fail()
    {
        TouchManager.instance.CurrentBodyPart?.SetSelected(active: false);
        TouchManager.instance.CurrentBodyPart?.SetFreezeConstraints(active: false);
        MeterPanelController.instance.SetActiveMeterSizeAnim(active: false);
        TouchManager.instance.SetActive(active: false);
        PlayerFollower.instance.PosTracker.SetActive(active: false);
        PlayerRopeManager.instance.FreeBalls();
        PlayerOutlineController.instance.SetActiveOutlines(active: false);
        CameraTargetController.instance.SetActiveFollow(active: false);
        CameraManager.instance.Shake("Air");
        MeterPanelController.SetVisibility(active: false, force: true).Forget();
        UIManager.instance.Complete(active: false, delay: 0.9f).Forget();
        
    }

    public void Win()
    {

    }
}
