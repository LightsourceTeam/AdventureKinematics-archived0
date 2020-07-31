using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : GameItem
{
    public float DashStrength;
    public float Cooldown;

    private float timeSinceJump;

    void Start()
    {
        timeSinceJump -= Cooldown;
    }

    public override void Apply(Controller controller)
    {
        if ((controller.joystick.Horizontal >= .6 || controller.joystick.Horizontal <= -.6) && (controller.joystick.Vertical <= .2 && controller.joystick.Vertical >= -.2) && timeSinceJump < Time.time)
        {
            playerController.rigBody.AddForce(controller.joystick.Direction.normalized * DashStrength, ForceMode2D.Impulse);
            timeSinceJump = Time.time + Cooldown;
        }
    }
}
