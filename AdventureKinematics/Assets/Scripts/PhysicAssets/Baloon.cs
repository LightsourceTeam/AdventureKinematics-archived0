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
    private DistanceJoint2D MyHinge;

    void Start()
    {
        MyHinge = gameObject.GetComponent<DistanceJoint2D>(); 
        if (!MyHinge) MyHinge = gameObject.AddComponent<DistanceJoint2D>();
        MyRigBody = gameObject.GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (isOn)
        {
            MyRigBody.AddForce(Vector2.up * BaloonForce * (MaxSpeed - Mathf.Abs(Vector2.Dot(MyRigBody.velocity, Vector2.up))) / MaxSpeed);
        }
    }
}
