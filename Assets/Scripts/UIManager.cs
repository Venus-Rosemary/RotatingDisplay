using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : Singleton<UIManager>
{
    [System.Serializable]
    public class LevelUI
    {
        [Header("分数显示")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI addScoreText;
        [Header("倒计时显示")]
        public TextMeshProUGUI timerText;
        [Header("完成度显示")]
        public TextMeshProUGUI groupText;
        [HideInInspector]
        public int currentScore = 0;
        [HideInInspector]
        public Vector2 addScoreOriginalPos;
    }

    [Header("动画设置")]
    public float scoreFlyDuration = 0.5f;
    public float scoreShowDuration = 1f;
    public Vector2 scoreOffset = new Vector2(0, 50f);

    [Header("关卡UI设置")]
    public LevelUI firstLevelUI;
    public LevelUI secondLevelUI;
    public LevelUI thirdLevelUI;
    public LevelUI fourLevelUI;

    private void Start()
    {
        InitializeLevelUI(firstLevelUI);
        InitializeLevelUI(secondLevelUI);
        InitializeLevelUI(thirdLevelUI);
    }

    private void InitializeLevelUI(LevelUI ui)
    {
        if (ui.addScoreText != null)
        {
            ui.addScoreOriginalPos = ui.addScoreText.rectTransform.anchoredPosition;
            ui.addScoreText.gameObject.SetActive(false);
        }
        UpdateLevelScore(ui, 0, 0);
    }

    #region 通用UI更新方法
    private void UpdateLevelScore(LevelUI ui, int newScore, int scoreChange)
    {
        ui.currentScore = newScore;
        
        if (ui.scoreText != null)
        {
            ui.scoreText.text = $"分数: {ui.currentScore}";
        }

        if (ui.addScoreText != null && scoreChange != 0)
        {
            ShowScoreChangeAnimation(ui, scoreChange);
        }
    }

    private void ShowScoreChangeAnimation(LevelUI ui, int scoreChange)
    {
        ui.addScoreText.gameObject.SetActive(true);
        ui.addScoreText.rectTransform.anchoredPosition = ui.addScoreOriginalPos;
        
        ui.addScoreText.text = (scoreChange > 0 ? "+" : "") + scoreChange.ToString();
        ui.addScoreText.color = scoreChange > 0 ? Color.green : Color.red;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(ui.addScoreText.rectTransform
            .DOAnchorPos(ui.addScoreOriginalPos + scoreOffset, scoreFlyDuration)
            .SetEase(Ease.OutCubic));

        sequence.Join(ui.addScoreText.DOFade(0, scoreFlyDuration)
            .SetEase(Ease.InQuad));

        sequence.OnComplete(() => {
            ui.addScoreText.gameObject.SetActive(false);
            ui.addScoreText.alpha = 1;
        });
    }

    public void UpdateTimer(LevelUI ui, float time)
    {
        if (ui.timerText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            ui.timerText.text = string.Format("倒计时：{0:00}:{1:00}", minutes, seconds);
        }
    }
    #endregion

    #region 公共接口
    // 第一关
    public void UpdateScore(int newScore, int scoreChange)
    {
        UpdateLevelScore(firstLevelUI, newScore, scoreChange);
    }

    // 第二关
    public void UpdateSecondScore(int newScore, int scoreChange)
    {
        UpdateLevelScore(secondLevelUI, newScore, scoreChange);
    }

    public void UpdateSecondTimer(float time)
    {
        UpdateTimer(secondLevelUI, time);
    }

    // 第三关
    public void UpdateThirdScore(int newScore, int scoreChange)
    {
        UpdateLevelScore(thirdLevelUI, newScore, scoreChange);
    }

    public void UpdateThirdTimer(float time)
    {
        UpdateTimer(thirdLevelUI, time);
    }

    public void UpdateThirdGroup(int groupIndex, int numberGroup)
    {
        if (thirdLevelUI.groupText != null)
        {
            thirdLevelUI.groupText.text = $"组数： {groupIndex + 1} / {numberGroup}";
        }
    }

    // 第四关
    public void UpdateFourScore(int newScore, int scoreChange)
    {
        UpdateLevelScore(fourLevelUI, newScore, scoreChange);
    }

    public void UpdateFourTimer(float time)
    {
        UpdateTimer(fourLevelUI, time);
    }

    public void UpdateFourGroup(int groupIndex, int numberGroup)
    {
        if (fourLevelUI.groupText != null)
        {
            fourLevelUI.groupText.text = $"组数： {groupIndex} / {numberGroup}";
        }
    }
    #endregion
}