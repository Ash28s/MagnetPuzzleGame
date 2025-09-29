using UnityEngine;

public class Magnet : MonoBehaviour
{
    [Header("Magnet Properties")]
    public bool isAttract = true;
    public float strength = 25f;
    public float range = 5f;
    public bool isTrapMagnet = false;

    [Header("Physics Tuning")]
    public float magnetRadius = 0.4f;      // Physical size of magnet
    public float maxForce = 15f;
    public float smoothingFactor = 0.7f;
    public float deadZoneRadius = 0.1f;    // NEW: Complete stop zone around magnet
    
    [Header("Stop Mechanics")]
    public float stopSpeedThreshold = 0.3f;    // Speed below which ball can "stick"
    public float forceReductionZone = 0.8f;    // Distance from surface where force starts reducing
    
    [Header("Visual Feedback")]
    public bool showRange = true;
    public float pulseSpeed = 2f;
    
    private SpriteRenderer sr;
    private MetalBall ball;
    private CircleCollider2D rangeCollider;
    private CircleCollider2D physicsCollider;
    private Vector2 lastForce = Vector2.zero;
    private Rigidbody2D ballRb;
    private bool ballIsStuck = false;      // NEW: Track if ball is "stuck" to magnet
    private float ballRadius = 0.25f;
    
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        
        if (sr.sprite == null)
            sr.sprite = SpriteCreator.CreateSquareSprite(isAttract ? Color.blue : Color.red);
        
        transform.localScale = Vector3.one * 0.8f;
        
        // Setup range visualization (trigger)
        rangeCollider = GetComponent<CircleCollider2D>();
        if (rangeCollider == null) rangeCollider = gameObject.AddComponent<CircleCollider2D>();
        rangeCollider.isTrigger = true;
        rangeCollider.radius = range;
        
        // Setup physical collision boundary (solid)
        physicsCollider = gameObject.AddComponent<CircleCollider2D>();
        physicsCollider.isTrigger = false;
        physicsCollider.radius = magnetRadius;
        
        // NO bounce material - completely stop motion
        PhysicsMaterial2D noBounceMaterial = new PhysicsMaterial2D("NoBounceMagnet");
        noBounceMaterial.friction = 1f;        // High friction to stop sliding
        noBounceMaterial.bounciness = 0f;      // No bounce at all
        physicsCollider.sharedMaterial = noBounceMaterial;
        
        ball = FindObjectOfType<MetalBall>();
        if (ball != null)
        {
            ballRb = ball.GetComponent<Rigidbody2D>();
            var ballCollider = ball.GetComponent<CircleCollider2D>();
            if (ballCollider != null) ballRadius = ballCollider.radius;
        }
        
