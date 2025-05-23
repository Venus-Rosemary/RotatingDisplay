using UnityEngine;

public class AreaTypeChecker : Singleton<AreaTypeChecker>
{
    [Header("检测区域设置")]
    public Transform areaOne;
    public Transform areaTwo;
    public Transform areaThree;
    public Vector3 checkSize = new Vector3(1f, 1f, 1f); // 检测区域大小
    public LayerMask checkLayer; // 检测层级

    public bool CheckAreasType()
    {
        // 检测区域一
        Collider[] hitColliders1 = Physics.OverlapBox(areaOne.position, checkSize / 2, areaOne.rotation, checkLayer);
        if (hitColliders1.Length == 0) return false;

        // 检测区域二
        Collider[] hitColliders2 = Physics.OverlapBox(areaTwo.position, checkSize / 2, areaTwo.rotation, checkLayer);
        if (hitColliders2.Length == 0) return false;

        // 获取两个区域内物体的类型(因为ASingleGrid在检测到的物体的父物体上，ASingleGrid挂载的物体没有colloder，所以会用GetComponentInParent)
        string type1 = hitColliders1[0].GetComponentInParent<ASingleGrid>()?.type ?? "";
        string type2 = hitColliders2[0].GetComponentInParent<ASingleGrid>()?.type ?? "";

        //Debug.Log(type1 + "和" + type2 + "名字1：" + hitColliders1[0].name + "名字2：" + hitColliders2[0].name);

        // 返回类型是否相同
        return !string.IsNullOrEmpty(type1) && type1 == type2;
    }


    public bool CheckAreasIsCorrect()
    {
        Collider[] hitColliders1 = Physics.OverlapBox(areaThree.position, checkSize / 2, areaThree.rotation, checkLayer);
        if (hitColliders1.Length == 0) return false;

        bool isCorrect1 = hitColliders1[0].GetComponentInParent<ASingleGrid>()?.isCorrect ?? false;

        Debug.Log(hitColliders1[0].name + "bool值：" + isCorrect1);

        return isCorrect1;
    }

    public ASingleGrid CheckAreasObject()
    {
        Collider[] hitColliders1 = Physics.OverlapBox(areaThree.position, checkSize / 2, areaThree.rotation, checkLayer);
        if (hitColliders1.Length == 0) return null;

        ASingleGrid areaObject = hitColliders1[0].GetComponentInParent<ASingleGrid>();
        
        return areaObject;

    }

#if UNITY_EDITOR
    // 用于在Scene视图中显示检测区域
    private void OnDrawGizmos()
    {
        if (areaOne != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(areaOne.position, areaOne.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, checkSize);
        }

        if (areaTwo != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = Matrix4x4.TRS(areaTwo.position, areaTwo.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, checkSize);
        }

        if (areaThree != null)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.TRS(areaThree.position, areaThree.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, checkSize);
        }
    }
#endif
}