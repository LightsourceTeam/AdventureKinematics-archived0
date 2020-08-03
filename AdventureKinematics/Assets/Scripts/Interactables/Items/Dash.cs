using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : GameItem
{
    public float DashStrength;
    public float Cooldown;

    private float timeSinceJump = 0f;

    public override void OnEndApply(Controller controller)
    {
        if ((!controller.joystick.State) && timeSinceJump <= Time.time)
        {
            controller.mainController.rigBody.AddForce(Vector2.right * Vector2.Dot(controller.lastDirection.normalized, Vector2.right) * DashStrength, ForceMode2D.Impulse);
            timeSinceJump = Time.time + Cooldown;
        }
    }
}
