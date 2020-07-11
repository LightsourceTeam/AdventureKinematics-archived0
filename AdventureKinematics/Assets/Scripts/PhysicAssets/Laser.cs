using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public float LineLength = 5f;
    public bool isOn = false;
    public LayerMask layerMask;

    private RaycastHit2D hit;
    private LineRenderer Line;

    void Start()
    {
        Line = GetComponent<LineRenderer>();
        Line.SetVertexCount(2);
        Line.enabled = false;

        Line.useWorldSpace = true;
    }

    void Update()
    {
        if (isOn)
        {
            Fire();
            return;
        }

        Line.enabled = false;
    }

    private void Fire()
    {
        
        hit = Physics2D.Raycast(transform.position, transform.right, LineLength, layerMask);
        
        Line.enabled = true;
        if (hit)
        {
            Line.SetPosition(0, transform.position);
            Line.SetPosition(1, hit.point);
            return;
        }

        Line.SetPosition(0, transform.position);
        Line.SetPosition(1, ((Vector2)transform.right * LineLength) + (Vector2)transform.position);
    }
}



