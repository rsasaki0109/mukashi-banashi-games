# クイックセットアップガイド

## Step 1: Unity プロジェクト作成

1. Unity Hub を開く
2. 「New Project」→「2D (Built-in Render Pipeline)」
3. プロジェクト名: `HanasakaJisan`
4. 作成

## Step 2: スクリプトをコピー

このリポジトリの以下をUnityプロジェクトにコピー:

```
Assets/
├── Scripts/
│   ├── GameManager.cs
│   ├── AshSpawner.cs
│   ├── Ash.cs
│   ├── TreeController.cs
│   └── UIController.cs
└── Editor/
    └── GameSetupEditor.cs
```

## Step 3: 自動セットアップ実行

Unity エディタで:

```
1. Tools > Hanasaka Game > Setup Tags
2. Tools > Hanasaka Game > Create Ash Prefab
3. Tools > Hanasaka Game > Setup Scene
4. AshSpawner の Inspector で Ash Prefab を設定
5. シーンを保存 (Ctrl+S)
```

## Step 4: WebGL ビルド

### Build Settings

1. `File > Build Settings`
2. プラットフォーム: `WebGL`
3. 「Switch Platform」
4. 「Player Settings」

### Player Settings

| カテゴリ | 項目 | 値 |
|----------|------|-----|
| Resolution | Default Canvas Width | 960 |
| Resolution | Default Canvas Height | 600 |
| Publishing Settings | Compression Format | Gzip |
| Publishing Settings | Decompression Fallback | ✓ ON |

### ビルド実行

1. 「Build」ボタン
2. 保存先: `docs/Build/`
3. ビルド完了を待つ

## Step 5: ファイル名調整

ビルド後、`docs/Build/` 内のファイル名を確認:

```
# 期待する構成:
docs/Build/
├── Build.loader.js       # ローダースクリプト
├── Build.data.gz         # ゲームデータ
├── Build.framework.js.gz # フレームワーク
└── Build.wasm.gz         # WebAssembly
```

ファイル名が異なる場合は `docs/index.html` 内のパスを修正:

```javascript
var config = {
    dataUrl: buildUrl + "/【実際のファイル名】.data.gz",
    frameworkUrl: buildUrl + "/【実際のファイル名】.framework.js.gz",
    codeUrl: buildUrl + "/【実際のファイル名】.wasm.gz",
    // ...
};
```

## Step 6: GitHub Pages 公開

```bash
# リポジトリにプッシュ
git add .
git commit -m "Add WebGL build for GitHub Pages"
git push origin main
```

GitHub リポジトリ設定:

1. Settings > Pages
2. Source: `Deploy from a branch`
3. Branch: `main` / `/docs`
4. Save

数分後にアクセス可能:
```
https://<username>.github.io/<repo-name>/
```

## トラブルシューティング

### 「Unable to parse Build」エラー

→ ビルドファイル名と index.html 内のパスが一致しているか確認

### 画面が真っ黒

→ ブラウザの開発者ツール (F12) でエラーを確認

### モバイルでタッチが効かない

→ Canvas の EventSystem が正しく設定されているか確認

### ロードが100%で止まる

→ Decompression Fallback が ON になっているか確認
