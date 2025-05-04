using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigKnifeObstacle : Obstacle
{
    [Title("Main")]
    [SerializeField] bool exp;

    private void Update()
    {
        base.Look();
    }
}
