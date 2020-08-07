using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class GameInventorySlot : MonoBehaviour, IPointerClickHandler
{
    [NonSerialized] public GameItem item;
    public GameInventory inventorySystem;
    public GameObject previewSpriteObject;

    public void OnPointerClick(PointerEventData eventData)
    {
        inventorySystem.selectedSlot = this;
    }

}
