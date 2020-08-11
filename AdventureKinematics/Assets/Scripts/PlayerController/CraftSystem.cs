using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftSystem : MonoBehaviour
{
    public GameItem TryCraft(Blueprint currentBlueprint, List<GameItem> itemCreationList) 
    {

        // check if there are no missing craft items
        List<GameItem> missingCraftItems = itemCreationList;    
        foreach (GameItem craftItem in currentBlueprint.craftList)
        { 
            if (missingCraftItems.Contains(craftItem)) missingCraftItems.Remove(craftItem); 
        }


        // if there are some missing items, craft does not happen
        if (missingCraftItems.Count != 0) return null;


        // if there are no missing craft items, take what player gave us, and turn it into item he wants
        foreach (GameItem item in itemCreationList) Destroy(item.gameObject);
        return currentBlueprint.Craft();

    }
}
