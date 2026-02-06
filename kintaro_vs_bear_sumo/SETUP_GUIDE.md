# 金太郎 vs クマ 相撲 - セットアップガイド

## Unity プロジェクト設定

### 1. 新規プロジェクト作成
1. Unity Hub で「New Project」
2. テンプレート: **2D (URP)** または **2D**
3. プロジェクト名: `KintaroSumo`

### 2. スクリプト配置
```
Assets/
├── Scripts/
│   ├── SumoGameManager.cs      # メインスクリプト
│   └── Editor/
│       └── SumoGameSetup.cs    # エディタツール
```

### 3. シーン自動セットアップ
1. メニュー: **Tools > Setup Sumo Game Scene**
2. 自動でUI要素が配置される

### 4. ビルド設定
1. **File > Build Settings**
2. Platform: **WebGL** を選択
3. **Switch Platform** をクリック

### 5. Player Settings（重要）
```
Edit > Project Settings > Player

【Resolution and Presentation】
- Default Canvas Width: 1080
- Default Canvas Height: 1920
- Run In Background: ✓

【WebGL Template】
- Minimal

【Publishing Settings】
- Compression Format: Gzip（GitHub Pagesの場合）
- Decompression Fallback: ✓
```

### 6. ビルド
1. **File > Build Settings > Build**
2. 出力先フォルダ: `docs/` （GitHub Pages用）

---

## GitHub Pages 公開

### フォルダ構成
```
repository/
├── docs/                    # ← ビルド出力先
│   ├── index.html
│   ├── Build/
│   │   ├── docs.data.gz
│   │   ├── docs.framework.js.gz
│   │   ├── docs.loader.js
│   │   └── docs.wasm.gz
│   └── TemplateData/
│       └── ...
├── Assets/
│   └── Scripts/
│       └── ...
└── README.md
```

### GitHub Pages 設定
1. リポジトリの **Settings** タブ
2. 左メニュー: **Pages**
3. Source: **Deploy from a branch**
4. Branch: `main` / `docs` フォルダを選択
5. **Save**

### URL
```
https://<username>.github.io/<repository>/
```

---

## WebGL 注意点

### ブラウザ制限
```csharp
// 音声はユーザー操作後でないと再生不可
// 最初のタップ後にAudioSourceを再生する

// ローカルストレージ制限
// PlayerPrefsは使用可能だが、Safari Private モードでは制限あり
```

### パフォーマンス
```csharp
// GCを減らすためにキャッシュを活用
private Vector2 cachedPosition = Vector2.zero;

void Update()
{
    // 毎フレームnew Vector2()を避ける
    cachedPosition.x = someValue;
    cachedPosition.y = someValue;
    transform.position = cachedPosition;
}
```

### モバイル対応
```csharp
// タッチとマウスを統一処理
if (Input.GetMouseButtonDown(0))  // タップ・クリック両対応
{
    // 処理
}
```

---

## トラブルシューティング

### Q: ビルドが重い
A: Project Settings > Player > Publishing Settings で以下を確認
- Development Build: OFF
- Code Optimization: Speed
- Enable Exceptions: None

### Q: GitHub Pagesで表示されない
A:
1. Compression FormatをGzipに設定
2. Decompression Fallbackを有効化
3. ビルド先フォルダ名が `docs` であることを確認

### Q: スマホで操作できない
A:
1. Canvas ScalerがScale With Screen Sizeになっているか確認
2. EventSystemが存在するか確認
3. GraphicRaycasterがCanvasに付いているか確認

---

## ゲームパラメータ調整

`SumoGameManager` の Inspector で調整可能：

| パラメータ | デフォルト | 説明 |
|-----------|-----------|------|
| Gauge Speed | 2.0 | ゲージの速度 |
| Success Zone Min | 0.45 | 成功ゾーン開始位置 |
| Success Zone Max | 0.55 | 成功ゾーン終了位置 |
| Advance Amount | 30 | 成功時の前進量 |
| Push Back Amount | 50 | 失敗時の後退量 |
| Win Threshold | 200 | 勝敗判定の位置 |

**難易度調整のコツ：**
- 簡単にする: Success Zone を広げる (0.35〜0.65)
- 難しくする: Gauge Speed を上げる (3.0〜4.0)
