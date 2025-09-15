using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    // References to UI buttons
    public Button startButton;
    public Button settingsButton;
    public Button quitButton;
    public AudioSource clickSound;

    public GameObject settingsPanel;
    // Start is called before the first frame update
    void Start()
    {
        // Ensure buttons are assigned and add listeners
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
     public void StartGame()
    {
        clickSound.Play();
        SceneManager.LoadScene("Level_1"); // Replace with your game scene name
    }

    // Open settings panel
    public void OpenSettings()
    {
        clickSound.Play();
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    // Close settings panel
    public void CloseSettings()
    {
        clickSound.Play();
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // Quit the application
    public void QuitGame()
    {
        clickSound.Play();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
