using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelAutoroatation : Singleton<ModelAutoroatation>
{
    public GameObject currentObject;
    public float initialAngle;
    [SerializeField] private float rotationSpeed = 30f; // Y轴自转速度（度/秒）

    void Start()
    {
        if (currentObject!=null)
        {
            // 设置初始X轴旋转为-30度
            currentObject.transform.localRotation = Quaternion.Euler(-initialAngle, 0f, 0f);
        }

    }

    void Update()
    {
        // 绕Y轴持续旋转
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    public void SetUpdateModel(GameObject gameObject)
    {
        currentObject = gameObject;
        // 设置初始X轴旋转为-30度
        currentObject.transform.localRotation = Quaternion.Euler(-initialAngle, 0f, 0f);
    }
}
