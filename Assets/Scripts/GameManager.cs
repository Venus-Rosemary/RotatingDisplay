using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
using static UnityEngine.ParticleSystem;

public class GameManager : Singleton<GameManager>
{
    [Header("���")]
    public GameObject playerObject;
    public Transform playerResetPoint;


    [Header("��һ��")]
    public GameObject firstObjects;
    public GameObject firstUI;
    public ConveyorBelt conveyorBelt;

    [Header("�ڶ���")]
    public GameObject secondObjects;
    public GameObject secondUI;
    public DisplayGenerativeModel displayGenerativeModel;

    [Header("������")]
    public GameObject thirdObjects;
    public GameObject thirdUI;
    public ModelMatchController modelMatchController;

    [Header("���Ĺ�")]
    public GameObject fourObjects;
    public GameObject fourUI;
    public FourPassModelGenerative fourPassModelGenerative;

    [Header("UI����")]
    public GameObject StartPanelUI;
    public GameObject GamingPanelUI;
    public GameObject EndPanelUI;

    [Header("��ť����")]
    public List<GameObject> groundButton = new List<GameObject>();

    [Header("�ؿ�״̬")]
    public bool isFirstPass = false;
    public bool isSecondPass = false;
    public bool isThirdPass = false;
    public bool isFourPass = false;

    void Start()
    {
        SetUIManagement(true, false, false);
        playerObject.SetActive(false);
        SetPassObject(false, false, false, false);
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void FirstPassStart()
    {
        isFirstPass = true;
        playerObject.SetActive(true);
        playerObject.transform.position = playerResetPoint.position;

        SetPassObject(true, false, false, false);

        SetGamingUI(true, false, false, false);
        SetUIManagement(false, true, false);
        conveyorBelt.MoveConveyorBelt();
    }

    public void SecondPassStart()
    {
        isSecondPass = true;
        playerObject.SetActive(true);
        playerObject.transform.position = playerResetPoint.position;

        SetPassObject(false, true, false, false);

        SetGamingUI(false, true, false, false);
        SetUIManagement(false, true, false);
        displayGenerativeModel.StartGenerateModels();
    }

    public void ThirdPassStart()
    {
        isThirdPass = true;
        playerObject.SetActive(true);
        playerObject.transform.position = playerResetPoint.position;

        SetPassObject(false, false, true, false);

        SetGamingUI(false, false, true, false);
        SetUIManagement(false, true, false);
        modelMatchController.StartThreeGame();
    }

    public void FourPassStart()
    {
        isFourPass = true;
        playerObject.SetActive(true);
        playerObject.transform.position = playerResetPoint.position;

        SetPassObject(false, false, false, true);

        SetGamingUI(false, false, false, true);
        SetUIManagement(false, true, false);
        fourPassModelGenerative.StartGM();
    }

    #region ��ݷ���
    public void SetPassObject(bool first,bool second,bool third,bool four)
    {
        firstObjects.SetActive(first);
        secondObjects.SetActive(second);
        thirdObjects.SetActive(third);
        fourObjects.SetActive(four);
    }

    public void SetGamingUI(bool first,bool second,bool third,bool four)
    {
        firstUI.SetActive(first);
        secondUI.SetActive(second);
        thirdUI.SetActive(third);
        fourUI.SetActive(four);
    }

    public void SetUIManagement(bool start,bool game,bool end)
    {
        StartPanelUI.SetActive(start);
        GamingPanelUI.SetActive(game);
        EndPanelUI.SetActive(end);
    }
    #endregion

    //��������������
    public void EndPanelActive()
    {
        if (groundButton!=null)
        {
            foreach (var item in groundButton)
            {
                item.GetComponent<PressurePlate>().SetButtonState();
            }
        }
        playerObject.SetActive(false);
        SetGamingUI(false, false, false, false);
        SetUIManagement(false, false, true);
    }

    //����
    public void RestartGame()
    {
        if (isFirstPass)
        {
            FirstPassStart();
        }
        else if (isSecondPass)
        {
            SecondPassStart();
        }
        else if (isThirdPass)
        {
            ThirdPassStart();
        }
        else if (isFourPass)
        {
            FourPassStart();
        }
    }

    //����
    public void ReturnMain()
    {
        isFirstPass = false;
        isSecondPass = false;
        isThirdPass = false;
        isFourPass = false;

        SetPassObject(false, false, false, false);
        SetGamingUI(false, false, false, false);
        SetUIManagement(true, false, false);
    }

    //�˳�
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
