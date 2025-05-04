using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollower : MonoBehaviour
{
    public static PlayerFollower instance;
 
    public PosTracker PosTracker { get { return posTracker; } }
    
    [SerializeField] PosTracker posTracker;

    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }
}
