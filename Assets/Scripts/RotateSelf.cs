using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSelf : MonoBehaviour
{
    // ��ת�ٶȣ���λ����/�룩
    public Vector3 rotationSpeed = new Vector3(0, 30, 0);

    void Update()
    {
        // ����ʱ������������ת�Ƕ�
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
