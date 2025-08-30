using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class MetalBall : MonoBehaviour
{
    private Rigidbody2D rb;
    public float maxVelocity = 100f;
    Vector2 startPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;   // Keep gravity OFF for a magnet puzzle (add later if you want)
        rb.mass = 1f;
        rb.drag = 1f;
        startPos = transform.position;

        var sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteCreator.CreateSquareSprite(new Color(0.8f, 0.8f, 0.9f));
            transform.localScale = new Vector3(0.5f, 0.5f, 1f); // make it smaller
        }
    }
    

    public void ApplyForce(Vector2 f)
    {
        if(rb.velocity.magnitude<maxVelocity)
        {
        rb.AddForce(-f, ForceMode2D.Force);
        }
    }
}