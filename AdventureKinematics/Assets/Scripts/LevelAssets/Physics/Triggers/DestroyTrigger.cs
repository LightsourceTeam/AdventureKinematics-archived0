using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTrigger : MonoBehaviour
{   
    public Switch UnlockSystem;
    
    void Update()
    {
        if (UnlockSystem.isChecked)
        {
            Destroy(gameObject);
        }
    }
}