        UpdateVisuals();
    }
    
    void FixedUpdate()
    {
        if (ball == null || ballRb == null) return;
        
        float distance = Vector2.Distance(transform.position, ball.transform.position);
        float surfaceDistance = magnetRadius + ballRadius;
        if(isTrapMagnet==false)
        {
        // Check if ball should be "stuck" (for attract magnets only)
        if (isAttract && distance <= surfaceDistance + deadZoneRadius)
        {
            if (ballRb.velocity.magnitude <= stopSpeedThreshold)
            {
                // STICK the ball - complete stop
                StickBallToSurface();
                UpdateVisualFeedback();
                return;
            }
        }
        
        // Check if stuck ball should be released
        if (ballIsStuck)
        {
            if (!isAttract || distance > surfaceDistance + deadZoneRadius * 2f)
            {
                ballIsStuck = false; // Release the ball
            }
            else
            {
                // Keep it stuck
                StickBallToSurface();
                UpdateVisualFeedback();
                return;
            }
        }
        
        Vector2 force = CalculateMagneticForce();
        ApplySmoothForce(force);
        }
        else
        {
            TrapMagnet();
        }

        UpdateVisualFeedback();
    }
    
    void StickBallToSurface()
    {
        ballIsStuck = true;
        
        // Position ball exactly at surface
        Vector2 direction = ((Vector2)ball.transform.position - (Vector2)transform.position).normalized;
        Vector2 targetPosition = (Vector2)transform.position + direction * (magnetRadius + ballRadius);
        
        // Hard snap to position
        ballRb.position = targetPosition;
        ballRb.velocity = Vector2.zero;
        ballRb.angularVelocity = 0f;
        lastForce = Vector2.zero;
    }
    
    Vector2 CalculateMagneticForce()
    {
        Vector2 direction = (Vector2)transform.position - (Vector2)ball.transform.position;
        float distance = direction.magnitude;
        
        // Outside range - no force
        if (distance > range) return Vector2.zero;
        
        // Near surface - dramatically reduce force to prevent oscillation
        float surfaceDistance = magnetRadius + ballRadius;
        if (distance < surfaceDistance + forceReductionZone)
        {
            float distanceFromSurface = distance - surfaceDistance;
            if (distanceFromSurface < 0.05f) // Very close to surface
            {
                return Vector2.zero; // No force when at surface
            }
            
            // Reduce force based on proximity to surface
            float forceMultiplier = distanceFromSurface / forceReductionZone;
            forceMultiplier = Mathf.Clamp01(forceMultiplier);
            
            float reducedStrength = strength * forceMultiplier * 0.3f; // Much weaker near surface
            float normalizedDistance = distance / range;
            float forceMagnitude = reducedStrength * (1f - normalizedDistance * normalizedDistance);
            
            Vector2 force = direction.normalized * forceMagnitude;
            return isAttract ? force : -force;
        }
        
        // Normal force calculation
        float normalizedDist = distance / range;
        float forceMag = strength * (1f - normalizedDist * normalizedDist);
        forceMag = Mathf.Min(forceMag, maxForce);
        
        Vector2 normalForce = direction.normalized * forceMag;
        return isAttract ? normalForce : -normalForce;
    }
    
    void ApplySmoothForce(Vector2 targetForce)
    {
        if (ballIsStuck) return; // Don't apply force when stuck
        
        lastForce = Vector2.Lerp(lastForce, targetForce, smoothingFactor);
        ball.ApplyForce(lastForce);
    }
    
    public void Toggle()
    {
        isAttract = !isAttract;
        ballIsStuck = false; // Release ball when toggling
        UpdateVisuals();
        lastForce = Vector2.zero;
    }
    
    void UpdateVisuals()
    {
        if (sr != null)
        {
            Color baseColor = isAttract ? Color.blue : Color.red;
            if (isTrapMagnet)
            {
            baseColor = Color.yellow;
            }
            else if (isAttract)
            {
            baseColor = Color.blue;
            }
            else
            {
            baseColor = Color.red;
            }
            if (ballIsStuck) baseColor = Color.Lerp(baseColor, Color.white, 0.3f); // Lighter when stuck
            sr.color = baseColor;
        }
    }
    
    void UpdateVisualFeedback()
    {
        if (!showRange || sr == null) return;
        
        Vector2 direction = (Vector2)ball.transform.position - (Vector2)transform.position;
        float distance = direction.magnitude;
        
        if (distance <= range)
        {
            float pulseIntensity = 1f - (distance / range);
            float pulse = 1f + (Mathf.Sin(Time.time * pulseSpeed) * 0.2f * pulseIntensity);
            
            Color currentColor = isAttract ? Color.blue : Color.red;
            
            if (isTrapMagnet)
            {
            currentColor = Color.yellow;
            }
            else if (isAttract)
            {
            currentColor = Color.blue;
            }
            else 
            {
            currentColor = Color.red;
            } 
            if (ballIsStuck) currentColor = Color.Lerp(currentColor, Color.white, 0.4f);
            currentColor *= pulse;
            sr.color = currentColor;
        }
        else
        {
            
            if (isTrapMagnet)
            {
            sr.color = Color.yellow;
            }
            else if (isAttract)
            {
            sr.color = Color.blue;
            }
            else
            {
            sr.color = Color.red;
            } 
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw range circle
        Gizmos.color = isAttract ? Color.blue * 0.3f : Color.red * 0.3f;
        Gizmos.DrawWireSphere(transform.position, range);
        
        // Draw physical boundary
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
        
        // Draw dead zone
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRadius + ballRadius + deadZoneRadius);
        
        if (Application.isPlaying && ball != null)
        {
            Gizmos.color = ballIsStuck ? Color.green : Color.red;
            Gizmos.DrawRay(transform.position, lastForce.normalized * 2f);
        }
    }

    void TrapMagnet()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range/2f);
        foreach (Collider2D col in colliders)
        {
           
            if (col != null && (col.tag=="Obstacle"|| col.tag=="Block"))
            {
                Rigidbody2D rb = col.attachedRigidbody;
                if (rb != null)
                {
                    rb.isKinematic = false;
                    Vector3 direction = (transform.position - col.transform.position).normalized;
                    rb.AddForce(direction * strength*0.5f);
                }
            }
        }
    }
}