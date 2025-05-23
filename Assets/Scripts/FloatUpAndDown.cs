using UnityEngine;

public class FloatUpAndDown : MonoBehaviour
{
    // ���¸������ٶ�
    public float speed = 1.0f;
    // ���¸����ķ��ȣ����߶ȣ�
    public float magnitude = 1.0f;

    // ��ʼλ�ã����ڼ���ƫ����
    private Vector3 startPos;

    void Start()
    {
        // ��ȡ����ĳ�ʼλ��
        startPos = transform.position;
    }

    void Update()
    {
        // ����y�᷽���ϵ�ƫ����������ʹ��Mathf.Sin����������һ�����Ҳ���
        float yOffset = Mathf.Sin(Time.time * speed) * magnitude;

        // ���������λ��
        transform.position = startPos + new Vector3(0, yOffset, 0);
    }
}
