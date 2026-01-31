/**
 * Mukashi Party Battle
 * 日本の童話キャラクターを操作するアクションゲーム
 */

// ゲーム設定
const CONFIG = {
    width: 800,
    height: 600,
    backgroundColor: 0x2d4a3e,

    // キャラクター設定
    characters: {
        momotaro: {
            name: '桃太郎',
            color: 0xff6b9d,  // ピンク
            maxHp: 100,
            speed: 180,
            size: 28,
            offsetX: 0,
            offsetY: 0,
            attackColor: 0xff6b9d,
            attackRange: 60,
            attackAngle: 90
        },
        kintaro: {
            name: '金太郎',
            color: 0xffd700,  // ゴールド
            maxHp: 150,
            speed: 150,
            size: 32,
            offsetX: 50,
            offsetY: -30,
            attackColor: 0xffd700,
            attackRange: 50
        },
        issunboshi: {
            name: '一寸法師',
            color: 0x6bcfff,  // 水色
            maxHp: 60,
            speed: 220,
            size: 20,
            offsetX: -50,
            offsetY: 30,
            attackColor: 0x6bcfff,
            dashDistance: 120,
            dashSpeed: 600
        }
    },

    // 敵設定
    enemy: {
        name: '鬼',
        color: 0xcc3333,
        maxHp: 300,
        speed: 100,
        size: 48,
        attackDamage: 15,
        chargeSpeed: 300,
        chargeInterval: 3000
    },

    // スキルクールタイム（ミリ秒）
    skillCooldown: 2000,
    ultimateCooldown: 5000
};

// メインシーン
class MainScene extends Phaser.Scene {
    constructor() {
        super({ key: 'MainScene' });
        this.party = [];
        this.enemy = null;
        this.gameOver = false;
        this.lastSkillTime = 0;
        this.lastUltimateTime = 0;
        this.partyCenter = { x: 400, y: 450 };
        this.facingDirection = { x: 0, y: -1 };  // 初期は上向き
    }

    create() {
        // 背景のグリッド描画
        this.createBackground();

        // パーティメンバー作成
        this.createParty();

        // 敵作成
        this.createEnemy();

        // UI作成
        this.createUI();

        // 入力設定
        this.setupInput();

        // 攻撃エフェクト用グラフィックス
        this.attackGraphics = this.add.graphics();

        // ゲーム開始メッセージ
        this.showMessage('FIGHT!', 1500);
    }

    createBackground() {
        const graphics = this.add.graphics();

        // 草地風の背景
        graphics.fillStyle(0x3d5a4a, 1);
        graphics.fillRect(0, 0, CONFIG.width, CONFIG.height);

        // グリッドライン
        graphics.lineStyle(1, 0x4a6a5a, 0.3);
        for (let x = 0; x < CONFIG.width; x += 40) {
            graphics.moveTo(x, 0);
            graphics.lineTo(x, CONFIG.height);
        }
        for (let y = 0; y < CONFIG.height; y += 40) {
            graphics.moveTo(0, y);
            graphics.lineTo(CONFIG.width, y);
        }
        graphics.strokePath();

        // 闘技場の境界線
        graphics.lineStyle(3, 0x8b7355, 1);
        graphics.strokeRect(30, 30, CONFIG.width - 60, CONFIG.height - 60);
    }

    createParty() {
        const chars = CONFIG.characters;

        // 桃太郎
        this.party.push(this.createCharacter(
            'momotaro',
            chars.momotaro,
            this.partyCenter.x + chars.momotaro.offsetX,
            this.partyCenter.y + chars.momotaro.offsetY
        ));

        // 金太郎
        this.party.push(this.createCharacter(
            'kintaro',
            chars.kintaro,
            this.partyCenter.x + chars.kintaro.offsetX,
            this.partyCenter.y + chars.kintaro.offsetY
        ));

        // 一寸法師
        this.party.push(this.createCharacter(
            'issunboshi',
            chars.issunboshi,
            this.partyCenter.x + chars.issunboshi.offsetX,
            this.partyCenter.y + chars.issunboshi.offsetY
        ));
    }

