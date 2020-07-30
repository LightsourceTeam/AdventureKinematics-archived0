using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weight : MonoBehaviour
{
    public SwitchButton Button;
    Rigidbody2D rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
    }
    private void Update()
    {
        if (Button.isChecked == true)
        {
            rb.isKinematic = false;
        }
    }
}
