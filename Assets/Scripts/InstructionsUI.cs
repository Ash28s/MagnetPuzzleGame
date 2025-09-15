using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InstructionsUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject instructionsPanel;
    public Button showInstructionsButton;
    public Button closeInstructionsButton;
    public TextMeshProUGUI instructionsText;
    
    [Header("Settings")]
    public bool showOnStart = true;
    
    void Start()
    {
        SetupInstructionsText();
        
        // Setup button events
        if (showInstructionsButton != null)
            showInstructionsButton.onClick.AddListener(ShowInstructions);
        if (closeInstructionsButton != null)
            closeInstructionsButton.onClick.AddListener(HideInstructions);
            
        // Show instructions on first launch
        if (showOnStart && !HasSeenInstructions())
        {
            ShowInstructions();
            MarkInstructionsAsSeen();
        }
        else
        {
            HideInstructions();
        }
    }
    
    void SetupInstructionsText()
    {
        if (instructionsText == null) return;
        
<<<<<<< HEAD
        instructionsText.text = @"<size=24><b>🧲 MAGNET PUZZLE GAME</b></size>

<size=18><b>🎯 OBJECTIVE:</b></size>
Guide the metal ball to the goal without hitting obstacles!

<size=18><b>📱 TOUCH CONTROLS:</b></size>

<b>🔵 ATTRACT MAGNET (Blue):</b>
• <b>Single tap</b> on empty space
• Pulls the ball towards it

<b>🔴 REPEL MAGNET (Red):</b>
• <b>Two finger tap</b> simultaneously
• Pushes the ball away

<b>🔄 TOGGLE MAGNET:</b>
• <b>Long press</b> any magnet
• Changes blue ↔ red

<b>🗑️ REMOVE MAGNET:</b>
• <b>Double tap</b> any magnet
• Deletes the magnet

<size=18><b>⚠️ GAME RULES:</b></size>
=======
        instructionsText.text = @"<size=18><b> MAGNET PUZZLE GAME</b></size>

<size=14><b>OBJECTIVE:</b></size>
Guide the metal ball to the goal without hitting obstacles!

<size=14><b> TOUCH CONTROLS:</b></size>

<b> ATTRACT MAGNET (Blue):</b>
• <b>Single tap</b> on empty space
• Pulls the ball towards it

<b> REPEL MAGNET (Red):</b>
• <b>Two finger tap</b> simultaneously
• Pushes the ball away

<b> TOGGLE MAGNET:</b>
• <b>Long press</b> any magnet
• Changes blue ↔ red

<b> REMOVE MAGNET:</b>
• <b>Double tap</b> any magnet
• Deletes the magnet

<size=14><b> GAME RULES:</b></size>
>>>>>>> vishal
• Maximum <b>5 magnets</b> at once
• Don't let ball hit <b>obstacles</b>
• Beat the timer!

<<<<<<< HEAD
<size=18><b>💡 TIPS:</b></size>
=======
<size=14><b> TIPS:</b></size>
>>>>>>> vishal
• Use attract magnets to pull ball around corners
• Use repel magnets to push ball away from danger
• Combine both types for precise control
• Plan your magnet placement carefully!

<<<<<<< HEAD
<size=16><i>Good luck! 🍀</i></size>";
=======
<size=14><i>Good luck!</i></size>";
>>>>>>> vishal
    }
    
    public void ShowInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
            Time.timeScale = 0f; // Pause game
        }
    }
    
    public void HideInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
            Time.timeScale = 1f; // Resume game
        }
    }
    
    bool HasSeenInstructions()
    {
        return PlayerPrefs.GetInt("HasSeenInstructions", 0) == 1;
    }
    
    void MarkInstructionsAsSeen()
    {
        PlayerPrefs.SetInt("HasSeenInstructions", 1);
        PlayerPrefs.Save();
    }
    
    public void ResetInstructionsFlag()
    {
        PlayerPrefs.DeleteKey("HasSeenInstructions");
    }
}