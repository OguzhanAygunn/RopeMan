using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public static ObstacleManager instance;
    public static ObstacleManager Instance { get { return instance; } }

    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }
}
