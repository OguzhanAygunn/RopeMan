using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirBreakablePipeParentController : MonoBehaviour
{
    [SerializeField] List<AirBreakablePipeController> controllers;
    [SerializeField] GameObject airVehicle;
    private AirVehicle vehicle;

    private void Start()
    {
        vehicle = airVehicle.GetComponent<AirVehicle>();
    }

    [Button(size:ButtonSizes.Large)]
    public void BreakPieces()
    {
        CameraManager.instance.Shake("Air");
        CameraManager.instance.ActiveOffset(id: "false", delay: 1.4f);
        CameraManager.instance.Zoom("false", delay: 1.4f);
        controllers.ForEach(controller =>
        {
            controller.BreakPieces();
        });

        TimeManager.instance.PlayTimeAnim("default", 0.5f);
    }

    public void ActiveAirVehicle()
    {
        //Camera
        CameraManager.instance.ActiveOffset("Air");
        CameraManager.instance.Zoom("Air");

        //Vehicle Active
        airVehicle.SetActive(true);
        vehicle.Go();
    }


    public void NearTheHit()
    {
        TimeManager.instance.PlayTimeAnim("air");
    }
}
