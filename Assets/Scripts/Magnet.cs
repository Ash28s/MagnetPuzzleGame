using UnityEngine;

public class Magnet : MonoBehaviour
{
    public bool isAttract = true;
    public float strength = 40f;   // try 40 then adjust
    public float range = 5f;

    SpriteRenderer sr;
    MetalBall ball;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        if (sr.sprite == null)
            sr.sprite = SpriteCreator.CreateSquareSprite(isAttract ? Color.blue : Color.red);

        transform.localScale = Vector3.one * 0.8f;

        var col = GetComponent<CircleCollider2D>();
        if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = range;

        ball = FindObjectOfType<MetalBall>();
    }

    void Update()
    {
        if (ball == null) return;
        Vector2 dir = (ball.transform.position - transform.position);
        float dist = dir.magnitude;
        if (dist > range || dist < 0.05f) return;

        float forceMag = strength / (dist * dist); // inverse square
        Vector2 force = dir.normalized * forceMag;
        if (!isAttract) force = -force;

        ball.ApplyForce(force);
    }

    public void Toggle()
    {
        isAttract = !isAttract;
        sr.color = isAttract ? Color.blue : Color.red;
    }
}