using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchButton : Switch
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        isChecked = true;
        UnityEngine.Debug.Log("Collision");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        isChecked = false;
    }
}
