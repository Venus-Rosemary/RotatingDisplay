using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class SecondDisplayFrame : Singleton<SecondDisplayFrame>
{
    public GameObject framePerfab;
    public List<GameObject> framePoint = new List<GameObject>();
    [SerializeField] private List<GameObject> frame = new List<GameObject>();

    [Header("动画设置")]
    public float moveDuration = 1f;
    public float scaleDuration = 0.5f;
    public float modelSize = 0.6f;
    public float MagnificationFactor = 2;

    public void SpawnAndMoveFrame()
    {
        // 如果已有物体，则移动到下一个点
        if (frame.Count > 0 && frame[0] != null)
        {
            MoveExistingFrame();
        }

        // 生成新物体
        GameObject newFrame = Instantiate(framePerfab, framePoint[0].transform.position, Quaternion.identity);
        frame.Add(newFrame);

        // 生成当前展示的模型
        if (DisplayGenerativeModel.Instance.currentModel != null)
        {
            GameObject newModel = Instantiate(DisplayGenerativeModel.Instance.currentModel,
                newFrame.transform.position,
                newFrame.transform.rotation);
            newModel.transform.SetParent(newFrame.transform);

            newModel.AddComponent<RotateSelf>();

            // 设置统一大小
            newModel.transform.localScale = Vector3.one * modelSize;
            //ModelUtils.FitToBounds(newModel.transform, 0.6f);
        }

        // 设置初始缩放为0
        newFrame.transform.localScale = Vector3.zero;

        // 创建动画序列
        Sequence sequence = DOTween.Sequence();
        sequence.Join(newFrame.transform.DOScale(Vector3.one * MagnificationFactor, scaleDuration));
        sequence.Join(newFrame.transform.DOMove(framePoint[1].transform.position, moveDuration));
    }

    private void MoveExistingFrame()
    {
        Vector3[] scales = new Vector3[] { 
            Vector3.zero,  // 起始点缩放为0
            Vector3.one * 2f * MagnificationFactor,  // 中间点缩放为2
            Vector3.zero   // 终点缩放为0
        };

        // 使用List副本进行遍历
        List<GameObject> framesCopy = new List<GameObject>(frame);
        foreach (GameObject existingFrame in framesCopy)
        {
            if (existingFrame == null) continue;

            // 获取当前位置索引
            int currentIndex = GetCurrentPointIndex(existingFrame);
            if (currentIndex >= framePoint.Count - 1)
            {
                frame.Remove(existingFrame);
                Destroy(existingFrame);
                continue;
            }

            // 移动到下一个点
            int nextIndex = currentIndex + 1;
            Sequence sequence = DOTween.Sequence();
            sequence.Join(existingFrame.transform.DOMove(framePoint[nextIndex].transform.position, moveDuration));
            sequence.Join(existingFrame.transform.DOScale(scales[nextIndex], scaleDuration));
        }
    }

    private int GetCurrentPointIndex(GameObject obj)
    {
        for (int i = 0; i < framePoint.Count; i++)
        {
            if (Vector3.Distance(obj.transform.position, framePoint[i].transform.position) < 0.1f)
            {
                return i;
            }
        }
        return 0;
    }

    public void CleraModelDiaplayBox()
    {
        foreach (var item in frame)
        {
            Destroy(item);
        }
        frame.Clear();
    }
}
