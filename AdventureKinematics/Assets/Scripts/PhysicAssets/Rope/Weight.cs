using UnityEngine;

public class Weight : MonoBehaviour
{
    public float DistanceFromRopeEnd;

    private DistanceJoint2D joint;

    public void ConnectToRopeEnd(Rigidbody2D rb)
    {
        joint = gameObject.GetComponent<DistanceJoint2D>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = rb;
        joint.distance = DistanceFromRopeEnd;
    }
}
