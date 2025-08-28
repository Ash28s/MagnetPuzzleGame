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
            // Clear active touches that ended last frame
            return;
        }

        // Register began
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

        // Two finger simultaneous tap = spawn repel magnet at average
        if (Input.touchCount == 2)
        {
            Touch other = GetOtherTouch(t.fingerId);
            if (other.phase == TouchPhase.Began)
            {
                Vector2 avgScreen = 0.5f * (t.position + other.position);
                SpawnMagnetAtScreen(avgScreen, attract: false);
            }
        }
    }

    void OnTouchHold(Touch t)
    {
        if (!activeTouches.TryGetValue(t.fingerId, out var info)) return;

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

        // If this was a short tap (no long press)
        if (!info.longPressTriggered && duration < longPressThreshold)
        {
            // Double tap detection (on magnet)
            if (info.pressedMagnet != null)
            {
                // Use magnet instanceID as key or simple time diff
                if (Time.time - info.firstTapTime <= doubleTapMaxGap)
                {
                    // Second tap within same info (rare for single finger).
                    // We do removal only if we detect a second tap pattern; simpler approach:
                    // Keep a global lastTap data.
                }
            }

            // Single finger tap (not on existing magnet) => attract magnet
            if (info.pressedMagnet == null && Input.touchCount == 1)
            {
                SpawnMagnetAtScreen(info.position, attract: true);
            }

            // Double tap removal logic (global)
            HandleDoubleTapRemoval(info);
        }

        activeTouches.Remove(t.fingerId);
    }

    void HandleDoubleTapRemoval(TapInfo info)
    {
        // We store last tap static
        const string key = "_lastDoubleTapTime";
        // We'll just keep one last tap timestamp & magnet
        // Simpler: reuse static fields
        if (info.pressedMagnet == null) return;

        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetFloat(key, Time.time);
            PlayerPrefs.SetInt("_lastMagnetID", info.pressedMagnet.GetInstanceID());
            return;
        }

        float lastTime = PlayerPrefs.GetFloat(key);
        int lastId = PlayerPrefs.GetInt("_lastMagnetID");

        if (Time.time - lastTime <= doubleTapMaxGap && lastId == info.pressedMagnet.GetInstanceID())
        {
            // Remove
            Destroy(info.pressedMagnet.gameObject);
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.DeleteKey("_lastMagnetID");
        }
        else
        {
            PlayerPrefs.SetFloat(key, Time.time);
            PlayerPrefs.SetInt("_lastMagnetID", info.pressedMagnet.GetInstanceID());
        }
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
        if (magnetPrefab == null) return;
        Vector3 world = cam.ScreenToWorldPoint(screenPos);
        world.z = 0f;

        if (bootstrap != null)
            world = bootstrap.ClampInside(world, spawnClampMargin);

        var m = Instantiate(magnetPrefab, world, Quaternion.identity);
        m.isAttract = attract;
        var sr = m.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = attract ? Color.blue : Color.red;
        m.name = attract ? "Magnet_Attract" : "Magnet_Repel";
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
        // Left click = attract
        if (Input.GetMouseButtonDown(0))
        {
            SpawnMagnetAtScreen(Input.mousePosition, true);
        }
        // Right click = repel
        if (Input.GetMouseButtonDown(1))
        {
            SpawnMagnetAtScreen(Input.mousePosition, false);
        }

        // Double click remove
        if (Input.GetMouseButtonDown(0))
        {
            float t = Time.time;
            if (t - lastMouseClickTime <= doubleTapMaxGap)
            {
                mouseClickCount++;
                if (mouseClickCount == 2)
                {
                    var mag = MagnetUnderScreenPoint(Input.mousePosition);
                    if (mag != null) Destroy(mag.gameObject);
                    mouseClickCount = 0;
                }
            }
            else
            {
                mouseClickCount = 1;
            }
            lastMouseClickTime = t;
        }
    }
}