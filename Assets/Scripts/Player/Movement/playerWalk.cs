using UnityEngine;
using UnityEngine.InputSystem;

public class playerWalk : MonoBehaviour
{
    public InputActionAsset InputActions;

    private InputAction m_moveAction;
    private InputAction m_lookAction;
    private InputAction m_jumpAction;

    private Vector2 m_moveAmt;
    private Vector2 m_lookAmt;
    private Animator m_animator;
    private Rigidbody m_rigidBody;

    public float walkSpeed = 5.0f;
    public float rotateSpeed = 5.0f;
    public float jumpSpeed = 5.0f;

    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        // Get actions from the asset you assigned in the Inspector
        var map = InputActions.FindActionMap("Player", true);
        m_moveAction = map.FindAction("Move", true);
        m_lookAction = map.FindAction("Look", true);
        m_jumpAction = map.FindAction("Jump", true);

        m_animator = GetComponent<Animator>();
        m_rigidBody = GetComponent<Rigidbody>();

        // Keep upright while using physics
        m_rigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        m_moveAmt = m_moveAction.ReadValue<Vector2>();
        m_lookAmt = m_lookAction.ReadValue<Vector2>();

        if (m_jumpAction.WasPressedThisFrame())
        {
            Jump();
        }
    }

    public void Jump() 
    {
        m_rigidBody.AddForceAtPosition(new Vector3(0.0f, 5.0f, 0.0f), Vector3.up, ForceMode.Impulse);
        //m_animator.SetTrigger("Jump");
    }



    private void FixedUpdate()
    {
        Walking();
        Rotating();
    }

    private void Walking()
    {
        //if (m_animator) m_animator.SetFloat("Speed", m_moveAmt.magnitude);

        Vector3 move = new Vector3(m_moveAmt.x, 0f, m_moveAmt.y);
        move = transform.TransformDirection(move); // turn input into world-space

        m_rigidBody.MovePosition(m_rigidBody.position + move * walkSpeed * Time.fixedDeltaTime);
    }


    private void Rotating()
    {
        float rotationAmtHorizontal = m_lookAmt.x * rotateSpeed * Time.fixedDeltaTime;
        float rotationAmtVertical = m_lookAmt.y * rotateSpeed * Time.fixedDeltaTime;
        if (Mathf.Abs(rotationAmtHorizontal) > 0.0001f)
        {
            Quaternion deltaRotation = Quaternion.Euler(rotationAmtVertical, rotationAmtHorizontal, 0f);
            m_rigidBody.MoveRotation(m_rigidBody.rotation * deltaRotation);
        }
    }

}
