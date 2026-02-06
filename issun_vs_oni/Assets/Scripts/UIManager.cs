using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public Text timeText;
    public Text scoreText;
    public Slider hpBar;

    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public Text resultTitleText;
    public Text finalScoreText;
    public Button restartButton;

    void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (hpBar != null)
        {
            hpBar.maxValue = GameManager.Instance.oniMaxHP;
            hpBar.value = GameManager.Instance.oniMaxHP;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        // 残り時間
        if (timeText != null)
        {
            int seconds = Mathf.CeilToInt(GameManager.Instance.RemainingTime);
            timeText.text = $"TIME: {seconds}";
        }

        // スコア
        if (scoreText != null)
        {
            scoreText.text = $"SCORE: {GameManager.Instance.Score}";
        }

        // 鬼HP
        if (hpBar != null)
        {
            hpBar.value = GameManager.Instance.OniCurrentHP;
        }
    }

    public void ShowGameOver(bool isCleared)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (resultTitleText != null)
        {
            resultTitleText.text = isCleared ? "CLEAR!" : "GAME OVER";
            resultTitleText.color = isCleared ? Color.yellow : Color.red;
        }

        if (finalScoreText != null)
        {
            string bonus = isCleared ? "\n(No Damage Bonus: +500)" : "";
            finalScoreText.text = $"SCORE: {GameManager.Instance.Score}\nHITS: {GameManager.Instance.HitCount}{bonus}";
        }
    }

    void OnRestartClicked()
    {
        GameManager.Instance.Restart();
    }
}
