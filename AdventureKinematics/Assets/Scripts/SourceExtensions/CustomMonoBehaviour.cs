using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMonoBehaviour : MonoBehaviour
{
    protected virtual void OnEnable() { CustomMonoEventDispatcher.preUpdate += EarlyUpdate; }

    protected virtual void OnDisable() { CustomMonoEventDispatcher.preUpdate -= EarlyUpdate; }

    // gets called in the beginning of the frame before other functions
    protected virtual void EarlyUpdate() { }

    // gets called several unfixed amount of times per frame with fixed interval, used for physics simulations 
    protected virtual void FixedUpdate() { }

    // heart of the cycle that gets called once per frame
    protected virtual void Update() { }

    // gets called in the end of the frame
    protected virtual void LateUpdate() { }


}
