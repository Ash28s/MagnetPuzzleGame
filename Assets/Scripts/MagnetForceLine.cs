using UnityEngine;

[RequireComponent(typeof(Magnet))]
public class MagnetForceLine : MonoBehaviour
{
    [Header("Line Settings")]
    public bool enableLine = true;
    public float minWidth = 0.03f;
    public float maxWidth = 0.12f;
    public float pulseSpeed = 6f;
    [Range(0f, 1f)] public float alpha = 0.9f;
    public Material lineMaterial; // Optional; defaults to Sprites/Default

    [Header("Colors")]
    public Color attractColor = new Color(0.3f, 0.7f, 1f, 1f); // Blue
    public Color repelColor = new Color(1f, 0.4f, 0.4f, 1f);   // Red
    public Color trapColor = new Color(1f, 0.9f, 0.2f, 1f);    // Yellow

    Magnet magnet;
    MetalBall ball;
    LineRenderer lr;
    Vector3[] positions = new Vector3[2];

    void Awake()
    {
        magnet = GetComponent<Magnet>();
        SetupLine();
        FindBall();
    }

    void OnEnable()
    {
        if (lr != null) lr.enabled = false;
    }

    void SetupLine()
    {
        lr = gameObject.GetComponent<LineRenderer>();
        if (lr == null) lr = gameObject.AddComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.numCapVertices = 6;
        lr.numCornerVertices = 6;
        lr.textureMode = LineTextureMode.Stretch;
        lr.alignment = LineAlignment.View;

        // Material
        if (lineMaterial != null)
        {
            lr.material = lineMaterial;
        }
        else
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            lr.material = mat;
        }

        // Start thin
        lr.startWidth = minWidth;
        lr.endWidth = minWidth;

        // Render above sprites
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 10;

        // Initial color
        SetLineColor(attractColor);
        lr.enabled = false;
    }

    void FindBall()
    {
        if (ball == null)
            ball = FindObjectOfType<MetalBall>();
    }

    void Update()
    {
        if (!enableLine) { if (lr.enabled) lr.enabled = false; return; }
        if (magnet == null) return;
        if (ball == null) { FindBall(); if (ball == null) { if (lr.enabled) lr.enabled = false; return; } }

        Vector2 mpos = transform.position;
        Vector2 bpos = ball.transform.position;
        float distance = Vector2.Distance(mpos, bpos);

        // Only show line within magnet range
        if (distance > magnet.range)
        {
            if (lr.enabled) lr.enabled = false;
            return;
        }

        if (!lr.enabled) lr.enabled = true;

        positions[0] = mpos;
        positions[1] = bpos;
        lr.SetPositions(positions);

        // Estimate force similarly to Magnet.cs
        float normalizedDist = Mathf.Clamp01(distance / Mathf.Max(0.0001f, magnet.range));
        float baseForce = magnet.strength * (1f - normalizedDist * normalizedDist);

        // Width scales with force + subtle pulse
        float widthT = Mathf.InverseLerp(0f, Mathf.Max(0.0001f, magnet.maxForce), baseForce);
        float pulse = 0.85f + 0.15f * Mathf.Sin(Time.time * pulseSpeed);
        float width = Mathf.Lerp(minWidth, maxWidth, Mathf.Clamp01(widthT)) * pulse;

        lr.startWidth = width;
        lr.endWidth = width * 0.85f;

        // Color based on magnet type/state
        Color c = magnet.isTrapMagnet ? trapColor : (magnet.isAttract ? attractColor : repelColor);
        c.a = alpha * (0.8f + 0.2f * Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed * 0.66f)));
        SetLineColor(c);
    }

    void SetLineColor(Color c)
    {
        if (lr == null) return;
        var grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(c, 0f),
                new GradientColorKey(c, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(c.a, 0f),
                new GradientAlphaKey(c.a, 1f)
            }
        );
        lr.colorGradient = grad;
    }
}