using UnityEngine;
using System.Collections;

public class OniController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject slamAttackPrefab;
    public GameObject grabAttackPrefab;
    public GameObject breathAttackPrefab;

    [Header("Attack Settings")]
    public float warningDuration = 0.8f;  // 予兆時間

    private float attackTimer;
    private bool isAttacking;

    void Start()
    {
        attackTimer = 2f; // 初回攻撃までの猶予
        isAttacking = false;
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver || GameManager.Instance.IsCleared) return;
        if (isAttacking) return;

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0)
        {
            StartCoroutine(ExecuteAttack());
            attackTimer = GameManager.Instance.GetAttackInterval();
        }
    }

    IEnumerator ExecuteAttack()
    {
        isAttacking = true;

        int diffLevel = GameManager.Instance.GetDifficultyLevel();
        int attackType = ChooseAttackType(diffLevel);

        switch (attackType)
        {
            case 0:
                yield return StartCoroutine(SlamAttack());
                break;
            case 1:
                yield return StartCoroutine(GrabAttack());
                break;
            case 2:
                yield return StartCoroutine(BreathAttack());
                break;
        }

        isAttacking = false;
    }

    int ChooseAttackType(int diffLevel)
    {
        switch (diffLevel)
        {
            case 0:
                return 0; // 叩きのみ
            case 1:
                return Random.Range(0, 2); // 叩き or 掴み
            default:
                return Random.Range(0, 3); // 全攻撃
        }
    }

    // 叩き攻撃：プレイヤー方向に攻撃
    IEnumerator SlamAttack()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 予兆生成
        GameObject warning = Instantiate(slamAttackPrefab, transform.position, Quaternion.Euler(0, 0, angle));
        Attack attack = warning.GetComponent<Attack>();
        attack.ShowWarning();

        yield return new WaitForSeconds(warningDuration);

        // 攻撃実行
        attack.Execute();
    }

    // 掴み攻撃：プレイヤーの現在位置をロック
    IEnumerator GrabAttack()
    {
        Vector3 targetPos = player.position;

        // 予兆生成（ターゲット位置）
        GameObject warning = Instantiate(grabAttackPrefab, targetPos, Quaternion.identity);
        Attack attack = warning.GetComponent<Attack>();
        attack.ShowWarning();

        yield return new WaitForSeconds(warningDuration);

        // 攻撃実行
        attack.Execute();
    }

    // 息攻撃：扇形範囲攻撃
    IEnumerator BreathAttack()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 予兆生成
        GameObject warning = Instantiate(breathAttackPrefab, transform.position, Quaternion.Euler(0, 0, angle));
        Attack attack = warning.GetComponent<Attack>();
        attack.ShowWarning();

        yield return new WaitForSeconds(warningDuration);

        // 攻撃実行
        attack.Execute();
    }
}
