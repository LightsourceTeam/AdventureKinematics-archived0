using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookRaycaster : MonoBehaviour
{
    [SerializeField] private LayerMask mask;

    public RaycastHit2D Ray(Vector2 orgin, Vector2 target, float distance)
    {
        return Physics2D.Raycast(orgin, target - orgin, distance, mask);
    }
}
