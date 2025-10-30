using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class Bomb : MonoBehaviour
{
    [Header("Auto-assign por nombre (si las refs están en null)")]
    public string gridName = "Grid";
    public string solidMapName = "Walls_Solid";
    public string breakableMapName = "Walls_Breakable";

    [Header("Refs (puedes dejarlas vacías y se asignan solas)")]
    public Grid grid;                    // instancia de escena
    public Tilemap solidMap;             // Walls_Solid
    public Tilemap breakableMap;         // Walls_Breakable

    [Header("Explosión")]
    public float fuseSeconds = 1.5f;
    public int range = 2;
    public GameObject explosionVfx;
    public float vfxSeconds = 0.35f;

    private Vector3Int cellOrigin;

    private void Awake()
    {
        // Si no hay Grid asignado, buscar por nombre o por tipo
        if (grid == null)
        {
            var go = GameObject.Find(gridName);
            if (go) grid = go.GetComponent<Grid>();
            if (grid == null)
            {
                // API nueva sin warning (Unity 2022+)
                grid = Object.FindFirstObjectByType<Grid>();
                if (grid == null) grid = Object.FindAnyObjectByType<Grid>();
            }
        }

        // Tilemaps por nombre
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

        // Fallback por búsqueda de todos los Tilemap si no se encontraron por nombre
        if (solidMap == null || breakableMap == null)
        {
            var maps = Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var m in maps)
            {
                if (solidMap == null && m.name == solidMapName) solidMap = m;
                if (breakableMap == null && m.name == breakableMapName) breakableMap = m;
            }
        }
    }

    private void Start()
    {
        if (grid == null)
        {
            Debug.LogError("[Bomb] No se encontró Grid en la escena.");
            return;
        }

        // Centrar a celda
        cellOrigin = grid.WorldToCell(transform.position);
        Vector3 center = grid.GetCellCenterWorld(cellOrigin);
        transform.position = new Vector3(center.x, center.y, 0f);

        StartCoroutine(Fuse());
    }

    private IEnumerator Fuse()
    {
        yield return new WaitForSeconds(fuseSeconds);
        Explode();
        Destroy(gameObject);
    }

    private void Explode()
    {
        SpawnVFX(cellOrigin);

        Vector3Int[] dirs = new Vector3Int[]
        {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
        };

        foreach (var dir in dirs)
        {
            for (int step = 1; step <= range; step++)
            {
                Vector3Int cell = cellOrigin + dir * step;

                if (solidMap != null && solidMap.HasTile(cell))
                    break;

                if (breakableMap != null && breakableMap.HasTile(cell))
                {
                    breakableMap.SetTile(cell, null);
                    SpawnVFX(cell);
                    break;
                }

                SpawnVFX(cell);
            }
        }
    }

    private void SpawnVFX(Vector3Int cell)
    {
        if (explosionVfx == null) return;
        Vector3 pos = grid.GetCellCenterWorld(cell);
        var fx = Instantiate(explosionVfx, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
        Destroy(fx, vfxSeconds);
    }
}
