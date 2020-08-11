using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubBlueTreeSlots : MonoBehaviour
{
    [NonSerialized] public InventorySlot inventorySlot;

    public Transform LinePositionUp;
    public Transform LinePositionDown;

    public void Start()
    {
        inventorySlot = GetComponent<InventorySlot>();
    }

}
