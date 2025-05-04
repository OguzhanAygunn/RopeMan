using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    public static FXManager instance;
    public static FXManager Instance { get { return instance; } }

    [SerializeField] List<FXInfo> infos;


    [SerializeField] List<FX> allFx;

    private Transform fxParent;
    private void Awake()
    {
        instance = (!instance) ? this : instance;
        fxParent = new GameObject("FX Manager").transform;

        Pool();
    }

    void Start()
    {
        
    }

    private void Pool()
    {
        foreach(FXInfo info in infos)
        {
            int index = 0;

            while(index < info.count)
            {
                index++;
                ParticleSystem prt = Instantiate(info.prefab).GetComponent<ParticleSystem>();
                prt.transform.SetParent(fxParent);
                prt.gameObject.SetActive(value: false);
                prt.AddComponent<PosTracker>();
                FX fx = new FX()
                {
                    particle = prt,
                    id = info.id
                };
                allFx.Add(fx);
            }
        }
    }


    public static ParticleSystem GetFX(string id)
    {
        ParticleSystem particle = instance.allFx.Find(fx => fx.id == id && !fx.particle.gameObject.activeInHierarchy).particle;

        return particle;
    }

    public static async UniTaskVoid PlayFX(string id,Vector3 pos,float desTime=2)
    {
        
        ParticleSystem particle = GetFX(id);

        if (!particle)
            return;

        particle.transform.position = pos;
        particle.gameObject.SetActive(value:true);
        particle.Play(withChildren: true);


        await UniTask.Delay(TimeSpan.FromSeconds(desTime));
        if (particle)
            particle.gameObject.SetActive(false);

    }

    public static async UniTaskVoid PlayFX(string id, Transform newParent, float desTime = 2)
    {

        ParticleSystem particle = GetFX(id);

        if (!particle)
            return;

        particle.transform.SetParent(newParent);
        particle.transform.localPosition = Vector3.zero;
        particle.gameObject.SetActive(value: true);
        particle.Play(withChildren: true);


        await UniTask.Delay(TimeSpan.FromSeconds(desTime));
        particle.transform.parent = null;
        particle.gameObject.SetActive(false);
    }


    public static async UniTaskVoid PlayFXWithTracker(string id, Transform target,float desTime)
    {

        ParticleSystem particle = GetFX(id);

        if (!particle)
            return;

        PosTracker tracker = particle.GetComponent<PosTracker>();

        
        particle.gameObject.SetActive(value: true);
        tracker.AssignTarget(target);
        particle.Play(withChildren: true);


        await UniTask.Delay(TimeSpan.FromSeconds(desTime));
        tracker.ClearTarget();
        particle.gameObject.SetActive(false);
    }

    public static async UniTaskVoid PlayFXWithTracker(string id, Transform target,Vector3 offset, float desTime)
    {

        ParticleSystem particle = GetFX(id);

        if (!particle)
            return;

        PosTracker tracker = particle.GetComponent<PosTracker>();


        particle.gameObject.SetActive(value: true);
        tracker.AssignTarget(newTarget: target, offset: offset);
        particle.Play(withChildren: true);


        await UniTask.Delay(TimeSpan.FromSeconds(desTime));
        tracker.ClearTarget();
        particle.gameObject.SetActive(false);
    }
}


[System.Serializable]
public class FXInfo
{
    public string id;
    public GameObject prefab;
    public int count;
}

[System.Serializable]
public class FX
{
    public string id;
    public ParticleSystem particle;
}
