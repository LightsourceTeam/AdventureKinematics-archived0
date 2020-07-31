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
    [NonSerialized] public GameInventorySlot activeSlot;
    
    public MainController playerController;
    public int slotsCount;

    public void Init(MainController controller)
    {
        itemSlots = new List<GameInventorySlot>(slotsCount);
        for (int i = 0; i < slotsCount; i++)
        {
            playerController = controller;
            
            GameObject slot = Instantiate(slotPrefab, slotParent);
            itemSlots.Add(slot.GetComponent<GameInventorySlot>());
            slot.SetActive(true);
        }
        activeSlot = itemSlots[0];
    }

    public void SwitchActiveItem(GameItem item, Vector2 WhereToThrowDroppedItem)
    {
        if (activeSlot.item != null)
        {
            activeSlot.item.gameObject.SetActive(true);
            activeSlot.item.transform.position = playerController.gameObject.transform.position;
            activeSlot.item.gameObject.GetComponent<Rigidbody2D>().AddForce(WhereToThrowDroppedItem * 5, ForceMode2D.Impulse);
            activeSlot.item = null;
            activeSlot.previewSpriteObject.GetComponent<Image>().sprite = null;
        }

        
    }
}
