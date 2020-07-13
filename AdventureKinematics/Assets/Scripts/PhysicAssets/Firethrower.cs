using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firethrower : MonoBehaviour
{
    public float LineLength;
    public LayerMask layerMask;
    public bool isOn = false;

    private ParticleSystem Particles;
    private bool particlesIsRunning;

    void Start()
    {
        Particles = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (isOn)
        {
                if (!particlesIsRunning)
                {
                    particlesIsRunning = true;
                    Particles.Play(true);
                }
        }
        else
        {
            Particles.Stop(true);
            particlesIsRunning = false;
        }
    }
}