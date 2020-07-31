using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
    Rigidbody2D TransformItem;
    public float JumpForce;

    private void OnCollisionEnter2D(Collision2D other)
    {
        Vector2 Jump = new Vector2(0, JumpForce);
        TransformItem = other.gameObject.GetComponent<Rigidbody2D>();

        if (TransformItem)
        {
            TransformItem.AddForce(Jump,ForceMode2D.Impulse);
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        
    }
}
