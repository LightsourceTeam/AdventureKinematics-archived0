using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : Controller
{
    [NonSerialized] public MainController playerController;
    public InventoryController inventory;

    public SpriteRenderer itemHandler;

    public override void Init(MainController controller)
    {
        mainController = controller;
        joystick = mainController.itemJoystick;

        lastJState = joystick.State;
        lastDirection = joystick.Direction;
    }

    protected override void FixedUpdate() 
    { 
        if (inventory.selectedSlot.item != null) 
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
