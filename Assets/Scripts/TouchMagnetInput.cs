using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class TouchMagnetInput : MonoBehaviour
{
    [Header("References")]
    public Magnet magnetPrefab;
    public LevelBootstrap bootstrap;   // assign in Inspector
    public GameManager gameManager;

    [Header("Timing")]
    public float doubleTapMaxGap = 0.25f;
    public float longPressThreshold = 0.45f;

    [Header("Placement")]
    public float spawnClampMargin = 0.4f;

    Camera cam;

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

    // Double tap removal tracking
    private static float lastTapTime = -1f;
    private static Magnet lastTappedMagnet = null;

#if UNITY_EDITOR
    float lastMouseClickTime = -1f;
    int mouseClickCount = 0;
#endif

    void Awake()
    {
        cam = Camera.main;
        if (bootstrap == null) bootstrap = FindObjectOfType<LevelBootstrap>();
    }

    void Update()
    {
#if UNITY_EDITOR
        HandleMouseFallback();
#endif
        HandleTouches();
    }

    // ---------------- TOUCH HANDLING ----------------
    void HandleTouches()
    {
        if (Input.touchCount == 0)
        {
            CleanupDeadTouches();
            return;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            // Ignore UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
                continue;

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

    void CleanupDeadTouches()
    {
        var toRemove = new List<int>();
        foreach (var kvp in activeTouches)
        {
            bool stillActive = false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).fingerId == kvp.Key) { stillActive = true; break; }
            }
            if (!stillActive) toRemove.Add(kvp.Key);
        }
        foreach (var id in toRemove) activeTouches.Remove(id);
    }

    void OnTouchBegan(Touch t)
    {
        var info = new TapInfo
        {
            fingerId = t.fingerId,
            firstTapTime = Time.time,
            position = t.position,
            downTime = Time.time,
            pressedMagnet = MagnetUnderScreenPoint(t.position)
        };

        activeTouches[t.fingerId] = info;
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
                Debug.Log("Magnet toggled by long press.");
            }
        }
    }

    void OnTouchEnded(Touch t)
    {
        if (!activeTouches.TryGetValue(t.fingerId, out var info))
            return;

        float duration = Time.time - info.downTime;
        Debug.Log("Touch End");
        if (!info.longPressTriggered && duration < longPressThreshold)
        {
            Debug.Log("Trigger tap");
            if (info.pressedMagnet != null)
            {
                //HandleDoubleTapRemoval(info.pressedMagnet); // Disabled double tap removal for magnets.
                // Single-tap removal
                Destroy(info.pressedMagnet.gameObject);
                Debug.Log("Magnet removed by single tap.");
            }
            else
            {
                TrySpawnFromPending(info.position);
            }
        }

        activeTouches.Remove(t.fingerId);
    }

    // ---------------- SPAWN LOGIC ----------------
    void TrySpawnFromPending(Vector2 screenPos)
    {       
        // Check pending spawn selection
        var pending = MagnetSpawnSelector.PendingSpawn;
        if (pending == MagnetSpawnSelector.PendingSpawnType.None)
        {
            Debug.Log("nothing "+pending);
            // Nothing selected: do not spawn
            return;
        }

        bool attract = (pending == MagnetSpawnSelector.PendingSpawnType.Attract);
        Debug.Log("Spawn Magnet");
        SpawnMagnetAtScreen(screenPos, attract);
        Debug.Log(pending == MagnetSpawnSelector.PendingSpawnType.None);
        // Consume the selection (one-shot)
        //MagnetSpawnSelector.Consume();
    }

    void HandleDoubleTapRemoval(Magnet magnet)
    {
        if (magnet == null) return;

        if (lastTappedMagnet == magnet && Time.time - lastTapTime <= doubleTapMaxGap)
        {
            Destroy(magnet.gameObject);
            lastTappedMagnet = null;
            lastTapTime = -1f;
            Debug.Log("Magnet removed by double tap.");
        }
        else
        {
            lastTappedMagnet = magnet;
            lastTapTime = Time.time;
        }
    }

    Magnet SpawnMagnetAtScreen(Vector2 screenPos, bool attract)
    {
        if (gameManager != null && !gameManager.CanSpawnMagnet())
        {
            Debug.Log("Maximum magnets reached!");
            return null;
        }
        if (magnetPrefab == null)
        {
            Debug.LogWarning("Magnet prefab not assigned!");
            return null;
        }

        Vector3 world = cam.ScreenToWorldPoint(screenPos);
        world.z = 0f;

        if (bootstrap != null)
            world = bootstrap.ClampInside(world, spawnClampMargin);

        var pending = MagnetSpawnSelector.PendingSpawn;
        var m = Instantiate(magnetPrefab, world, Quaternion.identity);
        m.isAttract = attract;
        if(pending == MagnetSpawnSelector.PendingSpawnType.Trap)
        {
            m.isTrapMagnet = true;
        }
        var sr = m.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = attract ? Color.blue : Color.red;
            if (pending == MagnetSpawnSelector.PendingSpawnType.Trap)
            {
            sr.color = Color.yellow;
            }
            if (sr.sprite == null)
            {
                sr.sprite = SpriteCreator.CreateSquareSprite(attract ? Color.blue : Color.red);
            }
        }

        m.SendMessage("UpdateVisuals", SendMessageOptions.DontRequireReceiver);
        m.name = attract ? "Magnet_Attract" : "Magnet_Repel";
        
        if (pending == MagnetSpawnSelector.PendingSpawnType.Trap)
        {
            sr.color = Color.yellow;
            m.name = "Trap_Magnet";
        }
        Debug.Log($"Spawned {(attract ? "Attract" : "Repel")} magnet @ {world}");
        return m;
    }

    Magnet MagnetUnderScreenPoint(Vector2 screenPos)
    {
        Vector2 world = cam.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(world, Vector2.zero, 0f);
        if (hit.collider != null)
            return hit.collider.GetComponent<Magnet>();
        return null;
    }

#if UNITY_EDITOR
    void HandleMouseFallback()
    {
        if (Input.touchCount > 0) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;  

        if (Input.GetMouseButtonDown(0))
        {
            var magnet = MagnetUnderScreenPoint(Input.mousePosition);
            if (magnet != null)
            {
                // // Double click removal
                // float timeSinceLastClick = Time.time - lastMouseClickTime;
                // if (timeSinceLastClick <= doubleTapMaxGap)
                // {
                //     mouseClickCount++;
                //     if (mouseClickCount >= 2)
                //     {
                //         Destroy(magnet.gameObject);
                //         mouseClickCount = 0;
                //         Debug.Log("Removed magnet via mouse double-click");
                //     }
                // }
                // else
                // {
                //     mouseClickCount = 1;
                // }
                // lastMouseClickTime = Time.time;

                // Single-click removal in Editor
                Destroy(magnet.gameObject);
                Debug.Log("Removed magnet via mouse single-click (Editor).");
            }
            else
            {
                // Try spawn based on one-shot selection
                TrySpawnFromPending(Input.mousePosition);
                mouseClickCount = 0;
            }
        }
    }
#endif
}