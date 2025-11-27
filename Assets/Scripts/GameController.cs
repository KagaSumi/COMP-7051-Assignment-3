using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    public MazeGenerator mazeGenerator;
    public Transform player;
    public Transform enemy;

    [Header("UI Settings")]
    public TextMeshProUGUI scoreText;
    // NOTE: This reference might become null if the player picks up the ball!
    public Transform ball;
    public GameObject ballPickupPrefab; // Need this reference to respawn it if needed
    public VisualEffectsController visualEffects;
    public MusicManager musicManager;
    public int score = 0;
    private string savePath;

    void Awake()
    {
        if (Instance == null) Instance = this;
        savePath = Application.persistentDataPath + "/gameData.json";

        if (!File.Exists(savePath))
        {
            mazeGenerator.useRandomSeed = true;
            mazeGenerator.GenerateNewMaze();
            UpdateScoreUI();
        }
        else
        {
            LoadGame();
        }
    }
    public void ResetGame()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }

        // 2. Reload the Scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // 3. Reset Time
        Time.timeScale = 1;
    }
    void OnApplicationQuit() => SaveGame();
    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    // -----------------------------
    // SAVING
    // -----------------------------
    public void SaveGame()
    {
        int seed = mazeGenerator.GetCurrentSeed();
        PlayerController pc = player.GetComponent<PlayerController>();
        bool playerHasBall = pc != null && pc.hasBall;

        Vector3 currentBallPos = Vector3.zero;
        if (!playerHasBall)
        {
            if (ball != null) currentBallPos = ball.position;
            else
            {
                GameObject existingBall = GameObject.FindGameObjectWithTag("Pickup");
                if (existingBall != null) currentBallPos = existingBall.transform.position;
            }
        }

        // --- CAPTURE SETTINGS ---
        bool night = visualEffects != null ? visualEffects.isNight : false;
        bool fog = visualEffects != null ? visualEffects.isFoggy : false;
        bool flash = visualEffects != null ? visualEffects.isFlashlightOn : false;
        bool music = musicManager != null ? musicManager.isMusicPlaying : true;

        GameData data = new GameData(
            seed, player.position, enemy.position, currentBallPos, score, playerHasBall,
            night, fog, flash, music // <--- Pass new values
        );

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game Saved with Settings!");
    }

    // -----------------------------
    // LOADING
    // -----------------------------
    public void LoadGame()
    {
        if (!File.Exists(savePath)) return;
        string json = File.ReadAllText(savePath);
        GameData data = JsonUtility.FromJson<GameData>(json);

        // 1. Restore Game Logic (Maze, Pos, Score)
        mazeGenerator.useRandomSeed = false;
        mazeGenerator.seed = data.mazeSeed;
        mazeGenerator.GenerateNewMaze();
        score = data.score;
        UpdateScoreUI();

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        player.position = data.playerPos;
        if (cc) cc.enabled = true;

        var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent) agent.Warp(data.enemyPos);
        else enemy.position = data.enemyPos;

        // 2. Restore Ball Logic
        PlayerController pc = player.GetComponent<PlayerController>();
        GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
        foreach (var b in pickups) Destroy(b);

        if (data.hasBall) { pc.hasBall = true; }
        else
        {
            pc.hasBall = false;
            if (ballPickupPrefab != null)
            {
                GameObject newBall = Instantiate(ballPickupPrefab, data.ballPos, Quaternion.identity);
                ball = newBall.transform;
            }
        }

        // --- 3. RESTORE VISUALS & MUSIC ---
        if (visualEffects != null)
        {
            visualEffects.isNight = data.isNight;
            visualEffects.isFoggy = data.isFoggy;
            visualEffects.isFlashlightOn = data.isFlashlightOn;

            // Force the shader to update NOW so we don't see a flash of wrong lighting
            visualEffects.UpdateShaderState();
        }

        if (musicManager != null)
        {
            musicManager.SetMusicState(data.isMusicPlaying);
        }
    }
}
