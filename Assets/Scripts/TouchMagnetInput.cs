using UnityEngine;
using System.Collections.Generic;

public class TouchMagnetInput : MonoBehaviour
{
    [Header("References")]
    public Magnet magnetPrefab;
    public LevelBootstrap bootstrap;   // assign in Inspector

    [Header("Timing")]
    public float doubleTapMaxGap = 0.25f;
    public float longPressThreshold = 0.45f;

    [Header("Placement")]
    public float spawnClampMargin = 0.4f;

    Camera cam;

    // Tracking taps
    class TapInfo
    {
        public int fingerId;
        public float firstTapTime;
        public Vector2 position;
        public bool longPressTriggered;
        public float downTime;
        public Magnet pressedMagnet;
    }

    Dictionary<int, TapInfo> activeTouches = new Dictionary<int, TapInfo>();
    float lastMouseClickTime = -1f;
    int mouseClickCount = 0;
    
    // For double-tap removal tracking
    private static float lastTapTime = -1f;
    private static Magnet lastTappedMagnet = null;

    void Awake()
    {
        cam = Camera.main;
        if (bootstrap == null) bootstrap = FindObjectOfType<LevelBootstrap>();
    }

    void Update()
    {
#if UNITY_EDITOR
        HandleMouseFallback();   // so you can still test in editor
#endif
        HandleTouches();
    }

