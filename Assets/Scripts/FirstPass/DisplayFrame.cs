using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayFrame : Singleton<DisplayFrame>
{
    public GameObject framePerfab;
    public List<GameObject> framePoint = new List<GameObject>();
    [SerializeField]private List<GameObject> frame = new List<GameObject>();

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


        ModelInDisplayBox.Instance.SpawnModelOfBox(newFrame.transform, modelSize);

        newFrame.transform.localScale = Vector3.zero;



        Sequence sequence = DOTween.Sequence();
        sequence.Join(newFrame.transform.DOScale(Vector3.one * MagnificationFactor, scaleDuration));
        sequence.Join(newFrame.transform.DOMove(framePoint[1].transform.position, moveDuration));
    }

    private void MoveExistingFrame()
    {
        Vector3[] scales = new Vector3[] { 
            Vector3.zero * MagnificationFactor,
            Vector3.one * MagnificationFactor,
            Vector3.one * 2 * MagnificationFactor,
            Vector3.one * MagnificationFactor,
            Vector3.zero * MagnificationFactor
        };

        // 使用List副本进行遍历，因为可能会在遍历过程中移除元素
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

    public void ClearFrameObject()
    {
        foreach (var item in frame)
        {
            Destroy(item);
        }
        frame.Clear();
    }
}
