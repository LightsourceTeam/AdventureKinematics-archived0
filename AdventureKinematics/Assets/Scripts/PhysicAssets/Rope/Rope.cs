using UnityEngine;

public class Rope : MonoBehaviour
{
    public Rigidbody2D Hook;
    public GameObject LinkPrefab;
    public int Links = 7;
    public Weight weight;
    
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
            if (i < Links - 1)
            {
                previousRB = link.GetComponent<Rigidbody2D>();
            }
            else
            {
                if(weight) weight.ConnectToRopeEnd(link.GetComponent<Rigidbody2D>());
            }
                
        }
    }
}
