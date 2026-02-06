using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 金太郎 vs クマ 相撲 - メインゲームマネージャー
///
/// 【WebGL向け注意点】
/// - Time.timeScaleの変更はWebGLでも動作するが、pauseにはApplication.runInBackgroundを考慮
/// - Input.GetMouseButtonDown(0)はタッチとクリック両対応
/// - WebGLではPlayerPrefsの保存タイミングに注意（明示的にSave()を呼ぶ）
/// - Audioはユーザーインタラクション後でないと再生されない（ブラウザ制限）
/// - BuildSettingsでCompression FormatはGzipかBrotliを推奨
/// </summary>
public class SumoGameManager : MonoBehaviour
{
    // ========== ゲーム状態 ==========
    private enum GameState
    {
        Ready,      // 開始待ち
        Playing,    // プレイ中
        Win,        // 金太郎の勝ち
        Lose        // クマの勝ち
    }
    private GameState currentState = GameState.Ready;

    // ========== UI参照 ==========
    [Header("キャラクター")]
    [SerializeField] private RectTransform kintaroTransform;  // 金太郎
    [SerializeField] private RectTransform bearTransform;      // クマ

    [Header("ゲージUI")]
    [SerializeField] private RectTransform gaugeBackground;    // ゲージ背景
    [SerializeField] private RectTransform gaugePointer;       // 動くポインター
    [SerializeField] private RectTransform successZone;        // 成功ゾーン表示

    [Header("テキスト")]
    [SerializeField] private Text instructionText;             // 説明テキスト
    [SerializeField] private Text resultText;                  // 結果テキスト

    [Header("土俵ライン")]
    [SerializeField] private RectTransform topLine;            // 上の土俵ライン
    [SerializeField] private RectTransform bottomLine;         // 下の土俵ライン

    // ========== ゲームパラメータ ==========
    [Header("ゲーム設定")]
    [SerializeField] private float gaugeSpeed = 2.0f;          // ゲージ速度
    [SerializeField] private float successZoneMin = 0.45f;     // 成功ゾーン下限
    [SerializeField] private float successZoneMax = 0.55f;     // 成功ゾーン上限
    [SerializeField] private float advanceAmount = 30f;        // 成功時の前進量
    [SerializeField] private float pushBackAmount = 50f;       // 失敗時の後退量
    [SerializeField] private float winThreshold = 200f;        // 勝敗判定の位置閾値

    // ========== 内部変数 ==========
    private float gaugeValue = 0f;                             // 0〜1のゲージ値
    private int gaugeDirection = 1;                            // ゲージ移動方向
    private float battlePosition = 0f;                         // 押し合い位置（+で金太郎有利、-でクマ有利）
    private float initialKintaroY;
    private float initialBearY;

    // ========== Unity ライフサイクル ==========

    private void Start()
    {
        // 初期位置を保存
        if (kintaroTransform != null) initialKintaroY = kintaroTransform.anchoredPosition.y;
        if (bearTransform != null) initialBearY = bearTransform.anchoredPosition.y;

        ResetGame();
    }

    private void Update()
    {
        switch (currentState)
        {
            case GameState.Ready:
                UpdateReady();
                break;
            case GameState.Playing:
                UpdatePlaying();
                break;
            case GameState.Win:
            case GameState.Lose:
                UpdateGameOver();
                break;
        }
    }

    // ========== 状態別Update ==========

    private void UpdateReady()
    {
        // タップで開始
        if (Input.GetMouseButtonDown(0))
        {
            currentState = GameState.Playing;
            if (instructionText != null) instructionText.text = "タップ！";
        }
    }

    private void UpdatePlaying()
    {
        // ゲージを動かす
        UpdateGauge();

        // タップ判定
        if (Input.GetMouseButtonDown(0))
        {
            JudgeTiming();
        }

        // キャラクター位置を更新
        UpdateCharacterPositions();

        // 勝敗判定
        CheckWinCondition();
    }

    private void UpdateGameOver()
    {
        // タップでリトライ
        if (Input.GetMouseButtonDown(0))
        {
            ResetGame();
        }
    }

    // ========== ゲージ処理 ==========

