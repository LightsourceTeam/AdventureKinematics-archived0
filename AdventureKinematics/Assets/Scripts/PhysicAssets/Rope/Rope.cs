using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public GameObject StartPoint;
    public GameObject EndPoint;

    public float segmentLength = 0.25f;
    public float lineWidth = 0.1f;
    public int segmentCount = 35;

    [NonSerialized] public Vector2 externalForces = Vector2.zero;
    [NonSerialized] public Vector2 externalImpulses = Vector2.zero;

    private LineRenderer lineRenderer;
    private List<RopeSegment> ropeSegments = new List<RopeSegment>();
   

    // Use this for initialization
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        Vector3 ropeStartPoint = StartPoint.transform.position;

        for (int i = 0; i < segmentCount; i++)
        {
            ropeSegments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint -= segmentLength * transform.up;
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        lineRenderer.positionCount = segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            lineRenderer.SetPosition(i, ropeSegments[i].posNow);
        }
    }

    private void FixedUpdate()
    {
        // SIMULATION
        Simulate();


        //CONSTRAINTS
        for (int i = 0; i < 50; i++) ApplyConstraint();

        externalForces = externalImpulses = Vector2.zero;
    }

    private void Simulate()
    {
        externalForces += new Vector2(0f, -9.81f);

        for (int i = 1; i < segmentCount - 1; i++)
        {
            RopeSegment firstSegment = ropeSegments[i];

            firstSegment.posNow += firstSegment.posNow - firstSegment.posOld;
            firstSegment.posOld = firstSegment.posNow;
            firstSegment.posNow += externalForces * Time.fixedDeltaTime + externalImpulses;
            ropeSegments[i] = firstSegment;
        }

    }

    private void ApplyConstraint()
    {
        //Constraint to First Point 
        RopeSegment firstSegment = ropeSegments[0];
        firstSegment.posNow = StartPoint.transform.position;
        ropeSegments[0] = firstSegment;

        for (int i = 0; i < segmentCount - 1; i++)
        {
            RopeSegment firstSeg = ropeSegments[i];
            RopeSegment secondSeg = ropeSegments[i + 1];

            Vector2 dist = (secondSeg.posNow - firstSeg.posNow);

            float Solver = Mathf.Max(dist.magnitude - segmentLength, 0f);
            // Solver *= Mathf.Max(1f - (.1f / Mathf.Abs(Solver)), 0.9f);

            Vector2 changeAmount = dist.normalized * Solver;
            
            if (i != 0)
            {
                firstSeg.posNow += changeAmount * 0.5f;
                ropeSegments[i] = firstSeg;
                secondSeg.posNow -= changeAmount * 0.5f;
                ropeSegments[i + 1] = secondSeg;
            }
            else
            {
                secondSeg.posNow -= changeAmount;
                ropeSegments[i + 1] = secondSeg;
            }
        }

        //Constraint to Second Point 
        RopeSegment endSegment = ropeSegments[ropeSegments.Count - 1];
        endSegment.posNow = EndPoint.transform.position;
        ropeSegments[segmentCount - 1] = endSegment;
    }

    public struct RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos)
        {
            posNow = pos;
            posOld = pos;
        }
    }
}