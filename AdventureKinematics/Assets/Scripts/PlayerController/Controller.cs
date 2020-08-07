
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : CustomMonoBehaviour
{
    [NonSerialized] public MainController mainController;
    [NonSerialized] public Joystick joystick;


    [NonSerialized] public bool lastJState = false;
    [NonSerialized] public Vector2 lastDirection;

    // these variables are not used anywhere except this place, to make sure that lastJState and lastDirection
    // are certainly from the previous frame
    private Vector2 prevFrameDirection;
    private bool prevFrameJState;

    public virtual void Init(MainController controller) { }

    // Note: it is not desired to perform operations with joystick and direction in this funtion.
    protected override void EarlyUpdate()
    {
        lastJState = prevFrameJState;
        lastDirection = prevFrameDirection;
    }

    protected override void LateUpdate()
    {
        prevFrameJState = joystick.State;
        prevFrameDirection = joystick.Direction;
    }

}
