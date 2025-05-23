using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class TitleSetting
{
    public int titleIndex;

    [Header("模型配置")]
    [Tooltip("正确模型")]
    public GameObject correctModel;
    [Tooltip("干扰模型1")]
    public GameObject InterferenceModelOne;
    [Tooltip("干扰模型2")]
    public GameObject InterferenceModelTwo;

    [Header("按钮图片配置")]
    public Sprite CM;
    public Sprite IMO;
    public Sprite IMT;
}

public class ModelMatchController : Singleton<ModelMatchController>
{
    [Header("模型投影设置")]

    public Transform modelPoint;
    public Material targetMaterial;
    public List<Transform> cameraPreset = new List<Transform>();
    public List<GameObject> cameraObject = new List<GameObject>();

    public List<TitleSetting> titleSettings = new List<TitleSetting>();

    [Header("按钮展示模型")]
    public Transform sceneModelPoint;
    public GameObject currentSuccessObject;
    public GameObject oneObject;
    public GameObject twoObject;
    public GameObject threeObject;
    public List<GameObject> buttons = new List<GameObject>();
    private Dictionary<GameObject, GameObject> buttonModelMap = new Dictionary<GameObject, GameObject>();

    [Header("游戏设置")]
    public float judgementTime = 15f; // 每组判断时间
    public float switchDelay = 2f; // 切换延迟时间
    private float currentTimer = 0f;
    private int currentTitleIndex = 0;
    private bool isJudging = false;

