using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [NonSerialized] public PlayerController playerController;

    public float walkSpeed = 10f;
    public float jumpHeight = 400f;
    public float minJumpAirtime = 0.1f;
    public float acceptableMotionSlope = 45f;

    private List<ContactPoint2D> contactPoints;

    public float timeSinceLastJump;
    private bool shouldJump = false;
    private bool alreadyJumped = false;

    // Initialising Movement System
    public void Init(PlayerController controller)
    {
        playerController = controller;

        contactPoints = new List<ContactPoint2D>();
        timeSinceLastJump = -minJumpAirtime - 1f;
    }

    private void Update()
    {
        // ig you press jump button, then player should jump
        if (playerController.movementJoystick.Vertical >= .5) shouldJump = true;
        else
        {
            alreadyJumped = false;
            shouldJump = false;         // set it to false, because if you release button, player then will jump if you press the next time
        }

    }


    void FixedUpdate()
    {
        Move();
    }


    void Move()
    {
        // get count of collisions, and collisions themselves, and also get maximal possible index of collisios
        int n = playerController.playerCollider.GetContacts(contactPoints);

        // then check each object it's colliding with
        foreach (ContactPoint2D contactPoint in contactPoints)
        {
            // if the contact point is something like our legs, and not a head, and jump airtime is already longer than minimal acceptable, then:
            if (Vector2.Dot(transform.up, contactPoint.normal) > Mathf.Cos(acceptableMotionSlope * Mathf.Deg2Rad))
            {
                // move
                playerController.rigBody.AddForce((playerController.rigBody.mass * walkSpeed * (new Vector2(playerController.movementJoystick.Horizontal, 0))) * ((walkSpeed - Mathf.Abs(Vector2.Dot(playerController.rigBody.velocity, transform.right))) / walkSpeed), ForceMode2D.Impulse);

                // if our player should jump, and it's not already jumped
                if (shouldJump && !alreadyJumped)
                {

                    if (timeSinceLastJump - Time.time + minJumpAirtime < 0f)
                    {
                        // jump
                        timeSinceLastJump = Time.time;
                        playerController.rigBody.AddForce(transform.up * jumpHeight, ForceMode2D.Impulse);
                        alreadyJumped = true;

                    }
                }

                break;
            }
        }

    }
}
