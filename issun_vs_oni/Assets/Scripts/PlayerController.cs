using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Needle Attack")]
    public float attackRange = 1.5f;
    public float attackInterval = 0.5f;
    public int attackDamage = 5;
    public Transform oni;

    [Header("Mobile")]
    public float swipeSensitivity = 0.01f;

    private Rigidbody2D rb;
    private float attackTimer;
    private Vector2 touchStartPos;
    private bool isTouching;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        attackTimer = 0f;
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsCleared) return;

        HandleInput();
        HandleNeedleAttack();
    }

    void HandleInput()
    {
        Vector2 moveDir = Vector2.zero;

        // PC: 矢印キー or WASD
        moveDir.x = Input.GetAxisRaw("Horizontal");
        moveDir.y = Input.GetAxisRaw("Vertical");

        // スマホ: スワイプ
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    isTouching = true;
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (isTouching)
                    {
                        Vector2 delta = touch.position - touchStartPos;
                        moveDir = delta * swipeSensitivity;
                        // 最大値を1に制限
                        if (moveDir.magnitude > 1f)
                        {
                            moveDir = moveDir.normalized;
                        }
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isTouching = false;
                    break;
            }
        }

        // 移動適用
        rb.linearVelocity = moveDir.normalized * moveSpeed;
    }

    void HandleNeedleAttack()
    {
        if (oni == null) return;

        float distance = Vector2.Distance(transform.position, oni.position);

        // 攻撃範囲内
        if (distance <= attackRange)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                // 針攻撃ヒット
                GameManager.Instance.DamageOni(attackDamage);
                attackTimer = attackInterval;

                // TODO: 攻撃エフェクト
            }
        }
        else
        {
            // 範囲外ではタイマーリセット
            attackTimer = 0;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 攻撃に当たった
        if (other.CompareTag("OniAttack"))
        {
            GameManager.Instance.PlayerHit();
        }
    }

    void OnDrawGizmosSelected()
    {
        // 攻撃範囲を可視化
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
