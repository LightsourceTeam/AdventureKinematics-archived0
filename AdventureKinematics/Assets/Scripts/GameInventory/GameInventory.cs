using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInventory : MonoBehaviour
{
    public List<GameInventorySlot> itemSlots;
    public List<GameObject> invSlots;
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
        short tempSlot = activeSlot.GetComponent<GameInventorySlot>().index;
        if(itemSlots[tempSlot].item == null)
        {
            itemSlots[tempSlot].item = item;
            itemSlots[tempSlot].spriteObject.GetComponent<Image>().sprite = item.sprite;
            item.gameObject.SetActive(false);
        }
    }

    public void Drop()
    {
        short tempSlot = activeSlot.GetComponent<GameInventorySlot>().index;
        Debug.Log("Drop!");
        if (itemSlots[tempSlot].item != null)
        {
            Debug.Log("Dropped an Item!");
            itemSlots[tempSlot].item.transform.position = transform.position;
            itemSlots[tempSlot].item.gameObject.SetActive(true);
            itemSlots[tempSlot].item = null;
            itemSlots[tempSlot].spriteObject.GetComponent<Image>().sprite = null;
        }
    }
}
