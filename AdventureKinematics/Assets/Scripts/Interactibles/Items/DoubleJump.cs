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

    public override void OnEndApply(Controller controller)
    {     
        if(timeSinceJump < Time.time) {
            controller.mainController.rigBody.AddForce(controller.lastDirection * JumpStrength, ForceMode2D.Impulse);
            timeSinceJump = Time.time + Cooldown;
        }
    }
}
