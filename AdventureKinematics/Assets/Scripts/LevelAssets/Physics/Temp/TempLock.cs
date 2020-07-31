using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempLock : MonoBehaviour
{
    public GameObject Key;
    Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == Key)
        {
            rb.isKinematic = false;
        }
    }
}
