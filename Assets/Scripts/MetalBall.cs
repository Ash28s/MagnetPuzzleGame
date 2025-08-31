using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class MetalBall : MonoBehaviour
{
    [Header("Physics Settings")]
    public float maxVelocity = 8f;        // Prevent ball from moving too fast
    public float stabilityThreshold = 0.1f; // Minimum velocity before stopping
    
    private Rigidbody2D rb;
    private Vector2 startPos;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Optimized physics settings for smooth magnet interaction
        rb.gravityScale = 0f;      // Pure magnet physics
        rb.mass = 1f;
        rb.drag = 1.5f;            // Smooth deceleration
        rb.angularDrag = 3f;       // Reduce spinning
        
        startPos = transform.position;
        
        // Setup visual
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteCreator.CreateSquareSprite(new Color(0.9f, 0.9f, 0.95f));
            transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        }
        
        // Setup collider
        var col = GetComponent<CircleCollider2D>();
        col.radius = 0.25f;
    }
    
    void FixedUpdate()
    {
        // Clamp velocity to prevent crazy speeds
        if (rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity = rb.velocity.normalized * maxVelocity;
        }
        
        // Stop micro-movements for stability
        if (rb.velocity.magnitude < stabilityThreshold && rb.velocity.magnitude > 0)
        {
            rb.velocity *= 0.9f; // Gradual slowdown
        }
    }
    
    public void ApplyForce(Vector2 force)
    {
        // Apply force more smoothly
        rb.AddForce(force, ForceMode2D.Force);
    }
    
    public void ResetPosition()
    {
        transform.position = startPos;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
    
    // Visual feedback for debugging
    void OnDrawGizmos()
    {
        if (rb != null)
        {
            // Draw velocity vector
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, rb.velocity);
        }
    }
}