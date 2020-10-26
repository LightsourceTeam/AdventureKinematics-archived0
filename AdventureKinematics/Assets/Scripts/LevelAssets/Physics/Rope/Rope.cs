using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using SourceExtensions;

public class Rope : MonoBehaviour
{
    public GameObject StartPoint;
    public GameObject EndPoint;

    public float segmentLength = 0.25f;
    public float lineWidth = 0.1f;
    [RuntimeReadOnly] public int segmentCount = 35;

    public int constraintFrequency = 15;
    public float acceptableUnitError = .005f;
    public float contraintMinimalPrecision = .5f;

    [NonSerialized] public Vector2 externalForces = Vector2.zero;
    [NonSerialized] public Vector2 externalImpulses = Vector2.zero;

    private LineRenderer lineRenderer;
    private List<RopeSegment> ropeSegments = new List<RopeSegment>();
   

    // Use this for initialization
    void Start()
    {
        if (segmentCount < 3) segmentCount = 3;

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
            lineRenderer.SetPosition(i, ropeSegments[i].position);
        }
    }

    private void FixedUpdate()
    {
        if (segmentLength < 0f) segmentLength = 0f;
        if (constraintFrequency < 1) constraintFrequency = 1;
        if (acceptableUnitError < .0f) acceptableUnitError = .0f;

        if (contraintMinimalPrecision > 1f) contraintMinimalPrecision = 1f;
        if (contraintMinimalPrecision < 0f) contraintMinimalPrecision = 0f;


        // SIMULATION
        Simulate();


        //CONSTRAINTS
        ComputeConstraints();

        externalForces = externalImpulses = Vector2.zero;
    }

    private void Simulate()
    {
        externalForces += new Vector2(0f, -0.981f);

        for (int i = 0; i < segmentCount - 1; i++)
        {
            ropeSegments[i].UpdateExternalPosition((externalForces * Time.fixedDeltaTime) + externalImpulses);
        }

    }

    private void ComputeConstraints()
    {
        for (int j = 0; j < constraintFrequency; j++)
        {
            ropeSegments[0].position = StartPoint.transform.position;

            RopeSegment prevSeg = null, currSeg = null;
            for (int i = 1; i < segmentCount; i++)
            {
                prevSeg = ropeSegments[i - 1];
                currSeg = ropeSegments[i];

                Vector2 dist = (prevSeg.position - currSeg.position);

                float Solver = dist.magnitude - segmentLength;
                Solver *= Mathf.Max(1f - Mathf.Abs(acceptableUnitError), contraintMinimalPrecision);

                Vector2 changeAmount = dist.normalized * Solver;

                if (i != (segmentCount - 1))
                {
                    if (i == 1)
                    {
                        currSeg.AddContrsintImpulse(changeAmount);
                        continue;
                    }


                    prevSeg.AddContrsintImpulse(-changeAmount * 0.5f);
                    currSeg.AddContrsintImpulse(changeAmount * 0.5f);
                    continue;
                }
                else
                {
                    prevSeg.AddContrsintImpulse(-changeAmount);
                    continue;
                }

            }

            //Constraint to Second Point 
            currSeg.position = EndPoint.transform.position;

        }
    }

    public class RopeSegment
    {
        public Vector2 position;
        private Vector2 positionOld;


        public Vector2 contraintImpulse;

        public Vector2 velocity;

        public RopeSegment(Vector2 pos)
        {
            position = positionOld = pos;
            velocity = Vector2.zero;
        }

        public void UpdateExternalPosition(Vector2 impulse)
        {
            velocity = (position - positionOld);

            positionOld = position;
            position += velocity;
            position += impulse;
        }

        public void AddContrsintImpulse(Vector2 impulse)
        {
            position += impulse;
        }
    }
}