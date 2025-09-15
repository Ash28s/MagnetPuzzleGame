using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems; // to ignore UI touches

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
    public GameManager gameManager;
<<<<<<< HEAD

    [SerializeField] MagnetSwitcher magnetSwitcher;

=======
    // Tracking taps
>>>>>>> vishal
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

    private static float lastTapTime = -1f;
    private static Magnet lastTappedMagnet = null;

    void Awake()
    {
        cam = Camera.main;
        if (bootstrap == null) bootstrap = FindObjectOfType<LevelBootstrap>();
        if (magnetSwitcher == null) magnetSwitcher = FindObjectOfType<MagnetSwitcher>();
    }

    void Update()
    {
#if UNITY_EDITOR
        HandleMouseFallback();
#endif
        HandleTouches();
    }

    void HandleTouches()
    {
        if (Input.touchCount == 0)
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
            foreach (int id in toRemove) activeTouches.Remove(id);
            return;
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            // Ignore UI touches
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

    void OnTouchBegan(Touch t)
    {
        var info = new TapInfo
        {
            fingerId = t.fingerId,
            firstTapTime = Time.time,
            position = t.position,
            downTime = Time.time
        };

        // If finger starts on a magnet, set it active for the switcher
        info.pressedMagnet = MagnetUnderScreenPoint(t.position);
        if (info.pressedMagnet != null)
            magnetSwitcher?.SetActiveMagnet(info.pressedMagnet);

        activeTouches[t.fingerId] = info;

        // Two-finger: spawn opposite of current mode at average position
        if (Input.touchCount == 2)
        {
            Touch other = GetOtherTouch(t.fingerId);
            if (other.phase == TouchPhase.Began || (Time.time - GetTouchInfo(other.fingerId)?.downTime) < 0.1f)
            {
                Vector2 avgScreen = 0.5f * (t.position + other.position);
                bool currentModeAttract = magnetSwitcher != null ? magnetSwitcher.GetCurrentMode() : MagnetSwitcher.IsAttractMode;
                SpawnMagnetAtScreen(avgScreen, attract: !currentModeAttract);
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

                // Sync switcher with this magnet
                magnetSwitcher?.SetActiveMagnet(info.pressedMagnet);
            }
        }
    }

    void OnTouchEnded(Touch t)
    {
        if (!activeTouches.TryGetValue(t.fingerId, out var info)) return;

        float duration = Time.time - info.downTime;

        if (!info.longPressTriggered && duration < longPressThreshold)
        {
            if (info.pressedMagnet != null)
            {
                HandleDoubleTapRemoval(info.pressedMagnet);
            }
            else
            {
                // Single tap on empty space = spawn using current mode
                if (Input.touchCount == 1)
                {
                    bool currentModeAttract = magnetSwitcher != null ? magnetSwitcher.GetCurrentMode() : MagnetSwitcher.IsAttractMode;
                    var spawned = SpawnMagnetAtScreen(info.position, attract: currentModeAttract);

                    // Make the new magnet the active one for button control
                    if (spawned != null) magnetSwitcher?.SetActiveMagnet(spawned);
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
            Destroy(magnet.gameObject);
            lastTappedMagnet = null;
            lastTapTime = -1f;
        }
        else
        {
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

    // Spawn and return the created magnet
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

        var m = Instantiate(magnetPrefab, world, Quaternion.identity);

        m.isAttract = attract;

        var sr = m.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = attract ? Color.blue : Color.red;

        if (sr != null && sr.sprite == null)
            sr.sprite = SpriteCreator.CreateSquareSprite(attract ? Color.blue : Color.red);

        m.SendMessage("UpdateVisuals", SendMessageOptions.DontRequireReceiver);

        m.name = attract ? "Magnet_Attract" : "Magnet_Repel";
        Debug.Log($"Spawned {(attract ? "ATTRACT" : "REPEL")} magnet at {world}");

        return m;
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

#if UNITY_EDITOR
    void HandleMouseFallback()
    {
        if (Input.touchCount > 0) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // Left click
        if (Input.GetMouseButtonDown(0))
        {
            var magnet = MagnetUnderScreenPoint(Input.mousePosition);
            if (magnet != null)
            {
                // Select clicked magnet for button control
                magnetSwitcher?.SetActiveMagnet(magnet);

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
                bool mode = magnetSwitcher != null ? magnetSwitcher.GetCurrentMode() : MagnetSwitcher.IsAttractMode;
                var spawned = SpawnMagnetAtScreen(Input.mousePosition, mode);
                if (spawned != null) magnetSwitcher?.SetActiveMagnet(spawned);
                mouseClickCount = 0;
            }
        }

        // Right click = spawn opposite mode
        if (Input.GetMouseButtonDown(1))
        {
            bool mode = magnetSwitcher != null ? magnetSwitcher.GetCurrentMode() : MagnetSwitcher.IsAttractMode;
            var spawned = SpawnMagnetAtScreen(Input.mousePosition, !mode);
            if (spawned != null) magnetSwitcher?.SetActiveMagnet(spawned);
        }

        // Middle click = toggle magnet under cursor (also sync switcher)
        if (Input.GetMouseButtonDown(2))
        {
            var magnet = MagnetUnderScreenPoint(Input.mousePosition);
            if (magnet != null)
            {
                magnet.Toggle();
                magnetSwitcher?.SetActiveMagnet(magnet);
                Debug.Log($"Toggled magnet to {(magnet.isAttract ? "ATTRACT" : "REPEL")}");
            }
        }
    }
#endif
}