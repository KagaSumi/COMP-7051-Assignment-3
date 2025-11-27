using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

// Helper class to represent a single cell in the maze grid
public class MazeCell
{
    public bool visited = false;
    public bool wallNorth = true;
    public bool wallEast = true;
    public bool wallSouth = true;
    public bool wallWest = true;
}

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Dimensions")]
    [Range(5, 100)]
    public int width = 10;
    [Range(5, 100)]
    public int height = 10;

    [Header("Maze Prefabs & Materials")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public Transform mazeParent;
    public GameObject doorPrefab;

    // --- NEW: Slot for the pickup version of the ball ---
    public GameObject ballPickupPrefab;

    [Header("Object Placement")]
    public GameObject winZone;
    public GameObject enemyObject;
    public GameObject playerObject;

    [Header("AI Navigation")]
    public NavMeshSurface navMeshSurface;

    [Header("Wall Textures")]
    public Material northMaterial;
    public Material southMaterial;
    public Material eastMaterial;
    public Material westMaterial;
    public Material floorMaterial;

    [Header("Maze Generation Seed")]
    public int seed = 0;
    public bool useRandomSeed = true;

    private Vector2Int playerSpawnCoords;
    private MazeCell[,] mazeGrid;

    // Singleton instance to allow the ball to easily find the generator
    public static MazeGenerator Instance;

    void Awake()
    {
        Instance = this;
    }

    public int GetCurrentSeed()
    {
        return seed;
    }

    public void GenerateNewMaze()
    {
        ClearOldMaze();

        if (useRandomSeed)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }

        Random.InitState(seed);

        InitializeGrid();
        GenerateMaze(0, 0);
        DrawMaze();
        PositionPlayer();
        PositionEnemy();
        PositionWinZone();

        // --- NEW: Place the ball initially ---
        SpawnBallRandomly();

        if (navMeshSurface != null)
            navMeshSurface.BuildNavMesh();

        Debug.Log("Maze generated with seed: " + seed);
    }

    private void ClearOldMaze()
    {
        if (mazeParent == null) return;
        for (int i = mazeParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(mazeParent.GetChild(i).gameObject);
        }

        // Clean up any loose pickups that might exist
        var existingBalls = FindObjectsOfType<BallPickup>();
        foreach (var b in existingBalls) DestroyImmediate(b.gameObject);
    }

    private void InitializeGrid()
    {
        mazeGrid = new MazeCell[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                mazeGrid[x, y] = new MazeCell();
    }

    private void GenerateMaze(int x, int y)
    {
        mazeGrid[x, y].visited = true;

        while (true)
        {
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(x, y);
            if (neighbors.Count == 0)
                break;

            Vector2Int chosen = neighbors[Random.Range(0, neighbors.Count)];
            RemoveWall(x, y, chosen.x, chosen.y);
            GenerateMaze(chosen.x, chosen.y);
        }
    }

    private List<Vector2Int> GetUnvisitedNeighbors(int x, int y)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        if (y + 1 < height && !mazeGrid[x, y + 1].visited) neighbors.Add(new Vector2Int(x, y + 1));
        if (x + 1 < width && !mazeGrid[x + 1, y].visited) neighbors.Add(new Vector2Int(x + 1, y));
        if (y - 1 >= 0 && !mazeGrid[x, y - 1].visited) neighbors.Add(new Vector2Int(x, y - 1));
        if (x - 1 >= 0 && !mazeGrid[x - 1, y].visited) neighbors.Add(new Vector2Int(x - 1, y));
        return neighbors;
    }

    private void RemoveWall(int cx, int cy, int nx, int ny)
    {
        if (nx > cx) { mazeGrid[cx, cy].wallEast = false; mazeGrid[nx, ny].wallWest = false; }
        else if (nx < cx) { mazeGrid[cx, cy].wallWest = false; mazeGrid[nx, ny].wallEast = false; }
        else if (ny > cy) { mazeGrid[cx, cy].wallNorth = false; mazeGrid[nx, ny].wallSouth = false; }
        else if (ny < cy) { mazeGrid[cx, cy].wallSouth = false; mazeGrid[nx, ny].wallNorth = false; }
    }

    private void DrawMaze()
    {
        float wallHeight = 5f;
        float wallThickness = 0.5f;

        // 1. Create a list to track all created walls
        List<GameObject> allWalls = new List<GameObject>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Floor (Keep existing code)
                Vector3 floorPos = new Vector3(x * 10, 0, y * 10);
                GameObject floor = Instantiate(floorPrefab, floorPos, Quaternion.identity, mazeParent);
                floor.GetComponent<Renderer>().material = floorMaterial;

                // --- WALL GENERATION (Updated to add to list) ---

                // North Wall
                if (mazeGrid[x, y].wallNorth)
                {
                    Vector3 pos = new Vector3(x * 10, wallHeight / 2, y * 10 + 5 - wallThickness / 2);
                    GameObject w = Instantiate(wallPrefab, pos, Quaternion.identity, mazeParent);
                    w.transform.GetChild(0).GetComponent<Renderer>().material = northMaterial;
                    w.transform.GetChild(1).GetComponent<Renderer>().material = southMaterial;
                    allWalls.Add(w); // <--- Add to list
                }

                // East Wall
                if (mazeGrid[x, y].wallEast)
                {
                    Vector3 pos = new Vector3(x * 10 + 5 - wallThickness / 2, wallHeight / 2, y * 10);
                    GameObject w = Instantiate(wallPrefab, pos, Quaternion.Euler(0, 90, 0), mazeParent);
                    w.transform.GetChild(0).GetComponent<Renderer>().material = eastMaterial;
                    w.transform.GetChild(1).GetComponent<Renderer>().material = westMaterial;
                    allWalls.Add(w); // <--- Add to list
                }

                // South boundary (Keep existing logic, add to list)
                if (y == 0 && mazeGrid[x, y].wallSouth)
                {
                    Vector3 pos = new Vector3(x * 10, wallHeight / 2, y * 10 - 5 + wallThickness / 2);
                    GameObject w = Instantiate(wallPrefab, pos, Quaternion.identity, mazeParent);
                    w.transform.GetChild(0).GetComponent<Renderer>().material = southMaterial;
                    w.transform.GetChild(1).GetComponent<Renderer>().material = northMaterial;
                    allWalls.Add(w);
                }

                // West boundary (Keep existing logic, add to list)
                if (x == 0 && mazeGrid[x, y].wallWest)
                {
                    Vector3 pos = new Vector3(x * 10 - 5 + wallThickness / 2, wallHeight / 2, y * 10);
                    GameObject w = Instantiate(wallPrefab, pos, Quaternion.Euler(0, 90, 0), mazeParent);
                    w.transform.GetChild(0).GetComponent<Renderer>().material = westMaterial;
                    w.transform.GetChild(1).GetComponent<Renderer>().material = eastMaterial;
                    allWalls.Add(w);
                }
            }
        }

        // 2. SWAP ONE RANDOM WALL FOR A DOOR
        if (allWalls.Count > 0 && doorPrefab != null)
        {
            // Pick a random wall from the list
            int randomIndex = Random.Range(0, allWalls.Count);
            GameObject wallToRemove = allWalls[randomIndex];

            // Capture its position and rotation
            Vector3 doorPos = wallToRemove.transform.position;
            Quaternion doorRot = wallToRemove.transform.rotation;

            // Destroy the wall
            DestroyImmediate(wallToRemove);

            // Spawn the door in its place
            Instantiate(doorPrefab, doorPos, doorRot, mazeParent);
            Debug.Log("Door spawned at: " + doorPos);
        }
    }

    // Helper to keep code clean
    void SpawnWall(int x, int y, int xOff, int yOff, Material m1, Material m2, bool rot)
    {
        float wallHeight = 5f;
        Vector3 pos = new Vector3(x * 10 + (xOff * 4.75f), wallHeight / 2, y * 10 + (yOff * 4.75f));
        Quaternion q = rot ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
        GameObject wall = Instantiate(wallPrefab, pos, q, mazeParent);
        wall.transform.GetChild(0).GetComponent<Renderer>().material = m1;
        wall.transform.GetChild(1).GetComponent<Renderer>().material = m2;
    }

    private void PositionPlayer()
    {
        if (playerObject == null) return;
        int px = Random.Range(0, width);
        int py = Random.Range(0, height);
        playerSpawnCoords = new Vector2Int(px, py);
        Vector3 pos = new Vector3(px * 10, 1f, py * 10);

        var controller = playerObject.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;
        playerObject.transform.position = pos;
        if (controller != null) controller.enabled = true;
    }

    private void PositionEnemy()
    {
        if (enemyObject == null) return;
        int ex, ey;
        do { ex = Random.Range(0, width); ey = Random.Range(0, height); }
        while (ex == playerSpawnCoords.x && ey == playerSpawnCoords.y);

        Vector3 pos = new Vector3(ex * 10, 1f, ey * 10);
        var agent = enemyObject.GetComponent<NavMeshAgent>();
        if (agent != null) agent.Warp(pos);
        else enemyObject.transform.position = pos;
    }

    private void PositionWinZone()
    {
        if (winZone == null) return;
        winZone.transform.position = new Vector3((width - 1) * 10, 1f, (height - 1) * 10);
    }

    // --- NEW: Logic to spawn the ball anywhere ---
    public void SpawnBallRandomly()
    {
        if (ballPickupPrefab == null) return;

        int bx = Random.Range(0, width);
        int by = Random.Range(0, height);

        // Calculate position (slightly off ground so it doesn't clip)
        Vector3 pos = new Vector3(bx * 10, 0.5f, by * 10);

        Instantiate(ballPickupPrefab, pos, Quaternion.identity);
        Debug.Log($"Ball spawned at {bx}, {by}");
    }
}