using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [Header("传送带设置")]
    public GameObject startPoint;
    public GameObject endPoint;
    public List<GameObject> Belt = new List<GameObject>();
    
    [Header("移动设置")]
    public float moveSpeed = 3.5f; // 移动速度
    private float originalSpeed = 0f; // 记录最初速度
    public float pauseDuration = 1f; // 停顿时间
    private bool isMoving = false;

    private int cycleCount = 1; // 添加周期计数器

    [Header("每波重置生成设置")]
    public int resetCount = 4; // 重置次数
    private int spawnCount = 0; // 添加计数器
    public float modelSize = 0.3f; // 添加模型大小设置
    private string currentType = "A"; // 添加当前type标记
    private int randomModelCount = 2; // 添加随机模型计数
    private bool isRandomModel = false; // 标记是否生成随机模型
    private bool isStartRandom = true; // 第一波的随机
    private List<int> randomPositions = new List<int>(); // 记录随机位置

    [Header("得分设置设置")]
    public int score = 0;
    public int DelayCount = 0;
    public int delay = 0;
    public GameObject indicatorLight; // 指示灯物体
    private Material indicatorMaterial; // 指示灯材质
    private Color originalColor; // 原始颜色

    [Header("按钮设置")]
    public PressurePlate deletePlate;

    private void Start()
    {
        DelayCount = Belt.Count - delay;
        originalSpeed = moveSpeed;
        // 获取指示灯材质和原始颜色
        if (indicatorLight != null)
        {
            indicatorMaterial = indicatorLight.GetComponent<Renderer>().material;
            originalColor = indicatorMaterial.color;
        }
    }

    private void ResetSpawnCycle()
    {
        spawnCount = 0;
        currentType = currentType == "A" ? "B" : "A";
        randomModelCount = Random.Range(1, 3); // 随机决定这组需要生成几个随机模型
        isRandomModel = false;

        // 计数并调整速度
        cycleCount++;
        if (cycleCount >= 2)
        {
            cycleCount = 0;
            moveSpeed = Mathf.Max(1f, moveSpeed - 0.5f);
        }

        // 重新生成随机位置
        randomPositions.Clear();
        List<int> positions = new List<int> { 0, 1, 2, 3 };
        for (int i = 0; i < randomModelCount; i++)
        {
            int index = Random.Range(0, positions.Count);
            randomPositions.Add(positions[index]);
            positions.RemoveAt(index);
        }
    }

    public void MoveConveyorBelt()
    {
        if (!isMoving)
        {
            isMoving = true;
            DisplayFrame.Instance.SpawnAndMoveFrame();

            GameTimer.Instance.StartTimer(); // 添加这行来启动计时器

            if (isStartRandom)
            {
                isStartRandom = false;
                randomModelCount = Random.Range(1, 3);
                randomPositions.Clear();
                List<int> positions = new List<int> { 0, 1, 2, 3 };
                for (int i = 0; i < randomModelCount; i++)
                {
                    int index = Random.Range(0, positions.Count);
                    randomPositions.Add(positions[index]);
                    positions.RemoveAt(index);
                }
            }

            StartCoroutine(MoveBeltCoroutine());
        }
    }

    private IEnumerator MoveBeltCoroutine()
    {
        // 计算总移动距离
        float totalDistance = Vector3.Distance(startPoint.transform.position, endPoint.transform.position);
        float unitLength = totalDistance / (Belt.Count - 1);

        while (isMoving)
        {
            // 移动每个格子
            for (int i = 0; i < Belt.Count; i++)
            {
                if (Belt[i].activeSelf)
                {
                    // 计算下一个位置
                    Vector3 currentPos = Belt[i].transform.position;
                    Vector3 targetPos = currentPos + (endPoint.transform.position - startPoint.transform.position).normalized * unitLength;

                    // 检查是否到达终点
                    if (Vector3.Distance(currentPos, endPoint.transform.position) <= unitLength)
                    {
                        // 重置到起点
                        Belt[i].SetActive(false);
                        Belt[i].transform.position = new Vector3(startPoint.transform.position.x, Belt[i].transform.position.y,
                            startPoint.transform.position.z);

                        // 检查是否需要生成模型
                        if (ModelInDisplayBox.Instance.currentModel != null)
                        {
                            Transform spawnPoint = Belt[i].transform.GetChild(1);
                            if (spawnPoint != null)
                            {
                                // 检查并删除生成点下的所有子物体
                                if (spawnPoint.childCount != 0)
                                {
                                    Destroy(spawnPoint.GetChild(0).gameObject);
                                }

                                if (spawnPoint != null)
                                {
                                    if (spawnPoint.childCount != 0)
                                    {
                                        Destroy(spawnPoint.GetChild(0).gameObject);
                                    }

                                    // 决定是否生成随机模型
                                    if (randomModelCount > 0 && randomPositions.Contains(spawnCount))
                                    {
                                        randomModelCount--;
                                        GameObject newModel = ModelInDisplayBox.Instance.SpawnModelOfBelt(spawnPoint, modelSize);
                                        if (newModel != null)
                                        {
                                            ASingleGrid grid = Belt[i].GetComponent<ASingleGrid>();
                                            grid.type = currentType;
                                            grid.isCorrect = false;
                                        }
                                    }
                                    else
                                    {
                                        GameObject newModel = ModelInDisplayBox.Instance.SpawnModelOfBelt(spawnPoint, modelSize, false);
                                        if (newModel != null)
                                        {
                                            ASingleGrid grid = Belt[i].GetComponent<ASingleGrid>();
                                            grid.type = currentType;
                                            grid.isCorrect = true;
                                        }
                                    }

                                    spawnCount++;
                                    if (spawnCount >= resetCount)
                                    {
                                        ModelInDisplayBox.Instance.currentModel = null;
                                        ResetSpawnCycle();
                                    }
                                }
                            }
                            Belt[i].SetActive(true);
                        }

                    }
                    else
                    {
                        // 移动到下一个位置
                        Belt[i].transform.DOMove(targetPos, moveSpeed).SetEase(Ease.Linear);
                    }
                }
            }

            // 等待移动完成和暂停时间
            yield return new WaitForSeconds(moveSpeed);

            if (deletePlate != null)
            {
                deletePlate.SetCanDelete(true);
            }
            yield return new WaitForSeconds(pauseDuration);

            if (deletePlate != null)
            {
                deletePlate.SetCanDelete(false);
            }


            if (AreaTypeChecker.Instance.CheckAreasType())
            {
                //Debug.Log("两个区域内物体类型相同！");
            }
            else
            {
                DisplayFrame.Instance.SpawnAndMoveFrame();
                //Debug.Log("两个区域内物体类型不相同！");
            }

            if (DelayCount == 0)
            {
                if (AreaTypeChecker.Instance.CheckAreasIsCorrect())
                {
                    // 检查传送带上的物体
                    Transform modelPoint = AreaTypeChecker.Instance.CheckAreasObject().gameObject.transform.GetChild(1);
                    if (modelPoint != null)
                    {
                        // 如果是正确的物体且存在，加分；如果不存在，扣分
                        if (modelPoint.childCount > 0)
                        {
                            SetAddScore(10);
                        }
                        else
                        {
                            SetAddScore(-20);
                        }
                    }
                }
                else
                {
                    // 检查传送带上的物体
                    Transform modelPoint = AreaTypeChecker.Instance.CheckAreasObject().gameObject.transform.GetChild(1);
                    if (modelPoint != null)
                    {
                        // 如果是错误的物体且存在，扣分；如果不存在，加分
                        if (modelPoint.childCount > 0)
                        {
                            SetAddScore(-20);
                        }
                        else
                        {
                            SetAddScore(10);
                        }
                    }
                }
            }
            else
            {
                DelayCount -= 1;
            }


        }
    }

    #region 指示灯功能
    private IEnumerator ChangeIndicatorColor(bool isGood)
    {
        if (indicatorMaterial != null)
        {
            // 根据得分情况改变颜色
            indicatorMaterial.color = isGood ? Color.green : Color.red;

            // 等待1秒
            yield return new WaitForSeconds(0.5f);

            // 恢复原始颜色
            indicatorMaterial.color = originalColor;
        }
    }


    #endregion

    // 加减分
    public void SetAddScore(int addS)
    {
        score = Mathf.Max(0, score + addS);
        // 启动颜色变化协程
        StartCoroutine(ChangeIndicatorColor(addS > 0));
        //设置UI

        UIManager.Instance.UpdateScore(score, addS);
    }


    #region 初始化
    private void InitializeConveyorBelt()
    {
        // 重置所有计数器和状态
        cycleCount = 1;
        spawnCount = 0;
        currentType = "A";
        randomModelCount = 2;
        isRandomModel = false;
        isStartRandom = true;
        isMoving = false;
        score = 0;
        moveSpeed = originalSpeed;

        DelayCount = Belt.Count - delay;

        // 清空随机位置列表
        randomPositions.Clear();
        DisplayFrame.Instance.ClearFrameObject();

        GameTimer.Instance.ResetTimer();

        // 初始化传送带位置
        if (Belt != null && Belt.Count > 0)
        {
            float totalDistance = Vector3.Distance(startPoint.transform.position, endPoint.transform.position);
            float unitLength = totalDistance / (Belt.Count - 1);
            Vector3 direction = (endPoint.transform.position - startPoint.transform.position).normalized;

            for (int i = 0; i < Belt.Count; i++)
            {
                if (Belt[i] != null)
                {
                    // 设置初始位置
                    Vector3 position = startPoint.transform.position + direction * (unitLength * i);
                    Belt[i].transform.position = new Vector3(position.x, Belt[i].transform.position.y, position.z);
                    Belt[i].SetActive(true);

                    // 清理可能存在的模型
                    Transform spawnPoint = Belt[i].transform.GetChild(1);
                    if (spawnPoint != null && spawnPoint.childCount > 0)
                    {
                        Destroy(spawnPoint.GetChild(0).gameObject);
                    }
                }
            }
        }

        // 更新UI显示
        UIManager.Instance.UpdateScore(score, 0);
    }
    #endregion

    public void ResetConveyorBelt()
    {
        GameManager.Instance.EndPanelActive();
        StopConveyorBelt();
        StopAllCoroutines();
        InitializeConveyorBelt();
    }

    // 停止传送带
    public void StopConveyorBelt()
    {
        isMoving = false;
        // 停止所有DOTween动画
        foreach (var belt in Belt)
        {
            belt.transform.DOKill();
        }
    }

    private void OnDisable()
    {
        //ResetConveyorBelt();
    }
}
