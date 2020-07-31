using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameItem : MonoBehaviour
{
    public Sprite previewSprite;
    [NonSerialized] public MainController playerController;
    public bool isFixedUpdate = false;

    public List<GameItem> craftCreationList;
    public bool isCraftable = false;

    [NonSerialized] public int craftItemCount = 0;

    public virtual void Apply(Controller controller)
    {

    }

    public virtual void Pick(MainController controller) { playerController = controller; }


}
