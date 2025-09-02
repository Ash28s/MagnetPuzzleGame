// using UnityEngine;
// using TMPro;

// public class GameManager : MonoBehaviour
// {
//     public static GameManager Instance;

//     [Header("Timer Settings")]
//     public float timeLimit = 60f; // Time limit in seconds
//     private float currentTime;
//     public TextMeshProUGUI timerText; // Assign in Inspector: UI text for timer

//     [Header("Game Over Settings")]
//     public GameObject gameOverPanel; // Assign in Inspector: GameOver UI panel
//     public TextMeshProUGUI gameOverText; // Assign in Inspector: GameOver text

//     [Header("Magnet Limit")]
//     public int maxMagnets = 5; // Maximum number of magnets allowed

//     private bool isGameOver = false;

//     void Awake()
//     {
//         // Singleton pattern
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//             return;
//         }

//         // Initialize
//         currentTime = timeLimit;
//         if (gameOverPanel != null) gameOverPanel.SetActive(false);
//         UpdateTimerDisplay();
//     }

//     void Update()
//     {
//         if (!isGameOver && currentTime > 0)
//         {
//             currentTime -= Time.deltaTime;
//             UpdateTimerDisplay();

//             if (currentTime <= 0)
//             {
//                 TriggerGameOver();
//             }
//         }
//     }

//     void UpdateTimerDisplay()
//     {
//         if (timerText != null)
//         {
//             int minutes = Mathf.FloorToInt(currentTime / 60);
//             int seconds = Mathf.FloorToInt(currentTime % 60);
//             timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
//         }
//     }

//     void TriggerGameOver()
//     {
//         isGameOver = true;
//         if (gameOverPanel != null) gameOverPanel.SetActive(true);
//         if (gameOverText != null) gameOverText.text = "Game Over! Time's Up!";

//         // Optional: Disable magnet spawning scripts
//         var touchInput = FindObjectOfType<TouchMagnetInput>();
//         if (touchInput != null) touchInput.enabled = false;
//         var simpleSpawner = FindObjectOfType<SimpleMagnetSpawner>();
//         if (simpleSpawner != null) simpleSpawner.enabled = false;
//     }

//     public bool CanSpawnMagnet()
//     {
//         Magnet[] magnets = FindObjectsOfType<Magnet>();
//         return magnets.Length < maxMagnets;
//     }
// }


using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Timer Settings")]
    public float timeLimit = 60f; // Time limit in seconds
    private float currentTime;
    public TextMeshProUGUI timerText; // Assign in Inspector: UI text for timer

    [Header("Game Over Settings")]
    public GameObject gameOverPanel; // Assign in Inspector: GameOver UI panel
    public TextMeshProUGUI gameOverText; // Assign in Inspector: GameOver text

    [Header("Magnet Limit")]
    public int maxMagnets = 5; // Maximum number of magnets allowed

    private bool isGameOver = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize
        currentTime = timeLimit;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (!isGameOver && currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();

            if (currentTime <= 0)
            {
                TriggerGameOver("Time's Up!");
            }
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // CHANGED: Made this method PUBLIC and added string parameter
    public void TriggerGameOver(string reason = "Game Over!")
    {
        if (isGameOver) return; // Prevent multiple calls
        
        isGameOver = true;
        Debug.Log("Game Over triggered: " + reason);
        
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameOverText != null) gameOverText.text = reason;

        // Optional: Disable magnet spawning scripts
        var touchInput = FindObjectOfType<TouchMagnetInput>();
        if (touchInput != null) touchInput.enabled = false;
        var simpleSpawner = FindObjectOfType<SimpleMagnetSpawner>();
        if (simpleSpawner != null) simpleSpawner.enabled = false;
    }

    public bool CanSpawnMagnet()
    {
        Magnet[] magnets = FindObjectsOfType<Magnet>();
        return magnets.Length < maxMagnets;
    }
}