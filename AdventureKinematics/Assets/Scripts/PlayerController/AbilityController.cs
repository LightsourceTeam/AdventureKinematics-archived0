using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityController : Controller
{
    [NonSerialized] public MainController playerController;
    
    [SerializeField] private GameItem ability;

    private bool lastJState;

    // Initialising Ability Systems
    public override void Init(MainController controller)
    {
        playerController = controller;
        joystick = playerController.ultraJoystick;

        if(ability != null) ability.Pick(playerController);
    }

    public void Update()
    {
        if (ability != null)
        {
            if (!ability.isFixedUpdate)
            {
                if (lastJState) ability.Apply(this);
            }
        }

        lastJState = joystick;
    }

    public void FixedUpdate() { if (ability != null) { if (ability.isFixedUpdate) { if (lastJState) ability.Apply(this); } } }

}
