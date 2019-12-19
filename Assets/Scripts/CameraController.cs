using UnityEngine;

public class CameraController : MonoBehaviour {

    public float sensitivity = 1, mouseSensitivity = 5, targetDistacne = 2;
    public float pitchMin = -20, pitchMax = 85;
    public Vector3 target;

    public float rotationSmoothTime = 0.1f;
    Vector3 rotationSmoothVelocity, currentRotation;

    float yaw, pitch;

    void LateUpdate () {
        PositionCamera();
    }

    void PositionCamera()
    {
        if (Input.GetMouseButton(2))
        {
            yaw += Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (Input.GetMouseButtonUp(2))
        {         
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            yaw -= Input.GetAxisRaw("Horizontal") * sensitivity;
            pitch += Input.GetAxisRaw("Vertical") * sensitivity;
        }

        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        transform.position = target - transform.forward * targetDistacne;
    }
}