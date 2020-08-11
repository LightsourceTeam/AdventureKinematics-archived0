using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class SubBlueprintViewer : MonoBehaviour
{
    public GameObject subBlueprintSlot;
    public Transform subBlueprintParent;
    
    public InventorySystem inventory;
    public GameObject blueprintGUI;
    
    public List<ViewedBlueprint> allSubBlueprints = new List<ViewedBlueprint>();
    public List<GameObject> subBlueprintsSlots = new List<GameObject>();

    public void ExpandBlueprint()
    {
        blueprintGUI.gameObject.SetActive(true);

        subBlueprintsSlots.Clear();
        allSubBlueprints.Clear();

        List<int> currentLayerIndicies = new List<int>();
        currentLayerIndicies.Add(0);

        WalkOverSubBlueprints(inventory.selectedBlueprintSlot.originalBlueprint, 0, 0, currentLayerIndicies);

        foreach (ViewedBlueprint blueprint in allSubBlueprints)
        {
            subBlueprintsSlots.Add(Instantiate(subBlueprintSlot, subBlueprintParent));
            Debug.Log(blueprint.blueprint + " index: " + blueprint.index + " parentIndex: " + blueprint.parentIndex + " layerIndex: " + blueprint.layerIndex);
        }
    }

    public void CollapseBlueprint()
    {
        blueprintGUI.gameObject.SetActive(false);
    }

    private void WalkOverSubBlueprints(Blueprint currentBlueprint, int layerIndex, int parentIndex, List<int> currentIndicies)
    {
        layerIndex++;               // step over to a new layer

        // if there is no such a layer, add one
        if (currentIndicies.Count <= layerIndex) currentIndicies.Add(0);    
  
            
        foreach (Blueprint blueprint in currentBlueprint.subBlueprints)     // iterate over all the subblueprints in this layer
        {
            // add new item in the layer
            allSubBlueprints.Add(new ViewedBlueprint(currentIndicies[layerIndex], layerIndex, parentIndex, blueprint)); 
            
            // walk over all the subblueprints in the current blueprint
            WalkOverSubBlueprints(blueprint, layerIndex, currentIndicies[layerIndex], currentIndicies);

            // change an index of the current item
            currentIndicies[layerIndex] = currentIndicies[layerIndex] + 1;
        }
    }

    public class ViewedBlueprint
    {
        public ViewedBlueprint(int idx, int layerIdx, int parIdx, Blueprint bp)
        {
            index = idx;
            layerIndex = layerIdx;
            parentIndex = parIdx;
            blueprint = bp;
        }
        public int index;
        public int layerIndex;
        public int parentIndex;

        public Blueprint blueprint;
    }

}