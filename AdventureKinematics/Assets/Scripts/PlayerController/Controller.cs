using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [NonSerialized] public MainController mainController;
    [NonSerialized] public Joystick joystick;

    public virtual void Init(MainController controller) { }
}
