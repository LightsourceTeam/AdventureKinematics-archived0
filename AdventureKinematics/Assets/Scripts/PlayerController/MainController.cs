using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainController : MonoBehaviour
{
    public MovementController movementController;
    public AbilityController abilityController;
    public ItemController itemController;
    public GameInventory inventory;
    public InteractionController interactionController;
    public CraftSystem craftSystem;

    [NonSerialized] public Rigidbody2D rigBody;
    [NonSerialized] public CapsuleCollider2D playerCollider;

    // controller values

    public Joystick movementJoystick;
    public Joystick itemJoystick;
    public Joystick interactionJoystick;
    public Joystick abilityJoystick;

    void Start()
    {
        rigBody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();

        movementController.Init(this);
        inventory.Init(this);
        itemController.Init(this);
        interactionController.Init(this);
        abilityController.Init(this);
        // craftSystem.Init(this);          // TODO craft system
    }

}
