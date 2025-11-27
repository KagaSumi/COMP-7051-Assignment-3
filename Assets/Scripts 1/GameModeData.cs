using UnityEngine;

[CreateAssetMenu(fileName = "GameModeData", menuName = "Game/GameModeData")]
public class GameModeData : ScriptableObject
{
    // Define the available game modes here
    public enum Mode
    {
        PlayerVsPlayer,
        PlayerVsAI
    }

    [Header("Selected Mode")]
    public Mode selectedMode = Mode.PlayerVsAI; // default to Player vs AI
}
