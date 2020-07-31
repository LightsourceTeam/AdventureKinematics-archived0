using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftSystem : MonoBehaviour
{
    public MainController playerController;
    public List<GameItem> itemCreationList;
    public GameItem craftItem;

    public void Init(MainController controller) { }

    void Update()
    {
        craftItem.craftItemCount = 0;
        List<GameItem> Temp = itemCreationList;
        foreach (GameItem craftItem in craftItem.craftCreationList)
        {
            if (Temp.Contains(craftItem)) Temp.Remove(craftItem);
        }
        if (Temp.Count == 0) Craft();
    }

    void Craft()
    {
        craftItem.gameObject.SetActive(true);
        foreach (GameItem item in itemCreationList) Destroy(item.gameObject);
    }
}
