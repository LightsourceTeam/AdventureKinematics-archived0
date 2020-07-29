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
    public float substeppingCoefficient = .5f;

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
        if (constraintFrequency < 2) constraintFrequency = 2;
        if (acceptableUnitError < .0f) acceptableUnitError = .0f;

        if (substeppingCoefficient > 1f) substeppingCoefficient = 1f;
        if (substeppingCoefficient < 0f) substeppingCoefficient = 0f;

        if (velocityDamping > 1f) velocityDamping = 1f;
        if (velocityDamping < 0f) velocityDamping = 0f;


        // SIMULATION
        Simulate();


        //CONSTRAINTS
        for (int i = 0; i < constraintFrequency; i++) ComputeConstraints();

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

        ropeSegments[0].position = StartPoint.transform.position;

        RopeSegment firstSeg = null, secondSeg = null;
        for (int i = 0; i < segmentCount - 1; i++)
        {
            firstSeg = ropeSegments[i];
            secondSeg = ropeSegments[i + 1];

            Vector2 dist = (secondSeg.position - firstSeg.position);

            float Solver = (dist.magnitude - segmentLength);
            Solver *= Mathf.Min(Mathf.Abs(Solver) / acceptableUnitError, substeppingCoefficient);

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

        for (int i = 0; i < segmentCount - 1; i++)
        {
            ropeSegments[i].ApplyContrsintImpulse();
        }
    }

    public class RopeSegment
    {
        public Vector2 position;
        private Vector2 positionOld;


        public Vector2 contraintImpulse;

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
            velocity  = (position - positionOld) * Damping;

            position += velocity;
            positionOld = position;
            position += impulse;
        }

        public void AddContrsintImpulse(Vector2 impulse)
        {
            contraintImpulse += impulse;
        }

        public void ApplyContrsintImpulse()
        {
            position += contraintImpulse;
            contraintImpulse = Vector2.zero;
        }
    }
}