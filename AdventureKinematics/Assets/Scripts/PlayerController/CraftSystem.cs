using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftSystem : MonoBehaviour
{
    public GameItem TryCraft(Blueprint currentBlueprint, List<GameItem> itemCreationList) 
    {
        HashSet<Type> givenItems = null;
        if (itemCreationList.Count > 0)
        {
            givenItems = new HashSet<Type>();
            foreach (var item in itemCreationList) givenItems.Add(item.GetType());

            if (givenItems.SetEquals(new HashSet<Type>(currentBlueprint.craftList)) ||
                givenItems.SetEquals(new HashSet<Type>(currentBlueprint.hierarchyCraftList)))
            {
                // if there are no missing craft items, take what player gave us, and turn it into item he wants
                foreach (GameItem item in itemCreationList) Destroy(item.gameObject);
                return currentBlueprint.Craft();
            }
        }
        return null;
    }
}
