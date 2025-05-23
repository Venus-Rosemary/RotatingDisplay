using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

[System.Serializable]
public class FourPassModelSet
{
    [Header("正确模型")]
    public GameObject Correct;

    [Header("互补模型")]
    public GameObject Complementarity;

    [Header("干扰模型")]
    public GameObject Disturb;
}

public class FourPassModelGenerative : Singleton<FourPassModelGenerative>
{
    [Header("模型管理设置")]
    public List<FourPassModelSet> fourPassModel = new List<FourPassModelSet>();

    [Header("模型存放位置")]
    public Transform CorrectPoint;
    public List<Transform> OtherPoints = new List<Transform>();

    [Header("记录当前正确模型")]
    public GameObject currentModel;
    public GameObject currentGM;

    [Header("按钮设置")]
    public List<PressurePlate> deletePlate = new List<PressurePlate>();

    [Header("得分设置")]
    public int score = 0;

    [Header("选择后的处理，不论对错")]
    private bool isPaused = false;
    public float pauseTime = 10f;      // 用来处理动画显示
    public float waveTime = 30f;        // 每波用时
    private float currentPauseTimer = 0f;
    private float currentWaveTimer = 0f;

    [Header("游戏设置")]
    private int maxRounds = 2; // 最大轮数
    private int currentRound = 0; // 当前轮数
    private int currentGroup = 0; // 当前组数

    private Coroutine GMCoroutine;
    private Coroutine pauseCoroutine;

    [SerializeField] private List<GameObject> model = new List<GameObject>();
    private bool isGenerating = false;
    private int currentModelIndex = 0;  // 当前模型索引
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isGenerating)
        {
            if (isPaused)
            {
                currentPauseTimer -= Time.deltaTime;
                UIManager.Instance.UpdateFourTimer(currentPauseTimer);
            }
            else
            {
                currentWaveTimer -= Time.deltaTime;
                UIManager.Instance.UpdateFourTimer(currentWaveTimer);
            }
        }
    }

    public void StartGM()
    {
        if (!isGenerating && fourPassModel.Count>0)
        {
            isGenerating = true;
            currentRound = 0;
            currentModelIndex = 0;
            currentGroup = 0;
            GMCoroutine = StartCoroutine(GenerateModel());
        }
    }

    private void ShuffleModelList()
    {
        // Fisher-Yates 洗牌算法
        for (int i = fourPassModel.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            FourPassModelSet temp = fourPassModel[i];
            fourPassModel[i] = fourPassModel[randomIndex];
            fourPassModel[randomIndex] = temp;
        }
    }

    // 暂停方法
    public void PauseGeneration(GameObject gameObject)
    {
        if (!isPaused && isGenerating)
        {
            isPaused = true;
            
            //刷新协程状态
            StopCoroutine(GMCoroutine);
            GMCoroutine = StartCoroutine(GenerateModel());

            pauseCoroutine = StartCoroutine(AutoResume(gameObject));
        }
    }

    private IEnumerator AutoResume(GameObject gameObject)
    {

        currentPauseTimer = pauseTime;
        //处理模型移动
        // 获取当前正确模型的位置和旋转
        Vector3 targetPosition = CorrectPoint.position;

        // 移动物体到目标位置并设置旋转
        gameObject.transform.SetParent(null);
        gameObject.transform.DOMove(targetPosition, 3f).OnComplete(()=> { 
            gameObject.transform.SetParent(CorrectPoint);
            if (currentGM != null)
            {
                Quaternion targetRotation = currentGM.transform.localRotation;
                gameObject.transform.localRotation = targetRotation;
            }
        });
        //gameObject.transform.position = targetPosition;

        yield return new WaitForSeconds(pauseTime);
        isPaused = false;
    }

    private IEnumerator GenerateModel()
    {
        while (isGenerating)
        {
            // 检查是否暂停
            while (isPaused)
            {
                yield return null;
            }
            // 重置按钮
            if (deletePlate != null)
            {
                foreach (var item in deletePlate)
                {
                    item.SetCanDelete(true);
                }
            }
            // 清空
            if (model!=null)
            {
                foreach (var item in model)
                {
                    Destroy(item);
                }
                model.Clear();
            }

            // 打乱模型列表
            if (currentModelIndex == 0)
            {
                currentRound++;
                if (currentRound > maxRounds)
                {
                    isGenerating = false;

                    GameManager.Instance.EndPanelActive();
                    Debug.Log("已完成所有轮次");
                    yield break;
                }
                ShuffleModelList();
            }

            currentGroup++;
            UIManager.Instance.UpdateFourGroup(currentGroup, fourPassModel.Count * 2);

            currentWaveTimer = waveTime;

            FourPassModelSet selectedSet = fourPassModel[currentModelIndex];
            currentModelIndex = (currentModelIndex + 1) % fourPassModel.Count;

            //记录
            currentModel = selectedSet.Correct;

            //正确模型生成
            GameObject aModel = Instantiate(selectedSet.Correct, CorrectPoint.position, Quaternion.identity);

            currentGM = aModel;

            aModel.transform.SetParent(CorrectPoint);
            aModel.transform.rotation = Quaternion.Euler(30, 0, 0);
            model.Add(aModel);

            if (OtherPoints!=null && OtherPoints.Count>=2)
            {
                // 打乱顺序
                for (int i = OtherPoints.Count - 1; i > 0; i--)
                {
                    int randomIndex = Random.Range(0, i + 1);
                    Transform temp = OtherPoints[i];
                    OtherPoints[i] = OtherPoints[randomIndex];
                    OtherPoints[randomIndex] = temp;
                }

                //互补模型生成
                GameObject bModel = Instantiate(selectedSet.Complementarity, OtherPoints[0].position, Quaternion.identity);
                bModel.transform.SetParent(OtherPoints[0]);
                bModel.transform.rotation = Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f));
                model.Add(bModel);

                //干扰模型生成
                GameObject cModel = Instantiate(selectedSet.Disturb, OtherPoints[1].position, Quaternion.identity);
                cModel.transform.SetParent(OtherPoints[1]);
                cModel.transform.rotation = Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f));
                model.Add(cModel);
            }

            yield return new WaitForSeconds(waveTime);

            //isGenerating = false;

        }
    }

    #region 加减分
    // 加减分
    public void SetAddScoreFour(int addS)
    {
        score = Mathf.Max(0, score + addS);

        //设置UI

        UIManager.Instance.UpdateFourScore(score, addS);
    }
    #endregion
}
