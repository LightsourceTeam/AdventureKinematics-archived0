using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbingHook : GameItem
{
    
    [SerializeField] private float hookLength =  20f;
    [SerializeField] private float hookForce =  50f;
    [SerializeField] private LayerMask mask;

    [SerializeField] private Rope rope;

    private bool bIsHooked = false;

    private RaycastHit2D lastHit;
    private Controller controllerToUse;

    public override void OnEndApply(Controller controller)
    {
        if(!bIsHooked) 
        {
            lastHit = Physics2D.Raycast(controller.mainController.itemController.itemHandler.transform.position, controller.lastDirection.normalized, hookLength, mask);

            if(lastHit) 
            {
                controllerToUse = controller;
                
                rope.StartPoint.transform.parent = controllerToUse.mainController.itemController.itemHandler.transform;
                rope.EndPoint.transform.transform.position = lastHit.point;
                rope.StartPoint.transform.localPosition = Vector2.zero;

                bIsHooked = true;
                
                rope.gameObject.SetActive(true);
            }
           // else  lastHookPosition = (Vector2)transform.position + (controller.joystick.Direction.normalized * hookLength);
        }
        else
        {
            rope.gameObject.SetActive(false);
            
            rope.StartPoint.transform.parent = rope.transform;
            controllerToUse = null;
            bIsHooked = false;
        }
    }

    public void FixedUpdate()
    {
        if(bIsHooked)
        {
            Vector2 forceDirection = lastHit.point - (Vector2)controllerToUse.mainController.itemController.itemHandler.transform.position;
            controllerToUse.mainController.rigBody.AddForce(forceDirection.normalized * hookForce, ForceMode2D.Force);
        }
    }
    
}
