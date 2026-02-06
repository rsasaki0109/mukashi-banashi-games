using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// エディタ上でシーンを自動セットアップするスクリプト
/// メニュー: Tools > Setup Sumo Game Scene
/// </summary>
public class SumoGameSetup : EditorWindow
{
    [MenuItem("Tools/Setup Sumo Game Scene")]
    public static void SetupScene()
    {
        // 既存のGameManagerがあれば警告
        if (Object.FindObjectOfType<SumoGameManager>() != null)
        {
            if (!EditorUtility.DisplayDialog("確認",
                "既にSumoGameManagerが存在します。新しく作成しますか？",
                "はい", "キャンセル"))
            {
                return;
            }
        }

        // ========== Canvas作成 ==========
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ========== 背景 ==========
        GameObject bgGO = CreateImage(canvasGO.transform, "Background",
            new Vector2(0, 0), new Vector2(1080, 1920),
            new Color(0.9f, 0.85f, 0.7f)); // 土俵っぽい色
        bgGO.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        bgGO.GetComponent<RectTransform>().anchorMax = Vector2.one;
        bgGO.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        bgGO.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        // ========== 土俵（円） ==========
        GameObject dohyoGO = CreateImage(canvasGO.transform, "Dohyo",
            new Vector2(0, 0), new Vector2(900, 900),
            new Color(0.8f, 0.7f, 0.5f));

        // ========== 土俵ライン（上） ==========
        GameObject topLineGO = CreateImage(canvasGO.transform, "TopLine",
            new Vector2(0, 350), new Vector2(800, 20),
            Color.red);

        // ========== 土俵ライン（下） ==========
        GameObject bottomLineGO = CreateImage(canvasGO.transform, "BottomLine",
            new Vector2(0, -350), new Vector2(800, 20),
            Color.red);

        // ========== クマ（上部） ==========
        GameObject bearGO = CreateCharacter(canvasGO.transform, "Bear",
            new Vector2(0, 400), new Vector2(200, 200),
            new Color(0.4f, 0.25f, 0.1f), "熊");

        // ========== 金太郎（下部） ==========
        GameObject kintaroGO = CreateCharacter(canvasGO.transform, "Kintaro",
            new Vector2(0, -400), new Vector2(200, 200),
            new Color(1f, 0.6f, 0.6f), "金");

        // ========== ゲージ背景 ==========
        GameObject gaugeBackgroundGO = CreateImage(canvasGO.transform, "GaugeBackground",
            new Vector2(0, -700), new Vector2(600, 60),
            new Color(0.2f, 0.2f, 0.2f));

        // ========== 成功ゾーン ==========
        GameObject successZoneGO = CreateImage(gaugeBackgroundGO.transform, "SuccessZone",
            new Vector2(0, 0), new Vector2(60, 50), // 10%の幅
            new Color(0.3f, 0.8f, 0.3f, 0.8f));

        // ========== ゲージポインター ==========
        GameObject gaugePointerGO = CreateImage(gaugeBackgroundGO.transform, "GaugePointer",
            new Vector2(0, 0), new Vector2(20, 80),
            Color.white);

        // ========== 説明テキスト ==========
        GameObject instructionGO = CreateText(canvasGO.transform, "InstructionText",
            new Vector2(0, -550), new Vector2(800, 100),
            "タップで開始", 60, Color.white);

        // ========== 結果テキスト ==========
        GameObject resultGO = CreateText(canvasGO.transform, "ResultText",
            new Vector2(0, 0), new Vector2(800, 400),
            "", 80, Color.yellow);
        resultGO.SetActive(false);

        // ========== GameManager作成 ==========
        GameObject managerGO = new GameObject("GameManager");
        SumoGameManager manager = managerGO.AddComponent<SumoGameManager>();

        // SerializedObjectを使って参照を設定
        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("kintaroTransform").objectReferenceValue = kintaroGO.GetComponent<RectTransform>();
        so.FindProperty("bearTransform").objectReferenceValue = bearGO.GetComponent<RectTransform>();
        so.FindProperty("gaugeBackground").objectReferenceValue = gaugeBackgroundGO.GetComponent<RectTransform>();
        so.FindProperty("gaugePointer").objectReferenceValue = gaugePointerGO.GetComponent<RectTransform>();
        so.FindProperty("successZone").objectReferenceValue = successZoneGO.GetComponent<RectTransform>();
        so.FindProperty("instructionText").objectReferenceValue = instructionGO.GetComponent<Text>();
        so.FindProperty("resultText").objectReferenceValue = resultGO.GetComponent<Text>();
        so.FindProperty("topLine").objectReferenceValue = topLineGO.GetComponent<RectTransform>();
        so.FindProperty("bottomLine").objectReferenceValue = bottomLineGO.GetComponent<RectTransform>();
        so.ApplyModifiedProperties();

        // ========== EventSystem作成 ==========
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ========== カメラ設定 ==========
        Camera.main.backgroundColor = new Color(0.2f, 0.2f, 0.3f);

        Debug.Log("シーンセットアップ完了！");
        EditorUtility.DisplayDialog("完了", "金太郎 vs クマ 相撲 のシーンセットアップが完了しました！\n\nPlay ボタンで動作確認できます。", "OK");
    }

    private static GameObject CreateImage(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = color;

        return go;
    }

    private static GameObject CreateCharacter(Transform parent, string name, Vector2 position, Vector2 size, Color color, string label)
    {
        GameObject go = CreateImage(parent, name, position, size, color);

        // キャラクターラベル
        GameObject textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);

        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text text = textGO.AddComponent<Text>();
        text.text = label;
        text.fontSize = 100;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return go;
    }

    private static GameObject CreateText(Transform parent, string name, Vector2 position, Vector2 size, string content, int fontSize, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = go.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // アウトライン追加
        Outline outline = go.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(3, -3);

        return go;
    }
}
