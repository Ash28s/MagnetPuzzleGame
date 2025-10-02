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
    //public static GameManager Instance;

    [Header("Timer Settings")]
    public float timeLimit = 60f; // Time limit in seconds
    private float currentTime;
    public TextMeshProUGUI timerText; // Assign in Inspector: UI text for timer

    [Header("Game Over Settings")]
    public GameObject gameOverPanel; // Assign in Inspector: GameOver UI panel
    public TextMeshProUGUI gameOverText; // Assign in Inspector: GameOver text

    [Header("Magnet Limit")]
    public int maxAttractMagnets = 5; // Maximum number of magnets allowed
    public int maxRepelMagnets = 3; 
    public int maxTrapMagnets = 1; 
    public int maxParabolicMagnets = 1; 
    public TextMeshProUGUI magnetText;
    public TextMeshProUGUI magnetRepelText;
    public TextMeshProUGUI magnetTrapText;
    public TextMeshProUGUI magnetParabolicText;
    private bool isGameOver = false;
    void Awake()
    {
        
        int level = PlayerPrefs.GetInt("Level",1);
        // Initialize
        currentTime = timeLimit+level*2.5f;
        maxAttractMagnets += (int)(maxAttractMagnets*level*0.1f); 
        maxRepelMagnets += (int)(maxRepelMagnets*level*0.1f); 
        maxTrapMagnets+=(int)(maxTrapMagnets*level*0.1f);
        maxParabolicMagnets+=(int)(maxParabolicMagnets*level*0.1f);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateTimerDisplay();
        magnetText.text = (maxAttractMagnets).ToString();
        magnetRepelText.text = (maxRepelMagnets).ToString();
        magnetTrapText.text = (maxTrapMagnets).ToString();
        magnetParabolicText.text = maxParabolicMagnets.ToString();
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
            magnetText.text = maxAttractMagnets.ToString();
            magnetRepelText.text = (maxRepelMagnets).ToString();
            magnetTrapText.text = (maxTrapMagnets).ToString();
            magnetParabolicText.text = maxParabolicMagnets.ToString();
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
        var pending = MagnetSpawnSelector.PendingSpawn;
        if (pending == MagnetSpawnSelector.PendingSpawnType.Trap)
            {
            return maxTrapMagnets>0;
            }
            else if (pending == MagnetSpawnSelector.PendingSpawnType.Attract)
            {
            return maxAttractMagnets>0;
            }
            else if(pending == MagnetSpawnSelector.PendingSpawnType.Repel)
            {
            return maxRepelMagnets>0;
            }
            else if(pending == MagnetSpawnSelector.PendingSpawnType.Parabolic)
            {
                return maxParabolicMagnets>0;
            }
        return false;
    }

    public void MagnetSpawned()
    {
        maxAttractMagnets--;
    }
    
    public void RepelMagnetSpawned()
    {
        maxRepelMagnets--;
    }

    public void TrapMagnetSpawned()
    {
        maxTrapMagnets--;
    }

    public void ParabolicMagnetSpawned()
    {
        maxParabolicMagnets--;
    }
}