using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookRenderer : MonoBehaviour
{
    [SerializeField] private LineRenderer LineRenderer;

    public void Render(Vector2 startpoint, Vector2 endpoint)
    {
        LineRenderer.enabled = true;
        LineRenderer.SetPosition(0, startpoint);
        LineRenderer.SetPosition(1, endpoint);
    }
    public void Disable()
    {
        LineRenderer.enabled = false;
    }
}
