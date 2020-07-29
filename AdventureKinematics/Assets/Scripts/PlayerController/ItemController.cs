using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public PlayerController playerController;
    public GameInventory inventory;

    public SpriteRenderer itemHandler;

    private bool lastJState;

    public void Init(PlayerController controller)
    {
        playerController = controller;

        lastJState = playerController.itemJoystick.State;
    }


    void FixedUpdate() { if (inventory.activeSlot.item != null) { if (inventory.activeSlot.item.isFixedUpdate) Call(); } }
  
    void Update()
    {
        if (inventory.activeSlot.item != null) { if (!inventory.activeSlot.item.isFixedUpdate) Call(); }
        else itemHandler.sprite = null;

        lastJState = playerController.itemJoystick.State;
    }


    void Call()
    {
        itemHandler.sprite = inventory.activeSlot.item.previewSprite;
        if (lastJState) inventory.activeSlot.item.Apply(playerController.itemJoystick.State, playerController.itemJoystick.Direction);
    
    }
}
