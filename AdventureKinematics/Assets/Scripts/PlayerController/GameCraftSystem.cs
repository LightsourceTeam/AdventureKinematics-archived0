using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCraftSystem : MonoBehaviour
{
    public MainController playerController;
    public List<GameItem> itemCreationList;
    public Blueprint currentBlueprint;

    public void Init(MainController controller) { }

    void Update()
    {
        currentBlueprint.craftItem.craftItemCount = 0;
        List<GameItem> Temp = itemCreationList;

        foreach (GameItem craftItem in currentBlueprint.craftItem.craftCreationList)
        {
            if (Temp.Contains(craftItem)) Temp.Remove(craftItem);
        }

        if (Temp.Count == 0)
        {
            currentBlueprint.Craft();
        }
    }

}
