using UnityEngine;

public class ModelRotationController : MonoBehaviour
{
    public float rotationAngle = 90f;
    public float rotationSpeed = 360f; // 每秒旋转的角度速度

    private bool isRotating = false;
    private Quaternion targetRotation;

    void Update()
    {
        if (isRotating)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.01f)
            {
                transform.rotation = targetRotation;
                isRotating = false;
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            SetTargetRotation(Vector3.right, rotationAngle);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            SetTargetRotation(Vector3.right, -rotationAngle);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            SetTargetRotation(Vector3.up, -rotationAngle);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            SetTargetRotation(Vector3.up, rotationAngle);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            SetTargetRotation(Vector3.forward, rotationAngle);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            SetTargetRotation(Vector3.forward, -rotationAngle);
        }
    }

    void SetTargetRotation(Vector3 axis, float angle)
    {
        Quaternion deltaRotation = Quaternion.AngleAxis(angle, axis);
        targetRotation = deltaRotation * transform.rotation;
        isRotating = true;
    }
}