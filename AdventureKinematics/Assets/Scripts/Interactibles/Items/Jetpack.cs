using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jetpack : GameItem
{
    public float Strength;
    public float MaxVelocity;

    public override void OnFixedApply(Controller controller)
    {
        if (controller.joystick.Direction.magnitude >= .3)
        {
            Vector2 joystickPos = new Vector2(controller.joystick.Horizontal, Mathf.Max(controller.joystick.Vertical, 0f));
            controller.mainController.rigBody.AddForce(joystickPos * (MaxVelocity - Vector2.Dot(controller.mainController.rigBody.velocity, joystickPos.normalized) / MaxVelocity) * Strength, ForceMode2D.Force);
        }
    }
}
