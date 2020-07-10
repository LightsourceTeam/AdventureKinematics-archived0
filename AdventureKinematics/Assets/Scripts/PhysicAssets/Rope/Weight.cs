using UnityEngine;

public class Weight : MonoBehaviour
{
    public float DistanseFromRopeEnd;
    
    public void ConnectToRopeEnd(Rigidbody2D rb)
    {
        HingeJoint2D joint = gameObject.AddComponent<HingeJoint2D>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = rb;
        joint.anchor = Vector2.zero;
        joint.connectedAnchor = new Vector2(0f, - DistanseFromRopeEnd);
    }
}
