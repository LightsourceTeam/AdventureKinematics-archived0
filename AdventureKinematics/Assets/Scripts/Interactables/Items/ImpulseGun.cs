using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpulseGun : GameItem
{
    public List<Collider2D> otherCollider;
    private Collider2D coll;
    private Controller controllerToUse;
    
    public float forse;
    private void Start()
    {
        coll = GetComponent<Collider2D>();
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        otherCollider.Add(collision);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        otherCollider.Remove(collision);
    }

    public override void OnApply(Controller controller)
    {
        controllerToUse = controller;

        Vector2 direction = controllerToUse.joystick.Direction;

        foreach (Collider2D x in otherCollider)
        {
            if (x.GetComponent<Rigidbody2D>())
            {
                x.GetComponent<Rigidbody2D>().AddForce(direction * forse * Time.deltaTime, ForceMode2D.Impulse);
            }
        }
    }
}
