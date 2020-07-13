using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lock : MonoBehaviour
{
    FixedJoint2D FJ;
    public GameObject Key;
 
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        FJ = GetComponent<FixedJoint2D>();
        if (other.gameObject == Key)
        {
            Destroy(FJ);
        }
    }
}
