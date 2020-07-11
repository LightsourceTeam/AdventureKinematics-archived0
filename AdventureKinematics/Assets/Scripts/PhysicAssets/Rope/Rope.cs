using UnityEngine;

public class Rope : MonoBehaviour
{
    public Rigidbody2D Hook;
    public GameObject LinkPrefab;
    public int Links = 7;
    public Rigidbody2D weight;
    public float OffsetDistanceFromWeight;
    public Vector2 WeightAnchor;
    public bool AutoConfigureWeightConnectedAnchor = false;
    public Vector2 WeightConnectedAnchor;
    
    void Start()
    {
       GenerateRope();
    }
    void GenerateRope()
    {
        Rigidbody2D previousRB = Hook;
        for (int i = 0; i < Links; i++)
        {
            GameObject link = Instantiate(LinkPrefab, transform);
            HingeJoint2D joint = link.GetComponent<HingeJoint2D>();
            joint.connectedBody = previousRB;

            previousRB = link.GetComponent<Rigidbody2D>();       
        }

        if (!weight) return;
        DistanceJoint2D lastJoint = weight.gameObject.AddComponent<DistanceJoint2D>();
        if (!lastJoint) { Debug.LogError("Invalid weight object - there is no distance joint on it."); return; }

        lastJoint.autoConfigureDistance = false;
        lastJoint.autoConfigureConnectedAnchor = AutoConfigureWeightConnectedAnchor;
        lastJoint.connectedBody = previousRB;
        lastJoint.distance = OffsetDistanceFromWeight;
        lastJoint.anchor = WeightAnchor;
        lastJoint.connectedAnchor = WeightConnectedAnchor;
    }
}
