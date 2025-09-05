using UnityEngine;

public class LevelBootstrap : MonoBehaviour
{
    public MetalBall ballPrefab;
    public Magnet magnetPrefab;

    [Header("Runtime Generated Sprites")]
    public Sprite wallSprite;
    public Sprite floorSprite;

    [Header("Mobile Arena Settings")]
    public float borderThickness = 1f;        // thickness in world units (1 tile)
    public float innerPadding = 0.5f;         // space between inside edge & spawn clamping
    public bool buildOnAwake = true;

    // Computed safe (interior) bounds (after building walls)
    public Rect innerPlayableRect;

    void Awake()
    {
        if (!buildOnAwake) return;

        // Create simple solid color sprites if not assigned
        if (wallSprite == null) wallSprite = SpriteCreator.CreateSquareSprite(new Color(0.25f, 0.25f, 0.30f));
        if (floorSprite == null) floorSprite = SpriteCreator.CreateSquareSprite(new Color(0.38f, 0.38f, 0.43f));

        BuildMobileBorder();   // dynamic based on camera

    }

    //Level Generator calls SpawnBall
    public void SpawnBall(Vector3 pos)
    {
        if (ballPrefab != null)
        {
            var ball = Instantiate(ballPrefab, pos, Quaternion.identity);
            ball.name = "MetalBall";
        }
    }

    void BuildMobileBorder()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic)
        {
            Debug.LogWarning("Main Camera missing or not orthographic.");
            return;
        }

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        // Define interior rectangle (without walls)
        innerPlayableRect = new Rect(-halfW + borderThickness,
                                     -halfH + borderThickness,
                                     (halfW - borderThickness) * 2f,
                                     (halfH - borderThickness) * 2f);

        // Build four border strips (top, bottom, left, right)
        BuildWallStrip(new Vector2(0, innerPlayableRect.yMax + borderThickness / 2f),   // Top
                       new Vector2(innerPlayableRect.width + borderThickness * 2f, borderThickness));
        BuildWallStrip(new Vector2(0, innerPlayableRect.yMin - borderThickness / 2f),   // Bottom
                       new Vector2(innerPlayableRect.width + borderThickness * 2f, borderThickness));
        BuildWallStrip(new Vector2(innerPlayableRect.xMin - borderThickness / 2f, 0),   // Left
                       new Vector2(borderThickness, innerPlayableRect.height + borderThickness * 2f));
        BuildWallStrip(new Vector2(innerPlayableRect.xMax + borderThickness / 2f, 0),   // Right
                       new Vector2(borderThickness, innerPlayableRect.height + borderThickness * 2f));
    }

    void BuildWallStrip(Vector2 center, Vector2 size)
    {
        GameObject g = new GameObject("WallStrip");
        g.transform.position = center;
        var sr = g.AddComponent<SpriteRenderer>();
        sr.sprite = wallSprite;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
        sr.sortingOrder = 0;
        g.layer = LayerMask.NameToLayer("Ground");

        var col = g.AddComponent<BoxCollider2D>();
        col.size = size;
        var rb = g.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
    }

    // Helper for other scripts to clamp spawning inside playable area
    public Vector2 ClampInside(Vector2 worldPos, float margin = 0.3f)
    {
        float x = Mathf.Clamp(worldPos.x,
                              innerPlayableRect.xMin + innerPadding + margin,
                              innerPlayableRect.xMax - innerPadding - margin);
        float y = Mathf.Clamp(worldPos.y,
                              innerPlayableRect.yMin + innerPadding + margin,
                              innerPlayableRect.yMax - innerPadding - margin);
        return new Vector2(x, y);
    }
}