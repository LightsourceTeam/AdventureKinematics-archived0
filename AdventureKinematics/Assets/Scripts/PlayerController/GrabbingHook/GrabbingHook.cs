using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbingHook : MonoBehaviour
{
    [SerializeField] private MainController playercontroller;
    [SerializeField] private float ClimbSpeed;
    [SerializeField] private float DownClimbSpeed;
    [SerializeField] private float MaxHookDistance;
    [SerializeField] private HookRaycaster HookRaycaster;
    [SerializeField] private DistanceJoint2D DistanceJoint;
    [SerializeField] private HookRenderer HookRenderer;

    private Camera camera;
    private RaycastHit2D _hit;

    public bool JointEnable => DistanceJoint.enabled;

    private bool IsHooking;
    private void Start()
    {
        camera = Camera.main;
        DistanceJoint.enabled = false;
    }

    public void Climb()
    {
        EditJointDistance(-ClimbSpeed);
    }
    public void Descend()
    {
        EditJointDistance(DownClimbSpeed);
    }
    private void EditJointDistance(float delta)
    {
        DistanceJoint.distance += delta * Time.deltaTime;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CreateHook();
        }
        else if (Input.GetMouseButtonUp(0))
        {   
            Disable();
        }
        if (DistanceJoint.enabled)
        {
            HookRenderer.Render(transform.position, _hit.point);
        }
        if (JointEnable)
        {
            if (Input.GetKey(KeyCode.W))
            {
                Climb();
            }
            else if (Input.GetKey(KeyCode.S))
            {
                Descend();
            }
        }
        if (IsHooking)
        {
            playercontroller.enabled = false;
        }
        else
        {
            playercontroller.enabled = true;
        }
    }
    public void Disable()
    {
        IsHooking = false;
        DistanceJoint.enabled = false;
        HookRenderer.Disable();
    }
    public void CreateHook()
    {
        IsHooking = true;
        Vector2 target = camera.ScreenToWorldPoint(Input.mousePosition);
        _hit = HookRaycaster.Ray(transform.position, target, MaxHookDistance);
        DistanceJoint.distance = Vector2.Distance(transform.position, _hit.point);

        if (_hit.collider != null)
        {
            DistanceJoint.enabled = true;
            DistanceJoint.connectedAnchor = _hit.point;
            HookRenderer.Render(transform.position, _hit.point);
        }
    }
}
