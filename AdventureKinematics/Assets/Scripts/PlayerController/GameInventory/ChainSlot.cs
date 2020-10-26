using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainSlot : MonoBehaviour
{
    // BlueprintTreeSlot
    private BlueprintChain parentChain;
    private Blueprint blueprint;

    private RectTransform myTransform;
    public RectTransform rectTransform { get { return myTransform; } } 

    public void Remove()
    {
        Destroy(gameObject);
    }

    static public ChainSlot NewSlot(Blueprint blueprint, BlueprintChain parentChain)
    {
        ChainSlot slot = Instantiate(parentChain.slotTemplate, parentChain.slotsParent);
        slot.blueprint = blueprint;
        slot.parentChain = parentChain;
        slot.myTransform = slot.GetComponent<RectTransform>();

        return slot;
    }
}
