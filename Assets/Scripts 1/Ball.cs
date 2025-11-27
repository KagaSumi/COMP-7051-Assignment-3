using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class Ball : MonoBehaviour
{
    public float startingSpeed = 10f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Rigidbody settings
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Random initial direction
        bool isRight = UnityEngine.Random.value > 0.5f;
        float zVelocity = isRight ? 1f : -1f;
        float xVelocity = UnityEngine.Random.Range(-0.5f, 0.5f);

        rb.linearVelocity = new Vector3(xVelocity, 0, zVelocity).normalized * startingSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Optional: tweak velocity to prevent flat angles
        Vector3 vel = rb.linearVelocity;
        if (Mathf.Abs(vel.z) < 1f)
            vel.z = Mathf.Sign(vel.z) * 1f;
        rb.linearVelocity = vel.normalized * startingSpeed;

        // Goal detection (if using Colliders instead of Triggers)
        if (collision.gameObject.name == "AI Goal")
        {
            Debug.Log("Player Scores!");
            ResetBall(-1);
        }
        else if (collision.gameObject.name == "Player Goal")
        {
            Debug.Log("AI Scores!");
            ResetBall(1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "AI Goal")
        {
            GameManager.Instance.ScorePoint("Player");
        }
        else if (other.gameObject.name == "Player Goal")
        {
            GameManager.Instance.ScorePoint("AI");
        }
    }


    public void ResetBall(int direction)
    {
        // Reset position to center
        transform.position = Vector3.zero;

        // Randomize X again
        float xVelocity = UnityEngine.Random.Range(-0.5f, 0.5f);

        // Launch towards scoring player (AI = +1, Player = -1)
        rb.linearVelocity = new Vector3(xVelocity, 0, direction).normalized * startingSpeed;
    }
}
