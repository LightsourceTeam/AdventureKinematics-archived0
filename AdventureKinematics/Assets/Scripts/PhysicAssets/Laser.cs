using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public float MaxRange;
    public Switch laserSwitch;
    public LayerMask layerMask;

    private LineRenderer Line;

    void Start()
    {
        Line = GetComponent<LineRenderer>();
        Line.enabled = true;

        Line.useWorldSpace = true;
    }

    void Update()
    {
        if (laserSwitch.isChecked)
        {
            Line.enabled = true;
            Fire();
            return;
        }

        Line.enabled = false;
    }

    private void Fire()
    {

        Vector2 LastRayBeginLocation = transform.position;
        Vector2 LastRayDirection = transform.right;
        float RemainingRange = MaxRange;
        
        Line.positionCount = 1;

        RaycastHit2D hit = Physics2D.Raycast(LastRayBeginLocation, LastRayDirection, RemainingRange, layerMask);

        if(hit)
        {
            // if laser hit object that is not a mirror, then just draw ray and quit
            if (hit.transform.tag != "Mirror")
            {
                Line.positionCount += 1;
                Line.SetPosition(0, LastRayBeginLocation);
                Line.SetPosition(1, hit.point);
                return;
            }
            // but if object hit by raycast is a mirror, then do reflection stuff;
            else
            {
                // initialize start point
                Line.SetPosition(0, LastRayBeginLocation);

                while (hit)
                {

                    // ----    draw current ray, if it hit something

                    Line.positionCount += 1;
                    Line.SetPosition(Line.positionCount - 1, hit.point);

                    // if object that we hit is not mirror, there is no sense to reflect further, and so, we quit function
                    if (hit.transform.tag != "Mirror") return; 

                    
                    // ----    reflect ray, and throw raycast for it    ----

                    // calculate remaining range on which we will throw raycast.
                    RemainingRange -= Vector2.Distance(hit.point, LastRayBeginLocation);

                    // if length is negative, there is no sense to draw further, and so, we quit function
                    if (RemainingRange < 0f) return;

                    // now, the new begin location is in hitpoint,a nd the new direction is a an old vector reflected by normal
                    LastRayBeginLocation = hit.point;
                    LastRayDirection = Vector2.Reflect(LastRayDirection, hit.normal);

                    // throw a raycast
                    hit = Physics2D.Raycast(LastRayBeginLocation + LastRayDirection * 0.05f, LastRayDirection, RemainingRange, layerMask);
                }

                // if last reflected ray didn't hit anything, draw it using all the remaining range
                Line.positionCount += 1;
                Line.SetPosition(Line.positionCount - 1, LastRayBeginLocation + LastRayDirection * RemainingRange);
            }
        }
        else
        {
            // still, if there were no hits detected at all, 
            // ray is drawn straight using all the remaining range (which is in fact max range)
            Line.positionCount += 1;
            Line.SetPosition(0, LastRayBeginLocation);
            Line.SetPosition(1, LastRayBeginLocation + LastRayDirection * RemainingRange);
        }
    }
}



