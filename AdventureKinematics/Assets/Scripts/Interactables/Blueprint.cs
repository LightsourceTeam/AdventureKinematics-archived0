using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blueprint : MonoBehaviour
{
    public List<GameItem> craftList;
    public GameItem craftItem;

    public Blueprint subBlueprint;
    public Blueprint parentBlueprint;

    public virtual GameItem Craft()
    {
        return Instantiate(craftItem.gameObject).GetComponent<GameItem>();
    }
}
