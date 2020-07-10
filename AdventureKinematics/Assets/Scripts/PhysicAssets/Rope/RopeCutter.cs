using UnityEngine;

public class RopeCutter : MonoBehaviour
{
   
    void Update()
    {
        
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.forward);
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Link")
            {
                Destroy(hit.collider.gameObject);
            }
        }
    }
}
