using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lock : MonoBehaviour
{
    public Switch lockSwitch;
    public Rigidbody2D ObjectToLock;
    public bool isBlocked;

    private void Start()
    {

    }

    private void Update()
    {
        if(lockSwitch.isChecked != isBlocked)
        {
            BlockOrRelease();
            isBlocked = !isBlocked;
        }
    }

    private void BlockOrRelease()
    {
        if(lockSwitch.isChecked)
        {
            ObjectToLock.bodyType = RigidbodyType2D.Kinematic;
        }
        else
        {
            ObjectToLock.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}
