using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    [Title("Main")]
    [SerializeField] List<VirtualCameraInfo> cameras;
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    public CinemachineFramingTransposer transposer;
    public VirtualCameraInfo currentInfo;

    [Space(6)]

    [Title("Camera Distance")]
    [SerializeField] bool zoomOut;
    [SerializeField] float minZoomDistance;
    [SerializeField] float maxZoomDistance;
    [SerializeField] float zoomSpeed;

    [Space(5)]

    [Title("Cam Effect")]
    [SerializeField] List<CamShakeInfo> shakes;
    [SerializeField] Transform shakerTrs;

    [Space(5)]

    [Title("Zoom Effects")]
    [SerializeField] bool zoomEffectActive;
    [SerializeField] List<CamZoomInfo> zooms;
    private CamZoomInfo activeZoomInfo;

    [Space(5)]
    [Title("Offset Effects")]
    [SerializeField] bool offsetEffectActive;
    [SerializeField] float offsetEffectSpeed;
    [SerializeField] List<CamOffsetInfo> offsetInfos;
    private CamOffsetInfo activeOffsetInfo;
    [SerializeField] Vector3 extraOffset;
    [SerializeField] Vector3 posOffset;

    private void Awake()
    {
        instance = (!instance) ? this : instance;
        cameras.ForEach(c => c.Init());
        SetCamera("Game");
    }

    private void Start()
    {
        shakes.ForEach(s => s.AssignCamera(virtualCamera));
        posOffset = transposer.m_TrackedObjectOffset;
    }

    private void Update()
    {
        currentInfo.Update();
        ZoomController();
        OffsetController();
    }

    private void OffsetController()
    {
        Vector3 targetOffset = offsetEffectActive ? activeOffsetInfo.offset : Vector3.zero;

        extraOffset = Vector3.Lerp(extraOffset, targetOffset, offsetEffectSpeed * Time.deltaTime);

        Vector3 offset = posOffset + extraOffset;

        transposer.m_TrackedObjectOffset = offset;
        
    }

    public void SetCamera(string id)
    {
        VirtualCameraInfo info = cameras.Find(cam => cam.id == id);
        currentInfo  = info;
        CinemachineVirtualCamera vCamera =info.virtualCamera;

        virtualCamera?.gameObject.SetActive(value: false);
        virtualCamera = vCamera;
        virtualCamera.gameObject.SetActive(value: true);
        transposer = info.transposer;

    }

    public void Shake(string id)
    {
        CamShakeInfo shakeInfo = shakes.Find(shake => shake.id == id);

        if (shakeInfo == null)
            return;

        shakeInfo.Shake().Forget();
    }

    public async void Zoom(string id,float delay = 0)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        if(id == "false")
        {
            activeZoomInfo = null;
            zoomEffectActive = false;
            return;
        }

        activeZoomInfo = zooms.Find(zoom => zoom.id == id);

        if (activeZoomInfo == null)
            return;

        zoomEffectActive = true;
    }

    [Button(size:ButtonSizes.Large)]
    public async void ActiveOffset(string id,float delay = 0)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        if(id == "false")
        {
            activeOffsetInfo = null;
            offsetEffectActive = false;
            return;
        }

        activeOffsetInfo = offsetInfos.Find(offset => offset.id == id);

        if (activeOffsetInfo == null)
            return;

        offsetEffectActive= true;
    }

    private void ZoomController()
    {

        zoomOut = PlayerController.State is PlayerState.Fall || TouchManager.IsSelectedBodyPart;

        float cameraDistance = transposer.m_CameraDistance;
        float targetDistance = zoomOut ? maxZoomDistance : minZoomDistance;

        //if (TouchManager.IsSelectedBodyPart)
        //    targetDistance += TouchManager.Meter;


        if (zoomEffectActive)
            targetDistance = activeZoomInfo.targetZoom;

        cameraDistance = Mathf.Lerp(cameraDistance, targetDistance, zoomSpeed * Time.deltaTime);
        transposer.m_CameraDistance = cameraDistance;
    }
}


[System.Serializable]
public class VirtualCameraInfo
{
    public string id;
    public CinemachineVirtualCamera virtualCamera;
    public CinemachineFramingTransposer transposer;
    private float defaultYDamping;
    [SerializeField] float defaultXDamping;
    public void Init()
    {
        transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        defaultYDamping = transposer.m_YDamping;
    }

    public void Update()
    {
        UpdateDamping();
    }

    private void UpdateDamping()
    {
        float startDamping = transposer.m_YDamping;
        float targetDamping = TouchManager.IsSelectedBodyPart || TouchManager.RopeBall ? 20f : defaultYDamping;

       transposer.m_YDamping = Mathf.Lerp(startDamping, targetDamping, 40 * Time.deltaTime);
    }
}

[System.Serializable]
public class CamShakeInfo
{
    [Title("Main")]
    public string id;
    public bool active;
    
    [Space(6)]

    [Title("Start")]
    public float startDuration;
    public float startDelay;
    public float startAmplitudeGain;
    public float startfrequencyGain;

    [Space(6)]

    [Title("End")]
    public float endDuration;
    public float endDelay;
    public float endAmplitudeGain;
    public float endfrequencyGain;

    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin perlin;
    public void AssignCamera(CinemachineVirtualCamera camera)
    {
        virtualCamera = camera;
        perlin= virtualCamera.GetCinemachineComponent
            <CinemachineBasicMultiChannelPerlin>();
    }


    public async UniTaskVoid Shake()
    {
        active = true;

        await UniTask.Delay(TimeSpan.FromSeconds(startDelay));

        DOTween.To(() => perlin.m_AmplitudeGain, x => perlin.m_AmplitudeGain = x, startAmplitudeGain, startDuration);
        DOTween.To(() => perlin.m_FrequencyGain, x => perlin.m_FrequencyGain = x, startfrequencyGain, startDuration);

        await UniTask.Delay(TimeSpan.FromSeconds(endDelay));

        DOTween.To(() => perlin.m_AmplitudeGain, x => perlin.m_AmplitudeGain = x, endAmplitudeGain, endDuration);
        DOTween.To(() => perlin.m_FrequencyGain, x => perlin.m_FrequencyGain = x, endfrequencyGain, endDuration);

        await UniTask.Delay(TimeSpan.FromSeconds(endDuration));

        active = false;
    }

}



[System.Serializable]
public class CamZoomInfo
{
    [Title("Main")]
    public string id;
    public bool active;
    public float targetZoom;

    private CinemachineVirtualCamera virtualCamera;


    public void AssignCamera(CinemachineVirtualCamera cam)
    {
        virtualCamera = cam;
    }

    public void Zoom()
    {
        active = true;
    }
}


[System.Serializable]
public class CamOffsetInfo
{
    [Title("Main")]
    public string id;
    public Vector3 offset;
}
