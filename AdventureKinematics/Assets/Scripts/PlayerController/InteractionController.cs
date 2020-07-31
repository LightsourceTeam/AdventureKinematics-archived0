using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionController : Controller
{

    private List<Collider2D> nearbyItems;
    private Interactable bestChoiceItem = null;
    private CircleCollider2D itemCollider;


    // Start is called before the first frame update
    public override void Init(MainController controller)
    {
        mainController = controller;
        joystick = mainController.interactionJoystick;

        nearbyItems = new List<Collider2D>();
        itemCollider = GetComponent<CircleCollider2D>();
        lastDirection = joystick.Direction;
    }

    private void FixedUpdate()
    {
        if (lastJState)
        {
            if (bestChoiceItem != null)
            {
                if (joystick.State) bestChoiceItem.OnFixedInteract(mainController);
            }
        }
    }

    void Update()
    {
        if (lastJState)
        {
            if (bestChoiceItem != null)
            {
                if (!joystick.State) { bestChoiceItem.OnEndInteract(mainController); }
                else bestChoiceItem.OnInteract(mainController);
            }
            else
            {
                if (mainController.inventory.activeSlot.item != null)
                {
                    if (!joystick.State)
                    {
                        mainController.inventory.activeSlot.item.gameObject.SetActive(true);
                        mainController.inventory.activeSlot.item.transform.position = mainController.gameObject.transform.position;
                        mainController.inventory.activeSlot.item.gameObject.GetComponent<Rigidbody2D>().AddForce(lastDirection * 5, ForceMode2D.Impulse);
                        mainController.inventory.activeSlot.item = null;
                        mainController.inventory.activeSlot.previewSpriteObject.GetComponent<Image>().sprite = null;
                    }
                }
            }
        }
    }

    private void LateUpdate()
    {
        bestChoiceItem = null;

        itemCollider.GetContacts(nearbyItems);
        if (nearbyItems.Count != 0)
        {

            float bestChoice = 0.5f;

            foreach (Collider2D obj in nearbyItems)
            {
                float ItemLayDirection = Vector2.Dot((obj.transform.position - transform.position).normalized, lastDirection);
                if (ItemLayDirection > bestChoice)
                {
                    bestChoice = ItemLayDirection;
                    bestChoiceItem = obj.GetComponent<Interactable>();
                }
            }
        }

        lastJState = joystick.State;
        lastDirection = joystick.Direction;
    }
}
