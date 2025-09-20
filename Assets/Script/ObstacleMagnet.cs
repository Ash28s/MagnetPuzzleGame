using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMagnet : MonoBehaviour
{  
    public float pullStrength = 10f;   // How strong the magnet pulls
    public float radius = 5f;          // Attraction radius

    void FixedUpdate()
    {
        // Find all obstacles within radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D col in colliders)
        {
           
            if (col != null && col.tag=="Obstacle")
            {
                Rigidbody2D rb = col.attachedRigidbody;
                if (rb != null)
                {
                    Vector3 direction = (transform.position - col.transform.position).normalized;
                    rb.AddForce(direction * pullStrength);
                }
            }
        }
    }
}
