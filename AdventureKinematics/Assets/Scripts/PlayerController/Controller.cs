using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [NonSerialized] public MainController mainController;
    [NonSerialized] public Joystick joystick;
    [NonSerialized] public bool lastJState = false;


    [NonSerialized] public Vector2 lastDirection;

    public virtual void Init(MainController controller) { }
}
