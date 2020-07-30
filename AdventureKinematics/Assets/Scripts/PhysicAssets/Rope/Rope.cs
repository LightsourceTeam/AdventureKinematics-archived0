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
    [RuntimeReadOnly] public int segmentCount = 35;

    public int constraintFrequency = 15;
    public float acceptableUnitError = .005f;
    public float velocityDamping = .5f;
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
            ropeSegments.Add(new RopeSegment(ropeStartPoint, velocityDamping));
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

        if (velocityDamping > 1f) velocityDamping = 1f;
        if (velocityDamping < 0f) velocityDamping = 0f;


        // SIMULATION
        Simulate();


        //CONSTRAINTS
        ComputeConstraints();

        externalForces = externalImpulses = Vector2.zero;
    }

    private void Simulate()
    {
        externalForces += new Vector2(0f, -9.81f);

        for (int i = 0; i < segmentCount - 1; i++)
        {
            ropeSegments[i].Damping = velocityDamping;
            ropeSegments[i].UpdateExternalPosition((externalForces * Time.fixedDeltaTime) + externalImpulses);
        }

    }

    private void ComputeConstraints()
    {
        for (int j = 0; j < constraintFrequency; j++)
        {
            ropeSegments[0].position = StartPoint.transform.position;

            RopeSegment firstSeg = null, secondSeg = null;
            for (int i = 0; i < segmentCount - 1; i++)
            {
                firstSeg = ropeSegments[i];
                secondSeg = ropeSegments[i + 1];

                Vector2 dist = (secondSeg.position - firstSeg.position);

                float Solver = Mathf.Max(dist.magnitude - segmentLength, 0f);
                Solver *= Mathf.Max(1f - acceptableUnitError, contraintMinimalPrecision);

                Vector2 changeAmount = dist.normalized * Solver;

                if (i != 0)
                {
                    firstSeg.AddContrsintImpulse(changeAmount * 0.5f);
                    secondSeg.AddContrsintImpulse(-changeAmount * 0.5f);
                    continue;
                }

                secondSeg.AddContrsintImpulse(-changeAmount);
            }

            //Constraint to Second Point 
            secondSeg.position = EndPoint.transform.position;

            //for (int i = 0; i < segmentCount - 1; i++)
            //{

            //    // Debug.Log("iteration: " + j + " index: " + i + " constraint:" + ropeSegments[i].contraintImpulse);
            //    ropeSegments[i].ApplyContrsintImpulse();
            //}
        }
    }

    public class RopeSegment
    {
        public Vector2 position;
        private Vector2 positionOld;


        public Vector2 contraintImpulse;
        public Vector2 generalConstraintImpulse;

        public Vector2 velocity;
        public float Damping;

        public RopeSegment(Vector2 pos, float damping)
        {
            position = positionOld = pos;
            contraintImpulse = Vector2.zero;
            Damping = damping;
        }

        public void UpdateExternalPosition(Vector2 impulse)
        {
            velocity = (position - positionOld) * (1 - Damping);
            velocity += Vector2.ClampMagnitude(Vector2.ClampMagnitude(velocity, 1f) * Mathf.Max(Vector2.Dot(generalConstraintImpulse,  -velocity.normalized), 0f), velocity.magnitude);
            generalConstraintImpulse = Vector2.zero;

            positionOld = position;
            position += velocity;
            position += impulse;
        }

        public void AddContrsintImpulse(Vector2 impulse)
        {
            position += impulse;
            generalConstraintImpulse += impulse;
        }

        public void ApplyContrsintImpulse()
        {
            position += contraintImpulse;
            generalConstraintImpulse += contraintImpulse;
            contraintImpulse = Vector2.zero;
        }
    }
}