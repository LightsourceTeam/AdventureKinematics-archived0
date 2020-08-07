using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityController : Controller
{
    [SerializeField] private GameItem ability;

    // Initialising Ability Systems
    public override void Init(MainController controller)
    {
        mainController = controller;
        joystick = mainController.abilityJoystick;

        if(ability != null) ability.mainController = mainController;
    }

    protected override void FixedUpdate() 
    { 
        if (ability != null) 
        { 
            if (lastJState) { if (joystick.State) ability.OnFixedApply(this); }
        } 
    }

    protected override void Update()
    {
        if (ability != null)
        {
            if (lastJState)
            {
                if (!joystick.State) ability.OnEndApply(this);
                else ability.OnApply(this);
            }
        }
    }
}
