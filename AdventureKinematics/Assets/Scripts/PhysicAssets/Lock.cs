using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lock : MonoBehaviour
{
    public Switch lockSwitch;
    [NonSerialized] public FixedJoint2D lockJoint;

    private void Start()
    {
        lockJoint = this.GetComponent<FixedJoint2D>();
    }

    private void Update()
    {
        lockJoint.enabled = lockSwitch.isChecked;
    }
}
