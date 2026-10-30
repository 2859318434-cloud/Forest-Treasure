using UnityEngine;
using UnityEngine.EventSystems;
public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 2f, 0f);
    public float mouseSensitivity = 2f;
    public float minPitch = -30f;
    public float maxPitch = 60f;
    private float yaw;
    private float pitch = 10f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        if (Input.GetMouseButtonDown(0) &&
            Cursor.lockState == CursorLockMode.None &&
            (EventSystem.current == null ||
             !EventSystem.current.IsPointerOverGameObject()))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        transform.position = player.position + offset;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
