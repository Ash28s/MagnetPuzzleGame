using UnityEngine;

public class Magnet : MonoBehaviour
{
    [Header("Magnet Properties")]
    public bool isAttract = true;
    public float strength = 25f;           // Reduced from 40f for smoother feel
    public float range = 5f;
    
    [Header("Physics Tuning")]
    public float minDistance = 0.8f;       // Prevents getting too close
    public float maxForce = 15f;           // Caps maximum force applied
    public float smoothingFactor = 0.7f;   // Smoother force application
    public float pushBackForce = 8f;       // Force to push ball out if too close
    
    [Header("Visual Feedback")]
    public bool showRange = true;
    public float pulseSpeed = 2f;
    
    private SpriteRenderer sr;
    private MetalBall ball;
    private CircleCollider2D rangeCollider;
    private Vector2 lastForce = Vector2.zero;  // For smoothing
    private float baseBrightness = 1f;
    
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        
        if (sr.sprite == null)
            sr.sprite = SpriteCreator.CreateSquareSprite(isAttract ? Color.blue : Color.red);
        
        transform.localScale = Vector3.one * 0.8f;
        
        // Setup range visualization
        rangeCollider = GetComponent<CircleCollider2D>();
        if (rangeCollider == null) rangeCollider = gameObject.AddComponent<CircleCollider2D>();
        rangeCollider.isTrigger = true;
        rangeCollider.radius = range;
        
        ball = FindObjectOfType<MetalBall>();
        UpdateVisuals();
    }
    
    void Start()
    {
        // Ensure ball has proper physics settings for smooth movement
        if (ball != null)
        {
            var rb = ball.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.drag = 1.5f;           // Add some drag for smoothness
                rb.angularDrag = 3f;      // Reduce spinning
                rb.gravityScale = 0f;     // No gravity for pure magnet physics
            }
        }
    }
    
    // Use FixedUpdate for consistent physics
    void FixedUpdate()
    {
        if (ball == null) return;
        
        Vector2 force = CalculateMagneticForce();
        ApplySmoothForce(force);
        UpdateVisualFeedback();
    }
    
    Vector2 CalculateMagneticForce()
{
    Vector2 direction = (Vector2)transform.position - (Vector2)ball.transform.position; // FLIPPED!
    float distance = direction.magnitude;
    
    // Outside range - no force
    if (distance > range) return Vector2.zero;
    
    // Too close - push away regardless of magnet type
    if (distance < minDistance)
    {
        Vector2 pushForce = -direction.normalized * pushBackForce; // Push away from magnet
        return pushForce;
    }
    
    // Calculate magnetic force with smoother falloff
    float normalizedDistance = distance / range;
    
    // Use smoother force curve (less aggressive than inverse square)
    float forceMagnitude = strength * (1f - normalizedDistance * normalizedDistance);
    
    // Clamp maximum force
    forceMagnitude = Mathf.Min(forceMagnitude, maxForce);
    
    Vector2 force = direction.normalized * forceMagnitude;
    
    // Apply attraction/repulsion
    if (!isAttract) force = -force;
    
    return force;
}
    
    void ApplySmoothForce(Vector2 targetForce)
    {
        // Smooth force transition to prevent jitter
        lastForce = Vector2.Lerp(lastForce, targetForce, smoothingFactor);
        
        // Apply the smoothed force
        ball.ApplyForce(lastForce);
    }
    
    void UpdateVisualFeedback()
    {
        if (!showRange || sr == null) return;
        
        // Pulse effect when ball is in range
        Vector2 direction = (Vector2)ball.transform.position - (Vector2)transform.position;
        float distance = direction.magnitude;
        
        if (distance <= range)
        {
            float pulseIntensity = 1f - (distance / range);
            float pulse = 1f + (Mathf.Sin(Time.time * pulseSpeed) * 0.2f * pulseIntensity);
            
            Color currentColor = isAttract ? Color.blue : Color.red;
            currentColor *= pulse;
            sr.color = currentColor;
        }
        else
        {
            // Default color when ball is out of range
            sr.color = isAttract ? Color.blue : Color.red;
        }
    }
    
    public void Toggle()
    {
        isAttract = !isAttract;
        UpdateVisuals();
        
        // Reset smoothing when polarity changes
        lastForce = Vector2.zero;
    }
    
    void UpdateVisuals()
    {
        if (sr != null)
        {
            sr.color = isAttract ? Color.blue : Color.red;
        }
    }
    
    // Visual debugging in Scene view
    void OnDrawGizmosSelected()
    {
        // Draw range circle
        Gizmos.color = isAttract ? Color.blue * 0.3f : Color.red * 0.3f;
        Gizmos.DrawWireSphere(transform.position, range);
        
        // Draw minimum distance circle
        Gizmos.color = Color.yellow * 0.5f;
        Gizmos.DrawWireSphere(transform.position, minDistance);
        
        // Draw force vector if in play mode
        if (Application.isPlaying && ball != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position, lastForce.normalized * 2f);
        }
    }
}