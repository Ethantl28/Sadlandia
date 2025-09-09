using UnityEditor.SceneManagement;
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

    public float stepHeight = 0.3f;
    public float stepCheckDistance = 0.5f;
    public float stepSmooth = 5.0f;

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
        
        if (jumpAction.WasPressedThisFrame() && isGrounded)
        {
            jumpQueued = true;
            animator.SetTrigger("Jump");
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        Move();
        HandleStepClimb();
        GravityMultiplier();
    }

    private void CheckGrounded()
    {
        //Is grounded check
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        bool hitGround = Physics.Raycast(origin, Vector3.down, groundCheckDistance + 0.1f, groundMask);

        if (hitGround)
        {
            lastGroundedTime = Time.time;
        }

        isGrounded = (Time.time - lastGroundedTime) <= coyoteTime;

        if (steppingUp) isGrounded = true;

        //Distance player is from ground check
        RaycastHit hit;
        Physics.Raycast(origin, Vector3.down, out hit, Mathf.Infinity, groundMask);
        groundDistance = hit.distance;
        animator.SetFloat("groundDistance", Mathf.Lerp(animator.GetFloat("groundDistance"), groundDistance, Time.deltaTime * 10.0f));
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
        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * gravityMultiplier, ForceMode.Acceleration);
        }
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

    private void HandleStepClimb()
    {
        Vector3 moveDir = new Vector3(rb.linearVelocity.x, 0.0f, rb.linearVelocity.z).normalized;
        if (moveDir.sqrMagnitude < 0.01f) return;

        Vector3 originLower = transform.position + Vector3.up * 0.05f;
        Vector3 originUpper = transform.position + Vector3.up * stepHeight;

        if (Physics.Raycast(originLower, moveDir, out RaycastHit lowerHit, stepCheckDistance, groundMask))
        {
            if (!Physics.Raycast(originUpper, moveDir, stepCheckDistance, groundMask))
            {
                steppingUp = true;
                rb.position += Vector3.up * stepSmooth * Time.fixedDeltaTime;
            }
            else
            {
                steppingUp = false;
            }
        }
        else
        {
            steppingUp = false;
        }

        Debug.DrawRay(originLower, moveDir * stepCheckDistance, Color.red);
        Debug.DrawRay(originUpper, moveDir * stepCheckDistance, Color.green);
    }
}
