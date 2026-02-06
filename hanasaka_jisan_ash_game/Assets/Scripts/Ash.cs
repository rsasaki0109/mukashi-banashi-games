using UnityEngine;

/// <summary>
/// 灰オブジェクトの挙動を制御するクラス
/// </summary>
public class Ash : MonoBehaviour
{
    [Header("灰の設定")]
    [SerializeField] private float fallSpeed = 3f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float spreadAmount = 0.5f;

    private Vector3 velocity;
    private float spawnTime;
    private bool hasHitTree;

    private void Start()
    {
        spawnTime = Time.time;

        // ランダムな初速度を設定（灰が散らばる感じ）
        float randomX = Random.Range(-spreadAmount, spreadAmount);
        float randomY = Random.Range(-0.5f, 0.5f);
        velocity = new Vector3(randomX, randomY - fallSpeed, 0f);
    }

    private void Update()
    {
        // 重力による落下
        velocity.y -= 2f * Time.deltaTime;

        // 移動
        transform.position += velocity * Time.deltaTime;

        // 寿命切れで削除
        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitTree) return;

        if (other.CompareTag("Tree"))
        {
            hasHitTree = true;
            GameManager.Instance?.OnAshHitTree();

            // 木に当たったら灰を消す（少し遅延させても良い）
            Destroy(gameObject, 0.1f);
        }
    }
}
