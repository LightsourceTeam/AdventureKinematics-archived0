using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : Controller
{
    [NonSerialized] public MainController playerController;
    public GameInventory inventory;

    public SpriteRenderer itemHandler;

    public override void Init(MainController controller)
    {
        mainController = controller;
        joystick = mainController.itemJoystick;

        lastJState = joystick.State;
    }

    void FixedUpdate() 
    { 
        if (inventory.activeSlot.item != null) 
        {
            if (lastJState)
            {
                if (joystick.State) inventory.activeSlot.item.OnFixedApply(this);
            }
        }
    }
  
    void Update()
    {
        if (inventory.activeSlot.item != null)
        {
            if (lastJState)
            {
                if (!joystick.State) inventory.activeSlot.item.OnEndApply(this);
                else inventory.activeSlot.item.OnApply(this);
            }
        }
        else itemHandler.sprite = null;
    }

    public void LateUpdate()
    {
        lastJState = joystick.State;
        lastDirection = joystick.Direction;
    }
}
