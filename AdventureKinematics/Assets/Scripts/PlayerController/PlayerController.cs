using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 10f;
    public float jumpHeight = 400f;
    public float minJumpAirtime = 0.1f;
    public float acceptableMotionSlope = 45f;

    // controller values

    public Joystick movementJoystick;
    public Joystick itemJoystick;
    public Joystick pickdropJoystick;
    public Joystick ultraJoystick;

    public GameInventory Inventory;


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
        if (movementJoystick.Vertical >= .5) shouldJump = true;
        else
        {
            alreadyJumped = false;
            shouldJump = false;         // set it to false, because if you release button, player then will jump if you press the next time
        }


        if (pickdropJoystick.Vertical >= .25)
        {
            Inventory.Drop();
        }      
    }


    void FixedUpdate()
    {
        Move();
    }

    public void OnItem(GameItem Item)
    {
        if(pickdropJoystick.Vertical <= -.25)
        {
            Inventory.Pick(Item);
        }
    }

    void Move()
    {
        // get count of collisions, and collisions themselves, and also get maximal possible index of collisios
        int n = CapsuleCollider.GetContacts(contactPoints);
        n = Mathf.Min(contactPointsLength, n);


        // then check each object it's colliding with
        for (int i = 0; i < n; i++)
        {
            // if the contact point is something like our legs, and not a head, and jump airtime is already longer than minimal acceptable, then:
            Debug.Log(Mathf.Cos(acceptableMotionSlope * Mathf.Deg2Rad));
            if (Vector2.Dot(transform.up, contactPoints[i].normal) > Mathf.Cos(acceptableMotionSlope * Mathf.Deg2Rad))
            {
                // move
                rigBody.AddForce((rigBody.mass * walkSpeed * (new Vector2(movementJoystick.Horizontal, 0))) * ((walkSpeed - Mathf.Abs(Vector2.Dot(rigBody.velocity, transform.right))) / walkSpeed), ForceMode2D.Impulse);

                // if our player should jump, and it's not already jumped
                if (shouldJump && !alreadyJumped)
                {

                    if ((timeSinceLastJump - Time.time + minJumpAirtime) < 0f)
                    {
                        // jump
                        timeSinceLastJump = Time.time;
                        rigBody.AddForce(transform.up * jumpHeight, ForceMode2D.Impulse);
                        alreadyJumped = true;

                    }
                }

                break;
            }
        }

    }
}