    createCharacter(id, config, x, y) {
        const graphics = this.add.graphics();

        // キャラクター本体（矩形）
        graphics.fillStyle(config.color, 1);
        graphics.fillRect(-config.size/2, -config.size/2, config.size, config.size);

        // 輪郭
        graphics.lineStyle(2, 0xffffff, 0.8);
        graphics.strokeRect(-config.size/2, -config.size/2, config.size, config.size);

        // 方向インジケーター
        graphics.fillStyle(0xffffff, 0.9);
        graphics.fillTriangle(0, -config.size/2 - 5, -5, -config.size/2, 5, -config.size/2);

        const container = this.add.container(x, y, [graphics]);

        return {
            id: id,
            config: config,
            container: container,
            graphics: graphics,
            hp: config.maxHp,
            alive: true,
            offsetX: config.offsetX,
            offsetY: config.offsetY,
            isDashing: false
        };
    }

    createEnemy() {
        const config = CONFIG.enemy;
        const graphics = this.add.graphics();

        // 鬼の本体（大きめの矩形）
        graphics.fillStyle(config.color, 1);
        graphics.fillRect(-config.size/2, -config.size/2, config.size, config.size);

        // 輪郭
        graphics.lineStyle(3, 0x880000, 1);
        graphics.strokeRect(-config.size/2, -config.size/2, config.size, config.size);

        // 角
        graphics.fillStyle(0x880000, 1);
        graphics.fillTriangle(-15, -config.size/2, -10, -config.size/2 - 15, -5, -config.size/2);
        graphics.fillTriangle(5, -config.size/2, 10, -config.size/2 - 15, 15, -config.size/2);

        // 目
        graphics.fillStyle(0xffff00, 1);
        graphics.fillCircle(-10, -5, 5);
        graphics.fillCircle(10, -5, 5);

        const container = this.add.container(400, 150, [graphics]);

        this.enemy = {
            config: config,
            container: container,
            graphics: graphics,
            hp: config.maxHp,
            alive: true,
            isCharging: false,
            chargeTarget: null,
            lastChargeTime: 0,
            moveDirection: { x: 1, y: 0 },
            moveTimer: 0
        };
    }

    createUI() {
        this.uiContainer = this.add.container(10, 10);

        // HPバー背景
        const barWidth = 120;
        const barHeight = 16;
        const spacing = 25;

        this.hpBars = [];

        this.party.forEach((char, index) => {
            const y = index * spacing;

            // 名前
            const nameText = this.add.text(0, y, char.config.name, {
                fontSize: '12px',
                fontFamily: 'Hiragino Sans, Meiryo, sans-serif',
                color: '#ffffff'
            });

            // HPバー背景
            const bgBar = this.add.graphics();
            bgBar.fillStyle(0x333333, 1);
            bgBar.fillRect(60, y, barWidth, barHeight);

            // HPバー
            const hpBar = this.add.graphics();
            hpBar.fillStyle(char.config.color, 1);
            hpBar.fillRect(60, y, barWidth, barHeight);

            this.uiContainer.add([nameText, bgBar, hpBar]);

            this.hpBars.push({
                bar: hpBar,
                bgBar: bgBar,
                width: barWidth,
                height: barHeight,
                y: y,
                char: char
            });
        });

        // 敵HPバー
        const enemyY = 10;
        const enemyBarWidth = 200;

        this.enemyHpContainer = this.add.container(CONFIG.width / 2 - enemyBarWidth / 2, enemyY);

        const enemyNameText = this.add.text(enemyBarWidth / 2, 0, '鬼', {
            fontSize: '14px',
            fontFamily: 'Hiragino Sans, Meiryo, sans-serif',
            color: '#ff6666'
        }).setOrigin(0.5, 0);

        const enemyBgBar = this.add.graphics();
        enemyBgBar.fillStyle(0x333333, 1);
        enemyBgBar.fillRect(0, 18, enemyBarWidth, 20);

        this.enemyHpBar = this.add.graphics();
        this.enemyHpBar.fillStyle(0xcc3333, 1);
        this.enemyHpBar.fillRect(0, 18, enemyBarWidth, 20);

        this.enemyHpContainer.add([enemyNameText, enemyBgBar, this.enemyHpBar]);
        this.enemyHpBarWidth = enemyBarWidth;

        // スキルクールダウン表示
        this.cooldownText = this.add.text(CONFIG.width - 150, CONFIG.height - 60, '', {
            fontSize: '12px',
            fontFamily: 'Hiragino Sans, Meiryo, sans-serif',
            color: '#aaaaaa'
        });

        // 操作説明
        this.add.text(CONFIG.width - 150, CONFIG.height - 40, 'J:攻撃 K:スキル L:合体技', {
            fontSize: '11px',
            fontFamily: 'Hiragino Sans, Meiryo, sans-serif',
            color: '#888888'
        });
    }

