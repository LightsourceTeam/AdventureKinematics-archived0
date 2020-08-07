using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomMonoEventDispatcher : MonoBehaviour
{
    public static event System.Action preUpdate;

    void Update()
    {
        preUpdate();
    }
}
