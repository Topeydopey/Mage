using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerMovement2D : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private bool cancelVerticalVelocityBeforeJump = true;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.45f;
    [SerializeField] private float groundCheckDistance = 0.1f;
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
        ReadMovementInput();
        ReadJumpInput();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        ApplyMovement();
        ApplyJump();
    }

    private void ReadMovementInput()
    {
        if (moveAction == null)
        {
            moveInput = 0f;
            return;
        }

        Vector2 inputValue = moveAction.action.ReadValue<Vector2>();
        moveInput = inputValue.x;
    }

    private void ReadJumpInput()
    {
        if (jumpAction == null)
            return;

        if (jumpAction.action.WasPressedThisFrame())
            jumpRequested = true;
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

    private void ApplyMovement()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveInput * moveSpeed;
        rb.linearVelocity = velocity;
    }

    private void ApplyJump()
    {
        if (!jumpRequested)
            return;

        jumpRequested = false;

        if (!isGrounded)
            return;

        if (cancelVerticalVelocityBeforeJump)
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