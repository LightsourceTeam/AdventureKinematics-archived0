using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameItem : MonoBehaviour
{

    public Sprite sprite;

    public virtual void Apply(short joystickState, Vector2 joystickPos)
    {

    }

}