    [Header("得分设置")]
    public int score = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isJudging)
        {
            currentTimer += Time.deltaTime;

            // 更新倒计时UI
            UIManager.Instance.UpdateThirdTimer(judgementTime - currentTimer);

            if (currentTimer >= judgementTime)
            {

                UIManager.Instance.UpdateThirdTimer(0);
                SetAddScoreThird(-10);
                NextTitle();
            }
        }
    }

    #region 15秒后切换和提前切换
    public void StartThreeGame()
    {
        currentTitleIndex = 0;
        score = 0;
        UIManager.Instance.UpdateThirdScore(score, 0);
        ShuffleModelList();
        StartNewTitle();
    }

    //打乱顺序
    private void ShuffleModelList()
    {
        // Fisher-Yates 洗牌算法
        for (int i = titleSettings.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            TitleSetting temp = titleSettings[i];
            titleSettings[i] = titleSettings[randomIndex];
            titleSettings[randomIndex] = temp;
        }
    }

    private void StartNewTitle()
    {
        currentTimer = 0f;
        isJudging = true;
        UIManager.Instance.UpdateThirdTimer(judgementTime);
        UIManager.Instance.UpdateThirdGroup(currentTitleIndex, titleSettings.Count);
        SetCurrentTitle(currentTitleIndex);
    }
    private void NextTitle()
    {
        isJudging = false;
        ClearModels();

        // 检查是否还有下一组
        if (currentTitleIndex < titleSettings.Count - 1)
        {
            currentTitleIndex++;
            StartCoroutine(StartNextTitleWithDelay());
        }
        else
        {
            Debug.Log("所有题目已完成");
            // 这里可以添加游戏结束逻辑
            GameOver();


        }
    }

    private void ClearModels()
    {
        // 清空modelPoint
        if (modelPoint != null)
        {
            foreach (Transform child in modelPoint)
            {
                Destroy(child.gameObject);
            }
        }

        // 清空sceneModelPoint
        if (sceneModelPoint != null)
        {
            foreach (Transform child in sceneModelPoint)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private IEnumerator StartNextTitleWithDelay()
    {
        yield return new WaitForSeconds(switchDelay);
        StartNewTitle();
    }

    #endregion

    //选择组
    public void SetCurrentTitle(int index)
    {
        currentSuccessObject = titleSettings[index].correctModel;
        SetModelView(index);
        RandomizeCameraPositions();
        RandomizeButtonModels(index);
    }

    //设置摄像头观察的render texture
    public void SetModelView(int index)
    {
        // 获取所有直接子物体（不包括自己）
        int childCount = modelPoint.childCount;
        GameObject[] childObjects = new GameObject[childCount];

        for (int i = 0; i < childCount; i++)
        {
            childObjects[i] = modelPoint.GetChild(i).gameObject;
        }

        if (childObjects != null)
        {
            foreach (var item in childObjects)
            {
                Destroy(item);
            }
        }


        GameObject model = Instantiate(titleSettings[index].correctModel);
        model.transform.SetParent(modelPoint, false);
        model.layer= LayerMask.NameToLayer("model");

        // 获取或添加Renderer组件
        MeshRenderer meshRenderer = model.GetComponent<MeshRenderer>();

        // 检查Renderer是否存在
        if (meshRenderer != null)
        {
            // 设置材质
            meshRenderer.material = targetMaterial;
        }
        else
        {
            Debug.LogError("Failed to find MeshRenderer component on the created object.");
        }
    }

    //设置3个摄像头随机位置旋转
    public void RandomizeCameraPositions()
    {

        // 创建位置索引列表
        List<int> availablePositions = new List<int> { 0, 1, 2 };
        float[] possibleRotations = new float[] { 0, 90, 180, 270 };

        // 为每个摄像机随机分配位置
        for (int i = 0; i < cameraObject.Count; i++)
        {
            // 随机选择一个可用位置索引
            int randomIndex = Random.Range(0, availablePositions.Count);
            int positionIndex = availablePositions[randomIndex];

            // 随机Z轴旋转
            float randomZRotation = possibleRotations[Random.Range(0, possibleRotations.Length)];

            // 将摄像机移动到选中的位置
            cameraObject[i].transform.position = cameraPreset[positionIndex].position;
            cameraObject[i].transform.rotation = cameraPreset[positionIndex].rotation;

            cameraObject[i].transform.Rotate(0, 0, randomZRotation, Space.Self);

            // 从可用位置列表中移除已使用的位置
            availablePositions.RemoveAt(randomIndex);
        }
    }

    public void RandomizeButtonModels(int titleIndex)
    {
        // 获取当前题目的三个模型
        GameObject[] models = new GameObject[]
        {
            titleSettings[titleIndex].correctModel,
            titleSettings[titleIndex].InterferenceModelOne,
            titleSettings[titleIndex].InterferenceModelTwo
        };

        // 清空之前的映射
        buttonModelMap.Clear();

        // 随机分配模型到按钮
        List<int> availableModels = new List<int> { 0, 1, 2 };

        foreach (GameObject button in buttons)
        {
            int randomIndex = Random.Range(0, availableModels.Count);
            int modelIndex = availableModels[randomIndex];

            // 保存按钮和模型的对应关系
            buttonModelMap[button] = models[modelIndex];

            availableModels.RemoveAt(randomIndex);
        }
    }
    public void OnButtonClick(GameObject buttonObject)
    {
        if (!isJudging) return;

        if (buttonModelMap.ContainsKey(buttonObject))
        {
            GameObject selectedModel = buttonModelMap[buttonObject];

            // 显示选中的模型
            SetSelectModelView(selectedModel);

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // 检查是否选择了正确的模型
                if (selectedModel == currentSuccessObject)
                {
                    Debug.Log("选择正确！");
                    // 在这里添加正确选择的处理逻辑
                    if (currentTimer <= 5f)
                    {
                        SetAddScoreThird(10);
                    }
                    else if (currentTimer > 5f && currentTimer <= 8f)
                    {
                        SetAddScoreThird(8);
                    }
                    else if (currentTimer > 8f && currentTimer <= 15f)
                    {
                        SetAddScoreThird(5);
                    }
                    NextTitle();
                }
                else
                {
                    Debug.Log("选择错误！");
                    // 在这里添加错误选择的处理逻辑
                    SetAddScoreThird(-10);
                    NextTitle();
                }
            }
        }
    }

    private void SetSelectModelView(GameObject modelPrefab)
    {
        // 清除现有模型
        foreach (Transform child in sceneModelPoint)
        {
            Destroy(child.gameObject);
        }

        // 实例化新模型
        GameObject model = Instantiate(modelPrefab);
        model.transform.SetParent(sceneModelPoint, false);

        ModelAutoroatation.Instance.SetUpdateModel(model);

    }

    #region 加减分
    // 加减分
    public void SetAddScoreThird(int addS)
    {
        score = Mathf.Max(0, score + addS);

        //设置UI

        UIManager.Instance.UpdateThirdScore(score, addS);
    }
    #endregion

    private void GameOver()
    {
        isJudging = false;
        ClearModels();

        GameManager.Instance.EndPanelActive();

        StopAllCoroutines();

        // 更新最终UI显示
        UIManager.Instance.UpdateThirdTimer(0);
        UIManager.Instance.UpdateThirdGroup(titleSettings.Count-1, titleSettings.Count);

    }
}
