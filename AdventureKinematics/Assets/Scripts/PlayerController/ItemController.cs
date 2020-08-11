using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : Controller
{
    [NonSerialized] public MainController playerController;
    [NonSerialized] public InventorySystem inventory;

    public SpriteRenderer itemHandler;

    public override void Init(MainController controller)
    {
        mainController = controller;
        joystick = mainController.itemJoystick;
        inventory = controller.inventorySystem;

        lastJState = joystick.State;
        lastDirection = joystick.Direction;
    }

    protected override void FixedUpdate() 
    { 
        if (inventory != null) 
        {
            if (lastJState)
            {
                if (joystick.State) inventory.selectedSlot.item.OnFixedApply(this);
            }
        }
    }
  
    protected override void Update()
    {
        if (inventory.selectedSlot.item != null)
        {
            if (lastJState)
            {
                if (!joystick.State) inventory.selectedSlot.item.OnEndApply(this);
                else inventory.selectedSlot.item.OnApply(this);
            }
        }
        else itemHandler.sprite = null;
    }
}
