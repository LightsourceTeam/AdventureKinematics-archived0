using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public PlayerController Player;

    private InventoryCanvasCheck script;

    void Start()
    {
        
    }

    void Update()
    {
        if (Player.onGet)
        {
            foreach(Transform child in transform)
            {
                script = child.GetComponent<InventoryCanvasCheck>();
                if (script.isBusy){
                    continue;
                }
                else
                {
                    GameObject Temp;
                    script.isBusy = true;
                    Temp = child.transform.Find("Image").gameObject;
                    Temp.gameObject.SetActive(true);
                    break;
                }
            }
        }
    }
}
