using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : Controller
{
    public float walkSpeed = 10f;
    public float jumpHeight = 400f;
    public float minJumpAirtime = 0.1f;
    public float acceptableMotionSlope = 45f;

    [NonSerialized] public float bonusSpeed = 0f;
    [NonSerialized] public float percentSpeed = 0f;

    private Vector2 motionDirection;
    private List<ContactPoint2D> contactPoints;

    private float finalSpeed;

    private float timeSinceLastJump;
    private bool shouldJump = false;
    private bool alreadyJumped = false;

    // Initialising Movement System
    public override void Init(MainController controller)
    {
        mainController = controller;
        joystick = mainController.movementJoystick;

        contactPoints = new List<ContactPoint2D>();
        timeSinceLastJump = -minJumpAirtime - 1f;
        motionDirection = new Vector2(joystick.Horizontal, 0);

        lastJState = joystick.State;
        lastDirection = joystick.Direction;
    }


    protected override void Update()
    {
        // if you press jump button, then player should jump
        if (joystick.Vertical >= .5) shouldJump = true;
        else
        {
            alreadyJumped = false;
            shouldJump = false;         // set it to false, because if you release button, player then will jump if you press the next time
        }

        motionDirection = new Vector2(joystick.Horizontal, 0);

        // resetting bonuses
        bonusSpeed = percentSpeed = 0f;
    }


    protected override void FixedUpdate()
    {
        // get count of collisions, and collisions themselves, and also get maximal possible index of collisios
        int n = mainController.playerCollider.GetContacts(contactPoints);
        finalSpeed = (walkSpeed + bonusSpeed) * ((100 + percentSpeed) / 100);

        // then check each object it's colliding with
        foreach (ContactPoint2D contactPoint in contactPoints)
        {
            // if the contact point is something like our legs, and not a head, and jump airtime is already longer than minimal acceptable, then:
            if (Vector2.Dot(transform.up, contactPoint.normal) > Mathf.Cos(acceptableMotionSlope * Mathf.Deg2Rad))
            {
                // move
                mainController.rigBody.AddForce((mainController.rigBody.mass * finalSpeed * motionDirection) * ((finalSpeed - Mathf.Max(Vector2.Dot(mainController.rigBody.velocity, motionDirection.normalized), 0f)) / finalSpeed), ForceMode2D.Impulse);

                // if our player should jump, and it's not already jumped
                if (shouldJump && !alreadyJumped)
                {

                    if (timeSinceLastJump - Time.time + minJumpAirtime < 0f)
                    {
                        // jump
                        timeSinceLastJump = Time.time;
                        mainController.rigBody.AddForce(transform.up * jumpHeight, ForceMode2D.Impulse);
                        alreadyJumped = true;

                    }
                }

                break;
            }
        }

    }
}
