using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameItem : MonoBehaviour
{

    public Sprite previewSprite;
    [NonSerialized] public GameInventory gameInventory;
    public bool isFixedUpdate = false;
    public virtual void Apply(bool joystickState, Vector2 joystickPos)
    {

    }

    public virtual void Pick(GameInventory inventory) { gameInventory = inventory; }

}
