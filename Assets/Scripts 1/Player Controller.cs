using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Paddle : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 20f;
    public Rigidbody rb;

    [Header("Player Settings")]
    [Tooltip("1 = Player1, 2 = Player2")]
    public int playerIndex = 1;

    [Header("AI Settings")]
    public bool isAI = false;         // Set true for AI paddle
    public Transform ball;             // Ball for AI tracking
    public float aiSpeed = 20f;        // AI movement speed
    public Vector3 startPos;
    public float reactionTime = 0.05f;  // AI reaction delay in seconds

    private Players playersInput;
    private InputAction movementAction;
    private float reactionTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        startPos = transform.position;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        if (GameManager.Instance != null )
            { 
            if (playerIndex == 2)
            {
                if (GameManager.Instance.currentMode == GameModeData.Mode.PlayerVsAI)
                {
                    isAI = true;
                }
            }
        }
        else
        {
            isAI = false;
        }
        if (!isAI)
        {
            // Initialize player input
            playersInput = new Players();
            movementAction = (playerIndex == 1) ? playersInput.Player1.Movement : playersInput.Player2.Movement;
            movementAction.Enable();
        }
    }

    private void FixedUpdate()
    {
        Vector3 move = Vector3.zero;

        if (!isAI)
        {
            // Player input (same as your original, responsive)
            Vector2 input = movementAction.ReadValue<Vector2>();
            move = new Vector3(input.x, 0, 0) * speed * Time.fixedDeltaTime;
        }
        else if (ball != null)
        {
            // AI follows ball with reaction delay
            reactionTimer += Time.fixedDeltaTime;
            if (reactionTimer >= reactionTime)
            {
                reactionTimer = 0f;

                float direction = ball.position.x - transform.position.x;
                direction = Mathf.Clamp(direction, -1f, 1f); // normalize direction
                move = new Vector3(direction * aiSpeed * Time.fixedDeltaTime, 0, 0);
            }
        }

        // Set velocity for snappy movement
        rb.linearVelocity = new Vector3(move.x / Time.fixedDeltaTime, 0, 0);
    }

    private void OnDestroy()
    {
        if (!isAI && movementAction != null)
            movementAction.Disable();
    }
}
