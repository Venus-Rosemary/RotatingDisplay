using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

[System.Serializable]
public class FourPassModelSet
{
    [Header("��ȷģ��")]
    public GameObject Correct;

    [Header("����ģ��")]
    public GameObject Complementarity;

    [Header("����ģ��")]
    public GameObject Disturb;
}

public class FourPassModelGenerative : Singleton<FourPassModelGenerative>
{
    [Header("ģ�͹�������")]
    public List<FourPassModelSet> fourPassModel = new List<FourPassModelSet>();

    [Header("ģ�ʹ��λ��")]
    public Transform CorrectPoint;
    public List<Transform> OtherPoints = new List<Transform>();

    [Header("��¼��ǰ��ȷģ��")]
    public GameObject currentModel;
    public GameObject currentGM;

    [Header("��ť����")]
    public List<PressurePlate> deletePlate = new List<PressurePlate>();

    [Header("�÷�����")]
    public int score = 0;

    [Header("ѡ���Ĵ������۶Դ�")]
    private bool isPaused = false;
    public float pauseTime = 10f;      // ������������ʾ
    public float waveTime = 30f;        // ÿ����ʱ
    private float currentPauseTimer = 0f;
    private float currentWaveTimer = 0f;

    [Header("��Ϸ����")]
    private int maxRounds = 2; // �������
    private int currentRound = 0; // ��ǰ����
    private int currentGroup = 0; // ��ǰ����

    private Coroutine GMCoroutine;
    private Coroutine pauseCoroutine;

    [SerializeField] private List<GameObject> model = new List<GameObject>();
    private bool isGenerating = false;
    private int currentModelIndex = 0;  // ��ǰģ������
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
        // Fisher-Yates ϴ���㷨
        for (int i = fourPassModel.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            FourPassModelSet temp = fourPassModel[i];
            fourPassModel[i] = fourPassModel[randomIndex];
            fourPassModel[randomIndex] = temp;
        }
    }

    // ��ͣ����
    public void PauseGeneration(GameObject gameObject)
    {
        if (!isPaused && isGenerating)
        {
            isPaused = true;
            
            //ˢ��Э��״̬
            StopCoroutine(GMCoroutine);
            GMCoroutine = StartCoroutine(GenerateModel());

            pauseCoroutine = StartCoroutine(AutoResume(gameObject));
        }
    }

    private IEnumerator AutoResume(GameObject gameObject)
    {

        currentPauseTimer = pauseTime;
        //����ģ���ƶ�
        // ��ȡ��ǰ��ȷģ�͵�λ�ú���ת
        Vector3 targetPosition = CorrectPoint.position;

        // �ƶ����嵽Ŀ��λ�ò�������ת
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
            // ����Ƿ���ͣ
            while (isPaused)
            {
                yield return null;
            }
            // ���ð�ť
            if (deletePlate != null)
            {
                foreach (var item in deletePlate)
                {
                    item.SetCanDelete(true);
                }
            }
            // ���
            if (model!=null)
            {
                foreach (var item in model)
                {
                    Destroy(item);
                }
                model.Clear();
            }

            // ����ģ���б�
            if (currentModelIndex == 0)
            {
                currentRound++;
                if (currentRound > maxRounds)
                {
                    isGenerating = false;

                    GameManager.Instance.EndPanelActive();
                    Debug.Log("����������ִ�");
                    yield break;
                }
                ShuffleModelList();
            }

            currentGroup++;
            UIManager.Instance.UpdateFourGroup(currentGroup, fourPassModel.Count * 2);

            currentWaveTimer = waveTime;

            FourPassModelSet selectedSet = fourPassModel[currentModelIndex];
            currentModelIndex = (currentModelIndex + 1) % fourPassModel.Count;

            //��¼
            currentModel = selectedSet.Correct;

            //��ȷģ������
            GameObject aModel = Instantiate(selectedSet.Correct, CorrectPoint.position, Quaternion.identity);

            currentGM = aModel;

            aModel.transform.SetParent(CorrectPoint);
            aModel.transform.rotation = Quaternion.Euler(30, 0, 0);
            model.Add(aModel);

            if (OtherPoints!=null && OtherPoints.Count>=2)
            {
                // ����˳��
                for (int i = OtherPoints.Count - 1; i > 0; i--)
                {
                    int randomIndex = Random.Range(0, i + 1);
                    Transform temp = OtherPoints[i];
                    OtherPoints[i] = OtherPoints[randomIndex];
                    OtherPoints[randomIndex] = temp;
                }

                //����ģ������
                GameObject bModel = Instantiate(selectedSet.Complementarity, OtherPoints[0].position, Quaternion.identity);
                bModel.transform.SetParent(OtherPoints[0]);
                bModel.transform.rotation = Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f));
                model.Add(bModel);

                //����ģ������
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

    #region �Ӽ���
    // �Ӽ���
    public void SetAddScoreFour(int addS)
    {
        score = Mathf.Max(0, score + addS);

        //����UI

        UIManager.Instance.UpdateFourScore(score, addS);
    }
    #endregion
}
