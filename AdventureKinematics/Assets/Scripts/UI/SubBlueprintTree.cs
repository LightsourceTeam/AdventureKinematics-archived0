using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubBlueprintTree : MonoBehaviour
{
    public BlueprintTreeSlot slotTemplate;
    public Transform subBlueprintParent;
    
    public InventorySystem inventory;
    
    public List<BlueprintTreeSlot> subBlueprintsSlots;

    public Blueprint test;

    public Vector2 slotSize;
    public Vector4 slotSpacing;
    
    public void InitBlueprint()
    {

        WalkOverSubBlueprints(test);
        CreateWindow();
    }

    private void WalkOverSubBlueprints(Blueprint currentBlueprint)
    {
        subBlueprintsSlots.Add(currentBlueprint);
        WalkOverSubBlueprints(currentBlueprint.subBlueprint);   
    }


    public void CreateWindow()
    {
    
    }

    public void ShowWindow()
    {
    
    }

    public void HideWindow()
    {
    
    }

}