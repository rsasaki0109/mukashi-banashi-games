using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float gameTime = 60f;
    public int oniMaxHP = 100;

    [Header("References")]
    public PlayerController player;
    public OniController oni;
    public UIManager ui;

    // Game State
    public float RemainingTime { get; private set; }
    public int OniCurrentHP { get; private set; }
    public int Score { get; private set; }
    public int HitCount { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsCleared { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        RemainingTime = gameTime;
        OniCurrentHP = oniMaxHP;
        Score = 0;
        HitCount = 0;
        IsGameOver = false;
        IsCleared = false;
    }

    void Update()
    {
        if (IsGameOver || IsCleared) return;

        // タイマー更新
        RemainingTime -= Time.deltaTime;

        // 時間切れ = クリア
        if (RemainingTime <= 0)
        {
            RemainingTime = 0;
            GameClear();
        }

        // スコア更新（生存時間）
        Score = Mathf.FloorToInt((gameTime - RemainingTime) * 10) + HitCount * 100;
    }

    public void DamageOni(int damage)
    {
        if (IsGameOver || IsCleared) return;

        OniCurrentHP -= damage;
        HitCount++;

        if (OniCurrentHP <= 0)
        {
            OniCurrentHP = 0;
            GameClear();
        }
    }

    public void PlayerHit()
    {
        if (IsGameOver || IsCleared) return;
        IsGameOver = true;
        ui.ShowGameOver(false);
    }

    void GameClear()
    {
        IsCleared = true;
        // ノーダメージボーナス
        int noDamageBonus = 500;
        Score += noDamageBonus;
        ui.ShowGameOver(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 難易度取得（経過時間に応じて0-2）
    public int GetDifficultyLevel()
    {
        float elapsed = gameTime - RemainingTime;
        if (elapsed < 20f) return 0;
        if (elapsed < 40f) return 1;
        return 2;
    }

    // 攻撃インターバル取得
    public float GetAttackInterval()
    {
        int level = GetDifficultyLevel();
        switch (level)
        {
            case 0: return 3f;
            case 1: return 2f;
            default: return 1.5f;
        }
    }
}
