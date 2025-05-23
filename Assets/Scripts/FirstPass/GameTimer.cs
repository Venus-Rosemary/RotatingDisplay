using UnityEngine;
using System.Collections;
using TMPro;

public class GameTimer : Singleton<GameTimer>
{
    public TextMeshProUGUI timerText;
    public float totalTime = 180f; // 3分钟
    private float currentTime;
    private bool isRunning = false;
    private ConveyorBelt conveyorBelt;

    private void Start()
    {
        conveyorBelt = FindObjectOfType<ConveyorBelt>();
        currentTime = totalTime;
        UpdateTimerDisplay();
    }

    public void StartTimer()
    {
        if (!isRunning)
        {
            isRunning = true;
            StartCoroutine(TimerCoroutine());
        }
    }

    private IEnumerator TimerCoroutine()
    {
        while (currentTime > 0 && isRunning)
        {
            yield return new WaitForSeconds(1f);
            currentTime--;

            UpdateTimerDisplay();

            // 时间结束
            if (currentTime <= 0)
            {
                GameOver();
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("倒计时：{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void GameOver()
    {
        isRunning = false;
        if (conveyorBelt != null)
        {
            conveyorBelt.ResetConveyorBelt();
            ModelInDisplayBox.Instance.currentModel = null; // 停止生成新模型
        }
        // 这里添加第一关游戏结束的其他逻辑
    }

    public void ResetTimer()
    {
        isRunning = false;
        currentTime = totalTime;
        UpdateTimerDisplay();
    }
}