using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ... [Keep your existing variables] ...
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float gravity = -9.81f;
    private CapsuleCollider capsuleCollider;

    [Header("Look Settings")]
    public Camera playerCamera;
    public float lookSensitivity = 100.0f;
    public float maxLookUp = 90.0f;
    public float maxLookDown = -90.0f;

    [Header("Reset Logic")]
    public EnemyController enemyToReset; // Note: Changed to EnemyController based on your provided scripts

    [Header("Ball Logic")]
    public GameObject ballProjectilePrefab; // The thrown version
    public Transform ballSpawnPoint;
    public bool hasBall = false; // Tracks inventory

    [Header("Audio Settings")]
    public AudioSource footstepSource; // The Looping one
    public AudioSource sfxSource;
    public AudioClip wallHitSound;

    public float footstepInterval = 0.5f; // How fast to play footsteps
    private float footstepTimer = 0;

    // [Keep references/private vars]
    private CharacterController controller;
    private PlayerControls playerControls;
    private Vector3 playerVelocity;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float cameraPitch = 0.0f;
    private bool noClipActive = false;
    private float wallHitTimer = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        capsuleCollider = GetComponentInChildren<CapsuleCollider>();
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Gameplay.Enable();
        playerControls.Gameplay.ToggleNoClip.performed += ToggleNoClip;
        playerControls.Gameplay.Reset.performed += ResetPlayerAndEnemy;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDisable()
    {
        playerControls.Gameplay.Disable();
        playerControls.Gameplay.ToggleNoClip.performed -= ToggleNoClip;
        playerControls.Gameplay.Reset.performed -= ResetPlayerAndEnemy;
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
        HandleThrow();
        if (wallHitTimer > 0)
        {
            wallHitTimer -= Time.deltaTime;
        }
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check collision and cooldown
        if (hit.gameObject.CompareTag("Wall") && !noClipActive && wallHitTimer <= 0)
        {
            // Use the SFX Source for the Thud
            // This source is never muted, so it always plays
            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(wallHitSound);
                wallHitTimer = 0.5f;
            }
        }
        if (hit.gameObject.CompareTag("Enemy"))
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.ResetGame();
            }
        }
    }

    // --- NEW: Method called by BallPickup ---
    public void EquipBall()
    {
        hasBall = true;
        Debug.Log("Player picked up the ball!");
    }

    private void HandleThrow()
    {
        // Check input AND inventory
        if (Input.GetKeyDown(KeyCode.Space) && hasBall)
        {
            // 1. Instinctive Aiming: Use the Camera's forward direction, not the body's
            Vector3 throwDirection = playerCamera.transform.forward;

            // 2. Instantiate at the Spawn Point (which should be parented to the Camera)
            GameObject ball = Instantiate(ballProjectilePrefab, ballSpawnPoint.position, Quaternion.identity);

            // 3. PHYSICS FIX: Ignore collision between Player and the new Ball
            // This prevents the ball from getting stuck inside you or instantly hitting you
            Collider ballCollider = ball.GetComponent<Collider>();

            // We need the player's collider (CapsuleCollider or CharacterController)
            // Note: CharacterController acts as a collider for this purpose
            if (ballCollider != null && controller != null)
            {
                Physics.IgnoreCollision(controller, ballCollider);
            }

            // If you have a separate child collider (like in your Awake method), ignore that too
            if (ballCollider != null && capsuleCollider != null)
            {
                Physics.IgnoreCollision(capsuleCollider, ballCollider);
            }

            // 4. Apply Force
            BallController ballCtrl = ball.GetComponent<BallController>();
            if (ballCtrl != null)
            {
                ballCtrl.Throw(throwDirection);
            }

            // 5. Remove from inventory
            hasBall = false;
        }
    }

    // ... [Rest of your Movement, Look, Reset code stays exactly the same] ...
    private void HandleMovement()
    {
        moveInput = playerControls.Gameplay.Move.ReadValue<Vector2>();

        if (noClipActive)
        {
            // --- NOCLIP MOVEMENT ---
            Vector3 move = transform.forward * moveInput.y + transform.right * moveInput.x;
            transform.position += move * moveSpeed * Time.deltaTime;
        }
        else
        {
            // --- NORMAL MOVEMENT ---
            if (controller.isGrounded && playerVelocity.y < 0) playerVelocity.y = -2f;

            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
            controller.Move(move * moveSpeed * Time.deltaTime);

            playerVelocity.y += gravity * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);

            // --- PASTE YOUR FOOTSTEP LOGIC HERE ---
            // We check 'isGrounded' so you don't play steps while jumping/falling

            bool isMoving = moveInput.magnitude > 0.1f && controller.isGrounded;

            // 2. Apply to the AudioSource
            if (footstepSource != null)
            {
                // If we are moving, Mute is FALSE (sound on).
                // If we stopped, Mute is TRUE (sound off).
                footstepSource.mute = !isMoving;
            }
        }
    }

    private void HandleLook()
    {
        lookInput = playerControls.Gameplay.Look.ReadValue<Vector2>();
        float mouseX = lookInput.x * lookSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * lookSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, maxLookDown, maxLookUp);
        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void ToggleNoClip(InputAction.CallbackContext context)
    {
        noClipActive = !noClipActive;
        controller.detectCollisions = !noClipActive;
        if (capsuleCollider != null) capsuleCollider.enabled = !noClipActive;
    }

    private void ResetPlayerAndEnemy(InputAction.CallbackContext context)
    {
        // Clean and modular!
        if (GameController.Instance != null)
        {
            GameController.Instance.ResetGame();
        }
    }
}