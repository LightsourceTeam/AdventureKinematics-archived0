using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbingHook : GameItem
{
    
    [SerializeField] private float hookLength =  20f;
    [SerializeField] private float hookForce =  50f;
    [SerializeField] private float minimalTargetDistance = 1f;
    
    public LayerMask mask;
    public Rope rope;

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
                
                rope.gameObject.SetActive(true);
                rope.EndPoint.transform.parent = controllerToUse.mainController.itemController.itemHandler.transform;
                rope.StartPoint.transform.position = lastHit.point;
                rope.EndPoint.transform.localPosition = Vector2.zero;

                bIsHooked = true;
            }
        }
        else
        {
            rope.EndPoint.transform.parent = rope.transform;
            rope.gameObject.SetActive(false);
            controllerToUse = null;

            bIsHooked = false;
        }
    }

    public void FixedUpdate()
    {
        if(bIsHooked)
        {
            Vector2 forceDirection = lastHit.point - (Vector2)controllerToUse.mainController.itemController.itemHandler.transform.position;

            controllerToUse.mainController.rigBody.velocity += .75f * Vector2.ClampMagnitude(forceDirection, 1f) * Mathf.Max(0f, Vector2.Dot(controllerToUse.mainController.rigBody.velocity, -forceDirection.normalized));
            controllerToUse.mainController.rigBody.AddForce(forceDirection.normalized * hookForce * Mathf.Clamp(forceDirection.magnitude - minimalTargetDistance, 0f, 1f), ForceMode2D.Force); ;
        }
    }
    
}
