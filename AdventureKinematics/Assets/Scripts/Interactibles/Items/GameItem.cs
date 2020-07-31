using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameItem : Interactable
{

    [NonSerialized] public MainController mainController;
    public Sprite previewSprite;

    public List<GameItem> craftCreationList;

    [NonSerialized] public int craftItemCount = 0;

    public virtual void OnApply(Controller controller) { }

    public virtual void OnFixedApply(Controller controller) { }

    public virtual void OnEndApply(Controller controller) { }

    public override void OnEndInteract(MainController controller)
    {
        mainController = controller;
        mainController.inventory.activeSlot.item = this;
        mainController.inventory.activeSlot.previewSpriteObject.GetComponent<Image>().sprite = previewSprite;
        gameObject.SetActive(false);
    }

}
