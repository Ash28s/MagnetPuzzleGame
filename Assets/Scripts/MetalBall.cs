using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class MetalBall : MonoBehaviour
{
    [Header("Physics Settings")]
    public float maxVelocity = 8f;        // Prevent ball from moving too fast
    public float stabilityThreshold = 0.1f; // Minimum velocity before stopping
    
    [Header("Collision Detection")]
    public string obstacleTag = "Obstacle";    // Tag for obstacle objects
    
    public ParticleSystem hitEffect;
    public AudioSource hitSound;
    private GameManager gameManager;
    private Rigidbody2D rb;
    private Vector2 startPos;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        gameManager = GameObject.FindObjectOfType<GameManager>();
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
        col.isTrigger = false; // Make sure it's a solid collider for obstacle detection
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
    
    // Collision detection for obstacles
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if ball hit an obstacle
        hitSound.Play();
        Debug.Log(collision.contacts[0].point);
        ParticleSystem particle = Instantiate(hitEffect,collision.contacts[0].point,Quaternion.identity);
        particle.Play();
        if (collision.gameObject.CompareTag(obstacleTag))
        {
            Debug.Log("Ball hit obstacle: " + collision.gameObject.name);
            StartCoroutine(HandleObstacleHit());
        }
    }
    
    // Alternative trigger detection (if obstacles are set as triggers)
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if ball entered an obstacle trigger
        if (other.CompareTag(obstacleTag))
        {
            Debug.Log("Ball entered obstacle trigger: " + other.gameObject.name);
            StartCoroutine(HandleObstacleHit());
        }
    }
    
    IEnumerator HandleObstacleHit()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("HandleObstacleHit called!");
        
        // Stop the ball immediately
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        Debug.Log("Calling GameManager.TriggerGameOver...");
        
        // Call the public TriggerGameOver method
        gameManager.TriggerGameOver("Ball Hit Obstacle!");
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