using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public InventorySlot slotPrefab;
    public Transform slotParent;


    public BlueprintChain chainPrefab;
    public Transform chainsParent;

    [NonSerialized] public List<InventorySlot> activeSlots;
    [NonSerialized] public List<InventorySlot> inventorySlots;
    [NonSerialized] public List<BlueprintChain> blueprintSlots;

    [NonSerialized] public InventorySlot selectedSlot;
    [NonSerialized] public BlueprintChain selectedBlueprintChain;

    [NonSerialized] public List<InventorySlot> craftSlots;

    private int slotsCount = 20;

    [NonSerialized] public MainController mainController;

    public void Init(MainController controller)
    {
        mainController = controller;

        activeSlots = new List<InventorySlot>(4);
        for (int i = 0; i < 4; i++)
        {
            InventorySlot slot = Instantiate(slotPrefab, slotParent);
            activeSlots.Add(slot);
            slot.inventorySystem = this;
            slot.gameObject.SetActive(true);
        }

        inventorySlots = new List<InventorySlot>(slotsCount);
        for (int i = 0; i < slotsCount; i++)
        {
            InventorySlot slot = Instantiate(slotPrefab, slotParent);
            inventorySlots.Add(slot);
            slot.inventorySystem = this;
            slot.gameObject.SetActive(true);
        }

        blueprintSlots = new List<BlueprintChain>(4);
        for (int i = 0; i < 4; i++)
        {
            BlueprintChain chain = Instantiate(chainPrefab, chainsParent);
            blueprintSlots.Add(chain);
            chain.gameObject.SetActive(true);
        }

        selectedSlot = activeSlots[0];
    }

    protected void Update()
    {
        // call events on items
        foreach (InventorySlot slot in activeSlots)
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


        // call events on items
        foreach (InventorySlot slot in activeSlots)
        {
            if (slot.item != null)
            {
                if (mainController.interactionController.joystick.State) slot.item.OnFixedMounted(mainController);
            }
        }
    }

    protected void LateUpdate()
    {


        // call events on items
        foreach (InventorySlot slot in activeSlots)
        {
            if (slot.item != null)
            {
                if (mainController.interactionController.joystick.State) slot.item.OnLateMounted(mainController);
            }
        }
    }

    public void RplaceItemFromSlot(InventorySlot invSlot)
    {
        InventorySlot tempSlot = invSlot;
        float minMagnitude = float.MaxValue;

        foreach(InventorySlot invenSlot in inventorySlots)
        {
            if(minMagnitude > (invenSlot.itemHolder.transform.position - tempSlot.gameObject.transform.position).magnitude) tempSlot = invenSlot;
        }

        GameItem T = tempSlot.item;
        tempSlot.item = invSlot.item;
        invSlot.item = T;
    }


    public void ResetBlueprintSelection()
    {

    }

}
