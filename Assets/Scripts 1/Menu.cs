using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public GameModeData gameModeData; // assign in inspector

    public void StartPlayerVsPlayer()
    {
        gameModeData.selectedMode = GameModeData.Mode.PlayerVsPlayer;
        SceneManager.LoadScene("Pong_Game");
    }

    public void StartPlayerVsAI()
    {
        gameModeData.selectedMode = GameModeData.Mode.PlayerVsAI;
        SceneManager.LoadScene("Pong_Game");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");

    }
}
