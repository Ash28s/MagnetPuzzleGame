using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MagnetSpawnSelector : MonoBehaviour
{
    public enum PendingSpawnType { None, Attract, Repel, Trap, Parabolic }

    [Header("Buttons")]
    public Button attractButton;
    public Button repelButton;
    public Button trapMagButton;
    public Button parabolicMagButton;

    [Header("Optional Labels / Icons")]
    public TextMeshProUGUI attractLabel;
    public TextMeshProUGUI repelLabel;
    public TextMeshProUGUI trapMagLabel;
    public TextMeshProUGUI parabolicMagLabel;

    [Header("Visual Feedback")]
    public Color idleColor = new Color(0.25f, 0.25f, 0.25f, 0.85f);
    public Color selectedColor = new Color(0.15f, 0.6f, 1f, 0.95f);
    public Color selectedRepelColor = new Color(1f, 0.35f, 0.35f, 0.95f);
    public Color selectedTrapColor = new Color(1f, 0.35f, 0.35f, 0.95f);
    public Color selectedParabolicColor= new Color(1f, 0.0f, 0.617527f, 0.95f);
    public static PendingSpawnType PendingSpawn { get; private set; } = PendingSpawnType.None;

    void Awake()
    {
        if (attractButton != null)
            attractButton.onClick.AddListener(() => Select(PendingSpawnType.Attract));
        if (repelButton != null)
            repelButton.onClick.AddListener(() => Select(PendingSpawnType.Repel));
        if(trapMagButton!=null)
            trapMagButton.onClick.AddListener(()=>Select(PendingSpawnType.Trap));
        if(parabolicMagButton!=null)
            parabolicMagButton.onClick.AddListener(()=>Select(PendingSpawnType.Parabolic));    
        RefreshVisuals();
    }

    void OnEnable() => RefreshVisuals();

    public void Select(PendingSpawnType type)
    {
        PendingSpawn = type;
        RefreshVisuals();
    }

    // Consumed by TouchMagnetInput after a successful spawn
    public static void Consume()
    {
        PendingSpawn = PendingSpawnType.None;
    }

    void RefreshVisuals()
    {
        // Basic highlight logic
        if (attractButton != null)
        {
            var img = attractButton.GetComponent<Image>();
            if (img) img.color =  (PendingSpawn == PendingSpawnType.Attract) ? selectedColor : idleColor;
        }
        if (repelButton != null)
        {
            var img = repelButton.GetComponent<Image>();
            if (img) img.color = (PendingSpawn == PendingSpawnType.Repel) ? selectedRepelColor : idleColor;
        }
        if(trapMagButton!=null)
        {
            var img = trapMagButton.GetComponent<Image>();
            if (img) img.color = (PendingSpawn == PendingSpawnType.Trap) ? selectedTrapColor : idleColor; 
        }
        if(trapMagButton!=null)
        {
            var img = trapMagButton.GetComponent<Image>();
            if (img) img.color = (PendingSpawn == PendingSpawnType.Trap) ? selectedTrapColor : idleColor; 
        }
        if(parabolicMagButton!=null)
        {
            var img = parabolicMagButton.GetComponent<Image>();
            if (img) img.color = (PendingSpawn == PendingSpawnType.Parabolic) ? selectedParabolicColor : idleColor; 
        }

        if (attractLabel != null)
            attractLabel.text = (PendingSpawn == PendingSpawnType.Attract) ? "Attract (READY)" : "Attract";
        if (repelLabel != null)
            repelLabel.text = (PendingSpawn == PendingSpawnType.Repel) ? "Repel (READY)" : "Repel";
        if (trapMagLabel != null)
            trapMagLabel.text = (PendingSpawn == PendingSpawnType.Trap) ? "Trap Magnet (READY)" : "Trap Magnet";  
        if (parabolicMagLabel != null)
            parabolicMagLabel.text = (PendingSpawn == PendingSpawnType.Parabolic) ? "Parabolic Magnet (READY)" : "Parabolic Magnet";  
    }    
    
}