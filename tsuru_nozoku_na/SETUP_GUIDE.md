# 鶴覗くな - Unity WebGL → GitHub Pages 公開手順書

## 概要

「鶴の恩返し」をモチーフにした「覗いてはいけない」を我慢するホラー風ミニゲーム。
本ドキュメントは Unity WebGL ビルドから GitHub Pages 公開までの完全手順です。

---

## 1. Unity WebGL ビルド設定手順

### 1-1. プロジェクト作成（初回のみ）

1. Unity Hub を開く
2. **New Project** → **2D (Built-in Render Pipeline)** を選択
3. プロジェクト名: `tsuru_nozoku_na`
4. 作成

> **推奨 Unity バージョン**: 2022.3 LTS 以降

### 1-2. Build Settings（ビルド設定）

**File → Build Settings** を開く

| 項目 | 設定値 |
|------|--------|
| Platform | **WebGL** |
| Target Platform | WebGL |

**Switch Platform** ボタンをクリック（初回は時間がかかる）

### 1-3. Player Settings（重要項目）

**Edit → Project Settings → Player** を開く

#### Resolution and Presentation（解像度）

| 項目 | 推奨値 | 理由 |
|------|--------|------|
| Default Canvas Width | **960** | 16:9 で軽量 |
| Default Canvas Height | **540** | 同上 |
| Run In Background | **OFF** | タブ非アクティブ時停止 |

#### Other Settings

| 項目 | 推奨値 | 理由 |
|------|--------|------|
| Color Space | **Gamma** | WebGL 互換性重視 |
| Auto Graphics API | **ON** | ブラウザに任せる |

#### Publishing Settings（圧縮設定）★重要

| 項目 | 推奨値 | 理由 |
|------|--------|------|
| Compression Format | **Gzip** | GitHub Pages が対応 |
| Decompression Fallback | **ON** | 必須。OFFだと動かないブラウザあり |
| Data Caching | **OFF** | 小規模ゲームでは不要 |

> **注意**: Brotli は GitHub Pages 非対応。必ず **Gzip** を選ぶこと。

### 1-4. ロード時間対策

#### Code Stripping（コード削減）

**Player Settings → Other Settings**

| 項目 | 推奨値 |
|------|--------|
| Managed Stripping Level | **Medium** |
| Strip Engine Code | **ON** |

#### 不要機能の除外

**Edit → Project Settings → Audio**
- 音声不使用なら Disable Audio にチェック

**Edit → Project Settings → Physics 2D**
- 物理演算不使用なら Auto Simulation を OFF

### 1-5. ビルド実行

1. **File → Build Settings**
2. **Scenes In Build** にメインシーンを追加
3. **Build** ボタンをクリック
4. 出力先フォルダ名: `docs` （GitHub Pages用）
5. 待機（初回は5〜10分程度）

---

## 2. GitHub Pages 用ディレクトリ構成

### 2-1. 推奨最小構成

```
tsuru_nozoku_na/
├── docs/                    ← GitHub Pages 公開ルート
│   ├── index.html           ← Unity が生成
│   ├── Build/
│   │   ├── docs.loader.js
│   │   ├── docs.framework.js.gz
│   │   ├── docs.data.gz
│   │   └── docs.wasm.gz
│   └── TemplateData/
│       ├── favicon.ico
│       ├── fullscreen-button.png
│       ├── progress-bar-empty-dark.png
│       ├── progress-bar-empty-light.png
│       ├── progress-bar-full-dark.png
│       ├── progress-bar-full-light.png
│       ├── style.css
│       └── webgl-logo.png
├── UnityProject/            ← Unity プロジェクト本体（任意）
├── README.md
├── SETUP_GUIDE.md           ← この手順書
└── .gitignore
```

### 2-2. ビルド後の確認ポイント

Unity でビルドすると、指定フォルダ内に以下が自動生成される:

- `index.html` - エントリポイント
- `Build/` - WebGL バイナリ
- `TemplateData/` - ロード画面素材

**何も編集せずそのまま使える**。

### 2-3. ローカル動作確認（任意）

WebGL は `file://` プロトコルでは動作しない。ローカル確認には:

```bash
cd docs
python3 -m http.server 8000
```

ブラウザで `http://localhost:8000` を開く。

---

## 3. GitHub Pages 設定手順

### 3-1. リポジトリへプッシュ

```bash
git add .
git commit -m "Add WebGL build"
git push origin main
```

### 3-2. GitHub Pages 有効化

1. GitHub リポジトリページを開く
2. **Settings** タブ
3. 左メニュー **Pages**
4. **Source** を以下に設定:

| 項目 | 設定値 |
|------|--------|
| Source | **Deploy from a branch** |
| Branch | **main** |
| Folder | **/docs** |

5. **Save** をクリック

### 3-3. 公開URL

設定後、1〜2分で以下の形式で公開される:

```
https://<ユーザー名>.github.io/tsuru_nozoku_na/
```

例: `https://sasaki.github.io/tsuru_nozoku_na/`

### 3-4. 初回公開までのチェックリスト

- [ ] `docs/index.html` が存在する
- [ ] `docs/Build/` 内に `.gz` ファイルがある
- [ ] GitHub Pages の Source が `/docs` になっている
- [ ] Actions タブでデプロイ成功を確認

---

## 4. トラブルシューティング

### 画面が真っ白

**原因**: Brotli 圧縮を使っている
**対策**: Player Settings → Publishing Settings → Compression Format を **Gzip** に変更して再ビルド

### ロード画面で止まる

**原因**: Decompression Fallback が OFF
**対策**: Player Settings → Publishing Settings → Decompression Fallback を **ON** にして再ビルド

### モバイルで動かない

WebGL はモバイル非対応が前提。README に注意書きを記載する。

### CORS エラー

GitHub Pages では発生しない。ローカルテスト時のみ発生するので無視可。

---

## 5. 更新時の手順

1. Unity でゲームを修正
2. **Build Settings → Build** で `docs` フォルダを上書き
3. Git でコミット＆プッシュ
4. 1〜2分後に自動反映

---

## 付録: 推奨タイムライン（2日間）

### Day 1
- [ ] Unity プロジェクト作成・WebGL 設定
- [ ] ゲームのコアループ実装
- [ ] 仮ビルド・ローカル動作確認

### Day 2
- [ ] 演出・UI 仕上げ
- [ ] 最終ビルド
- [ ] GitHub Pages 公開・動作確認
- [ ] README 整備
