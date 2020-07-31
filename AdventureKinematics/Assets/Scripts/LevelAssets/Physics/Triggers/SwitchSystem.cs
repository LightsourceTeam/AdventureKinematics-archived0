using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchSystem : Switch
{
    public List<Switch> unlockItems;
    public int checkAmount;

    void Update()
    {
        checkAmount = 0;
        foreach (Switch switchButton in unlockItems)
        {
            if (switchButton.isChecked)
            {
                checkAmount++;
                Debug.Log("Check Amount: " + checkAmount);
            }
        }
        if(checkAmount == unlockItems.Count)
        {
            isChecked = true;
            Debug.Log("isFinished State: " + isChecked);
        }
    }
}
