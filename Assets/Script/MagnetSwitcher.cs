using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MagnetSwitcher : MonoBehaviour
{
    [Header("UI References")]
    public Button switchButton;
    public Image buttonIcon;                 // Assign the Image on the button
    public TextMeshProUGUI buttonText;       // Optional label

    [Header("Sprites (Optional)")]
    public Sprite attractSprite;             // Optional: assign attract icon
    public Sprite repelSprite;               // Optional: assign repel icon

    [Header("Visual Settings (Fallback if no sprites)")]
    public Color attractColor = new Color(0.2f, 0.6f, 1f, 1f); // Blue
    public Color repelColor = new Color(1f, 0.3f, 0.3f, 1f);   // Red
    public string attractSymbol = "🧲";
    public string repelSymbol = "⚡";
    public string attractTextLabel = "ATTRACT";
    public string repelTextLabel = "REPEL";

    [Header("Animation")]
    public float scaleAnimation = 1.1f;
    public float animationDuration = 0.15f;

    // Global mode for next spawn (used when there is no active magnet)
    private bool isAttractMode = true;
    public static bool IsAttractMode { get; private set; } = true;

    // The currently “active” magnet (last spawned or last tapped)
    private Magnet activeMagnet;

    void Start()
    {
        if (switchButton != null)
            switchButton.onClick.AddListener(SwitchMagnetType);

        if (buttonIcon != null) buttonIcon.preserveAspect = true;
        UpdateButtonVisuals();
    }

    // Called by the button. If an active magnet exists, toggle THAT magnet.
    // Otherwise, toggle the global spawn mode.
    public void SwitchMagnetType()
    {
        CleanupActiveIfDestroyed();

        if (activeMagnet != null)
        {
            // Toggle the magnet itself
            activeMagnet.Toggle();

            // Sync global mode to the magnet so next spawns match what player sees
            isAttractMode = activeMagnet.isAttract;
            IsAttractMode = isAttractMode;

            UpdateButtonVisuals();
            StartCoroutine(PressAnim());
            Debug.Log($"Toggled ACTIVE magnet to {(isAttractMode ? "ATTRACT" : "REPEL")} and synced spawn mode.");
        }
        else
        {
            // No active magnet — toggle future spawn mode
            isAttractMode = !isAttractMode;
            IsAttractMode = isAttractMode;

            UpdateButtonVisuals();
            StartCoroutine(PressAnim());
            Debug.Log($"Switched SPAWN MODE to {(isAttractMode ? 'A' : 'R')}");
        }
    }

    // Set which magnet is controlled by the switch button
    public void SetActiveMagnet(Magnet magnet)
    {
        activeMagnet = magnet;
        CleanupActiveIfDestroyed();

        if (activeMagnet != null)
        {
            // Sync the UI and global mode to match the active magnet
            isAttractMode = activeMagnet.isAttract;
            IsAttractMode = isAttractMode;
        }

        UpdateButtonVisuals();
    }

    // Current mode reported to input (if an active magnet exists, reflect its mode)
    public bool GetCurrentMode()
    {
        CleanupActiveIfDestroyed();
        if (activeMagnet != null) return activeMagnet.isAttract;
        return isAttractMode;
    }

    // Optional external setter (e.g., if you want to force a mode)
    public void SetMode(bool attract)
    {
        CleanupActiveIfDestroyed();

        if (activeMagnet != null)
        {
            if (activeMagnet.isAttract != attract)
            {
                activeMagnet.Toggle();
            }
            isAttractMode = activeMagnet.isAttract;
        }
        else
        {
            isAttractMode = attract;
        }

        IsAttractMode = isAttractMode;
        UpdateButtonVisuals();
    }

    void CleanupActiveIfDestroyed()
    {
        if (activeMagnet == null) return; // Unity null covers destroyed objects
        // no-op; Unity sets destroyed UnityEngine.Object to null implicitly
    }

    void UpdateButtonVisuals()
    {
        // If we have an active magnet, show its current state, otherwise show global mode
        bool showAttract = GetCurrentMode();

        // Sprite-first approach
        if (buttonIcon != null && attractSprite != null && repelSprite != null)
        {
            buttonIcon.sprite = showAttract ? attractSprite : repelSprite;
            buttonIcon.color = Color.white;
        }
        else if (buttonIcon != null)
        {
            buttonIcon.color = showAttract ? attractColor : repelColor;
        }

        if (buttonText != null)
        {
            var symbol = showAttract ? attractSymbol : repelSymbol;
            var label = showAttract ? attractTextLabel : repelTextLabel;
            buttonText.text = $"{symbol}\n{label}";
            buttonText.color = Color.white;
        }
    }

    System.Collections.IEnumerator PressAnim()
    {
        if (switchButton == null) yield break;

        var t = switchButton.transform;
        var start = t.localScale;
        var up = start * scaleAnimation;

        float half = animationDuration * 0.5f;
        float e = 0f;
        while (e < half)
        {
            t.localScale = Vector3.Lerp(start, up, e / half);
            e += Time.unscaledDeltaTime;
            yield return null;
        }
        t.localScale = up;

        e = 0f;
        while (e < half)
        {
            t.localScale = Vector3.Lerp(up, start, e / half);
            e += Time.unscaledDeltaTime;
            yield return null;
        }
        t.localScale = start;
    }
}