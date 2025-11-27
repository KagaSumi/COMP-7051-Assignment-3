using UnityEngine;

public class BallController : MonoBehaviour
{
    public float throwForce = 500f;
    public AudioClip hitWallSound;
    public AudioClip hitFloorSound;
    public AudioClip hitEnemySound;

    // Assign the Ball PICKUP prefab here in inspector
    public GameObject ballPickupPrefab;

    private Rigidbody rb;
    private AudioSource audioSource;
    private bool isTransforming = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Throw(Vector3 direction)
    {
        rb.AddForce(direction.normalized * throwForce);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 1. Hit Enemy: Logic stays the same (Kill enemy, Respawn pickup elsewhere)
        if (collision.gameObject.CompareTag("Enemy"))
        {
            PlaySound(hitEnemySound);

            EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
            if (enemy != null) enemy.HitByBall();

            if (MazeGenerator.Instance != null) MazeGenerator.Instance.SpawnBallRandomly();

            Destroy(gameObject);
        }
        // 2. Hit Wall or Floor: DO NOT DESTROY IMMEDIATELY
        else if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Floor"))
        {
            // A. Play Sound
            AudioClip clip = collision.gameObject.CompareTag("Wall") ? hitWallSound : hitFloorSound;
            PlaySound(clip);

            // B. Do NOT destroy here. The Physics Material will handle the bounce automatically.

            // C. Start a timer to turn this bouncing ball back into a pickup
            if (!isTransforming)
            {
                StartCoroutine(TurnBackIntoPickup(7.0f)); // Wait 7 seconds
            }
        }
    }

    // New Coroutine to handle the "Settle" logic
    private System.Collections.IEnumerator TurnBackIntoPickup(float maxWaitTime)
    {
        isTransforming = true;

        // 1. Minimum Wait: Wait a tiny bit (e.g., 1 second) to allow initial bounces 
        // to happen so it doesn't turn into a pickup at the peak of a bounce 
        // where velocity momentarily hits 0.
        yield return new WaitForSeconds(1.0f);

        float timer = 0f;

        // 2. Velocity Check Loop
        // Run this loop as long as the ball is moving faster than 0.1 AND we haven't timed out.
        // We use sqrMagnitude because it is computationally faster than magnitude (no square root).
        while (rb.linearVelocity.sqrMagnitude > 0.01f && timer < maxWaitTime)
        {
            timer += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // 3. Transformation Logic (Ball has stopped OR timed out)
        if (ballPickupPrefab != null)
        {
            // Spawn the pickup at the current location
            Instantiate(ballPickupPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            // Note: Since we might destroy the object immediately, 
            // AudioSource.PlayClipAtPoint is safer to ensure sound finishes.
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}