    private void UpdateGauge()
    {
        // 0〜1の範囲で往復
        gaugeValue += gaugeDirection * gaugeSpeed * Time.deltaTime;

        // 端で反転
        if (gaugeValue >= 1f)
        {
            gaugeValue = 1f;
            gaugeDirection = -1;
        }
        else if (gaugeValue <= 0f)
        {
            gaugeValue = 0f;
            gaugeDirection = 1;
        }

        // ポインター位置を更新
        UpdatePointerPosition();
    }

    private void UpdatePointerPosition()
    {
        if (gaugePointer == null || gaugeBackground == null) return;

        float bgWidth = gaugeBackground.rect.width;
        float pointerX = (gaugeValue - 0.5f) * bgWidth;
        gaugePointer.anchoredPosition = new Vector2(pointerX, gaugePointer.anchoredPosition.y);
    }

    // ========== タイミング判定 ==========

    private void JudgeTiming()
    {
        bool isSuccess = (gaugeValue >= successZoneMin && gaugeValue <= successZoneMax);

        if (isSuccess)
        {
            // 成功：金太郎が前進
            battlePosition += advanceAmount;
            ShowFeedback("よし！", Color.green);
        }
        else
        {
            // 失敗：クマに押し返される
            battlePosition -= pushBackAmount;
            ShowFeedback("うっ！", Color.red);
        }
    }

    private void ShowFeedback(string text, Color color)
    {
        if (instructionText != null)
        {
            instructionText.text = text;
            instructionText.color = color;
        }
    }

    // ========== 位置管理 ==========

    private void UpdateCharacterPositions()
    {
        if (kintaroTransform == null || bearTransform == null) return;

        // battlePositionに応じてキャラクターを移動
        // 正の値：金太郎が上に進む（クマ側へ）
        // 負の値：金太郎が下に下がる（押される）

        float moveRatio = battlePosition / winThreshold;
        float maxMove = winThreshold * 0.8f;  // 最大移動量

        // 金太郎の位置
        float kintaroY = initialKintaroY + battlePosition * 0.5f;
        kintaroTransform.anchoredPosition = new Vector2(
            kintaroTransform.anchoredPosition.x,
            kintaroY
        );

        // クマの位置（金太郎と連動して押し合う）
        float bearY = initialBearY + battlePosition * 0.5f;
        bearTransform.anchoredPosition = new Vector2(
            bearTransform.anchoredPosition.x,
            bearY
        );
    }

    // ========== 勝敗判定 ==========

    private void CheckWinCondition()
    {
        if (battlePosition >= winThreshold)
        {
            // 金太郎の勝ち（クマを土俵外に押し出した）
            currentState = GameState.Win;
            ShowResult("金太郎の勝ち！", Color.yellow);
        }
        else if (battlePosition <= -winThreshold)
        {
            // クマの勝ち（金太郎が土俵外に出た）
            currentState = GameState.Lose;
            ShowResult("クマの勝ち...", Color.gray);
        }
    }

    private void ShowResult(string text, Color color)
    {
        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = text + "\n\nタップでリトライ";
            resultText.color = color;
        }
        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(false);
        }
    }

    // ========== リセット ==========

    private void ResetGame()
    {
        currentState = GameState.Ready;
        gaugeValue = 0.5f;
        gaugeDirection = 1;
        battlePosition = 0f;

        // キャラクター位置をリセット
        if (kintaroTransform != null)
        {
            kintaroTransform.anchoredPosition = new Vector2(
                kintaroTransform.anchoredPosition.x,
                initialKintaroY
            );
        }
        if (bearTransform != null)
        {
            bearTransform.anchoredPosition = new Vector2(
                bearTransform.anchoredPosition.x,
                initialBearY
            );
        }

        // UI更新
        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(true);
            instructionText.text = "タップで開始";
            instructionText.color = Color.white;
        }
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        UpdatePointerPosition();
    }

    // ========== エディタ用：自動セットアップ ==========
#if UNITY_EDITOR
    [ContextMenu("Setup UI References (Editor Only)")]
    private void SetupUIReferences()
    {
        Debug.Log("UIの手動設定が必要です。Hierarchy内のオブジェクトをInspectorにドラッグしてください。");
    }
#endif
}
