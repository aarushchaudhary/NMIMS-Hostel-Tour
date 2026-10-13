using UnityEngine;

public class LimitedLook : MonoBehaviour {
    public float sensitivity = 2f;
    public float touchSensitivity = 0.15f;
    public float maxYaw = 55f;
    public float maxPitch = 15f;
    float yaw, pitch;

    void Update() {
        if (Input.touchCount > 0) {
            Vector2 d = Input.GetTouch(0).deltaPosition;
            yaw   -= d.x * touchSensitivity;
            pitch += d.y * touchSensitivity;
        } else if (Input.GetMouseButton(0)) {
            yaw   += Input.GetAxis("Mouse X") * sensitivity;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        }

        yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
