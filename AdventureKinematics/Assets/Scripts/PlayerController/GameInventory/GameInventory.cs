using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class GameInventory : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotParent;
    
    [NonSerialized] public List<GameInventorySlot> itemSlots;
    [NonSerialized] public GameInventorySlot selectedSlot;
    
    public MainController mainController;
    public int slotsCount;

    public void Init(MainController controller)
    {
        itemSlots = new List<GameInventorySlot>(slotsCount);
        for (int i = 0; i < slotsCount; i++)
        {
            mainController = controller;
            
            GameObject slot = Instantiate(slotPrefab, slotParent);
            itemSlots.Add(slot.GetComponent<GameInventorySlot>());
            slot.SetActive(true);
        }
        selectedSlot = itemSlots[0];
    }

    protected void Update()
    {
        foreach (GameInventorySlot slot in itemSlots)
        {
            if  (slot.item != null)
            {
                if (mainController.interactionController.joystick.State) slot.item.OnMounted(mainController);
                else { if (mainController.interactionController.lastJState) slot.item.OnEndMounted(mainController); }
            }
        }
    }

    protected void FixedUpdate()
    {
        foreach (GameInventorySlot slot in itemSlots)
        {
            if (slot.item != null)
            {
                if (mainController.interactionController.joystick.State) slot.item.OnFixedMounted(mainController);
            }
        }
    }

    protected void LateUpdate()
    {
        foreach (GameInventorySlot slot in itemSlots)
        {
            if (slot.item != null)
            {
                if (mainController.interactionController.joystick.State) slot.item.OnLateMounted(mainController);
            }
        }
    }
}
