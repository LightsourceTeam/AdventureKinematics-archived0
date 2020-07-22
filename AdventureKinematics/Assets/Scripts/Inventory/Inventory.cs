using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public GameObject InventoryUI;

    private List<ItemList> itemList;

    public Inventory() {
        itemList = new List<ItemList>();

        AddItem(new ItemList { itemType = ItemList.ItemType.Item1 });

        Debug.Log("Inventory Created and count is: " + itemList.Count);
    }

    public void AddItem(ItemList item)
    {
        itemList.Add(item);
    }

    public List<ItemList> GetItemLists()
    {
        return itemList;
    }

}
