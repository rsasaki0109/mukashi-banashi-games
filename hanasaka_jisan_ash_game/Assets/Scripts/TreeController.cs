using UnityEngine;

/// <summary>
/// 枯れ木/満開の木を制御するクラス
/// </summary>
public class TreeController : MonoBehaviour
{
    [Header("参照")]
    [SerializeField] private GameObject deadTree;
    [SerializeField] private GameObject bloomingTree;
    [SerializeField] private SpriteRenderer deadTreeRenderer;
    [SerializeField] private SpriteRenderer bloomingTreeRenderer;

    [Header("色設定")]
    [SerializeField] private Color deadTreeColor = new Color(0.4f, 0.3f, 0.2f);
    [SerializeField] private Color bloomingTreeColor = new Color(1f, 0.7f, 0.8f);

    private int ashHitCount;
    private bool isBloom;

    public int AshHitCount => ashHitCount;
    public bool IsBloom => isBloom;

    private void Start()
    {
        ResetTree();
    }

    public void ResetTree()
    {
        ashHitCount = 0;
        isBloom = false;

        if (deadTree != null) deadTree.SetActive(true);
        if (bloomingTree != null) bloomingTree.SetActive(false);

        // 色を設定
        if (deadTreeRenderer != null) deadTreeRenderer.color = deadTreeColor;
        if (bloomingTreeRenderer != null) bloomingTreeRenderer.color = bloomingTreeColor;
    }

    public void AddAshHit()
    {
        ashHitCount++;
    }

    public void Bloom()
    {
        if (isBloom) return;

        isBloom = true;

        // 枯れ木を非表示、満開の木を表示
        if (deadTree != null) deadTree.SetActive(false);
        if (bloomingTree != null) bloomingTree.SetActive(true);
    }
}
