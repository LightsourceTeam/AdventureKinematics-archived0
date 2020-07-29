using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleJump : Ability
{
    public float JumpStrength;
    public float Cooldown;

    private float timeSinceJump;

    void Start()
    {
        timeSinceJump -= Cooldown;
    }

    public void Update()
    {
        if (playerController)
        {
            if (playerController.ultraJoystick.State)
            {
                Apply();
            }
        }
    }

    public override void Apply()
    {     
        if(playerController.ultraJoystick.Vertical >= .6 && timeSinceJump < Time.time) {
            playerController.rigBody.AddForce(playerController.ultraJoystick.Direction.normalized * JumpStrength, ForceMode2D.Impulse);
            timeSinceJump = Time.time + Cooldown;
        }
    }
}
