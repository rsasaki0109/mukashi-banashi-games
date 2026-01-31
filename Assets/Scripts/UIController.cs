using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI表示を制御するクラス
/// </summary>
public class UIController : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private Text timerText;
    [SerializeField] private Text ashCountText;
    [SerializeField] private Text instructionText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Text resultText;
    [SerializeField] private Button retryButton;

    [Header("色設定")]
    [SerializeField] private Color successColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color failureColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color warningColor = new Color(0.9f, 0.6f, 0.1f);
    [SerializeField] private Color normalColor = Color.white;

    private void Start()
    {
        if (instructionText != null)
        {
            instructionText.text = "クリック/タップで灰をまこう！";
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryButtonClicked);
        }

        HideResult();
    }

    public void UpdateTimer(float remainingTime)
    {
        if (timerText == null) return;

        int seconds = Mathf.CeilToInt(remainingTime);
        timerText.text = $"残り時間: {seconds}秒";

        // 残り5秒以下で警告色
        if (seconds <= 5)
        {
            timerText.color = warningColor;
        }
        else
        {
            timerText.color = normalColor;
        }
    }

    public void UpdateAshCount(int current, int min, int max)
    {
        if (ashCountText == null) return;

        ashCountText.text = $"灰: {current} / {min}～{max}";

        // 範囲内かどうかで色を変える
        if (current > max)
        {
            ashCountText.color = failureColor;
        }
        else if (current >= min)
        {
            ashCountText.color = successColor;
        }
        else
        {
            ashCountText.color = normalColor;
        }
    }

    public void ShowResult(bool success, string message)
    {
        if (resultPanel != null) resultPanel.SetActive(true);

        if (resultText != null)
        {
            resultText.text = message;
            resultText.color = success ? successColor : failureColor;
        }

        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(false);
        }
    }

    public void HideResult()
    {
        if (resultPanel != null) resultPanel.SetActive(false);

        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(true);
        }
    }

    private void OnRetryButtonClicked()
    {
        GameManager.Instance?.RetryGame();
    }
}
