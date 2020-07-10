using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class BallonScript : MonoBehaviour
{
    public float Speed;

    private Rigidbody2D RigBody;

    void Start()
    {
        RigBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        RigBody.AddForce(Vector2.up * Speed);
    }
}
