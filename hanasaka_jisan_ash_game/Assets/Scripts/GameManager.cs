using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の進行を管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("ゲーム設定")]
    [SerializeField] private float gameDuration = 30f;
    [SerializeField] private int minAshRequired = 10;
    [SerializeField] private int maxAshAllowed = 20;

    [Header("参照")]
    [SerializeField] private TreeController treeController;
    [SerializeField] private UIController uiController;
    [SerializeField] private AshSpawner ashSpawner;

    private float remainingTime;
    private int totalAshUsed;
    private bool isGameOver;
    private bool hasSucceeded;

    public bool IsGameOver => isGameOver;
    public int TotalAshUsed => totalAshUsed;
    public int MinAshRequired => minAshRequired;
    public int MaxAshAllowed => maxAshAllowed;

    private void Awake()
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

    private void Start()
    {
        InitializeGame();
    }

    private void Update()
    {
        if (isGameOver) return;

        remainingTime -= Time.deltaTime;
        uiController.UpdateTimer(remainingTime);

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            EndGame();
        }
    }

    private void InitializeGame()
    {
        remainingTime = gameDuration;
        totalAshUsed = 0;
        isGameOver = false;
        hasSucceeded = false;

        uiController.UpdateTimer(remainingTime);
        uiController.UpdateAshCount(totalAshUsed, minAshRequired, maxAshAllowed);
        uiController.HideResult();
        treeController.ResetTree();
    }

    /// <summary>
    /// 灰を使用した時に呼ばれる
    /// </summary>
    public void OnAshUsed()
    {
        if (isGameOver) return;

        totalAshUsed++;
        uiController.UpdateAshCount(totalAshUsed, minAshRequired, maxAshAllowed);

        // 灰を使いすぎたら即失敗
        if (totalAshUsed > maxAshAllowed)
        {
            hasSucceeded = false;
            EndGame();
        }
    }

    /// <summary>
    /// 枯れ木に灰が当たった時に呼ばれる
    /// </summary>
    public void OnAshHitTree()
    {
        treeController.AddAshHit();

        // 満開判定
        if (treeController.AshHitCount >= minAshRequired && !treeController.IsBloom)
        {
            treeController.Bloom();
        }
    }

    private void EndGame()
    {
        isGameOver = true;
        ashSpawner.SetEnabled(false);

        // 判定ロジック
        // 成功条件: 灰が範囲内 かつ 木が満開
        if (totalAshUsed >= minAshRequired && totalAshUsed <= maxAshAllowed && treeController.IsBloom)
        {
            hasSucceeded = true;
        }
        else
        {
            hasSucceeded = false;
        }

        string resultMessage = GetResultMessage();
        uiController.ShowResult(hasSucceeded, resultMessage);
    }

    private string GetResultMessage()
    {
        if (hasSucceeded)
        {
            return "大成功！\n桜が満開になりました！";
        }

        if (totalAshUsed < minAshRequired)
        {
            return "失敗...\n灰が足りませんでした";
        }

        if (totalAshUsed > maxAshAllowed)
        {
            return "失敗...\n灰をまきすぎました";
        }

        return "失敗...\n木に灰が当たりませんでした";
    }

    /// <summary>
    /// リトライボタン用
    /// </summary>
    public void RetryGame()
    {
        // WebGLでも安全にシーンをリロード
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
