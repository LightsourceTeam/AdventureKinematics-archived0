using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class SubBlueprintViewer : MonoBehaviour
{
    public GameObject subBlueprintSlot;
    public Transform subBlueprintParent;

    public GameObject subBlueprintArea;
    
    public InventorySystem inventory;
    
    public List<List<ViewedBlueprint>> allSubBlueprints = new List<ViewedBlueprint>();
    public List<GameObject> subBlueprintsSlots;
    

    private int lastIndex = 0;

    public void InitBlueprint()
    {
        subBlueprintsSlots = new List<List<GameObject>>();
        
        List<GameObject> firstLayer = new List<GameObject>();
        firstLayer.Add(inventory.selectedBlueprintSlot.originalBlueprint);
        allSubBlueprints.Add(firstLayer);

        WalkOverSubBlueprints(inventory.selectedBlueprintSlot.originalBlueprint, 0, 0);

        foreach (ViewedBlueprint blueprint in allSubBlueprints)
        {
            subBlueprintsSlots.Add(Instantiate(subBlueprintSlot, subBlueprintParent));
            Debug.Log(blueprint.blueprint + " index: " + blueprint.index + " parentIndex: " + blueprint.parentIndex + " layerIndex: " + blueprint.layerIndex);
        }
    }

    public void CollapseBlueprint()
    {
        
    }

    private void WalkOverSubBlueprints(Blueprint currentBlueprint, int layerIndex, int parentIndex)
    {
        layerIndex++;               // step over to a new layer

        // if there is no such a layer, add one
        if (allSubBlueprints.Count <= layerIndex) allSubBlueprints.Add(new List<ViewedBlueprint>());    
  
            
        foreach (Blueprint blueprint in currentBlueprint.subBlueprints)     // iterate over all the subblueprints in this layer
        {
            // add new item in the layer
            allSubBlueprints.Add(new ViewedBlueprint(parentIndex, blueprint)); 
            
            // walk over all the subblueprints in the current blueprint
            WalkOverSubBlueprints(blueprint, layerIndex, currentIndicies[layerIndex], currentIndicies);

            // change an index of the current item
            currentIndicies[layerIndex] = currentIndicies[layerIndex] + 1;

            //lastIndex = layerIndex;
        }
    }


    public void CreateWindow()
    {
        float width = subBlueprintArea.GetComponent<RectTransform>().sizeDelta.x;
        int y = 0;
        int x = 0;
        for (int i = 0; i < lastIndex; i++)
        {
            foreach(ViewedBlueprint viewedBlueprint in allSubBlueprints)
            {
                y++;
                x++;
                if (viewedBlueprint.getLayerIndex == i && allSubBlueprints[y].getLayerIndex != i)
                {
                    int j = (100 / x) / 100;
                    for(int z = 1; z == x; z++)
                    {
                        Transform tempTransform.x = subBlueprintArea.GetComponent<RectTransform>().sizeDelta.x / (j * z)
                        subBlueprintsSlots[y].transform.position.x = tempTransform;
                    }
                    x = 0;
                }
            }
        }
    }


    public class ViewedBlueprint
    {
        public Blueprint blueprint;
        public int parentIndex;

        public ViewedBlueprint(int parIdx, Blueprint bp)
        {
            parentIndex = parIdx;
            blueprint = bp;
        }
    }

}