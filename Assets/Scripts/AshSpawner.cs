using UnityEngine;

/// <summary>
/// クリック/タップ位置に灰を生成するクラス
/// </summary>
public class AshSpawner : MonoBehaviour
{
    [Header("灰の設定")]
    [SerializeField] private GameObject ashPrefab;
    [SerializeField] private float spawnCooldown = 0.1f;

    private Camera mainCamera;
    private float lastSpawnTime;
    private bool isEnabled = true;

    private void Start()
    {
        mainCamera = Camera.main;
        lastSpawnTime = -spawnCooldown;
    }

    private void Update()
    {
        if (!isEnabled) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // マウスクリック or タッチ検出
        if (Input.GetMouseButton(0))
        {
            if (Time.time - lastSpawnTime >= spawnCooldown)
            {
                SpawnAsh();
                lastSpawnTime = Time.time;
            }
        }
    }

    private void SpawnAsh()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // カメラからの距離
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

        if (ashPrefab != null)
        {
            Instantiate(ashPrefab, worldPos, Quaternion.identity);
            GameManager.Instance?.OnAshUsed();
        }
    }

    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
    }
}
