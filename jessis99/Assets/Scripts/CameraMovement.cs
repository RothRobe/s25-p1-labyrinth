using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;       // Der Ball
    public float height = 3f;     // Abstand nach oben

    void LateUpdate()
    {
        if (target == null) return;

        // Positioniere die Kamera über dem Ziel
        transform.position = target.position + Vector3.up * height - new Vector3(0,0,-2);

        // Schaue immer auf das Ziel
        transform.LookAt(target.position);
    }
}