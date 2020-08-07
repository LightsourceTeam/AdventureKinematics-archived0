using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameItem : Interactable
{

    [NonSerialized] public MainController mainController;
    public Sprite previewSprite;
    public bool isFixed = false;

    // apply functions

    public virtual void OnFixedApply(Controller controller) { }    
    
    public virtual void OnApply(Controller controller) { }

    public virtual void OnLateApply(Controller controller) { }

    public virtual void OnEndApply(Controller controller) { }
 
    // mounted functions

    public virtual void OnFixedMounted(MainController controller) { }
    
    public virtual void OnMounted(MainController controller) { }

    public virtual void OnLateMounted(MainController controller) { }

    public virtual void OnEndMounted(MainController controller) { }


    // overriding onEnd Target

    public override void OnEndTarget(MainController controller)
    {
        mainController = controller;
        mainController.inventory.selectedSlot.item = this;
        mainController.inventory.selectedSlot.previewSpriteObject.GetComponent<Image>().sprite = previewSprite;
        gameObject.SetActive(false);
    }
}
