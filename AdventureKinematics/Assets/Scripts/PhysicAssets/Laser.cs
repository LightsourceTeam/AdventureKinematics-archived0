using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public float LineLength;
    public Switch laserSwitch;
    public LayerMask layerMask;
    public int reflectionCount;

    private Ray2D ray;
    private RaycastHit2D hit;
    private Vector2 currentReflectPosition;
    private Vector2 direction;
    private LineRenderer Line;

    void Start()
    {
        Line = GetComponent<LineRenderer>();
        Line.enabled = false;

        Line.useWorldSpace = true;
    }

    void Update()
    {
        if (laserSwitch.isChecked)
        {
            Fire();
            return;
        }

        Line.enabled = false;
    }

    private void Fire()
    {
        Line.positionCount = 1;
        direction = transform.right;
        currentReflectPosition = transform.position;
        hit = Physics2D.Raycast(currentReflectPosition, direction, LineLength, layerMask);
        Line.enabled = true;
        float remainingDistance = LineLength;
        int currentReflectionCount = 0;

        do
        {
            if (currentReflectionCount > reflectionCount) break;
            if (hit)
            {
                Line.positionCount += 1;
                if (hit.transform.gameObject.tag != "Mirror") break;

                Line.SetPosition(Line.positionCount - 2, currentReflectPosition);
                Line.SetPosition(Line.positionCount - 1, currentReflectPosition + direction * remainingDistance);

                direction = Quaternion.AngleAxis(180, hit.normal) * -direction;

                remainingDistance -= Vector2.Distance(currentReflectPosition, hit.point);
                if (remainingDistance < 0) break;

                currentReflectPosition = hit.point;
                currentReflectionCount++;

                hit = Physics2D.Raycast(currentReflectPosition, direction, remainingDistance, layerMask);
            }
            else
            {
                Line.SetPosition(0, transform.position);
                Line.SetPosition(1, transform.position + transform.right * LineLength);
            }
        } while (hit);
    }
}



