using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInventory : MonoBehaviour
{
    public List<GameInventorySlot> itemSlots;
    public GameInventorySlot activeSlot;
    public PlayerController controller;
    public int slotsCount;


    void Start()
    {
        itemSlots = new List<GameInventorySlot>(slotsCount);
        foreach(Transform child in transform)
        {
            itemSlots.Add(child.gameObject.GetComponent<GameInventorySlot>());
        }
    }

    void Update()
    {
        
    }

    public void Pick(GameItem item)
    {
        //Debug.Log("Pick!");
            foreach(GameInventorySlot itemSlot in itemSlots)
            {
                if(itemSlot.item == null)
                {
                    Debug.Log("Not null");
                    itemSlot.item = item;
                    item.gameObject.SetActive(false);
                    break;
                }
            }
    }

    public void Drop()
    {

    }
}
