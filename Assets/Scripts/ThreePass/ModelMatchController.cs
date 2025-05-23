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

    [Header("ģ������")]
    [Tooltip("��ȷģ��")]
    public GameObject correctModel;
    [Tooltip("����ģ��1")]
    public GameObject InterferenceModelOne;
    [Tooltip("����ģ��2")]
    public GameObject InterferenceModelTwo;

    [Header("��ťͼƬ����")]
    public Sprite CM;
    public Sprite IMO;
    public Sprite IMT;
}

public class ModelMatchController : Singleton<ModelMatchController>
{
    [Header("ģ��ͶӰ����")]

    public Transform modelPoint;
    public Material targetMaterial;
    public List<Transform> cameraPreset = new List<Transform>();
    public List<GameObject> cameraObject = new List<GameObject>();

    public List<TitleSetting> titleSettings = new List<TitleSetting>();

    [Header("��ťչʾģ��")]
    public Transform sceneModelPoint;
    public GameObject currentSuccessObject;
    public GameObject oneObject;
    public GameObject twoObject;
    public GameObject threeObject;
    public List<GameObject> buttons = new List<GameObject>();
    private Dictionary<GameObject, GameObject> buttonModelMap = new Dictionary<GameObject, GameObject>();

    [Header("��Ϸ����")]
    public float judgementTime = 15f; // ÿ���ж�ʱ��
    public float switchDelay = 2f; // �л��ӳ�ʱ��
    private float currentTimer = 0f;
    private int currentTitleIndex = 0;
    private bool isJudging = false;

    [Header("�÷�����")]
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

            // ���µ���ʱUI
            UIManager.Instance.UpdateThirdTimer(judgementTime - currentTimer);

            if (currentTimer >= judgementTime)
            {

                UIManager.Instance.UpdateThirdTimer(0);
                SetAddScoreThird(-10);
                NextTitle();
            }
        }
    }

    #region 15����л�����ǰ�л�
    public void StartThreeGame()
    {
        currentTitleIndex = 0;
        score = 0;
        UIManager.Instance.UpdateThirdScore(score, 0);
        ShuffleModelList();
        StartNewTitle();
    }

    //����˳��
    private void ShuffleModelList()
    {
        // Fisher-Yates ϴ���㷨
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

        // ����Ƿ�����һ��
        if (currentTitleIndex < titleSettings.Count - 1)
        {
            currentTitleIndex++;
            StartCoroutine(StartNextTitleWithDelay());
        }
        else
        {
            Debug.Log("������Ŀ�����");
            // ������������Ϸ�����߼�
            GameOver();


        }
    }

    private void ClearModels()
    {
        // ���modelPoint
        if (modelPoint != null)
        {
            foreach (Transform child in modelPoint)
            {
                Destroy(child.gameObject);
            }
        }

        // ���sceneModelPoint
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

    //ѡ����
    public void SetCurrentTitle(int index)
    {
        currentSuccessObject = titleSettings[index].correctModel;
        SetModelView(index);
        RandomizeCameraPositions();
        RandomizeButtonModels(index);
    }

    //��������ͷ�۲��render texture
    public void SetModelView(int index)
    {
        // ��ȡ����ֱ�������壨�������Լ���
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

        // ��ȡ�����Renderer���
        MeshRenderer meshRenderer = model.GetComponent<MeshRenderer>();

        // ���Renderer�Ƿ����
        if (meshRenderer != null)
        {
            // ���ò���
            meshRenderer.material = targetMaterial;
        }
        else
        {
            Debug.LogError("Failed to find MeshRenderer component on the created object.");
        }
    }

    //����3������ͷ���λ����ת
    public void RandomizeCameraPositions()
    {

        // ����λ�������б�
        List<int> availablePositions = new List<int> { 0, 1, 2 };
        float[] possibleRotations = new float[] { 0, 90, 180, 270 };

        // Ϊÿ��������������λ��
        for (int i = 0; i < cameraObject.Count; i++)
        {
            // ���ѡ��һ������λ������
            int randomIndex = Random.Range(0, availablePositions.Count);
            int positionIndex = availablePositions[randomIndex];

            // ���Z����ת
            float randomZRotation = possibleRotations[Random.Range(0, possibleRotations.Length)];

            // ��������ƶ���ѡ�е�λ��
            cameraObject[i].transform.position = cameraPreset[positionIndex].position;
            cameraObject[i].transform.rotation = cameraPreset[positionIndex].rotation;

            cameraObject[i].transform.Rotate(0, 0, randomZRotation, Space.Self);

            // �ӿ���λ���б����Ƴ���ʹ�õ�λ��
            availablePositions.RemoveAt(randomIndex);
        }
    }

    public void RandomizeButtonModels(int titleIndex)
    {
        // ��ȡ��ǰ��Ŀ������ģ��
        GameObject[] models = new GameObject[]
        {
            titleSettings[titleIndex].correctModel,
            titleSettings[titleIndex].InterferenceModelOne,
            titleSettings[titleIndex].InterferenceModelTwo
        };

        // ���֮ǰ��ӳ��
        buttonModelMap.Clear();

        // �������ģ�͵���ť
        List<int> availableModels = new List<int> { 0, 1, 2 };

        foreach (GameObject button in buttons)
        {
            int randomIndex = Random.Range(0, availableModels.Count);
            int modelIndex = availableModels[randomIndex];

            // ���水ť��ģ�͵Ķ�Ӧ��ϵ
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

            // ��ʾѡ�е�ģ��
            SetSelectModelView(selectedModel);

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // ����Ƿ�ѡ������ȷ��ģ��
                if (selectedModel == currentSuccessObject)
                {
                    Debug.Log("ѡ����ȷ��");
                    // �����������ȷѡ��Ĵ����߼�
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
                    Debug.Log("ѡ�����");
                    // ��������Ӵ���ѡ��Ĵ����߼�
                    SetAddScoreThird(-10);
                    NextTitle();
                }
            }
        }
    }

    private void SetSelectModelView(GameObject modelPrefab)
    {
        // �������ģ��
        foreach (Transform child in sceneModelPoint)
        {
            Destroy(child.gameObject);
        }

        // ʵ������ģ��
        GameObject model = Instantiate(modelPrefab);
        model.transform.SetParent(sceneModelPoint, false);

        ModelAutoroatation.Instance.SetUpdateModel(model);

    }

    #region �Ӽ���
    // �Ӽ���
    public void SetAddScoreThird(int addS)
    {
        score = Mathf.Max(0, score + addS);

        //����UI

        UIManager.Instance.UpdateThirdScore(score, addS);
    }
    #endregion

    private void GameOver()
    {
        isJudging = false;
        ClearModels();

        GameManager.Instance.EndPanelActive();

        StopAllCoroutines();

        // ��������UI��ʾ
        UIManager.Instance.UpdateThirdTimer(0);
        UIManager.Instance.UpdateThirdGroup(titleSettings.Count-1, titleSettings.Count);

    }
}
