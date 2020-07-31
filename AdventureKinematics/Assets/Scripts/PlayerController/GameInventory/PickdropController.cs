using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickdropController : MonoBehaviour
{
    [NonSerialized] public MainController playerController;
    [NonSerialized] public GameInventory Inventory;

    private List<Collider2D> nearbyItems;
    private GameItem bestChoiceItem;
    private bool lastState = false;
    private Vector2 lastDirection;
    private CircleCollider2D itemCollider;

    // Start is called before the first frame update
    public void Init(MainController controller)
    {
        playerController = controller;
        Inventory = controller.inventory;

        nearbyItems = new List<Collider2D>();
        itemCollider = GetComponent<CircleCollider2D>();
        lastDirection = playerController.pickdropJoystick.Direction;
    }

    // Update is called once per frame
    void Update()
    {
        itemCollider.GetContacts(nearbyItems);

        float bestChoice = 0.5f;
        bestChoiceItem = null;
        foreach(Collider2D obj in nearbyItems)
        {
            float ItemLayDirection = Vector2.Dot((obj.transform.position - transform.position).normalized, lastDirection);
            if (ItemLayDirection > bestChoice)
            {
                bestChoice = ItemLayDirection;
                bestChoiceItem = obj.GetComponent<GameItem>();
            }
        }

        if (playerController.pickdropJoystick.State)
        {
            if(playerController.pickdropJoystick.State != lastState) lastState = true;

            lastDirection = playerController.pickdropJoystick.Direction;
        }
        else if ((playerController.pickdropJoystick.State != lastState) && !playerController.pickdropJoystick.State)
        {
            Inventory.SwitchActiveItem(bestChoiceItem, lastDirection);
            lastState = false;
        }
    }
}
