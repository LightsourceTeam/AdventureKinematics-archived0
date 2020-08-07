using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    // interact functions

    public virtual void OnFixedInteract(MainController controller) { }

    public virtual void OnInteract(MainController controller) { }

    public virtual void OnEndInteract(MainController controller) { }

    public virtual void OnLateInteract(MainController controller) { }
    
    // target functions

    public virtual void OnFixedTarget(MainController controller) { }

    public virtual void OnTarget(MainController controller) { }

    public virtual void OnEndTarget(MainController controller) { }

    public virtual void OnLateTarget(MainController controller) { }
}
