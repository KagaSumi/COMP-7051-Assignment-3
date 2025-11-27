using UnityEngine;

[System.Serializable]
public class GameData
{
    // Existing
    public int mazeSeed;
    public Vector3 playerPos;
    public Vector3 enemyPos;
    public Vector3 ballPos;
    public int score;
    public bool hasBall;

    // --- NEW: Settings State ---
    public bool isNight;
    public bool isFoggy;
    public bool isFlashlightOn;
    public bool isMusicPlaying;

    public GameData(int seed, Vector3 player, Vector3 enemy, Vector3 ball, int currentScore, bool holdingBall,
                    bool night, bool fog, bool flashlight, bool music)
    {
        mazeSeed = seed;
        playerPos = player;
        enemyPos = enemy;
        ballPos = ball;
        score = currentScore;
        hasBall = holdingBall;

        // Save new values
        isNight = night;
        isFoggy = fog;
        isFlashlightOn = flashlight;
        isMusicPlaying = music;
    }
}