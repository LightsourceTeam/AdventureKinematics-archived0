using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blueprint : MonoBehaviour
{
    public List<GameItem> craftList;
    public GameItem craftItem;
    public List<GameItem> subBlueprints;

    public virtual GameItem Craft()
    {
        return Instantiate(craftItem.gameObject).GetComponent<GameItem>();
    }
}
