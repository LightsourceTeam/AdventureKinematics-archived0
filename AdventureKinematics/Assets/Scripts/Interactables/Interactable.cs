using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public virtual void OnFixedInteract(MainController controller) { }

    public virtual void OnInteract(MainController controller) { }

    public virtual void OnEndInteract(MainController controller) { }
}
