using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class LevelGenerator : MonoBehaviour
{
    public int width = 20;
    public int height = 10;
    [SerializeField, Range(0f, 1f)] private float obstacleDensity = 0.25f;
    [SerializeField] private bool ensurePath = true;
    public GameObject startPrefab;
    public GameObject endPrefab;
    public GameObject obstaclePrefab;
    public GameObject trapPrefab;
    public int randomSeed = 1;
    public float cellSize = 1f; // Size of each grid cell in world units

    private char[,] grid;
    private List<GameObject> gridObjects = new List<GameObject>();
    private int level = 1;

    void Start()
    {
        Random.InitState(randomSeed);
        level = PlayerPrefs.GetInt("Level",1);
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        // Clear previous level
        foreach (GameObject obj in gridObjects)
        {
            Destroy(obj);
        }
        gridObjects.Clear();

        // Initialize grid
        grid = new char[height, width];

        // Set start and end positions
        int startX = 0;
        int startY = height / 2;
        int endX = width - 1;
        int endY = height / 2;

        // Fill grid with empty spaces
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[y, x] = '.';
            }
        }
        grid[startY, startX] = 'S';
        grid[endY, endX] = 'E';

        int maxAttempts = 100;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            // Reset obstacles
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid[y, x] != 'S' && grid[y, x] != 'E')
                    {
                        grid[y, x] = '.';
                    }
                }
            }

            // Place random obstacles
            int numObstacles = (int)(width * height * (obstacleDensity+level/100f));
            List<Vector2Int> positions = new List<Vector2Int>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid[y, x] != 'S' && grid[y, x] != 'E')
                    {
                        positions.Add(new Vector2Int(x, y));
                    }
                }
            }
            positions = positions.OrderBy(p => Random.value).ToList();
            for (int i = 0; i < Mathf.Min(numObstacles, positions.Count); i++)
            {
                grid[positions[i].y, positions[i].x] = 'O';
            }

            if (!ensurePath || BFSPathExists(startX, startY, endX, endY))
            {
                break;
            }
            attempts++;
        }

        if (attempts >= maxAttempts)
        {
            Debug.LogError("Could not generate a level with a valid path.");
            return;
        }

        // Spawn GameObjects
        SpawnGrid();
    }

    private bool BFSPathExists(int startX, int startY, int endX, int endY)
    {
        bool[,] visited = new bool[height, width];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startY, startX] = true;

        int[] dx = { 0, 1, 0, -1 }; // right, down, left, up
        int[] dy = { 1, 0, -1, 0 };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current.x == endX && current.y == endY)
            {
                return true;
            }

            for (int i = 0; i < 4; i++)
            {
                int nx = current.x + dx[i];
                int ny = current.y + dy[i];

                if (nx >= 0 && nx < width && ny >= 0 && ny < height && !visited[ny, nx] && grid[ny, nx] != 'O')
                {
                    visited[ny, nx] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }
        return false;
    }

    private void SpawnGrid()
    {
        // Calculate grid offset to center it at origin
        Vector2 offset = new Vector2(-(width * cellSize) / 2f + cellSize / 2f, -(height * cellSize) / 2f + cellSize / 2f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 position = new Vector2(x * cellSize, y * cellSize) + offset;
                GameObject prefab = null;

                switch (grid[y, x])
                {
                    case 'S':
                        prefab = startPrefab;
                        GameObject.FindObjectOfType<LevelBootstrap>().SpawnBall(position);
                        break;
                    case 'E':
                        endPrefab.transform.position = position;
                        //prefab = endPrefab;
                        break;
                    case 'O':
                        prefab = Random.Range(0,2)>0?obstaclePrefab:trapPrefab;
                        break;
                }

                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab, position, Quaternion.identity, transform);
                    gridObjects.Add(obj);
                }
            }
        }
    }

    // Optional: Regenerate level on key press (e.g., for testing)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateLevel();
        }
    }
}