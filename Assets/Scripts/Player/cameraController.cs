using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class cameraController : MonoBehaviour
{
    public Transform player;

    public float mouseSensitivity = 150f;
    public float controllerSensitivity = 150f;
    public float verticalClampTop = 80.0f;
    public float verticalClampBottom = -55.0f;
    public float distance = 3.5f;
    public float height = 1.5f;
    public float smoothTime = 0.1f;

    public LayerMask collisionMask;
    public float cameraRadius = 0.2f;
    public float minDistance = 0.5f;

    public InputActionAsset InputActions;

    private InputAction m_lookAction;
    private Vector2 m_lookAmt;

    private float yaw;
    private float pitch;
    private Vector3 velocity;

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
        var map = InputActions.FindActionMap("Player", true);
        m_lookAction = map.FindAction("Look", true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        Vector2 lookInput = m_lookAction.ReadValue<Vector2>();

        if (Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero)
        {
            m_lookAmt = lookInput * mouseSensitivity * Time.deltaTime;
        }
        else
        {
            m_lookAmt = lookInput * controllerSensitivity;
        }

        yaw += m_lookAmt.x;
        pitch -= m_lookAmt.y;
        pitch = Mathf.Clamp(pitch, verticalClampBottom + 25.0f, verticalClampTop);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0.0f);
        Vector3 target = player.position + Vector3.up * height;

        Vector3 offset = rotation * new Vector3(0.0f, 0.0f, -distance);
        Vector3 desiredPosition = target + offset;

        if (Physics.SphereCast(target, cameraRadius, offset.normalized, out RaycastHit hit, distance, collisionMask))
        {
            float hitDist = Mathf.Max(hit.distance - cameraRadius, minDistance);
            desiredPosition = target + offset.normalized * hitDist;
        }

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        transform.LookAt(target);
    }
}
