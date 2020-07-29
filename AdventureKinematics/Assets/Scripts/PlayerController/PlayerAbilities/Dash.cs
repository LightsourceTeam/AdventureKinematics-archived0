using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Ability
{
    public float DashStrength;
    public float Cooldown;

    private float timeSinceJump;

    void Start()
    {
        timeSinceJump -= Cooldown;
    }

    public override void Apply()
    {
        if ((playerController.ultraJoystick.Horizontal >= .6 || playerController.ultraJoystick.Horizontal <= -.6) && (playerController.ultraJoystick.Vertical <= .2 && playerController.ultraJoystick.Vertical >= -.2) && timeSinceJump < Time.time)
        {
            playerController.rigBody.AddForce(playerController.ultraJoystick.Direction.normalized * DashStrength, ForceMode2D.Impulse);
            timeSinceJump = Time.time + Cooldown;
        }
    }
}
