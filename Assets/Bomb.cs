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

    [Header("Refs de escena (pueden quedar en null y se auto-asignan)")]
    public Grid grid;
    public Tilemap solidMap;
    public Tilemap breakableMap;

    [Header("Explosión")]
    public float fuseSeconds = 1.0f;      // tiempo de mecha
    public int range = 1;                 // se setea desde el Player (power-ups)
    public GameObject explosionVfx;       // VFX de la explosión (centrales y brazos)
    public float vfxSeconds = 0.35f;

    [Header("Preview (cruz fantasma durante la mecha)")]
    public GameObject previewVfx;         // VFX fantasma (bajita opacidad)
    public float previewZOffset = 0.0f;   // por si quieres ponerla debajo/encima

    private Vector3Int cellOrigin;
    private readonly List<GameObject> spawnedPreview = new List<GameObject>();

    private void Awake()
    {
        // Auto-asignación básica si vienen en null
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

        // Centrar a celda
        cellOrigin = grid.WorldToCell(transform.position);
        Vector3 center = grid.GetCellCenterWorld(cellOrigin);
        transform.position = new Vector3(center.x, center.y, 0f);

        // Mostrar preview durante la mecha
        ShowPreview();

        StartCoroutine(Fuse());
    }

    private IEnumerator Fuse()
    {
        yield return new WaitForSeconds(fuseSeconds);
        ClearPreview();
        Explode();
        Destroy(gameObject); // elimina la propia bomba
    }

    private void ShowPreview()
    {
        if (previewVfx == null) return;

        // centro
        spawnedPreview.Add(SpawnVFX(previewVfx, cellOrigin, vfxSeconds: fuseSeconds, zOffset: previewZOffset));

        // brazos
        var dirs = new Vector3Int[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        foreach (var dir in dirs)
        {
            for (int step = 1; step <= range; step++)
            {
                var cell = cellOrigin + dir * step;

                if (solidMap != null && solidMap.HasTile(cell))
                    break;

                if (breakableMap != null && breakableMap.HasTile(cell))
                {
                    spawnedPreview.Add(SpawnVFX(previewVfx, cell, fuseSeconds, previewZOffset));
                    break;
                }

                spawnedPreview.Add(SpawnVFX(previewVfx, cell, fuseSeconds, previewZOffset));
            }
        }
    }

    private void ClearPreview()
    {
        // Si pusiste Destroy con tiempo, esto es casi opcional; por si acaso:
        foreach (var go in spawnedPreview)
            if (go) Destroy(go);
        spawnedPreview.Clear();
    }

    private void Explode()
    {
        // centro
        if (explosionVfx != null) SpawnVFX(explosionVfx, cellOrigin, vfxSeconds, 0f);

        var dirs = new Vector3Int[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        foreach (var dir in dirs)
        {
            for (int step = 1; step <= range; step++)
            {
                var cell = cellOrigin + dir * step;

                if (solidMap != null && solidMap.HasTile(cell))
                    break;

                if (breakableMap != null && breakableMap.HasTile(cell))
                {
                    breakableMap.SetTile(cell, null); // rompe
                    if (explosionVfx != null) SpawnVFX(explosionVfx, cell, vfxSeconds, 0f);
                    break;
                }

                if (explosionVfx != null) SpawnVFX(explosionVfx, cell, vfxSeconds, 0f);
            }
        }
    }

    private GameObject SpawnVFX(GameObject prefab, Vector3Int cell, float vfxSeconds, float zOffset)
    {
        Vector3 pos = grid.GetCellCenterWorld(cell);
        var go = Instantiate(prefab, new Vector3(pos.x, pos.y, zOffset), Quaternion.identity);
        if (vfxSeconds > 0f) Destroy(go, vfxSeconds);
        return go;
    }
}
