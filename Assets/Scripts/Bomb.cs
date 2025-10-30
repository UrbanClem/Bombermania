using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class Bomb : MonoBehaviour
{
    [Header("Auto-assign por nombre si está en null")]
    public string gridName = "Grid";
    public string solidMapName = "Walls_Solid";
    public string breakableMapName = "Walls_Breakable";

    [Header("Refs de escena")]
    public Grid grid;
    public Tilemap solidMap;
    public Tilemap breakableMap;

    [Header("Explosión")]
    public float fuseSeconds = 1.0f;
    public int range = 1;
    public GameObject explosionVfx;
    public float vfxSeconds = 0.25f;

    [Header("Daño")]
    public GameObject explosionHitboxPrefab; // <-- Asignar ExplosionHitbox prefab
    public float hitboxLifetime = 0.2f;

    [Header("Preview (opcional)")]
    public GameObject previewVfx;
    public float previewZOffset = 0f;

    [Header("Drops (power-ups)")]
    [Range(0f, 1f)] public float dropChance = 0.25f; // 25% prob por bloque destruido
    public GameObject[] dropPrefabs; // lista de prefabs posibles (rango, velocidad, capacidad)

    [HideInInspector] public PlayerBombPlacer owner; // para liberar capacidad

    private Vector3Int cellOrigin;
    private readonly List<GameObject> spawnedPreview = new List<GameObject>();

    private void Awake()
    {
        if (grid == null)
        {
            var go = GameObject.Find(gridName);
            if (go) grid = go.GetComponent<Grid>();
            if (grid == null)
            {
                grid = Object.FindFirstObjectByType<Grid>();
                if (grid == null) grid = Object.FindAnyObjectByType<Grid>();
            }
        }
        if (solidMap == null)
        {
            var solidGO = GameObject.Find(solidMapName);
            if (solidGO) solidMap = solidGO.GetComponent<Tilemap>();
        }
        if (breakableMap == null)
        {
            var breakGO = GameObject.Find(breakableMapName);
            if (breakGO) breakableMap = breakGO.GetComponent<Tilemap>();
        }
    }

    private void Start()
    {
        if (grid == null)
        {
            Debug.LogError("[Bomb] No se encontró Grid en la escena.");
            return;
        }

        cellOrigin = grid.WorldToCell(transform.position);
        Vector3 center = grid.GetCellCenterWorld(cellOrigin);
        transform.position = new Vector3(center.x, center.y, 0f);

        ShowPreview();

        StartCoroutine(Fuse());
    }

    private IEnumerator Fuse()
    {
        yield return new WaitForSeconds(fuseSeconds);
        ClearPreview();
        Explode();
        owner?.OnBombFinished(); // libera capacidad
        Destroy(gameObject);
    }

    private void ShowPreview()
    {
        if (previewVfx == null) return;

        spawnedPreview.Add(SpawnVFX(previewVfx, cellOrigin, fuseSeconds, previewZOffset));

        var dirs = new Vector3Int[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        foreach (var dir in dirs)
        {
            for (int step = 1; step <= range; step++)
            {
                var cell = cellOrigin + dir * step;

                if (solidMap != null && solidMap.HasTile(cell)) break;

                spawnedPreview.Add(SpawnVFX(previewVfx, cell, fuseSeconds, previewZOffset));

                if (breakableMap != null && breakableMap.HasTile(cell)) break;
            }
        }
    }

    private void ClearPreview()
    {
        foreach (var go in spawnedPreview) if (go) Destroy(go);
        spawnedPreview.Clear();
    }

    private void Explode()
    {
        // centro
        DoExplosionAt(cellOrigin);

        var dirs = new Vector3Int[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        foreach (var dir in dirs)
        {
            for (int step = 1; step <= range; step++)
            {
                var cell = cellOrigin + dir * step;

                if (solidMap != null && solidMap.HasTile(cell)) break;

                if (breakableMap != null && breakableMap.HasTile(cell))
                {
                    // Romper bloque
                    breakableMap.SetTile(cell, null);
                    DoExplosionAt(cell);   // golpea esa celda
                    TryDrop(cell);         // intenta soltar power-up
                    break;                 // detiene avance
                }

                DoExplosionAt(cell);
            }
        }
    }

    private void DoExplosionAt(Vector3Int cell)
    {
        if (explosionVfx != null) SpawnVFX(explosionVfx, cell, vfxSeconds, 0f);
        if (explosionHitboxPrefab != null)
        {
            Vector3 pos = grid.GetCellCenterWorld(cell);
            var hb = Instantiate(explosionHitboxPrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
            var comp = hb.GetComponent<ExplosionHitbox>();
            if (comp != null) comp.lifetime = hitboxLifetime;
            else Destroy(hb, hitboxLifetime);
        }
    }

    private void TryDrop(Vector3Int cell)
    {
        if (dropPrefabs == null || dropPrefabs.Length == 0) return;
        if (Random.value > dropChance) return;

        // elige un power-up al azar de la lista
        int idx = Random.Range(0, dropPrefabs.Length);
        Vector3 pos = grid.GetCellCenterWorld(cell);
        Instantiate(dropPrefabs[idx], new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
    }

    private GameObject SpawnVFX(GameObject prefab, Vector3Int cell, float life, float zOffset)
    {
        Vector3 pos = grid.GetCellCenterWorld(cell);
        var go = Instantiate(prefab, new Vector3(pos.x, pos.y, zOffset), Quaternion.identity);
        if (life > 0f) Destroy(go, life);
        return go;
    }
}
