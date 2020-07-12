using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    public bool Close = false;
    public GameObject Door;
    public float MaxDoorVert;
    public float DoorStartPosY;
    public float ClouseDoorSpeed;
    public float OpenDoorSpeed;
    void Start()
    {
       
    }

    void Update()
    {
        if (Close == true && Door.transform.position.y > DoorStartPosY)
        {
            Door.transform.Translate(ClouseDoorSpeed * Time.deltaTime, 0, 0);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Close = false;
        if (Door.transform.position.y < MaxDoorVert)
        {
            Door.transform.Translate(-OpenDoorSpeed * Time.deltaTime, 0, 0);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Close = true;
    }
}
