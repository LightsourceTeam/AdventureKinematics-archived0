using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlueprintSlot : MonoBehaviour, IPointerClickHandler
{
    [NonSerialized] public InventoryController inventoryController;

    [NonSerialized] public Blueprint originalBbueprint;
    [NonSerialized] public Blueprint blueprint;

    public void OnPointerClick(PointerEventData eventData)
    {
        inventoryController.selectedBlueprintSlot = this;
    }
}
