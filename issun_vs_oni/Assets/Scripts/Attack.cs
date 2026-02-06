using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour
{
    public enum AttackType
    {
        Slam,   // 叩き攻撃
        Grab,   // 掴み攻撃
        Breath  // 息攻撃
    }

    [Header("Attack Settings")]
    public AttackType attackType;
    public float attackDuration = 0.3f;
    public float attackSize = 2f;

    [Header("Slam Settings")]
    public float slamDistance = 3f;

    [Header("Breath Settings")]
    public float breathAngle = 60f;
    public float breathRange = 4f;

    [Header("Visual")]
    public SpriteRenderer warningRenderer;
    public SpriteRenderer attackRenderer;
    public Color warningColor = new Color(1f, 0f, 0f, 0.3f);
    public Color attackColor = new Color(1f, 0f, 0f, 0.8f);

    private Collider2D attackCollider;

    void Awake()
    {
        attackCollider = GetComponent<Collider2D>();
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
        }

        // 初期は非表示
        if (attackRenderer != null)
        {
            attackRenderer.enabled = false;
        }
    }

    public void ShowWarning()
    {
        if (warningRenderer != null)
        {
            warningRenderer.color = warningColor;
            warningRenderer.enabled = true;
        }

        // 攻撃タイプに応じた形状設定
        SetupShape();
    }

    void SetupShape()
    {
        switch (attackType)
        {
            case AttackType.Slam:
                // 長方形の叩き攻撃
                transform.localScale = new Vector3(slamDistance, attackSize, 1f);
                // 原点から前方に伸びるよう位置調整
                Vector3 offset = transform.right * (slamDistance / 2f);
                transform.position += offset;
                break;

            case AttackType.Grab:
                // 円形の掴み攻撃
                transform.localScale = new Vector3(attackSize, attackSize, 1f);
                break;

            case AttackType.Breath:
                // 扇形（簡略化として三角形スケール）
                transform.localScale = new Vector3(breathRange, breathRange * Mathf.Tan(breathAngle * 0.5f * Mathf.Deg2Rad) * 2f, 1f);
                Vector3 breathOffset = transform.right * (breathRange / 2f);
                transform.position += breathOffset;
                break;
        }
    }

    public void Execute()
    {
        StartCoroutine(ExecuteAttack());
    }

    IEnumerator ExecuteAttack()
    {
        // 予兆非表示、攻撃判定有効化
        if (warningRenderer != null)
        {
            warningRenderer.enabled = false;
        }

        if (attackRenderer != null)
        {
            attackRenderer.color = attackColor;
            attackRenderer.enabled = true;
        }

        if (attackCollider != null)
        {
            attackCollider.enabled = true;
        }

        yield return new WaitForSeconds(attackDuration);

        // 攻撃終了、オブジェクト削除
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // プレイヤーにヒット
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                GameManager.Instance.PlayerHit();
            }
        }
    }
}
