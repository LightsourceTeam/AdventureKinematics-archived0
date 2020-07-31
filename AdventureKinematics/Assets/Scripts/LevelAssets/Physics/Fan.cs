using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fan : MonoBehaviour
{
    public float FanForce;
    public Switch FanSwitch;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (FanSwitch.isChecked)
        {
            Vector2 fan = new Vector2(FanForce, 0);
            Rigidbody2D TransformItem = other.gameObject.GetComponent<Rigidbody2D>();

            if (TransformItem)
            {
                TransformItem.AddForce(fan);
            }
        }
    }
}
