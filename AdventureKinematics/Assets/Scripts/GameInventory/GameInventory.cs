using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInventory : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotParent;

    public List<GameInventorySlot> itemSlots;
    public GameInventorySlot activeSlot;
    public PlayerController controller;
    public int slotsCount;


    void Start()
    {
        itemSlots = new List<GameInventorySlot>(slotsCount);
        for(int i = 0; i < slotsCount; i++)
        {
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
            activeSlot.item.transform.position = controller.gameObject.transform.position;
            activeSlot.item.gameObject.GetComponent<Rigidbody2D>().AddForce(WhereToThrowDroppedItem * 5, ForceMode2D.Impulse);
            activeSlot.item = null;
            activeSlot.spriteObject.GetComponent<Image>().sprite = null;
        }

        if (item)
        {
            activeSlot.item = item;
            activeSlot.spriteObject.GetComponent<Image>().sprite = item.sprite;
            item.gameObject.SetActive(false);
        }
    }
}
