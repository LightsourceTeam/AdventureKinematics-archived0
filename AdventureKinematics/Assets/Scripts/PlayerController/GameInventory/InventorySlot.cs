using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [NonSerialized] public GameItem item;
    public InventoryController inventorySystem;
    public GameObject previewSpriteObject;

    public void OnPointerClick(PointerEventData eventData)
    {
        inventorySystem.selectedSlot = this;
    }

}
