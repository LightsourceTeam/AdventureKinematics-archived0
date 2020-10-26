using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlueprintSlot : MonoBehaviour, IPointerClickHandler
{
    [NonSerialized] public BlueprintChain parentChain;

    private Blueprint _originalBlueprint = null;
    public Blueprint originalBlueprint 
    { 
        get { return _originalBlueprint; } 
        set 
        { 
            if(_originalBlueprint == null) 
            {
                _originalBlueprint = value;
                blueprint = value;
            }
        } 
    }

    [NonSerialized] public Blueprint blueprint;

    public void OnPointerClick(PointerEventData eventData)
    {
        parentChain.ChangeActiveBlueprint(this);
    }
}
