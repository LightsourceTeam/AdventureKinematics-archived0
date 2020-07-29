using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingItem : GameItem
{
    public float flyForce;
    public float MaxVelocity;

    public override void Apply(bool joystickState, Vector2 joystickPos)
    {
        if (gameInventory.playerController.itemJoystick.Direction.magnitude >= .3)
        {
            gameInventory.playerController.rigBody.AddForce(joystickPos * (MaxVelocity - Mathf.Abs(Vector2.Dot(gameInventory.playerController.rigBody.velocity, gameInventory.playerController.itemJoystick.Direction.normalized)) / MaxVelocity) * flyForce, ForceMode2D.Force);
        }
    }
}
