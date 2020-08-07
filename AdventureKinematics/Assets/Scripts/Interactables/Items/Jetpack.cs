using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Path.GUIFramework;
using UnityEngine;

public class Jetpack : GameItem
{
    public float Strength;
    public float MaxVelocity;

    public override void OnFixedApply(Controller controller)
    {
        Vector2 flyForce = controller.joystick.Direction;
        flyForce.x = (MaxVelocity * flyForce.x - controller.mainController.rigBody.velocity.x) / MaxVelocity;
        flyForce.y = (Mathf.Max(MaxVelocity * flyForce.y, 0f) - Mathf.Max(controller.mainController.rigBody.velocity.y, 0f)) / MaxVelocity;
        flyForce *= Strength;

        controller.mainController.rigBody.AddForce(flyForce , ForceMode2D.Force);
    }

}