    // ---------------- TOUCH HANDLING ----------------
    void HandleTouches()
    {
        if (Input.touchCount == 0)
        {
            // Clean up ended touches
            var toRemove = new List<int>();
            foreach(var kvp in activeTouches)
            {
                bool stillActive = false;
                for(int i = 0; i < Input.touchCount; i++)
                {
                    if(Input.GetTouch(i).fingerId == kvp.Key)
                    {
                        stillActive = true;
                        break;
                    }
                }
                if(!stillActive) toRemove.Add(kvp.Key);
            }
            foreach(int id in toRemove) activeTouches.Remove(id);
            return;
        }

        // Process all active touches
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            switch (t.phase)
            {
                case TouchPhase.Began:
                    OnTouchBegan(t);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    OnTouchHold(t);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    OnTouchEnded(t);
                    break;
            }
        }
    }

    void OnTouchBegan(Touch t)
    {
        var info = new TapInfo
        {
            fingerId = t.fingerId,
            firstTapTime = Time.time,
            position = t.position,
            downTime = Time.time
        };

        // Check if starting on a magnet
        info.pressedMagnet = MagnetUnderScreenPoint(t.position);
        activeTouches[t.fingerId] = info;

        // Two finger simultaneous tap = spawn repel magnet at average position
        if (Input.touchCount == 2)
        {
            Touch other = GetOtherTouch(t.fingerId);
            if (other.phase == TouchPhase.Began || (Time.time - GetTouchInfo(other.fingerId)?.downTime) < 0.1f)
            {
                Vector2 avgScreen = 0.5f * (t.position + other.position);
                SpawnMagnetAtScreen(avgScreen, attract: false);
            }
        }
    }

    void OnTouchHold(Touch t)
    {
        if (!activeTouches.TryGetValue(t.fingerId, out var info)) return;

        // Long press on magnet = toggle polarity
        if (!info.longPressTriggered && info.pressedMagnet != null)
        {
            if (Time.time - info.downTime >= longPressThreshold)
            {
                info.longPressTriggered = true;
                info.pressedMagnet.Toggle();
            }
        }
    }

    void OnTouchEnded(Touch t)
    {
        if (!activeTouches.TryGetValue(t.fingerId, out var info))
            return;

        float duration = Time.time - info.downTime;

        // If this was a short tap (no long press triggered)
        if (!info.longPressTriggered && duration < longPressThreshold)
        {
            // Handle double tap removal
            if (info.pressedMagnet != null)
            {
                HandleDoubleTapRemoval(info.pressedMagnet);
            }
            else
            {
                // Single finger tap on empty space = attract magnet
                if (Input.touchCount == 1)
                {
                    SpawnMagnetAtScreen(info.position, attract: true);
                }
            }
        }

        activeTouches.Remove(t.fingerId);
    }

    void HandleDoubleTapRemoval(Magnet magnet)
    {
        if (magnet == null) return;

        if (lastTappedMagnet == magnet && Time.time - lastTapTime <= doubleTapMaxGap)
        {
            // Double tap detected - remove magnet
            Destroy(magnet.gameObject);
            lastTappedMagnet = null;
            lastTapTime = -1f;
        }
        else
        {
            // First tap - remember it
            lastTappedMagnet = magnet;
            lastTapTime = Time.time;
        }
    }

    TapInfo GetTouchInfo(int fingerId)
    {
        activeTouches.TryGetValue(fingerId, out var info);
        return info;
    }

    Touch GetOtherTouch(int currentId)
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.fingerId != currentId) return t;
        }
        return default;
    }

    // ---------------- SPAWNING ----------------
    void SpawnMagnetAtScreen(Vector2 screenPos, bool attract)
    {
        if (magnetPrefab == null) 
        {
            Debug.LogWarning("Magnet prefab not assigned!");
            return;
        }
        
        Vector3 world = cam.ScreenToWorldPoint(screenPos);
        world.z = 0f;

        if (bootstrap != null)
            world = bootstrap.ClampInside(world, spawnClampMargin);

        // Instantiate the magnet
        var m = Instantiate(magnetPrefab, world, Quaternion.identity);
        
        // Set the attract property FIRST
        m.isAttract = attract;
        
        // Force visual update
        var sr = m.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = attract ? Color.blue : Color.red;
        }
        
        // Update the sprite if using SpriteCreator
        if (sr != null && sr.sprite == null)
        {
            sr.sprite = SpriteCreator.CreateSquareSprite(attract ? Color.blue : Color.red);
        }
        
        // Call magnet's update method if it exists
        if (m.GetComponent<Magnet>() != null)
        {
            // Force the magnet to refresh its visuals
            m.SendMessage("UpdateVisuals", SendMessageOptions.DontRequireReceiver);
        }
        
        m.name = attract ? "Magnet_Attract" : "Magnet_Repel";
        
        Debug.Log($"Spawned {(attract ? "ATTRACT" : "REPEL")} magnet at {world}");
    }

    Magnet MagnetUnderScreenPoint(Vector2 screenPos)
    {
        Vector2 world = cam.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(world, Vector2.zero, 0f);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Magnet>();
        }
        return null;
    }

    // ---------------- MOUSE FALLBACK (EDITOR) ----------------
    void HandleMouseFallback()
    {
        // Don't handle mouse if we have active touches
        if (Input.touchCount > 0) return;

        // Left click = attract magnet (BLUE)
        if (Input.GetMouseButtonDown(0))
        {
            // Check if clicking on existing magnet for double-click removal
            var magnet = MagnetUnderScreenPoint(Input.mousePosition);
            if (magnet != null)
            {
                // Handle double click removal
                float timeSinceLastClick = Time.time - lastMouseClickTime;
                if (timeSinceLastClick <= doubleTapMaxGap)
                {
                    mouseClickCount++;
                    if (mouseClickCount >= 2)
                    {
                        Destroy(magnet.gameObject);
                        mouseClickCount = 0;
                        Debug.Log("Removed magnet via double-click");
                    }
                }
                else
                {
                    mouseClickCount = 1;
                }
                lastMouseClickTime = Time.time;
            }
            else
            {
                // Spawn attract magnet
                SpawnMagnetAtScreen(Input.mousePosition, true);
                mouseClickCount = 0;
            }
        }

        // Right click = repel magnet (RED)
        if (Input.GetMouseButtonDown(1))
        {
            SpawnMagnetAtScreen(Input.mousePosition, false);
        }

        // Middle click = toggle magnet under cursor
        if (Input.GetMouseButtonDown(2))
        {
            var magnet = MagnetUnderScreenPoint(Input.mousePosition);
            if (magnet != null)
            {
                magnet.Toggle();
                Debug.Log($"Toggled magnet to {(magnet.isAttract ? "ATTRACT" : "REPEL")}");
            }
        }

        // Keyboard shortcuts for testing
        if (Input.GetKeyDown(KeyCode.A))
        {
            SpawnMagnetAtScreen(Input.mousePosition, true);
            Debug.Log("Spawned ATTRACT magnet via 'A' key");
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            SpawnMagnetAtScreen(Input.mousePosition, false);
            Debug.Log("Spawned REPEL magnet via 'R' key");
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            var magnets = FindObjectsOfType<Magnet>();
            foreach(var mag in magnets)
                Destroy(mag.gameObject);
            Debug.Log($"Cleared {magnets.Length} magnets");
        }
    }
}