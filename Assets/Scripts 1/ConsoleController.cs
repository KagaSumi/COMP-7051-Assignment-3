using TMPro; // Remove if you're not using TextMeshPro
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConsoleController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject consoleUI;
    public TMP_InputField inputField; // Or UnityEngine.UI.InputField

    private bool isOpen = false;

    void Start()
    {
        consoleUI.SetActive(false);
    }

    void Update()
    {
        // Open console with C
        if (!isOpen && Input.GetKeyDown(KeyCode.C))
        {
            OpenConsole();
        }

        // Close console with Esc
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseConsole();
        }

        // Submit command with Enter
        if (isOpen && Input.GetKeyDown(KeyCode.Return))
        {
            string command = inputField.text;
            HandleCommand(command);
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    private void OpenConsole()
    {
        isOpen = true;
        consoleUI.SetActive(true);
        Time.timeScale = 0f; // Pause game
        inputField.ActivateInputField();
    }

    private void CloseConsole()
    {
        isOpen = false;
        consoleUI.SetActive(false);
        Time.timeScale = 1f; // Resume game
    }

    private void HandleCommand(string command)
    {
        switch (command.ToLower())
        {
            case "red":
                Camera.main.backgroundColor = Color.red;
                break;
            case "green":
                Camera.main.backgroundColor = Color.green;
                break;
            case "blue":
                Camera.main.backgroundColor = Color.blue;
                break;
            case "black":
                Camera.main.backgroundColor = Color.black;
                break;
            case "white":
                Camera.main.backgroundColor = Color.white;
                break;
            case "reset":
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ResetGame();
                    Debug.Log("Game reset via console command.");
                }
                else
                {
                    Debug.LogWarning("GameManager instance not found!");
                }
                break;
            case "exit":
                SceneManager.LoadScene("Pong_Main");
                break;

            default:
                Debug.LogWarning($"Unknown command: {command}");
                break;
        }
    }
}
