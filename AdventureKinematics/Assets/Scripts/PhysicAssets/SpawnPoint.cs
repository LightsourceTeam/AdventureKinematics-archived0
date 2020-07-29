using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public GameObject SpawnItem;
    public float Cooldown;

    public float leftPosition;
    public float rightPosition;

    private float cooldownCountdown;

    void Start()
    {
        cooldownCountdown = -Cooldown;
    }

    
    void Update()
    {
        if (cooldownCountdown - Time.time <= 0)
        {
            Vector2 position = new Vector2(Random.Range(transform.position.x - leftPosition, transform.position.x + rightPosition), transform.position.y);
            Instantiate(SpawnItem, position, Quaternion.identity);
            cooldownCountdown = Time.time + Cooldown;
            
        }
    }
}
