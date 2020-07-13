using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchButton : Switch
{
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        isChecked = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        isChecked = false;
    }
}
