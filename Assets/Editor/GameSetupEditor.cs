using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// ゲームのセットアップを自動化するエディタスクリプト
/// メニュー: Tools > Hanasaka Game > Setup Scene
/// </summary>
public class GameSetupEditor : Editor
{
    [MenuItem("Tools/Hanasaka Game/Setup Scene")]
    public static void SetupScene()
    {
        // 既存オブジェクトをクリーンアップ（カメラ以外）
        foreach (var go in FindObjectsOfType<GameObject>())
        {
            if (go.GetComponent<Camera>() == null && go.transform.parent == null)
            {
                if (go.name != "Main Camera")
                {
                    DestroyImmediate(go);
                }
            }
        }

        // カメラ設定
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = new Color(0.5f, 0.8f, 1f); // 空色
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5f;
        }

        // Canvas作成
        GameObject canvasObj = CreateCanvas();

        // UI要素作成
        Text timerText = CreateUIText(canvasObj.transform, "TimerText", "残り時間: 30秒",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(300, 50));

        Text ashCountText = CreateUIText(canvasObj.transform, "AshCountText", "灰: 0 / 10～20",
            new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -70), new Vector2(300, 50));

        Text instructionText = CreateUIText(canvasObj.transform, "InstructionText", "クリック/タップで灰をまこう！",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(400, 50));
        instructionText.alignment = TextAnchor.MiddleCenter;

        // 結果パネル作成
        GameObject resultPanel = CreateResultPanel(canvasObj.transform);

        // GameManager作成
        GameObject gameManagerObj = new GameObject("GameManager");
        GameManager gameManager = gameManagerObj.AddComponent<GameManager>();

        // UIController作成
        UIController uiController = canvasObj.AddComponent<UIController>();

        // AshSpawner作成
        GameObject ashSpawnerObj = new GameObject("AshSpawner");
        AshSpawner ashSpawner = ashSpawnerObj.AddComponent<AshSpawner>();

        // 枯れ木作成
        GameObject deadTree = CreateTree("DeadTree", new Color(0.4f, 0.3f, 0.2f), true);

        // 満開の木作成
        GameObject bloomingTree = CreateTree("BloomingTree", new Color(1f, 0.7f, 0.8f), false);

        // TreeController作成
        GameObject treeControllerObj = new GameObject("TreeController");
        TreeController treeController = treeControllerObj.AddComponent<TreeController>();

        // 灰Prefab作成
        GameObject ashPrefab = CreateAshPrefab();

        // SerializedObjectで参照を設定
        SerializedObject gmSo = new SerializedObject(gameManager);
        gmSo.FindProperty("treeController").objectReferenceValue = treeController;
        gmSo.FindProperty("uiController").objectReferenceValue = uiController;
        gmSo.FindProperty("ashSpawner").objectReferenceValue = ashSpawner;
        gmSo.ApplyModifiedProperties();

        SerializedObject uiSo = new SerializedObject(uiController);
        uiSo.FindProperty("timerText").objectReferenceValue = timerText;
        uiSo.FindProperty("ashCountText").objectReferenceValue = ashCountText;
        uiSo.FindProperty("instructionText").objectReferenceValue = instructionText;
        uiSo.FindProperty("resultPanel").objectReferenceValue = resultPanel;
        uiSo.FindProperty("resultText").objectReferenceValue = resultPanel.transform.Find("ResultText").GetComponent<Text>();
        uiSo.FindProperty("retryButton").objectReferenceValue = resultPanel.transform.Find("RetryButton").GetComponent<Button>();
        uiSo.ApplyModifiedProperties();

        SerializedObject tcSo = new SerializedObject(treeController);
        tcSo.FindProperty("deadTree").objectReferenceValue = deadTree;
        tcSo.FindProperty("bloomingTree").objectReferenceValue = bloomingTree;
        tcSo.FindProperty("deadTreeRenderer").objectReferenceValue = deadTree.GetComponent<SpriteRenderer>();
        tcSo.FindProperty("bloomingTreeRenderer").objectReferenceValue = bloomingTree.GetComponent<SpriteRenderer>();
        tcSo.ApplyModifiedProperties();

        SerializedObject asSo = new SerializedObject(ashSpawner);
        asSo.FindProperty("ashPrefab").objectReferenceValue = ashPrefab;
        asSo.ApplyModifiedProperties();

        Debug.Log("Hanasaka Game: Scene setup complete!");
        Debug.Log("IMPORTANT: Please save the Ash prefab to Assets/Prefabs folder and re-assign it to AshSpawner.");
    }

    private static GameObject CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        return canvasObj;
    }

    private static Text CreateUIText(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMin;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        Text textComp = textObj.AddComponent<Text>();
        textComp.text = text;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComp.fontSize = 28;
        textComp.color = Color.white;
        textComp.alignment = TextAnchor.MiddleLeft;

        // 影を追加して視認性向上
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = Color.black;
        shadow.effectDistance = new Vector2(2, -2);

        return textComp;
    }

    private static GameObject CreateResultPanel(Transform parent)
    {
        GameObject panel = new GameObject("ResultPanel");
        panel.transform.SetParent(parent);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(400, 250);

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.8f);

        // 結果テキスト
        GameObject resultTextObj = new GameObject("ResultText");
        resultTextObj.transform.SetParent(panel.transform);

        RectTransform rtText = resultTextObj.AddComponent<RectTransform>();
        rtText.anchorMin = new Vector2(0.5f, 0.6f);
        rtText.anchorMax = new Vector2(0.5f, 0.6f);
        rtText.pivot = new Vector2(0.5f, 0.5f);
        rtText.anchoredPosition = Vector2.zero;
        rtText.sizeDelta = new Vector2(380, 120);

        Text resultText = resultTextObj.AddComponent<Text>();
        resultText.text = "結果";
        resultText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        resultText.fontSize = 32;
        resultText.color = Color.white;
        resultText.alignment = TextAnchor.MiddleCenter;

        // リトライボタン
        GameObject buttonObj = new GameObject("RetryButton");
        buttonObj.transform.SetParent(panel.transform);

        RectTransform rtButton = buttonObj.AddComponent<RectTransform>();
        rtButton.anchorMin = new Vector2(0.5f, 0.2f);
        rtButton.anchorMax = new Vector2(0.5f, 0.2f);
        rtButton.pivot = new Vector2(0.5f, 0.5f);
        rtButton.anchoredPosition = Vector2.zero;
        rtButton.sizeDelta = new Vector2(200, 50);

        Image buttonBg = buttonObj.AddComponent<Image>();
        buttonBg.color = new Color(0.3f, 0.6f, 0.3f);
        buttonObj.AddComponent<Button>();

        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform);

        RectTransform rtBtnText = buttonTextObj.AddComponent<RectTransform>();
        rtBtnText.anchorMin = Vector2.zero;
        rtBtnText.anchorMax = Vector2.one;
        rtBtnText.pivot = new Vector2(0.5f, 0.5f);
        rtBtnText.anchoredPosition = Vector2.zero;
        rtBtnText.sizeDelta = Vector2.zero;

        Text buttonText = buttonTextObj.AddComponent<Text>();
        buttonText.text = "もう一度";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;

        panel.SetActive(false);
        return panel;
    }

    private static GameObject CreateTree(string name, Color color, bool active)
    {
        GameObject tree = new GameObject(name);
        tree.transform.position = new Vector3(0, -1f, 0);
        tree.tag = "Tree";

        SpriteRenderer sr = tree.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = color;
        tree.transform.localScale = new Vector3(3f, 4f, 1f);

        // 当たり判定
        CircleCollider2D col = tree.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        tree.SetActive(active);
        return tree;
    }

    private static GameObject CreateAshPrefab()
    {
        GameObject ash = new GameObject("Ash");

        SpriteRenderer sr = ash.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        ash.transform.localScale = new Vector3(0.2f, 0.2f, 1f);

        CircleCollider2D col = ash.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        ash.AddComponent<Ash>();

        return ash;
    }

    private static Sprite CreateCircleSprite()
    {
        // Unity組み込みのKnobスプライトを使用（円形）
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
    }

    [MenuItem("Tools/Hanasaka Game/Create Ash Prefab")]
    public static void CreateAndSaveAshPrefab()
    {
        // Prefabフォルダ作成
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        GameObject ash = CreateAshPrefab();

        // Prefabとして保存
        string prefabPath = "Assets/Prefabs/Ash.prefab";
        PrefabUtility.SaveAsPrefabAsset(ash, prefabPath);
        DestroyImmediate(ash);

        Debug.Log($"Ash prefab created at: {prefabPath}");
        Debug.Log("Please assign this prefab to AshSpawner.ashPrefab");
    }

    [MenuItem("Tools/Hanasaka Game/Setup Tags")]
    public static void SetupTags()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == "Tree")
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            tagsProp.GetArrayElementAtIndex(0).stringValue = "Tree";
            tagManager.ApplyModifiedProperties();
            Debug.Log("Tag 'Tree' has been added.");
        }
        else
        {
            Debug.Log("Tag 'Tree' already exists.");
        }
    }
}
