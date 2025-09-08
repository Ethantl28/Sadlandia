using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class cameraController : MonoBehaviour
{
    public Transform player;

    public float sensitivity = 150f;
    public float verticalClamp = 80.0f;
    public float distance = 3.5f;
    public float height = 1.5f;
    public float smoothTime = 0.1f;

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
        m_lookAmt = m_lookAction.ReadValue<Vector2>() * sensitivity * Time.deltaTime;

        yaw += m_lookAmt.x;
        pitch -= m_lookAmt.y;
        pitch = Mathf.Clamp(pitch, -verticalClamp, verticalClamp);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0.0f);

        Vector3 target = player.position + Vector3.up * height;

        Vector3 offset = rotation * new Vector3(0.0f, 0.0f, -distance);

        Vector3 desiredPosition = target + offset;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        transform.LookAt(target);
    }
}
