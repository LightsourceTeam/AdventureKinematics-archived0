using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public float MaxRange;
    public Switch laserSwitch;
    public LayerMask layerMask;
    public int reflectionsLimit = 30;

    private LineRenderer Line;

    void Start()
    {
        Line = GetComponent<LineRenderer>();
        Line.enabled = true;

        Line.useWorldSpace = true;      // till the start of the game, it is set to false for transform convenience purposes
    }

    void Update()
    {
        // if laser is on
        if (laserSwitch.isChecked)
        {
            Line.enabled = true;
            Cast();
            return;
        }

        // else don't use it.
        Line.enabled = false;
    }

    private void Cast()
    {

        Vector2 LastRayBeginLocation = transform.position;      // start point of the last ray being rendered
        Vector2 LastRayDirection = transform.right;             // direction in which the last ray was casted from its begin location
        float RemainingRange = MaxRange;                        // ray range that reamins after each ray part being rendered
        
        // draw starting point
        Line.positionCount = 1;
        Line.SetPosition(0, LastRayBeginLocation);

        RaycastHit2D hit = Physics2D.Raycast(LastRayBeginLocation, LastRayDirection, RemainingRange, layerMask);

        if(hit)
        {
            do
            {

                // ----    draw current ray, if it hit something

                Line.positionCount += 1;
                Line.SetPosition(Line.positionCount - 1, hit.point);
                
                // if reflections count exceeds maximal acceptable number of reflections, quit reflecting
                if ((Line.positionCount - 1) > reflectionsLimit) return;

                // if object that was hit is not a mirror, there is no sense to reflect further, so - quit function
                if (hit.transform.tag != "Mirror") return;


                // ----    reflect ray, and throw raycast for it    ----

                // calculate remaining range on which we will throw raycast.
                RemainingRange -= Vector2.Distance(hit.point, LastRayBeginLocation);

                // if length is negative, there is no sense to draw further, so - quit function
                if (RemainingRange < 0f) return;

                // now, the new begin location is in hitpoint, and the new direction is a an old vector reflected by normal
                LastRayBeginLocation = hit.point;
                LastRayDirection = Vector2.Reflect(LastRayDirection, hit.normal);

                // throw a raycast
                hit = Physics2D.Raycast(LastRayBeginLocation + LastRayDirection * 0.05f, LastRayDirection, RemainingRange, layerMask);
            } while (hit);

            // if code execution is at this stage - it means that the last reflected ray didn't hit anything,
            // that's why, all the remaining distance is used to draw this ray
            Line.positionCount += 1;
            Line.SetPosition(Line.positionCount - 1, LastRayBeginLocation + LastRayDirection * RemainingRange);
        }
        else
        {
            // still, if ray didn't hit anything on its path, all the remaining 
            // distance (which is in fact max distance) is used to draw it.
            Line.positionCount += 1;
            Line.SetPosition(1, LastRayBeginLocation + LastRayDirection * RemainingRange);
        }
            

    }
}



