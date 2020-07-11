using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fan : MonoBehaviour
{
    Rigidbody2D TransformItem;
    public float FanForce;
    
    void Start()
    {
        
    }

    
    void Update()
    {
       
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        Vector2 fan = new Vector2(FanForce, 0);
        TransformItem = other.gameObject.GetComponent<Rigidbody2D>();


            if (TransformItem)
            {
            
            TransformItem.AddForce(fan);
            }

    }
}
