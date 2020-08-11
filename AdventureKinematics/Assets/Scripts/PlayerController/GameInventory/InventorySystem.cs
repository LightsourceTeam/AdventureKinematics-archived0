using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotParent;

    public GameObject blueprintPrefab;
    public Transform blueprintParent;

    [NonSerialized] public List<InventorySlot> fastAccessSlots;
    [NonSerialized] public List<InventorySlot> inventorySlots;
    [NonSerialized] public List<InventorySlot> craftSlots;
    [NonSerialized] public List<BlueprintSlot> blueprintSlots;

    [NonSerialized] public InventorySlot selectedSlot;
    [NonSerialized] public BlueprintSlot selectedBlueprintSlot;

    private int slotsCount = 20;

    [NonSerialized] public MainController mainController;

    public void Init(MainController controller)
    {
        mainController = controller;

        fastAccessSlots = new List<InventorySlot>(slotsCount);
        for (int i = 0; i < 4; i++)
        {
            InventorySlot slot = Instantiate(slotPrefab, slotParent).GetComponent<InventorySlot>();
            fastAccessSlots.Add(slot);
            slot.inventorySystem = this;
            slot.gameObject.SetActive(true);
        } 

        inventorySlots = new List<InventorySlot>(slotsCount);
        for (int i = 0; i < 4; i++)
        { 
            InventorySlot slot = Instantiate(slotPrefab, slotParent).GetComponent<InventorySlot>();
            inventorySlots.Add(slot);
            slot.inventorySystem = this;
            slot.gameObject.SetActive(true);
        }

        blueprintSlots = new List<BlueprintSlot>(4);
        for (int i = 0; i < 4; i++)
        {
            GameObject slot = Instantiate(blueprintPrefab, blueprintParent);
            blueprintSlots.Add(slot.GetComponent<BlueprintSlot>());
            slot.SetActive(true);
        }

        selectedSlot = fastAccessSlots[0];
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

    public void ChangeSlot(InventorySlot invSlot)
    {
        InventorySlot tempSlot = invSlot;
        float minMagnitude = -1;

        foreach(InventorySlot invenSlot in inventorySlots)
        {
            if (minMagnitude < 0) minMagnitude = (invenSlot.itemHolder.transform.position - tempSlot.gameObject.transform.position).magnitude;
            if(minMagnitude > (invenSlot.itemHolder.transform.position - tempSlot.gameObject.transform.position).magnitude)
            {
                tempSlot = invenSlot;
            }
        }

        InventorySlot T;
        T = tempSlot;
        tempSlot.item = invSlot.item;
        invSlot.item = T.item;
    }

    public void ChangeActiveBlueprint(BlueprintSlot blueprint)
    {
        selectedBlueprintSlot = blueprint;
        craftSlots.Clear();
    }

}
