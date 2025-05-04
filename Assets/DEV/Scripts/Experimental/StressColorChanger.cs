using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StressColorChanger : MonoBehaviour
{
    [Title("Main")]
    [SerializeField] Color defaultColor;
    [SerializeField] Color stressColor;

    private Transform bodyPart;
    private MeshRenderer mr;
    private void Start()
    {
        mr = GetComponent<MeshRenderer>();
        bodyPart = PlayerBodyController.JumpBody.transform;
    }

    private void Update()
    {
        ColorUpdate();
    }

    private void ColorUpdate()
    {
        bool isActive = transform.position.y > bodyPart.position.y;
        Color targetColor = isActive ? stressColor : defaultColor;
        mr.material.color = targetColor;
    }
}
