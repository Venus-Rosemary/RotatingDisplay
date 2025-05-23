using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PressurePlate : MonoBehaviour
{
    [Header("Trigger设置")]
    public MeshRenderer triggerObject;//trigger的地面物体
    public Material SelectMaterial;//站上去后改变颜色


    private Material triggerOriginalMaterial;
    private bool playerInRange = false;

    private bool canDelete = false;

    [Header("不同关卡不同使用")]
    public bool firstPass = false;
    public bool secondPass = false;
    public bool threePass = false;
    public bool fourPass = false;

    [Header("第二关设置")]

    public Transform checkPoint;
    public Vector3 checkSize = new Vector3(1f, 1f, 1f); // 检测区域大小
    public LayerMask checkLayer; // 检测层级

    [Header("第四关设置")]
    public Transform locationPonit;
    private ASingleGrid locationASingleGrid;
    public Transform parentPoint;
    private List<ASingleGrid> allGrids = new List<ASingleGrid>();



    private void Awake()
    {
        triggerOriginalMaterial = triggerObject.material;
    }

    private void OnEnable()
    {
        triggerObject.material = triggerOriginalMaterial;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange  && canDelete && firstPass)
        {
            DeleteObjectInArea();
            canDelete = false;
        }

        if (playerInRange && canDelete && secondPass)
        {
            DeleteObjectInAreaSecond();
            canDelete = false;
        }

        if (playerInRange && threePass)
        {
            DeleteObjectInAreaThree();
            canDelete = false;
        }

        if (playerInRange && canDelete && fourPass && Mouse.current.leftButton.wasPressedThisFrame)
        {
            DeleteObjectInAreaFour();
            canDelete = false;
        }
    }

    #region 第一关
    public void SetCanDelete(bool can)
    {
        canDelete = can;
    }

    private void DeleteObjectInArea()
    {
        Collider[] hitColliders = Physics.OverlapBox(
            AreaTypeChecker.Instance.areaOne.position,
            AreaTypeChecker.Instance.checkSize / 2,
            AreaTypeChecker.Instance.areaOne.rotation,
            AreaTypeChecker.Instance.checkLayer
        );

        if (hitColliders.Length > 0)
        {
            Transform modelPoint = hitColliders[0].GetComponentInParent<ASingleGrid>()?.transform.GetChild(1);

            if (modelPoint != null && modelPoint.childCount > 0)
            {
                Destroy(modelPoint.GetChild(0).gameObject);
            }
        }
    }
    #endregion

    #region 第二关
    private void DeleteObjectInAreaSecond()
    {
        Collider[] hitColliders = Physics.OverlapBox(
            checkPoint.transform.position,
            checkSize / 2,
            checkPoint.transform.rotation,
            checkLayer
        );
        if (hitColliders.Length > 0)
        {
            ASingleGrid modelPoint = hitColliders[0].GetComponentInParent<ASingleGrid>();

            Debug.Log(modelPoint.name);
            Debug.Log(modelPoint.isCorrect);
            if (modelPoint != null)
            {
                if (modelPoint.isCorrect)
                {
                    Debug.Log("加分");
                    DisplayGenerativeModel.Instance.SetAddScoreSecond(10);
                }
                else
                {
                    DisplayGenerativeModel.Instance.SetAddScoreSecond(-20);
                    Debug.Log("扣分");
                }
            }
        }

        //踩上去判断对错，直接得分扣分
    }

    #endregion

    #region 第三关
    private void DeleteObjectInAreaThree()
    {
        if (ModelMatchController.Instance != null)
        {
            GameObject buttonObject = this.gameObject;
            ModelMatchController.Instance.OnButtonClick(buttonObject);
        }
    }
    #endregion

    #region 第四关
    private void DeleteObjectInAreaFour()
    {
        Collider[] hitColliders = Physics.OverlapBox(
            locationPonit.transform.position,
            checkSize / 2,
            locationPonit.transform.rotation,
            checkLayer
        );

        if (hitColliders.Length > 0)
        {
            ASingleGrid modelPoint = hitColliders[0].GetComponentInParent<ASingleGrid>();



            Debug.Log(modelPoint.name);

            if (modelPoint != null)
            {
                if (modelPoint.isCorrect)
                {
                    Debug.Log("加分");
                    FourPassModelGenerative.Instance.SetAddScoreFour(10);
                    //FourPassModelGenerative.Instance.PauseGeneration(modelPoint.gameObject);
                }
                else
                {
                    FourPassModelGenerative.Instance.SetAddScoreFour(-20);
                    //FourPassModelGenerative.Instance.PauseGeneration(modelPoint.gameObject);
                    Debug.Log("扣分");
                }

                // 无论是否正确，都移动互补模型
                if (parentPoint != null)
                {
                    allGrids.Clear();
                    // 获取所有子物体中的ASingleGrid组件
                    allGrids.AddRange(parentPoint.GetComponentsInChildren<ASingleGrid>());
                    foreach (var item in allGrids)
                    {
                        if (item.isCorrect==true)
                        {
                            FourPassModelGenerative.Instance.PauseGeneration(item.gameObject);
                        }
                    }
                }

            }

        }
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            triggerObject.material = SelectMaterial;
        }

    }

    private void OnTriggerStay(Collider other)
    {

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            triggerObject.material = triggerOriginalMaterial;
        }

    }

    public void SetButtonState()
    {
        playerInRange = false;
        triggerObject.material = triggerOriginalMaterial;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (checkPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(checkPoint.position, checkPoint.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, checkSize);
        }
    }
#endif
    }
