using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintChain : MonoBehaviour
{
    public InventorySystem inventorySystem;
    public ChainSlot slotTemplate;
    public BlueprintSlot selectedBlueprint;

    [NonSerialized] public RectTransform slotsParent;
    private List<ChainSlot> chainSlots = new List<ChainSlot>();

    public void Init(InventorySystem inventory, Blueprint rootBlueprint)
    {
        inventorySystem = inventory;
        slotsParent = GetComponent<RectTransform>();

        chainSlots.Clear();
        WalkOverChain(rootBlueprint);

        foreach (var slot in chainSlots) slot.gameObject.SetActive(true);
    }

    private void WalkOverChain(Blueprint currentBlueprint)
    {

        chainSlots.Add(ChainSlot.NewSlot(currentBlueprint, this));
        if (currentBlueprint.subBlueprint != null) WalkOverChain(currentBlueprint.subBlueprint);
    }

    public void ShowWindow()
    {
        gameObject.SetActive(true);
    }

    public void HideWindow()
    {
        gameObject.SetActive(false);
    }

    public void ChangeActiveBlueprint(BlueprintSlot blueprint)
    {
        selectedBlueprint = blueprint;
    }
}