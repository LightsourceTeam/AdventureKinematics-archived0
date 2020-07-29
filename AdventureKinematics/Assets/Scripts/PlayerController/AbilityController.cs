using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityController : MonoBehaviour
{
    public PlayerController playerController;
    public GameInventory inventory;
    
    [SerializeField] private Ability ability;

    private bool lastJState;

    // Initialising Ability Systems
    public void Init(PlayerController controller)
    {
        playerController = controller;

        ability.playerController = playerController;
    }

    public void Update() { if (ability != null) { if (!ability.isFixedUpdate) ability.Apply(); } }

    public void FixedUpdate() { if (ability != null) { if (ability.isFixedUpdate) ability.Apply(); } }

}
