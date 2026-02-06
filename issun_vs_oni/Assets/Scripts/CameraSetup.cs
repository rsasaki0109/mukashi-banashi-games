using UnityEngine;

/// <summary>
/// カメラに付けて画面境界の壁を自動生成
/// </summary>
public class CameraSetup : MonoBehaviour
{
    public float wallThickness = 1f;

    void Start()
    {
        CreateBoundaries();
    }

    void CreateBoundaries()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null) return;

        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;

        // 親オブジェクト
        GameObject boundaries = new GameObject("Boundaries");

        // 上
        CreateWall(boundaries.transform, "Top",
            new Vector3(0, cam.orthographicSize + wallThickness / 2f, 0),
            new Vector2(width + wallThickness * 2, wallThickness));

        // 下
        CreateWall(boundaries.transform, "Bottom",
            new Vector3(0, -cam.orthographicSize - wallThickness / 2f, 0),
            new Vector2(width + wallThickness * 2, wallThickness));

        // 左
        CreateWall(boundaries.transform, "Left",
            new Vector3(-width / 2f - wallThickness / 2f, 0, 0),
            new Vector2(wallThickness, height));

        // 右
        CreateWall(boundaries.transform, "Right",
            new Vector3(width / 2f + wallThickness / 2f, 0, 0),
            new Vector2(wallThickness, height));
    }

    void CreateWall(Transform parent, string name, Vector3 position, Vector2 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.parent = parent;
        wall.transform.position = position;

        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;
    }
}
