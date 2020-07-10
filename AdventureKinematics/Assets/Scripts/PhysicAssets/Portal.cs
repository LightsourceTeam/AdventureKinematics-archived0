using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Portal OutPortal;
    
    private Transform TItemTransform;
    public List<int> IncomingItems;
    BoxCollider2D bx;
    void Start()
    {
        bx = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "TeleportItem")
        {

            if (!IncomingItems.Contains(other.gameObject.GetInstanceID()))
            {
                TItemTransform = other.gameObject.transform.root;
                OutPortal.IncomingItems.Add(other.gameObject.GetInstanceID());
                Teleport();
            }

        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "TeleportItem")
        {
            this.IncomingItems.Remove(other.gameObject.GetInstanceID());
        }
    }

    void Teleport()
    {  
        TItemTransform.position = new Vector2(OutPortal.transform.position.x, OutPortal.transform.position.y);
    }
}
