using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float jumpHeight = 400f;
    public float minJumpAirtime = 0.1f;
    public Vector2 movement;

    private Rigidbody2D rigBody;
    private CapsuleCollider2D CapsuleCollider;

    private const int contactPointsLength = 30;
    private ContactPoint2D[] contactPoints = new ContactPoint2D[contactPointsLength];
    
    private float timeSinceLastJump;
    private bool shouldJump = false;
    private bool alreadyJumped = false;

    void Start()
    {
        timeSinceLastJump = -minJumpAirtime - 1f;
        rigBody = this.GetComponent<Rigidbody2D>();
        CapsuleCollider = this.GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        // ig you press jump button, then player should jump
        if (Input.GetButton("Jump")) shouldJump = true;
        else
        {
            alreadyJumped = false;
            shouldJump = false;         // set it to false, because if you release button, player then will jump if you press the next time
        }
    }

    void FixedUpdate()
    {
        moveCharacter(movement);
        checkForJumping();
    }


    void moveCharacter(Vector2 direction)
    {
        movement = new Vector2(Input.GetAxis("Horizontal"), 0);

        rigBody.AddForce((rigBody.mass * direction * walkSpeed) * ((walkSpeed - Mathf.Abs(Vector2.Dot(rigBody.velocity, transform.right))) / walkSpeed ), ForceMode2D.Impulse);
    }

    void checkForJumping()
    {
        // get count of collisions, and collisions themselves, and also get maximal possible index of collisios
        int n = CapsuleCollider.GetContacts(contactPoints);
        n = Mathf.Min(contactPointsLength, n);

        // if our player should jump, and it's not already jumped
        if (shouldJump && !alreadyJumped)
        {

            // then check each object it's colliding with
            for (int i = 0; i < n; i++)
            {
                // if the contact point is something like our legs, and not a head, and jump airtime is already longer than minimal acceptable, then:
                if ((Vector2.Dot(transform.up, contactPoints[i].normal) > 0) && ((timeSinceLastJump - Time.time + minJumpAirtime) < 0f))
                {
                    // jump
                    timeSinceLastJump = Time.time;
                    rigBody.AddForce(transform.up * jumpHeight, ForceMode2D.Impulse);
                    alreadyJumped = true;

                    // and break cycle to do not iterate further (for optimizing)
                    break;
                }
            }
        }
    }
}
