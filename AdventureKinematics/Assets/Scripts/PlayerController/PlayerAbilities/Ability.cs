using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    [NonSerialized] public PlayerController playerController;
    public bool isFixedUpdate = false;

    public virtual void Apply()
    {
        
    }
}
