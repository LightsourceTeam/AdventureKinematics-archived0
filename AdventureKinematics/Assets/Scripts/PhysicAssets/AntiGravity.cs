using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiGravity : MonoBehaviour
{
    public SwitchButton Button;

    private float gr;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Button.isChecked == true)
        {
            other.GetComponent<Rigidbody2D>().gravityScale *= -1; 
            
        }
        
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Rigidbody2D>().gravityScale < 0)
        {
            other.GetComponent<Rigidbody2D>().gravityScale *= -1;
        }
    }
}
