using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Baloon : MonoBehaviour
{
    public float BaloonForce;
    public float MaxSpeed = 50f;
    public bool isOn;

    private Rigidbody2D MyRigBody;

    void Start()
    {
        MyRigBody = gameObject.GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (isOn && !(MaxSpeed == 0f))
        {
            MyRigBody.AddForce(Vector2.up * BaloonForce * (MaxSpeed - Mathf.Min(MaxSpeed, Vector2.Dot(MyRigBody.velocity, Vector2.up))) / MaxSpeed);
        }
    }
}
