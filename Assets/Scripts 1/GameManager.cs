using TMPro; // <-- Needed for TMP
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton

    [Header("Settings")]
    public GameModeData gameModeData; // assign in inspector
    public float aiSpeed = 8f;         // AI paddle movement speed

    // Always read from GameModeData
    public GameModeData.Mode currentMode => gameModeData != null ? gameModeData.selectedMode : GameModeData.Mode.PlayerVsPlayer;

    [Header("References (Gameplay Scene)")]
    public Ball ball;
    public Transform playerPaddle;
    public Transform aiPaddle;

    [Header("UI")]
    public TMP_Text scoreText; // assign your TMP text in the Inspector

    private Vector3 playerStartPos;
    private Vector3 aiStartPos;

    private int playerScore = 0;
    private int aiScore = 0;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (currentMode == GameModeData.Mode.PlayerVsAI && aiPaddle != null && ball != null)
        {
            MoveAIPaddle();
        }
    }

    private void MoveAIPaddle()
    {
        Vector3 targetPos = new Vector3(aiPaddle.position.x, ball.transform.position.y, aiPaddle.position.z);
        aiPaddle.position = Vector3.MoveTowards(aiPaddle.position, targetPos, aiSpeed * Time.deltaTime);
    }

    public void ScorePoint(string scorer)
    {
        if (scorer == "Player")
        {
            playerScore++;
            Debug.Log("Player Scores! Total: " + playerScore);
            if (ball != null) ball.ResetBall(-1);
        }
        else if (scorer == "AI")
        {
            aiScore++;
            Debug.Log("AI Scores! Total: " + aiScore);
            if (ball != null) ball.ResetBall(1);
        }

        UpdateScoreUI();
        CheckWinner();
        ResetPaddles();
    }
    private void CheckWinner()
    {
        int winningScore = 5;

        if (playerScore >= winningScore)
        {
            Debug.Log("Player 1 Wins the Game!");
            EndGame("Player 1");
        }
        else if (aiScore >= winningScore)
        {
            Debug.Log("Player2/AI Wins the Game!");
            EndGame(currentMode == GameModeData.Mode.PlayerVsPlayer ? "Player 2" : "AI");
        }
    }

    // Example EndGame function
    private void EndGame(string winner)
    {
        // Update the TMP text to show the winner
        if (scoreText != null)
        {
            scoreText.text = $"{winner} Wins!";
        }

        // Stop the ball so gameplay ends
        if (ball != null)
            ball.gameObject.SetActive(false);

        // Optionally, you can disable paddles too
        if (playerPaddle != null) playerPaddle.gameObject.SetActive(false);
        if (aiPaddle != null) aiPaddle.gameObject.SetActive(false);

        Debug.Log($"{winner} is the winner!");

    }
    public void ResetGame()
    {
        Debug.Log("Resetting game and returning to Main Menu...");

        // Reset scores
        playerScore = 0;
        aiScore = 0;

        // Reset UI if needed
        if (scoreText != null)
        {
            scoreText.text = "Score: 0 - 0";
        }

        // Reactivate ball and paddles if they were disabled
        if (ball != null) ball.gameObject.SetActive(true);
        if (playerPaddle != null) playerPaddle.gameObject.SetActive(true);
        if (aiPaddle != null) aiPaddle.gameObject.SetActive(true);

        // Optional: reset their positions
        ResetPaddles();

        // Load the Main Menu scene
        SceneManager.LoadScene("Main Menu");

        // Resume time (in case game was paused)
        Time.timeScale = 1f;
    }


    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {playerScore} - {aiScore}";
        }
    }

    private void ResetPaddles()
    {
        if (playerPaddle != null && aiPaddle != null)
        {
            playerPaddle.position = playerPaddle.GetComponent<Paddle>().startPos;
            aiPaddle.position = aiPaddle.GetComponent<Paddle>().startPos;


            playerPaddle.rotation = Quaternion.identity;
            aiPaddle.rotation = Quaternion.identity;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ball = FindObjectOfType<Ball>();
        playerPaddle = GameObject.Find("Player")?.transform;
        aiPaddle = GameObject.Find("Enemy")?.transform;

        if (playerPaddle != null && aiPaddle != null)
        {
            playerStartPos = playerPaddle.position;
            aiStartPos = aiPaddle.position;
        }

        // Optional: Find the score text automatically if not assigned
        if (scoreText == null)
        {
            scoreText = GameObject.Find("Score")?.GetComponent<TMP_Text>();
        }

        UpdateScoreUI();
    }

    public static void EnsureExists()
    {
        if (Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }
    }
}
