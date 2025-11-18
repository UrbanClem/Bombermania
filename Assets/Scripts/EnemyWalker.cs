using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyWalker : MonoBehaviour
{
    [Header("Refs (auto-assign si null)")]
    public Grid grid;
    public Tilemap solidMap;
    public Tilemap breakableMap;

    [Header("Obstáculos adicionales (LayerMask)")]
    public LayerMask bombMask; //  asigna la capa de Bomb si la usas

    [Header("Movimiento por celdas")]
    public float stepTime = 0.22f;
    public bool startRandomDirection = true;

    private Vector3Int currentCell;
    private Vector3Int dir;
    private Rigidbody2D rb;

    private static readonly Vector3Int[] DIRS = {
        Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
    };

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        if (grid == null) grid = FindFirstObjectByType<Grid>();
        if (solidMap == null)
        {
            var go = GameObject.Find("Walls_Solid");
            if (go) solidMap = go.GetComponent<Tilemap>();
        }
        if (breakableMap == null)
        {
            var go = GameObject.Find("Walls_Breakable");
            if (go) breakableMap = go.GetComponent<Tilemap>();
        }

        SnapToCell();
        dir = startRandomDirection ? DIRS[Random.Range(0, DIRS.Length)] : Vector3Int.left;
        StartCoroutine(WalkLoop());
    }

    private void SnapToCell()
    {
        currentCell = grid.WorldToCell(transform.position);
        transform.position = grid.GetCellCenterWorld(currentCell);
    }

    private bool IsBlocked(Vector3Int cell)
    {
        // 1) Tilemaps
        if (solidMap != null && solidMap.HasTile(cell)) return true;
        if (breakableMap != null && breakableMap.HasTile(cell)) return true;

        // 2) Bombas u otros colliders por máscara
        if (bombMask.value != 0)
        {
            Vector2 p = (Vector2)grid.GetCellCenterWorld(cell);
            if (Physics2D.OverlapPoint(p, bombMask) != null) return true;
        }
        return false;
    }

    private Vector3Int PickNextDir()
    {
        List<Vector3Int> options = new List<Vector3Int>();
        foreach (var d in DIRS)
        {
            var n = currentCell + d;
            if (!IsBlocked(n)) options.Add(d);
        }
        if (options.Count == 0) return Vector3Int.zero;
        if (options.Contains(dir)) return dir;
        options.Remove(-dir);
        if (options.Count == 0) return -dir;
        return options[Random.Range(0, options.Count)];
    }

    private IEnumerator WalkLoop()
    {
        while (true)
        {
            dir = PickNextDir();
            if (dir == Vector3Int.zero)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            var nextCell = currentCell + dir;
            var nextWorld = grid.GetCellCenterWorld(nextCell);

            // moverse usando físicas (no atraviesa colliders)
            float t = 0f;
            Vector3 start = transform.position;
            while (t < stepTime)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / stepTime);
                rb.MovePosition(Vector3.Lerp(start, nextWorld, k));
                yield return null;
            }

            rb.MovePosition(nextWorld);
            currentCell = nextCell;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // si tu ExplosionHitbox tiene tag/nombre, destruye aquí
        if (other.gameObject.name.Contains("ExplosionHitbox"))
            Destroy(gameObject);
    }
}