    setupInput() {
        // 移動キー
        this.cursors = this.input.keyboard.createCursorKeys();
        this.wasd = {
            up: this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.W),
            down: this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.S),
            left: this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.A),
            right: this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.D)
        };

        // 攻撃キー
        this.input.keyboard.on('keydown-J', () => this.performAttack());
        this.input.keyboard.on('keydown-K', () => this.performSkill());
        this.input.keyboard.on('keydown-L', () => this.performUltimate());
    }

    update(time, delta) {
        if (this.gameOver) return;

        // パーティ移動
        this.updatePartyMovement(delta);

        // 敵AI
        this.updateEnemy(time, delta);

        // 当たり判定
        this.checkCollisions();

        // UI更新
        this.updateUI(time);

        // 勝敗判定
        this.checkGameEnd();
    }

    updatePartyMovement(delta) {
        let dx = 0;
        let dy = 0;

        // 入力チェック
        if (this.cursors.left.isDown || this.wasd.left.isDown) dx -= 1;
        if (this.cursors.right.isDown || this.wasd.right.isDown) dx += 1;
        if (this.cursors.up.isDown || this.wasd.up.isDown) dy -= 1;
        if (this.cursors.down.isDown || this.wasd.down.isDown) dy += 1;

        // 移動がある場合
        if (dx !== 0 || dy !== 0) {
            // 正規化
            const length = Math.sqrt(dx * dx + dy * dy);
            dx /= length;
            dy /= length;

            // 向き更新
            this.facingDirection = { x: dx, y: dy };

            // 生存キャラの平均速度を計算
            const aliveChars = this.party.filter(c => c.alive);
            if (aliveChars.length === 0) return;

            const avgSpeed = aliveChars.reduce((sum, c) => sum + c.config.speed, 0) / aliveChars.length;

            // パーティ中心を移動
            const moveX = dx * avgSpeed * (delta / 1000);
            const moveY = dy * avgSpeed * (delta / 1000);

            this.partyCenter.x += moveX;
            this.partyCenter.y += moveY;

            // 境界制限
            this.partyCenter.x = Phaser.Math.Clamp(this.partyCenter.x, 80, CONFIG.width - 80);
            this.partyCenter.y = Phaser.Math.Clamp(this.partyCenter.y, 80, CONFIG.height - 80);
        }

        // 各キャラの位置を更新（フォーメーション維持）
        this.party.forEach(char => {
            if (!char.alive) return;
            if (char.isDashing) return;  // ダッシュ中は個別制御

            // フォーメーションオフセットを向きに応じて回転
            const angle = Math.atan2(this.facingDirection.y, this.facingDirection.x) + Math.PI / 2;
            const cos = Math.cos(angle);
            const sin = Math.sin(angle);

            const rotatedX = char.offsetX * cos - char.offsetY * sin;
            const rotatedY = char.offsetX * sin + char.offsetY * cos;

            char.container.x = this.partyCenter.x + rotatedX;
            char.container.y = this.partyCenter.y + rotatedY;

            // 向きを更新
            char.container.rotation = angle - Math.PI / 2;
        });
    }

    updateEnemy(time, delta) {
        if (!this.enemy.alive) return;

        const enemy = this.enemy;
        const config = enemy.config;

        // 突進中
        if (enemy.isCharging && enemy.chargeTarget) {
            const dx = enemy.chargeTarget.x - enemy.container.x;
            const dy = enemy.chargeTarget.y - enemy.container.y;
            const dist = Math.sqrt(dx * dx + dy * dy);

            if (dist < 10) {
                enemy.isCharging = false;
                enemy.chargeTarget = null;
            } else {
                const moveX = (dx / dist) * config.chargeSpeed * (delta / 1000);
                const moveY = (dy / dist) * config.chargeSpeed * (delta / 1000);
                enemy.container.x += moveX;
                enemy.container.y += moveY;
            }
            return;
        }

        // 突進タイミングチェック
        if (time - enemy.lastChargeTime > config.chargeInterval) {
            const aliveChars = this.party.filter(c => c.alive);
            if (aliveChars.length > 0) {
                const target = Phaser.Utils.Array.GetRandom(aliveChars);
                enemy.chargeTarget = { x: target.container.x, y: target.container.y };
                enemy.isCharging = true;
                enemy.lastChargeTime = time;

                // 突進エフェクト
                this.showChargeWarning(enemy.container.x, enemy.container.y);
            }
        }

        // 通常移動（ランダム）
        enemy.moveTimer -= delta;
        if (enemy.moveTimer <= 0) {
            enemy.moveDirection = {
                x: Phaser.Math.FloatBetween(-1, 1),
                y: Phaser.Math.FloatBetween(-1, 1)
            };
            enemy.moveTimer = Phaser.Math.Between(500, 1500);
        }

        enemy.container.x += enemy.moveDirection.x * config.speed * (delta / 1000);
        enemy.container.y += enemy.moveDirection.y * config.speed * (delta / 1000);

        // 境界制限
        enemy.container.x = Phaser.Math.Clamp(enemy.container.x, 80, CONFIG.width - 80);
        enemy.container.y = Phaser.Math.Clamp(enemy.container.y, 60, CONFIG.height - 80);
    }

    showChargeWarning(x, y) {
        const warning = this.add.text(x, y - 40, '！', {
            fontSize: '32px',
            fontFamily: 'sans-serif',
            color: '#ff0000'
        }).setOrigin(0.5);

        this.tweens.add({
            targets: warning,
            alpha: 0,
            y: y - 60,
            duration: 500,
            onComplete: () => warning.destroy()
        });
    }

    performAttack() {
        if (this.gameOver) return;

        this.attackGraphics.clear();

        this.party.forEach(char => {
            if (!char.alive) return;

            const x = char.container.x;
            const y = char.container.y;
            const angle = char.container.rotation;

            let damage = 0;

            switch (char.id) {
                case 'momotaro':
                    // 扇形斬撃
                    damage = this.attackSweep(x, y, angle, char.config);
                    break;
                case 'kintaro':
                    // 周囲攻撃
                    damage = this.attackCircle(x, y, char.config);
                    break;
                case 'issunboshi':
                    // 前方突進
                    damage = this.attackDash(char, angle);
                    break;
            }
        });
    }

    attackSweep(x, y, angle, config) {
        // 扇形エフェクト
        this.attackGraphics.fillStyle(config.attackColor, 0.5);
        this.attackGraphics.beginPath();
        this.attackGraphics.moveTo(x, y);

        const range = config.attackRange;
        const spreadAngle = Phaser.Math.DegToRad(config.attackAngle / 2);
        const baseAngle = angle - Math.PI / 2;

        this.attackGraphics.arc(x, y, range, baseAngle - spreadAngle, baseAngle + spreadAngle);
        this.attackGraphics.closePath();
        this.attackGraphics.fillPath();

        // エフェクトフェードアウト
        this.time.delayedCall(100, () => this.attackGraphics.clear());

        // 敵への当たり判定
        if (this.enemy.alive) {
            const dx = this.enemy.container.x - x;
            const dy = this.enemy.container.y - y;
            const dist = Math.sqrt(dx * dx + dy * dy);

            if (dist < range + CONFIG.enemy.size / 2) {
                const angleToEnemy = Math.atan2(dy, dx);
                const angleDiff = Phaser.Math.Angle.Wrap(angleToEnemy - baseAngle);

                if (Math.abs(angleDiff) < spreadAngle) {
                    this.damageEnemy(25);
                    return 25;
                }
            }
        }
        return 0;
    }

    attackCircle(x, y, config) {
        // 円形エフェクト
        this.attackGraphics.fillStyle(config.attackColor, 0.4);
        this.attackGraphics.fillCircle(x, y, config.attackRange);

        this.time.delayedCall(100, () => this.attackGraphics.clear());

        // 敵への当たり判定
        if (this.enemy.alive) {
            const dx = this.enemy.container.x - x;
            const dy = this.enemy.container.y - y;
            const dist = Math.sqrt(dx * dx + dy * dy);

            if (dist < config.attackRange + CONFIG.enemy.size / 2) {
                this.damageEnemy(20);
                return 20;
            }
        }
        return 0;
    }

    attackDash(char, angle) {
        if (char.isDashing) return 0;

        char.isDashing = true;
        const config = char.config;

        const dirX = Math.cos(angle - Math.PI / 2);
        const dirY = Math.sin(angle - Math.PI / 2);

        const startX = char.container.x;
        const startY = char.container.y;
        const endX = startX + dirX * config.dashDistance;
        const endY = startY + dirY * config.dashDistance;

        // ダッシュ軌跡
        this.attackGraphics.lineStyle(config.size, config.attackColor, 0.5);
        this.attackGraphics.lineBetween(startX, startY, endX, endY);

        this.time.delayedCall(150, () => this.attackGraphics.clear());

        // ダッシュアニメーション
        this.tweens.add({
            targets: char.container,
            x: Phaser.Math.Clamp(endX, 80, CONFIG.width - 80),
            y: Phaser.Math.Clamp(endY, 80, CONFIG.height - 80),
            duration: 150,
            onComplete: () => {
                char.isDashing = false;
            }
        });

        // 敵への当たり判定（ライン）
        if (this.enemy.alive) {
            const enemyX = this.enemy.container.x;
            const enemyY = this.enemy.container.y;

            // 線分と円の当たり判定
            const dist = this.pointToLineDistance(enemyX, enemyY, startX, startY, endX, endY);

            if (dist < CONFIG.enemy.size / 2 + config.size / 2) {
                this.damageEnemy(30);
                return 30;
            }
        }
        return 0;
    }

    pointToLineDistance(px, py, x1, y1, x2, y2) {
        const A = px - x1;
        const B = py - y1;
        const C = x2 - x1;
        const D = y2 - y1;

        const dot = A * C + B * D;
        const lenSq = C * C + D * D;
        let param = -1;

        if (lenSq !== 0) param = dot / lenSq;

        let xx, yy;

        if (param < 0) {
            xx = x1;
            yy = y1;
        } else if (param > 1) {
            xx = x2;
            yy = y2;
        } else {
            xx = x1 + param * C;
            yy = y1 + param * D;
        }

        const dx = px - xx;
        const dy = py - yy;
        return Math.sqrt(dx * dx + dy * dy);
    }

    performSkill() {
        if (this.gameOver) return;

        const now = this.time.now;
        if (now - this.lastSkillTime < CONFIG.skillCooldown) return;

        this.lastSkillTime = now;

        // 各キャラの固有スキル
        this.party.forEach(char => {
            if (!char.alive) return;

            switch (char.id) {
                case 'momotaro':
                    // きびだんご効果：味方HP回復
                    this.party.forEach(c => {
                        if (c.alive) {
                            c.hp = Math.min(c.hp + 20, c.config.maxHp);
                            this.showHealEffect(c.container.x, c.container.y);
                        }
                    });
                    break;
                case 'kintaro':
                    // まさかり強撃：大ダメージ広範囲
                    this.attackGraphics.fillStyle(0xffd700, 0.6);
                    this.attackGraphics.fillCircle(char.container.x, char.container.y, 80);
                    this.time.delayedCall(200, () => this.attackGraphics.clear());

                    if (this.enemy.alive) {
                        const dx = this.enemy.container.x - char.container.x;
                        const dy = this.enemy.container.y - char.container.y;
                        if (Math.sqrt(dx * dx + dy * dy) < 80 + CONFIG.enemy.size / 2) {
                            this.damageEnemy(50);
                        }
                    }
                    break;
                case 'issunboshi':
                    // 打ち出の小槌：攻撃速度アップ（一時的に連続攻撃）
                    for (let i = 0; i < 3; i++) {
                        this.time.delayedCall(i * 100, () => {
                            if (char.alive && this.enemy.alive) {
                                this.attackDash(char, char.container.rotation);
                            }
                        });
                    }
                    break;
            }
        });
    }

    showHealEffect(x, y) {
        const healText = this.add.text(x, y, '+20', {
            fontSize: '16px',
            fontFamily: 'sans-serif',
            color: '#00ff00'
        }).setOrigin(0.5);

        this.tweens.add({
            targets: healText,
            y: y - 30,
            alpha: 0,
            duration: 800,
            onComplete: () => healText.destroy()
        });
    }

    performUltimate() {
        if (this.gameOver) return;

        const now = this.time.now;
        if (now - this.lastUltimateTime < CONFIG.ultimateCooldown) return;

        // 生存者が2人以上必要
        const aliveCount = this.party.filter(c => c.alive).length;
        if (aliveCount < 2) return;

        this.lastUltimateTime = now;

        // 合体技エフェクト
        const centerX = this.partyCenter.x;
        const centerY = this.partyCenter.y;

        // フラッシュエフェクト
        const flash = this.add.graphics();
        flash.fillStyle(0xffffff, 0.8);
        flash.fillCircle(centerX, centerY, 150);

        this.tweens.add({
            targets: flash,
            alpha: 0,
            duration: 500,
            onComplete: () => flash.destroy()
        });

        // 合体技テキスト
        const ultText = this.add.text(centerX, centerY - 80, '桃金寸\n三位一体!', {
            fontSize: '24px',
            fontFamily: 'Hiragino Sans, Meiryo, sans-serif',
            color: '#ffff00',
            align: 'center'
        }).setOrigin(0.5);

        this.tweens.add({
            targets: ultText,
            y: centerY - 120,
            alpha: 0,
            duration: 1500,
            onComplete: () => ultText.destroy()
        });

        // 大ダメージ
        if (this.enemy.alive) {
            this.damageEnemy(100);
        }
    }

    damageEnemy(amount) {
        if (!this.enemy.alive) return;

        this.enemy.hp -= amount;

        // ダメージ表示
        const damageText = this.add.text(
            this.enemy.container.x,
            this.enemy.container.y - 30,
            `-${amount}`,
            {
                fontSize: '20px',
                fontFamily: 'sans-serif',
                color: '#ff0000'
            }
        ).setOrigin(0.5);

        this.tweens.add({
            targets: damageText,
            y: this.enemy.container.y - 60,
            alpha: 0,
            duration: 600,
            onComplete: () => damageText.destroy()
        });

        // ヒットエフェクト
        this.tweens.add({
            targets: this.enemy.container,
            x: this.enemy.container.x + Phaser.Math.Between(-10, 10),
            duration: 50,
            yoyo: true
        });

        if (this.enemy.hp <= 0) {
            this.enemy.hp = 0;
            this.enemy.alive = false;
            this.enemy.container.setVisible(false);
        }
    }

    checkCollisions() {
        if (!this.enemy.alive) return;

        // 敵の突進攻撃がパーティに当たる判定
        if (this.enemy.isCharging) {
            this.party.forEach(char => {
                if (!char.alive) return;

                const dx = this.enemy.container.x - char.container.x;
                const dy = this.enemy.container.y - char.container.y;
                const dist = Math.sqrt(dx * dx + dy * dy);

                if (dist < CONFIG.enemy.size / 2 + char.config.size / 2) {
                    this.damageCharacter(char, CONFIG.enemy.attackDamage);
                }
            });
        }
    }

    damageCharacter(char, amount) {
        if (!char.alive) return;

        char.hp -= amount;

        // ダメージ表示
        const damageText = this.add.text(
            char.container.x,
            char.container.y - 20,
            `-${amount}`,
            {
                fontSize: '14px',
                fontFamily: 'sans-serif',
                color: '#ff6666'
            }
        ).setOrigin(0.5);

        this.tweens.add({
            targets: damageText,
            y: char.container.y - 40,
            alpha: 0,
            duration: 500,
            onComplete: () => damageText.destroy()
        });

        // ヒットエフェクト
        this.tweens.add({
            targets: char.container,
            alpha: 0.5,
            duration: 100,
            yoyo: true,
            repeat: 2
        });

        if (char.hp <= 0) {
            char.hp = 0;
            char.alive = false;
            char.container.setVisible(false);
        }
    }

    updateUI(time) {
        // パーティHPバー更新
        this.hpBars.forEach(hpBar => {
            hpBar.bar.clear();
            hpBar.bar.fillStyle(hpBar.char.config.color, 1);
            const hpRatio = hpBar.char.hp / hpBar.char.config.maxHp;
            hpBar.bar.fillRect(60, hpBar.y, hpBar.width * hpRatio, hpBar.height);
        });

        // 敵HPバー更新
        this.enemyHpBar.clear();
        this.enemyHpBar.fillStyle(0xcc3333, 1);
        const enemyHpRatio = this.enemy.hp / CONFIG.enemy.maxHp;
        this.enemyHpBar.fillRect(0, 18, this.enemyHpBarWidth * enemyHpRatio, 20);

        // クールダウン表示
        const skillRemain = Math.max(0, CONFIG.skillCooldown - (time - this.lastSkillTime));
        const ultRemain = Math.max(0, CONFIG.ultimateCooldown - (time - this.lastUltimateTime));

        let cooldownStr = '';
        if (skillRemain > 0) cooldownStr += `K: ${(skillRemain / 1000).toFixed(1)}s `;
        if (ultRemain > 0) cooldownStr += `L: ${(ultRemain / 1000).toFixed(1)}s`;
        this.cooldownText.setText(cooldownStr);
    }

    checkGameEnd() {
        // 勝利判定
        if (!this.enemy.alive) {
            this.gameOver = true;
            this.showMessage('VICTORY!', 0, '#00ff00');
            return;
        }

        // 敗北判定
        const aliveCount = this.party.filter(c => c.alive).length;
        if (aliveCount === 0) {
            this.gameOver = true;
            this.showMessage('GAME OVER', 0, '#ff0000');
        }
    }

    showMessage(text, duration = 0, color = '#ffffff') {
        const message = this.add.text(CONFIG.width / 2, CONFIG.height / 2, text, {
            fontSize: '48px',
            fontFamily: 'Hiragino Sans, Meiryo, sans-serif',
            color: color,
            stroke: '#000000',
            strokeThickness: 4
        }).setOrigin(0.5);

        if (duration > 0) {
            this.tweens.add({
                targets: message,
                alpha: 0,
                duration: 500,
                delay: duration - 500,
                onComplete: () => message.destroy()
            });
        } else {
            // リスタート案内
            const restartText = this.add.text(CONFIG.width / 2, CONFIG.height / 2 + 60, 'Press R to Restart', {
                fontSize: '20px',
                fontFamily: 'sans-serif',
                color: '#aaaaaa'
            }).setOrigin(0.5);

            this.input.keyboard.once('keydown-R', () => {
                this.scene.restart();
            });
        }
    }
}

// ゲーム初期化
const game = new Phaser.Game({
    type: Phaser.AUTO,
    width: CONFIG.width,
    height: CONFIG.height,
    parent: 'game-container',
    backgroundColor: CONFIG.backgroundColor,
    scene: [MainScene],
    physics: {
        default: 'arcade',
        arcade: {
            debug: false
        }
    }
});
