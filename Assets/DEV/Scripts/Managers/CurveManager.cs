using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveManager : MonoBehaviour
{
    public static CurveManager instance;

    public static CurveManager Instance { get { return instance; } }

    [SerializeField] List<CurveInfo> curveInfos;

    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }

    public static AnimationCurve GetCurve(string id)
    {
        return instance.curveInfos.Find(curve => curve.id == id).curve;
    }
}

[System.Serializable]
public class CurveInfo
{
    public string id;
    public AnimationCurve curve;
}
