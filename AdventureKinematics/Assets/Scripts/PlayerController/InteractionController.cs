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

    protected override void FixedUpdate()
    {
        if (lastJState)
        {
            if (bestChoiceItem != null)
            {
                if (joystick.State) bestChoiceItem.OnFixedTarget(mainController);
            }
        }
    }

    protected override void Update()
    {
        if (lastJState)
        {
            if (bestChoiceItem != null)
            {
                if (!joystick.State) { bestChoiceItem.OnEndTarget(mainController); }
                else bestChoiceItem.OnTarget(mainController);
            }
            else
            {
                if (mainController.inventory.selectedSlot.item != null)
                {
                    if (!joystick.State)
                    {
                        mainController.inventory.selectedSlot.item.gameObject.SetActive(true);
                        mainController.inventory.selectedSlot.item.transform.position = mainController.gameObject.transform.position;
                        mainController.inventory.selectedSlot.item.gameObject.GetComponent<Rigidbody2D>().AddForce(lastDirection * 5, ForceMode2D.Impulse);
                        mainController.inventory.selectedSlot.item = null;
                        mainController.inventory.selectedSlot.previewSpriteObject.GetComponent<Image>().sprite = null;
                    }
                }
            }
        }
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (lastJState)
        {
            if (bestChoiceItem != null)
            {
                if (joystick.State) bestChoiceItem.OnLateTarget(mainController);
            }
        }

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
    }
}
