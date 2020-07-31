using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleJump : GameItem
{
    public float JumpStrength;
    public float Cooldown;

    private float timeSinceJump;

    void Start()
    {
        timeSinceJump -= Cooldown;
    }

    public override void Apply(Controller controller)
    {     
        if(controller.joystick.Vertical >= .6 && timeSinceJump < Time.time) {
            playerController.rigBody.AddForce(controller.joystick.Direction.normalized * JumpStrength, ForceMode2D.Impulse);
            timeSinceJump = Time.time + Cooldown;
        }
    }
}
