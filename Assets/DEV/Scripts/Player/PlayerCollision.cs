using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    PlayerController controller;

    private void Awake()
    {
        controller = PlayerController.instance;
    }


}
