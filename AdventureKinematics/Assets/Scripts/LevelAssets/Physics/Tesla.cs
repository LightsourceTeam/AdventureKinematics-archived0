using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tesla : MonoBehaviour
{
    public GameObject Main;
    public GameObject Wait;
    public Transform LinePosition;
    public Switch TeslaSwitch;

    private bool isParticles;
    private bool isAudio;
    private ParticleSystem Particles;
    private AudioSource Audio;
    private LineRenderer Line;
    void Start()
    {
        Particles = GetComponent<ParticleSystem>();
        Audio = GetComponent<AudioSource>();
        Line = GetComponent<LineRenderer>();
        Particles.Stop(true);
        isParticles = false;
        isAudio = false;
    }

    void Update()
    {
        if (TeslaSwitch.isChecked)
        {
            Line.SetPosition(0, LinePosition.transform.position);
            Line.SetPosition(1, Wait.gameObject.transform.GetChild(0).transform.position);
            Line.enabled = true;
            if (!isParticles)
            {
                Particles.Play(true);
                isParticles = true;
            }
            if (!isAudio)
            {
                Audio.Play();
                isAudio = true;
            }
        }
        else
        {
            Line.enabled = false;
            Particles.Stop(true);
            isParticles = false;
            Audio.Stop();
            isAudio = false;
        }
    }
}
