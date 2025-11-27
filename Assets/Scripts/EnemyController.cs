using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Needed for NavMeshAgent

public class EnemyController : MonoBehaviour
{
    public int hitsToDie = 3;
    private int currentHits = 0;

    public AudioClip deathSound;
    public AudioClip respawnSound;

    private AudioSource audioSource;
    private NavMeshAgent agent;
    private Renderer rend;
    private Collider col;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        rend = GetComponentInChildren<Renderer>();
        col = GetComponent<Collider>();
    }

    public void HitByBall()
    {
        // 1. Update Score immediately on hit
        if (GameController.Instance != null)
        {
            GameController.Instance.AddScore(1);
        }

        currentHits++;

        // Check for death
        if (currentHits >= hitsToDie)
        {
            Die();
        }
    }

    private void Die()
    {
        if (deathSound) audioSource.PlayOneShot(deathSound);

        // Hide the enemy without destroying it (so we can respawn it)
        rend.enabled = false;
        col.enabled = false;

        // Stop moving while dead
        if (agent != null) agent.isStopped = true;

        currentHits = 0; // Reset hits for next life

        // Start the 5-second timer
        StartCoroutine(RespawnAfterDelay(5f));
    }
   
    private IEnumerator RespawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // CALCULATE RANDOM POSITION
        // We access the MazeGenerator to get the bounds (width/height)
        if (MazeGenerator.Instance != null)
        {
            int w = MazeGenerator.Instance.width;
            int h = MazeGenerator.Instance.height;

            int rx = Random.Range(0, w);
            int ry = Random.Range(0, h);

            // Multiply by 10 because your maze generation uses x*10 spacing
            Vector3 randomPos = new Vector3(rx * 10, 0, ry * 10);

            // Warp the agent to the new spot
            if (agent != null)
            {
                agent.Warp(randomPos);
                agent.isStopped = false;
            }
            else
            {
                transform.position = randomPos;
            }
        }

        // Re-enable visuals and collision
        rend.enabled = true;
        col.enabled = true;

        if (respawnSound) audioSource.PlayOneShot(respawnSound);
        Debug.Log("Enemy Respawned!");
    }
}