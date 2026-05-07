using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerMovement2D : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Horizontal Movement")]
    [SerializeField] private float maxMoveSpeed = 7f;
    [SerializeField] private float accelerationForce = 45f;
    [SerializeField] private float airAccelerationMultiplier = 0.55f;

    [Header("Damping / Inertia Feel")]
    [SerializeField] private float movingLinearDamping = 1.2f;
    [SerializeField] private float stoppingLinearDamping = 5.5f;
    [SerializeField] private float airLinearDamping = 0.4f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 11f;
    [SerializeField] private bool resetVerticalVelocityOnJump = true;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.28f;
    [SerializeField] private float groundCheckDistance = 0.08f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;

    private float moveInput;
    private bool jumpRequested;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        moveAction?.action.Enable();
        jumpAction?.action.Enable();
    }

    private void OnDisable()
    {
        moveAction?.action.Disable();
        jumpAction?.action.Disable();
    }

    private void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        ApplyDamping();
        ApplyHorizontalMovement();
        LimitHorizontalSpeed();
        ApplyJump();
    }

    private void ReadInput()
    {
        if (moveAction != null)
        {
            Vector2 moveValue = moveAction.action.ReadValue<Vector2>();
            moveInput = moveValue.x;
        }
        else
        {
            moveInput = 0f;
        }

        if (jumpAction != null && jumpAction.action.WasPressedThisFrame())
        {
            jumpRequested = true;
        }
    }

    private void CheckGrounded()
    {
        Vector2 origin = groundCheckPoint != null
            ? groundCheckPoint.position
            : transform.position;

        RaycastHit2D hit = Physics2D.CircleCast(
            origin,
            groundCheckRadius,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        isGrounded = hit.collider != null;
    }

    private void ApplyDamping()
    {
        if (!isGrounded)
        {
            rb.linearDamping = airLinearDamping;
            return;
        }

        bool playerIsTryingToMove = Mathf.Abs(moveInput) > 0.01f;

        rb.linearDamping = playerIsTryingToMove
            ? movingLinearDamping
            : stoppingLinearDamping;
    }

    private void ApplyHorizontalMovement()
    {
        if (Mathf.Abs(moveInput) <= 0.01f)
            return;

        float acceleration = isGrounded
            ? accelerationForce
            : accelerationForce * airAccelerationMultiplier;

        rb.AddForce(Vector2.right * moveInput * acceleration, ForceMode2D.Force);
    }

    private void LimitHorizontalSpeed()
    {
        Vector2 velocity = rb.linearVelocity;

        if (Mathf.Abs(velocity.x) > maxMoveSpeed)
        {
            velocity.x = Mathf.Sign(velocity.x) * maxMoveSpeed;
            rb.linearVelocity = velocity;
        }
    }

    private void ApplyJump()
    {
        if (!jumpRequested)
            return;

        jumpRequested = false;

        if (!isGrounded)
            return;

        if (resetVerticalVelocityOnJump)
        {
            Vector2 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;
        }

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = groundCheckPoint != null
            ? groundCheckPoint.position
            : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origin + Vector2.down * groundCheckDistance, groundCheckRadius);
    }
}