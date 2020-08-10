using System;            
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionController : Controller
{
    // nearbyItems
    [NonSerialized] public List<Collider2D> nearbyItems;
    [NonSerialized] public List<Collider2D> lastNearbyItems;

    private Interactable choiceItem = null;
    private CircleCollider2D itemCollider;

    // Start is called before the first frame update
    public override void Init(MainController controller)
    {
        mainController = controller;
        joystick = mainController.interactionJoystick;

        // initialisng arrays
        nearbyItems = new List<Collider2D>();
        lastNearbyItems = new List<Collider2D>();
        prevNearbyItems = new List<Collider2D>();

        itemCollider = GetComponent<CircleCollider2D>();
        
        lastDirection = joystick.Direction;
        lastJState = joystick.State;
    }

    protected override void EarlyUpdate()
    {
        base.EarlyUpdate();

        lastNearbyItems.Clear();
        foreach (Collider2D collider in prevNearbyItems) lastNearbyItems.Add(collider);

        choiceItem = null;
        nearbyItems.Clear();
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
                    choiceItem = obj.GetComponent<Interactable>();
                }
            }
        }
    }


    protected override void FixedUpdate()
    {
        foreach (Collider2D collider in nearbyItems) collider.GetComponent<Interactable>().OnFixedInteract(mainController);
        if (lastJState)
        {
            if (choiceItem != null)
            {
                if (joystick.State) choiceItem.OnFixedTarget(mainController);
            }
        }
    }

    protected override void Update()
    {
        foreach (Collider2D collider in nearbyItems) collider.GetComponent<Interactable>().OnInteract(mainController);

        foreach(Collider2D colliderOld in lastNearbyItems)
        { 
            if(!nearbyItems.Contains(colliderOld))
            {
                colliderOld.GetComponent<Interactable>().OnEndInteract(mainController);
            }
        }

        if (lastJState)
        {
            if (choiceItem != null)
            {
                if (!joystick.State) { choiceItem.OnEndTarget(mainController); }
                else choiceItem.OnTarget(mainController);
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

        foreach (Collider2D collider in nearbyItems) collider.GetComponent<Interactable>().OnLateInteract(mainController);

        if (lastJState)
        {
            if (choiceItem != null)
            {
                if (joystick.State) choiceItem.OnLateTarget(mainController);
            }
        }

        prevNearbyItems.Clear();
        foreach (Collider2D collider in nearbyItems) prevNearbyItems.Add(collider);
    }

    // operational variables

    private List<Collider2D> prevNearbyItems;
}
