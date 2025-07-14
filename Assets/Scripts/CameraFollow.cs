using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player; // Tham chiếu đến nhân vật
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10); // Offset từ nhân vật
    [SerializeField] private float smoothSpeed = 0.125f; // Tốc độ mượt mà (nếu muốn lerp)

    void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogWarning("Player not assigned to CameraFollow!");
            return;
        }

        // Vị trí mục tiêu của camera
        Vector3 desiredPosition = player.position + offset;

        // Di chuyển trực tiếp
        // transform.position = desiredPosition;

        // Hoặc di chuyển mượt mà với Lerp
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}