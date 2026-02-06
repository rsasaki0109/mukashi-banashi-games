const { chromium } = require('playwright');
const { spawn, exec } = require('child_process');
const path = require('path');
const fs = require('fs');

const DOCS_DIR = path.join(__dirname, '..', 'docs');
const OUTPUT_DIR = path.join(__dirname, '..', 'docs', 'assets');
const VIDEO_PATH = path.join(OUTPUT_DIR, 'gameplay.webm');
const GIF_PATH = path.join(OUTPUT_DIR, 'gameplay.gif');
const PORT = 8080;

async function startServer() {
    return new Promise((resolve, reject) => {
        const server = spawn('npx', ['serve', '-l', PORT.toString(), DOCS_DIR], {
            stdio: 'pipe'
        });

        server.stdout.on('data', (data) => {
            if (data.toString().includes('Accepting connections')) {
                resolve(server);
            }
        });

        server.stderr.on('data', (data) => {
            console.log('Server:', data.toString());
        });

        setTimeout(() => resolve(server), 3000);
    });
}

async function recordGameplay() {
    // 出力ディレクトリ作成
    if (!fs.existsSync(OUTPUT_DIR)) {
        fs.mkdirSync(OUTPUT_DIR, { recursive: true });
    }

    console.log('Starting local server...');
    const server = await startServer();

    console.log('Launching browser...');
    const browser = await chromium.launch({
        headless: false // ゲームの描画を確認するため
    });

    const context = await browser.newContext({
        viewport: { width: 960, height: 600 },
        recordVideo: {
            dir: OUTPUT_DIR,
            size: { width: 960, height: 600 }
        }
    });

    const page = await context.newPage();

    try {
        console.log('Loading game...');
        await page.goto(`http://localhost:${PORT}/`, {
            waitUntil: 'networkidle',
            timeout: 60000
        });

        // WebGLロード完了を待機（ローディングバー非表示）
        console.log('Waiting for game to load...');
        await page.waitForFunction(() => {
            const loadingBar = document.querySelector('#unity-loading-bar');
            return loadingBar && loadingBar.style.display === 'none';
        }, { timeout: 120000 });

        // ゲーム開始待機
        await page.waitForTimeout(2000);

        console.log('Recording gameplay...');

        // ゲームプレイをシミュレート（灰をまく）
        const canvas = page.locator('#unity-canvas');

        // 木の位置周辺をクリック（画面中央付近）
        const clicks = [
            { x: 480, y: 350 },
            { x: 450, y: 320 },
            { x: 510, y: 340 },
            { x: 470, y: 360 },
            { x: 490, y: 330 },
            { x: 460, y: 350 },
            { x: 500, y: 320 },
            { x: 480, y: 340 },
            { x: 440, y: 330 },
            { x: 520, y: 350 },
            { x: 475, y: 345 },
            { x: 485, y: 335 },
            { x: 455, y: 355 },
            { x: 505, y: 325 },
            { x: 465, y: 365 },
        ];

        for (const click of clicks) {
            await canvas.click({ position: click });
            await page.waitForTimeout(500 + Math.random() * 500);
        }

        // 結果表示を待つ
        console.log('Waiting for game result...');
        await page.waitForTimeout(5000);

    } catch (error) {
        console.error('Recording error:', error.message);
    } finally {
        // 動画を保存
        await page.close();
        await context.close();
        await browser.close();

        // サーバー停止
        server.kill();
    }

    // 録画されたビデオファイルを探す
    const videos = fs.readdirSync(OUTPUT_DIR).filter(f => f.endsWith('.webm'));
    if (videos.length > 0) {
        const recordedVideo = path.join(OUTPUT_DIR, videos[0]);
        console.log(`Video recorded: ${recordedVideo}`);

        // GIFに変換
        await convertToGif(recordedVideo, GIF_PATH);
    }
}

async function convertToGif(inputPath, outputPath) {
    return new Promise((resolve, reject) => {
        console.log('Converting to GIF...');

        // ffmpegでGIF変換（最適化済み）
        const cmd = `ffmpeg -y -i "${inputPath}" -vf "fps=15,scale=480:-1:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse" -loop 0 "${outputPath}"`;

        exec(cmd, (error, stdout, stderr) => {
            if (error) {
                console.error('FFmpeg error:', error.message);
                console.log('Trying alternative method with gifski...');

                // 代替：gifski使用
                const altCmd = `ffmpeg -y -i "${inputPath}" -vf "fps=15,scale=480:-1" /tmp/frames_%04d.png && gifski -o "${outputPath}" /tmp/frames_*.png && rm /tmp/frames_*.png`;
                exec(altCmd, (err2) => {
                    if (err2) {
                        reject(err2);
                    } else {
                        console.log(`GIF created: ${outputPath}`);
                        resolve();
                    }
                });
            } else {
                console.log(`GIF created: ${outputPath}`);
                resolve();
            }
        });
    });
}

recordGameplay().catch(console.error);
