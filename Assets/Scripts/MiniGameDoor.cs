using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MiniGameDoor : MonoBehaviour
{
    [Header("Settings")]
    public string pongSceneName = "Pong_Main"; // MATCH THIS TO YOUR EXACT SCENE NAME

    private bool isReady = false;

    IEnumerator Start()
    {
        // Wait 2 seconds before activating the door.
        // This prevents an infinite loop when you load back into the maze standing right here.
        yield return new WaitForSeconds(2.0f);
        isReady = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isReady) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("Entering Mini-Game...");

            // 1. SAVE the current maze state (Player pos, Seed, Score)
            // We use the SaveGame function you already wrote!
            if (GameController.Instance != null)
            {
                GameController.Instance.SaveGame();
            }

            // 2. Load the Pong Scene
            SceneManager.LoadScene(pongSceneName);
        }
    }
}