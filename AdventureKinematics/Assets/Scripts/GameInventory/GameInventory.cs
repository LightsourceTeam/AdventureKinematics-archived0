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

    void Update()
    {
        
    }

    public void Pick(GameItem item)
    {
        if (activeSlot.item != null) Drop();

        activeSlot.item = item;
        activeSlot.spriteObject.GetComponent<Image>().sprite = item.sprite;
        item.gameObject.SetActive(false);
    }

    public void Drop()
    {
        if (activeSlot.item != null)
        {
            activeSlot.item.gameObject.SetActive(true);
            activeSlot.item.transform.position = controller.gameObject.transform.position;
            activeSlot.item.gameObject.GetComponent<Rigidbody2D>().AddForce((new Vector2(controller.pickdropJoystick.Horizontal, controller.pickdropJoystick.Vertical)) * 5, ForceMode2D.Impulse);
            activeSlot.item = null;
            activeSlot.spriteObject.GetComponent<Image>().sprite = null;
        }
    }
}
