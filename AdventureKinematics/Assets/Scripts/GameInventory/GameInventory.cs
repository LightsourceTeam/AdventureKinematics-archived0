using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInventory : MonoBehaviour
{
    public List<GameInventorySlot> itemSlots;
    public GameInventorySlot activeSlot;
    public PlayerController controller;
    public int slotsCount;


    // Start is called before the first frame update
    void Start()
    {
        itemSlots = new List<GameInventorySlot>(slotsCount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Pick()
    {

    }

    void Drop()
    {

    }
}
