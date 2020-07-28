using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DecorativeRope : MonoBehaviour
{
    public GameObject StartPoint;
    public GameObject EndPoint;

    public float segmentLength = 0.25f;
    public float lineWidth = 0.1f;
    public int segmentCount = 35;
    public int constraintPrecision = 15;
    public float constraintStiffness = 50f;

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
        for (int i = 0; i < constraintPrecision; i++) ApplyConstraint();

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
        ropeSegments[0].posNow = StartPoint.transform.position;

        for (int i = 0; i < segmentCount - 1; i++)
        {
            RopeSegment firstSeg = ropeSegments[i];
            RopeSegment secondSeg = ropeSegments[i + 1];

            Vector2 dist = (secondSeg.posNow - firstSeg.posNow);

            float Solver = (dist.magnitude - segmentLength) * Time.deltaTime * constraintStiffness;

            Vector2 changeAmount = dist.normalized * Solver;
            
            if (i != 0)
            {
                firstSeg.posNow += changeAmount * 0.5f;
                secondSeg.posNow -= changeAmount * 0.5f;
            }
            else
            {
                secondSeg.posNow -= changeAmount;
            }
        }

        //Constraint to Second Point 
        ropeSegments[ropeSegments.Count - 1].posNow = EndPoint.transform.position;
    }

    public class RopeSegment
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