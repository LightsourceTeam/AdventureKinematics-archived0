using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : Controller
{
    [NonSerialized] public MainController playerController;
    public GameInventory inventory;

    public SpriteRenderer itemHandler;

    private bool lastJState;

    public override void Init(MainController controller)
    {
        mainController = controller;
        joystick = mainController.itemJoystick;

        lastJState = joystick.State;
    }


    void FixedUpdate() { if (inventory.activeSlot.item != null) { if (inventory.activeSlot.item.isFixedUpdate) Call(); } }
  
    void Update()
    {
        if (inventory.activeSlot.item != null) { if (!inventory.activeSlot.item.isFixedUpdate) Call(); }
        else itemHandler.sprite = null;

        lastJState = joystick.State;
    }


    void Call()
    {
       
        if (lastJState) inventory.activeSlot.item.Apply(this);
    
    }
}
