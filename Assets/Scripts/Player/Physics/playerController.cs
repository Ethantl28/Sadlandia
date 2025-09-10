using UnityEngine;
using UnityEngine.InputSystem;

public class playerController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float sprintSpeed = 8.0f;
    public float rotationSpeed = 10.0f;
    public float acceleration = 10.0f;
    public float coyoteTime = 0.1f;
    public float gravityMultiplier = 1.0f;

    public float jumpForce = 5.0f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask;

    public Transform cameraTransform;
    public InputActionAsset InputActions;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    private Rigidbody rb;
    private Animator animator;
    private bool isGrounded;

    private float currentBlendSpeed;
    private float groundDistance;
    private float lastGroundedTime;

    private bool jumpQueued = false;
    private bool steppingUp = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        animator = GetComponent<Animator>();

        var map = InputActions.FindActionMap("Player", true);
        moveAction = map.FindAction("Move", true);
        jumpAction = map.FindAction("Jump", true);
        sprintAction = map.FindAction("Sprint", false);
    }

    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    private void Update()
    {
        CheckGrounded();
        
        if (jumpAction.WasPressedThisFrame() && isGrounded && !jumpQueued)
        {
            jumpQueued = true;
            animator.SetTrigger("Jump");
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        Move();
        GravityMultiplier();
    }

    private void CheckGrounded()
    {
        float radius = 0.25f;
        float checkHeight = 0.05f;
        Vector3 spherePos = transform.position + Vector3.up * checkHeight;

        bool hitGround = Physics.CheckSphere(spherePos, radius, groundMask);

        if (hitGround)
        {
            lastGroundedTime = Time.time;
        }

        isGrounded = (Time.time - lastGroundedTime) <= coyoteTime;

        Color col = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(spherePos, Vector3.up * 0.2f, col);

        if (Physics.Raycast(spherePos, Vector3.down, out RaycastHit hit, 2.0f, groundMask))
        {
            groundDistance = hit.distance;
            animator.SetFloat("groundDistance", Mathf.Lerp(animator.GetFloat("groundDistance"), groundDistance, Time.deltaTime * 10.0f));
        }
    }

    private void Move()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        Vector3 forward = cameraTransform.forward;
        forward.y = 0.0f;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0.0f;
        right.Normalize();

        Vector3 moveDir = (forward * input.y + right * input.x).normalized;

        float targetSpeed = (sprintAction != null && sprintAction.IsPressed()) ? sprintSpeed : moveSpeed;
        Vector3 targetVelocity = moveDir * targetSpeed;

        Vector3 horizontalCurrent = new Vector3(rb.linearVelocity.x, 0.0f, rb.linearVelocity.z);
        Vector3 horizontalTarget = new Vector3(targetVelocity.x, 0.0f, targetVelocity.z);

        Vector3 newHorizontal = Vector3.Lerp(horizontalCurrent, horizontalTarget, acceleration * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector3(newHorizontal.x, rb.linearVelocity.y, newHorizontal.z);

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    private void UpdateAnimator()
    {
        Vector3 horizontalVel = rb.linearVelocity;
        horizontalVel.y = 0.0f;

        float targetBlend = horizontalVel.magnitude / sprintSpeed;
        currentBlendSpeed = Mathf.Lerp(currentBlendSpeed, targetBlend, Time.deltaTime * acceleration);
        animator.SetFloat("Speed", currentBlendSpeed);

        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("verticalVelocity", rb.linearVelocity.y);
    }

    private void GravityMultiplier()
    {
        float extra = isGrounded ? 1.0f : 2.5f;
        rb.AddForce(Vector3.down * gravityMultiplier * extra, ForceMode.Acceleration);
    }

    public void Jump()
    {
        if (jumpQueued)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0.0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpQueued = false;
        }
    }
}
