using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blueprint : MonoBehaviour
{
    [SerializeField] private List<GameItem> CraftList;
    
    public List<Type> craftList                   // list gets used when crafting from scratch, having only root items
    { 
        get
        {
            List<Type> returnVal = new List<Type>();
            foreach (var item in CraftList) returnVal.Add(item.GetType());
            return returnVal;
        }
    }

    public List<Type> hierarchyCraftList          // list gets used when crafting from base item (subBlueprint's item)
    {
        get
        {
            List<Type> returnVal = new List<Type>();
            foreach (var item in CraftList) returnVal.Add(item.GetType());
            if (subBlueprint != null)
            {
                foreach (var baseItem in subBlueprint.craftList) returnVal.Remove(baseItem);
                returnVal.Add(subBlueprint.craftItem.GetType());
            }
            return returnVal;
        }
    }


    public GameItem craftItem;

    public Blueprint subBlueprint;

    public virtual GameItem Craft()
    {
        return Instantiate(craftItem);
    }
}
