using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotParent;

    public GameObject blueprintPrefab;
    public Transform blueprintParent;

    [NonSerialized] public List<InventorySlot> fastAccessSlots;
    [NonSerialized] public List<InventorySlot> inventorySlots;
    [NonSerialized] public List<InventorySlot> craftSlots;
    [NonSerialized] public List<InventorySlot> blueprintSlots;
    
    [NonSerialized] public InventorySlot selectedSlot;
    [NonSerialized] public BlueprintSlot selectedBlueprintSlot;

    private int slotsCount = 20;

    public MainController mainController;

    public void Init(MainController controller)
    {
        fastAccessSlots = new List<InventorySlot>(slotsCount);
        for (int i = 0; i < 4; i++)
        {
            mainController = controller;
            
            GameObject slot = Instantiate(slotPrefab, slotParent);
            fastAccessSlots.Add(slot.GetComponent<InventorySlot>());
            slot.SetActive(true);
        }
        selectedSlot = fastAccessSlots[0];
        
        inventorySlots = new List<InventorySlot>(slotsCount);
        for (int i = 0; i < 4; i++)
        {
            mainController = controller;
            
            GameObject slot = Instantiate(slotPrefab, slotParent);
            inventorySlots.Add(slot.GetComponent<InventorySlot>());
            slot.SetActive(true);
        }
        blueprintSlots = new List<InventorySlot>(4);
        for (int i = 0; i < 4; i++)
        {
            mainController = controller;

            GameObject slot = Instantiate(blueprintPrefab, blueprintParent);
            blueprintSlots.Add(slot.GetComponent<InventorySlot>());
            slot.SetActive(true);
        }
    }

    protected void Update()
    {
        foreach (InventorySlot slot in fastAccessSlots)
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
        foreach (InventorySlot slot in fastAccessSlots)
        {
            if (slot.item != null)
            {
                if (mainController.interactionController.joystick.State) slot.item.OnFixedMounted(mainController);
            }
        }
    }

    protected void LateUpdate()
    {
        foreach (InventorySlot slot in fastAccessSlots)
        {
            if (slot.item != null)
            {
                if (mainController.interactionController.joystick.State) slot.item.OnLateMounted(mainController);
            }
        }
    }

}
