// EnemyAI.cs

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    public Transform playerTransform; // Reference to the player's transform
    public Animator animator;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private string speedAnimatorParam = "Speed"; // The name of the float parameter in your animator

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;

        // A check to make sure the player has been assigned
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is not assigned on the EnemyAI script!");
        }
    }

    void Update()
    {
        // If the player has been assigned, set its position as the destination
        if (playerTransform != null)
        {
            agent.SetDestination(playerTransform.position);
        }

        // Update the animator based on the agent's current speed
        if (animator != null)
        {
            UpdateAnimator();
        }
    }
    private void UpdateAnimator()
    {
        if (animator == null) return;

        // Get the agent's velocity in world space
        Vector3 worldVelocity = agent.velocity;

        // Convert the world velocity to the enemy's local space
        // This tells us how much we are moving forward (z) and sideways (x)
        Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);

        // Normalize the velocity values to a -1 to 1 range based on the agent's max speed
        float moveX = localVelocity.x / agent.speed;
        float moveY = localVelocity.z / agent.speed;

        // Send these values to the Animator
        animator.SetFloat("moveX", moveX, 0.1f, Time.deltaTime); // Using damp time for smooth transitions
        animator.SetFloat("moveY", moveY, 0.1f, Time.deltaTime);
    }
    // Public method that can be called to reset the enemy's position
    public void ResetPosition()
    {
        // Use agent.Warp to teleport an agent correctly on the NavMesh
        agent.Warp(startPosition);
    }
}