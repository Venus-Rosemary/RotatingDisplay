using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelAutoroatation : Singleton<ModelAutoroatation>
{
    public GameObject currentObject;
    public float initialAngle;
    [SerializeField] private float rotationSpeed = 30f; // Y����ת�ٶȣ���/�룩

    void Start()
    {
        if (currentObject!=null)
        {
            // ���ó�ʼX����תΪ-30��
            currentObject.transform.localRotation = Quaternion.Euler(-initialAngle, 0f, 0f);
        }

    }

    void Update()
    {
        // ��Y�������ת
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    public void SetUpdateModel(GameObject gameObject)
    {
        currentObject = gameObject;
        // ���ó�ʼX����תΪ-30��
        currentObject.transform.localRotation = Quaternion.Euler(-initialAngle, 0f, 0f);
    }
}
