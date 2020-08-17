using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintChain : MonoBehaviour
{
    public ChainSlot slotTemplate;
    public Transform subBlueprintParent;
    
    public InventorySystem inventory;
    
    public List<ChainSlot> subBlueprintsSlots;

    public BlueprintSlot blueprintSlot;

    public Vector2 slotSize;
    public Vector4 slotSpacing;
    
    public void InitBlueprint()
    {
        subBlueprintsSlots[0].currentBlueprint = blueprintSlot.originalBlueprint;
        WalkOverSubBlueprints(subBlueprintsSlots[0]);
        CreateWindow();
    }

    private void WalkOverSubBlueprints(ChainSlot currentBlueprint)
    {
        subBlueprintsSlots.Add(currentBlueprint);
        WalkOverSubBlueprints(currentBlueprint);   
    }


    public void CreateWindow()
    {
        foreach (ChainSlot blueSlot in subBlueprintsSlots)
        {
            Instantiate(slotTemplate);
        }
    }

    public void ShowWindow()
    {
        gameObject.SetActive(true);
    }

    public void HideWindow()
    {
        gameObject.SetActive(false);
    }

}