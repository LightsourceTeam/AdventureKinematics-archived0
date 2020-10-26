using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerUpHandler
{
    [NonSerialized] public GameItem item;
    [NonSerialized] public InventorySystem inventorySystem;
    public Image previewSpriteObject;
    public GameObject itemHolder;

    public void OnPointerClick(PointerEventData eventData)
    {
        inventorySystem.selectedSlot = this;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inventorySystem.RplaceItemFromSlot(this);   
    }

}
