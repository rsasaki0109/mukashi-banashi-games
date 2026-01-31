# 花咲じいさん - 灰まきゲーム

日本昔話「花咲じいさん」をモチーフにしたミニゲーム。
プレイヤーは灰をまき、枯れ木を桜で満開にします。

## ゲームルール

- **制限時間**: 30秒
- **操作**: マウスクリック / タップで灰をまく
- **成功条件**: 枯れ木に適量の灰を当てる（10〜20個）
- **失敗条件**:
  - 灰が足りない（10個未満）
  - 灰をまきすぎ（20個超過）

## Unity プロジェクトセットアップ

### 1. Unity プロジェクト作成

1. Unity Hub で新規 2D プロジェクトを作成
2. このリポジトリの `Assets/` フォルダの内容をプロジェクトにコピー

### 2. 自動セットアップ（推奨）

Unity エディタのメニューから:

```
Tools > Hanasaka Game > Setup Tags     ← 最初に実行
Tools > Hanasaka Game > Create Ash Prefab
Tools > Hanasaka Game > Setup Scene
```

### 3. 手動セットアップ（必要な場合）

#### タグ設定
1. Edit > Project Settings > Tags and Layers
2. Tags に `Tree` を追加

#### Hierarchy 構成

```
Main (Scene)
├── Main Camera
├── Canvas (Screen Space - Overlay)
│   ├── TimerText
│   ├── AshCountText
│   ├── ResultPanel
│   │   ├── ResultText
│   │   └── RetryButton
│   └── InstructionText
├── GameManager
├── TreeController
├── DeadTree (Tag: Tree)
├── BloomingTree (初期非表示)
└── AshSpawner
```

## WebGL ビルド設定

### Player Settings

1. **File > Build Settings** で WebGL を選択
2. **Player Settings** で以下を設定:

| 項目 | 設定値 |
|------|--------|
| Resolution | 960 x 600 |
| WebGL Template | Minimal |
| Compression Format | Gzip |
| Publishing Settings > Decompression Fallback | ON |

### ⚠️ 重要な注意点

```
✓ 使用OK
- SceneManager.LoadScene()
- Input.GetMouseButton()
- Time.deltaTime

✗ 使用NG（WebGL非対応）
- Application.Quit()
- Time.timeScale = 0 での完全停止
- System.Threading
- ファイルI/O
```

## GitHub Pages 公開手順

### ディレクトリ構成

```
/docs
├── index.html
├── Build/
│   ├── Build.loader.js
│   ├── Build.data.gz
│   ├── Build.framework.js.gz
│   └── Build.wasm.gz
└── TemplateData/
    └── style.css
```

### 手順

1. **WebGL ビルド実行**
   ```
   File > Build Settings > WebGL > Build
   出力先: docs/Build/
   ```

2. **ビルド後のファイル名変更**
   - ビルド出力のファイル名を `Build.*` に統一
   - または `index.html` 内のパスを修正

3. **GitHub リポジトリにプッシュ**
   ```bash
   git add .
   git commit -m "Add WebGL build"
   git push origin main
   ```

4. **GitHub Pages 設定**
   - リポジトリ Settings > Pages
   - Source: `Deploy from a branch`
   - Branch: `main`
   - Folder: `/docs`
   - Save

5. **公開確認**
   - 数分後に `https://<username>.github.io/<repo-name>/` でアクセス可能

## トラブルシューティング

### ロードが止まる / エラーが出る

1. **MIME Type エラー**
   - `.gz` ファイルの Content-Type が正しく設定されているか確認
   - Decompression Fallback を ON にする

2. **ファイルパスエラー**
   - `index.html` 内のパスとビルドファイル名が一致しているか確認

3. **モバイルで動かない**
   - iOS Safari では WebGL 2.0 に制限あり
   - Player Settings > Other Settings > Auto Graphics API を ON に

## スクリプト一覧

| ファイル | 役割 |
|----------|------|
| `GameManager.cs` | ゲーム進行管理、勝敗判定 |
| `AshSpawner.cs` | クリック位置に灰を生成 |
| `Ash.cs` | 灰の落下挙動、当たり判定 |
| `TreeController.cs` | 枯れ木/満開の切り替え |
| `UIController.cs` | UI表示の更新 |
| `GameSetupEditor.cs` | エディタ拡張（セットアップ自動化） |

## ライセンス

MIT License
