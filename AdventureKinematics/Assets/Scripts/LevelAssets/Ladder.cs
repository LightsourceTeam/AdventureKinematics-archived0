using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : Interactable
{


    public override void OnFixedInteract(MainController controller)
    {
        controller.rigBody.AddForce((controller.rigBody.mass * 5f * controller.interactionJoystick.Direction) * ((5f - Mathf.Max(Vector2.Dot(controller.rigBody.velocity, controller.interactionJoystick.Direction), 0f)) / 5f), ForceMode2D.Impulse);
    }
}
