using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelInDisplayBox : Singleton<ModelInDisplayBox>
{
    public List<GameObject> objects = new List<GameObject>();
    //public Transform spawnPoint; // 生成位置点
    
    [Header("随机设置")]
    [Range(0, 360)]
    public float maxRotationAngle = 360f; // 最大旋转角度
    public bool enableMirror = true; // 是否启用镜像

    [Header("记录当前模型")]
    public GameObject currentModel;
    private int lastRandomIndex = -1; // 添加这行来记录上一次的索引

    private void Start()
    {
        int randomIndex = Random.Range(0, objects.Count);
        currentModel = objects[randomIndex];
    }


    public GameObject SpawnModelOfBox(Transform spawnPoint, float desiredSize)
    {
        if (objects.Count == 0 || spawnPoint == null) return null;

        // 随机选择一个不同的模型
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, objects.Count);
        } while (randomIndex == lastRandomIndex && objects.Count > 1);

        lastRandomIndex = randomIndex;
        GameObject selectedPrefab = objects[randomIndex];

        currentModel = selectedPrefab;

        // 生成模型
        GameObject newObject = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);
        newObject.transform.SetParent(spawnPoint);

        // 设置统一大小
        newObject.transform.localScale = Vector3.one;
        ModelUtils.FitToBounds(newObject.transform, desiredSize);

        //// 随机旋转
        //float randomRotation = Random.Range(0f, maxRotationAngle);
        //newObject.transform.Rotate(0, 0, randomRotation, Space.Self);

        //// 随机镜像
        //if (enableMirror && Random.value > 0.5f)
        //{
        //    //newObject.transform.localScale = new Vector3(-1, 1, 1);
        //}

        return newObject;
    }


    public GameObject SpawnModelOfBelt(Transform spawnPoint, float desiredSize, bool isRandom)
    {
        if (objects.Count == 0 || spawnPoint == null) return null;

        // 随机选择一个模型
        int randomIndex = Random.Range(0, objects.Count);
        GameObject selectedPrefab = objects[randomIndex];

        if (isRandom)
        {
            selectedPrefab = objects[randomIndex];
        }
        else
        {
            selectedPrefab = currentModel;
        }

        // 生成模型
        GameObject newObject = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);
        newObject.transform.SetParent(spawnPoint);

        // 设置统一大小
        newObject.transform.localScale = Vector3.one;
        ModelUtils.FitToBounds(newObject.transform, desiredSize);

        // 随机旋转
        float randomRotation = Random.Range(0f, maxRotationAngle);
        newObject.transform.Rotate(0, 0, randomRotation, Space.Self);

        // 随机镜像
        if (enableMirror && Random.value > 0.5f)
        {
            //newObject.transform.localScale = new Vector3(-1, 1, 1);
        }

        return newObject;
    }

    public GameObject SpawnModelOfBelt(Transform spawnPoint, float desiredSize)
    {
        if (objects.Count == 0 || spawnPoint == null) return null;

        // 选择当前模型
        GameObject selectedPrefab = currentModel;

        // 生成模型
        GameObject newObject = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);
        newObject.transform.SetParent(spawnPoint);

        // 设置统一大小
        newObject.transform.localScale = Vector3.one;
        ModelUtils.FitToBounds(newObject.transform, desiredSize);

        // 设置旋转：先旋转180度，再随机旋转
        newObject.transform.rotation = Quaternion.Euler(0, 180f, 0);
        float randomRotation = Random.Range(0f, maxRotationAngle);
        newObject.transform.Rotate(0, 0, randomRotation, Space.Self);

#if UNITY_EDITOR

        // 设置模型颜色为黄色
        Renderer[] renderers = newObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                material.color = Color.yellow;
            }
        }
#endif
        return newObject;
    }

}

public static class ModelUtils
{
    public static void FitToBounds(Transform target, float desiredSize)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null) return;

        Bounds bounds = renderer.bounds;
        float currentSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float scale = desiredSize / currentSize;
        target.localScale = Vector3.one * scale;
    }
}
