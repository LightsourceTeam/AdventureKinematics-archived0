using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 10.0f;
    public float jumpHeight = 400.0f;
    public Vector2 movement;

    private Rigidbody2D rigBody;
    private CapsuleCollider2D CapsuleCollider;
    private ContactPoint2D[] contactPoints = new ContactPoint2D[30];
    private int contactPointsLength;

    private Vector2 upVector = new Vector2(0,1);
    private Vector2 forwardVector = new Vector2(1,0);

    void Start()
    {
        rigBody = this.GetComponent<Rigidbody2D>();
        CapsuleCollider = this.GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        contactPointsLength = CapsuleCollider.GetContacts(contactPoints);
        movement = new Vector2(Input.GetAxis("Horizontal"), 0);
        if (Input.GetButtonDown("Jump"))
        {
            for (int i = 0; i < contactPointsLength; i++)
            {
                if (Vector2.Dot(upVector, contactPoints[i].normal) > 0)
                {
                    jump();
                    break;
                }
            }
        }
    }

    void FixedUpdate()
    {
        moveCharacter(movement);
    }

    void moveCharacter(Vector2 direction)
    {
        rigBody.AddForce((rigBody.mass * direction * walkSpeed) * ((walkSpeed - Mathf.Abs(Vector2.Dot(rigBody.velocity, forwardVector))) / walkSpeed ), ForceMode2D.Impulse);
    }

    void jump()
    {
        rigBody.AddForce(Vector2.up * jumpHeight, ForceMode2D.Impulse);
    }
}
