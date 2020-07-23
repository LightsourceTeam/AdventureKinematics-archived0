using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class GameInventorySlot : MonoBehaviour
{
    public GameItem item;
    public GameInventory inventorySystem;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Is Clicked");
        inventorySystem.activeSlot = this;
    }

    void Start()
    {
     
    }

    void Update()
    {
        
    }
}
