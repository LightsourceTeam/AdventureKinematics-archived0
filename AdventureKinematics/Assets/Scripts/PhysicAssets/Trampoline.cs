using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
    Rigidbody2D TransformItem;
    public float JumpForse;
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        Vector2 Jump = new Vector2(0, JumpForse);
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
