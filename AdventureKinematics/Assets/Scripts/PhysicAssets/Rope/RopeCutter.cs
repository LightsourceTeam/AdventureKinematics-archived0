using UnityEngine;

public class RopeCutter : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Link")
        {
            HingeJoint2D linkJoint = collision.gameObject.GetComponent<HingeJoint2D>();
            if (linkJoint) linkJoint.enabled = false;
        }
    }
}
