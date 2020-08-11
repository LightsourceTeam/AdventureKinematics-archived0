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
    
    public List<List<ViewedBlueprint>> allSubBlueprints;
    public List<GameObject> subBlueprintsSlots;

    public Blueprint test;
    
    public void InitBlueprint()
    {
        allSubBlueprints = new List<List<ViewedBlueprint>>();
        
        List<ViewedBlueprint> firstLayer = new List<ViewedBlueprint>();
        firstLayer.Add(new ViewedBlueprint(test /*inventory.selectedBlueprintSlot.originalBlueprint*/, -1));
        allSubBlueprints.Add(firstLayer);

        WalkOverSubBlueprints(allSubBlueprints[0][0].blueprint, 0, 0);

        for(int i = 0; i < allSubBlueprints.Count; i++)
        {
            for (int j = 0; j < allSubBlueprints[i].Count; j++)
            {
                //subBlueprintsSlots.Add(Instantiate(subBlueprintSlot, subBlueprintParent));
                Debug.Log(allSubBlueprints[i][j].blueprint + " index: " + j + " parentIndex: " + allSubBlueprints[i][j].parentIndex + " layerIndex: " + i);
            }
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

        int i = 0;
        foreach (Blueprint blueprint in currentBlueprint.subBlueprints)     // iterate over all the subblueprints in this layer
        {
            if (blueprint != null)
            {
                // add new item in the layer
                allSubBlueprints[layerIndex].Add(new ViewedBlueprint(blueprint, parentIndex));

                // walk over all the subblueprints in the current blueprint
                WalkOverSubBlueprints(blueprint, layerIndex, allSubBlueprints[layerIndex].Count - 1);
            }
            else Debug.LogWarning("Blueprint \"" + currentBlueprint.name + "\" has got null-subblueprint at index " + i + "!");

            i++;
        }
    }


    public void CreateWindow()
    {
        //float width = subBlueprintArea.GetComponent<RectTransform>().sizeDelta.x;
        //int y = 0;
        //int x = 0;
        //for (int i = 0; i < lastIndex; i++)
        //{
        //    foreach(ViewedBlueprint viewedBlueprint in allSubBlueprints)
        //    {
        //        y++;
        //        x++;
        //        if (viewedBlueprint.getLayerIndex == i && allSubBlueprints[y].getLayerIndex != i)
        //        {
        //            int j = (100 / x) / 100;
        //            for(int z = 1; z == x; z++)
        //            {
        //                Transform tempTransform.x = subBlueprintArea.GetComponent<RectTransform>().sizeDelta.x / (j * z);
        //                subBlueprintsSlots[y].transform.position.x = tempTransform;
        //            }
        //            x = 0;
        //        }
        //    }
        //}
    }


    public class ViewedBlueprint
    {
        public Blueprint blueprint;
        public int parentIndex;

        public ViewedBlueprint(Blueprint bp, int parIdx)
        {
            parentIndex = parIdx;
            blueprint = bp;
        }
    }

}