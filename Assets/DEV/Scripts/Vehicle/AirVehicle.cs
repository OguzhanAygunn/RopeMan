using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirVehicle : MonoBehaviour
{
    public AirBreakablePipeParentController pipeParentController;


    [Title("Particles")]
    [SerializeField] List<ParticleSystem> trails;

    public void Go()
    {
        trails.ForEach(p => p.Play(withChildren: true));
    }
    public void BreakPieces()
    {
        pipeParentController.BreakPieces();
    }

    public void EndAction()
    {
        gameObject.SetActive(false);
    }

    public void NearTheHit()
    {
        pipeParentController.NearTheHit();
    }
}
