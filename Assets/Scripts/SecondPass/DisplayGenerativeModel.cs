using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SecondPassModelSet
{
    [Header("正确模型")]
    public GameObject Correct;

    [Header("干扰模型")]
    public GameObject Disturb1;
    public GameObject Disturb2;
}


public class DisplayGenerativeModel : Singleton<DisplayGenerativeModel>
{
    public List<SecondPassModelSet> secondModel = new List<SecondPassModelSet>();
    public Transform spawnPoint; // 生成位置
    private bool isGenerating = false;

    [Header("记录当前正确模型")]
    public GameObject currentModel;

    [Header("模型显示设置")]
    public float DisplayTime = 10f;
    private float originalDisplayTime = 0;
    public float DelayTime = 1f;
    private int currentModelIndex = 0;  // 当前模型索引

    [Header("按钮设置")]
    public PressurePlate deletePlate;

    [Header("得分设置")]
    public int score = 0;

    [Header("倒计时设置")]
    public float totalGameTime = 180f; // 3分钟
    private float currentTime;
    private bool isTimerRunning = false;

    // 创建模型序列
    [SerializeField] private List<GameObject> modelSequence = new List<GameObject>();

    private void Start()
    {
        originalDisplayTime = DisplayTime;
    }

    #region 开始模型生成
    //开始模型生成
    public void StartGenerateModels()
    {
        if (!isGenerating && secondModel.Count > 0)
        {
            isGenerating = true;
            currentTime = totalGameTime;
            isTimerRunning = true;

            currentModelIndex = 0; // 重置索引
            score = 0;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateSecondTimer(currentTime);
            }
            StartCoroutine(TimerCoroutine());
            StartCoroutine(GenerateModelSequence());
        }
    }

    private void ShuffleModelList()
    {
        // Fisher-Yates 洗牌算法
        for (int i = secondModel.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            SecondPassModelSet temp = secondModel[i];
            secondModel[i] = secondModel[randomIndex];
            secondModel[randomIndex] = temp;
        }
    }

    private IEnumerator GenerateModelSequence()
    {
        
        while (isGenerating)
        {
            if (currentModelIndex==0)
            {
                ShuffleModelList(); // 打乱模型列表
            }

            DisplayTime = Mathf.Max(3, DisplayTime - 1f);

            // 按顺序选择模型集合
            SecondPassModelSet selectedSet = secondModel[currentModelIndex];
            currentModelIndex = (currentModelIndex + 1) % secondModel.Count;


            // 记录正确的模型，方便展示盒中的生成
            currentModel = selectedSet.Correct;

            SecondDisplayFrame.Instance.SpawnAndMoveFrame();
            // 添加2个正确模型
            modelSequence.Add(selectedSet.Correct);
            modelSequence.Add(selectedSet.Correct);

            // 添加干扰模型
            modelSequence.Add(selectedSet.Disturb1);
            modelSequence.Add(selectedSet.Disturb2);

            // 打乱顺序
            for (int i = modelSequence.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                GameObject temp = modelSequence[i];
                modelSequence[i] = modelSequence[randomIndex];
                modelSequence[randomIndex] = temp;
            }

            // 依次生成模型
            foreach (GameObject modelPrefab in modelSequence)
            {
                // 生成模型
                GameObject newModel = Instantiate(modelPrefab, spawnPoint.position, spawnPoint.rotation);
                newModel.transform.SetParent(spawnPoint);

                newModel.transform.rotation = Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f)
                );


                if (deletePlate != null)
                {
                    deletePlate.SetCanDelete(true);
                }

                // 等待3秒
                yield return new WaitForSeconds(DisplayTime);



                if (deletePlate != null)
                {
                    deletePlate.SetCanDelete(false);
                }

                // 销毁模型
                Destroy(newModel);

                // 等待1秒后生成下一个
                yield return new WaitForSeconds(DelayTime);


            }


            //isGenerating = false;
            modelSequence.Clear();
        }
        
    }
    #endregion


    #region 加减分
    // 加减分
    public void SetAddScoreSecond(int addS)
    {
        score = Mathf.Max(0, score + addS);

        //设置UI

        UIManager.Instance.UpdateSecondScore(score, addS);
    }
    #endregion

    #region 倒计时
    private IEnumerator TimerCoroutine()
    {
        while (isTimerRunning && currentTime > 0)
        {
            yield return new WaitForSeconds(1f);
            currentTime--;

            // 更新UI显示
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateSecondTimer(currentTime);
            }

            // 时间结束
            if (currentTime <= 0)
            {
                GameOver();
            }
        }
    }

    private void GameOver()
    {
        isTimerRunning = false;
        isGenerating = false;

        DisplayTime = originalDisplayTime;

        ClearModelSequence();

        GameManager.Instance.EndPanelActive();

        StopAllCoroutines();
        SecondDisplayFrame.Instance.CleraModelDiaplayBox();
        // 清理当前显示的模型
        if (spawnPoint != null && spawnPoint.childCount > 0)
        {
            foreach (Transform child in spawnPoint)
            {
                Destroy(child.gameObject);
            }
        }

        currentModel = null;
    }

    public void ClearModelSequence()
    {
        modelSequence.Clear();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isTimerRunning = false;
        isGenerating = false;
    }
    #endregion
}
