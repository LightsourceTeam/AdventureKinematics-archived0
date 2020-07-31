using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTrigger : MonoBehaviour
{
    public Switch UnlockSystem;

    private Switch objectSwitch;

    private void Start()
    {
        objectSwitch = GetComponent<Switch>();
    }

    void Update()
    {
        if (UnlockSystem.isChecked)
        {
            objectSwitch.isChecked = true;
        }
    }
}
