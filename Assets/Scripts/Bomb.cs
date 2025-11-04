using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class Bomb : MonoBehaviour
{
    [Header("Drops")]
<<<<<<< HEAD
    public LayerMask powerUpMask;  // ← asigna la capa PowerUp en el Inspector
=======
public LayerMask powerUpMask;  // ← asigna la capa PowerUp en el Inspector
>>>>>>> f963b82694d3c94ef95ffb5f1fc54b046b09a11a

    [Header("Auto-assign por nombre si está en null")]
    public string gridName = "Grid";
    public string solidMapName = "Walls_Solid";
    public string breakableMapName = "Walls_Breakable";

    [Header("Refs de escena")]
    public Grid grid;
    public Tilemap solidMap;
    public Tilemap breakableMap;

    [Header("Detección por Colliders (fallback)")]
    public LayerMask solidMask;       // ← asigna capa SolidWalls
    public LayerMask breakableMask;   // ← asigna capa BreakableWalls

    // Reutilizamos buffer para evitar allocs
    private readonly Collider2D[] _overlapBuf = new Collider2D[8];

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

    [Header("SFX")]
    public AudioClip explosionSfx;
    private AudioSource _audio;
    [Range(0f, 1f)] public float explosionVolume = 1f;
<<<<<<< HEAD
=======


>>>>>>> f963b82694d3c94ef95ffb5f1fc54b046b09a11a

    [HideInInspector] public PlayerBombPlacer owner; // para liberar capacidad

    private Vector3Int cellOrigin;
    private readonly List<GameObject> spawnedPreview = new List<GameObject>();

    private LevelManager levelManager;

    private void Awake()
    {
        if (levelManager == null) levelManager = FindFirstObjectByType<LevelManager>();
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
        _audio = GetComponent<AudioSource>();
        if (_audio == null)
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 0f; // 2D (ajusta si quieres 3D)
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

    // --- ShowPreview consistente con colisiones ---
    private void ShowPreview()
    {
        if (previewVfx == null) return;

        // centro
        spawnedPreview.Add(SpawnVFX(previewVfx, cellOrigin, fuseSeconds, previewZOffset));

        var dirs = new Vector3Int[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        foreach (var dir in dirs)
        {
            var cells = GetLineCells(dir);
            for (int i = 0; i < cells.Count; i++)
            {
                spawnedPreview.Add(SpawnVFX(previewVfx, cells[i], fuseSeconds, previewZOffset));
            }
        }
    }


    private void ClearPreview()
    {
        foreach (var go in spawnedPreview) if (go) Destroy(go);
        spawnedPreview.Clear();
    }

    // ====== Helpers robustos (sin API obsoleta) ======
    private bool IsSolid(Vector3Int cell)
    {
        // 1) Tilemap primero
        if (solidMap != null && solidMap.HasTile(cell)) return true;

        // 2) Fallback por collider (LayerMask)
        Vector2 p = (Vector2)grid.GetCellCenterWorld(cell);
        // Devuelve el primer collider que encuentre en la máscara
        Collider2D hit = Physics2D.OverlapPoint(p, solidMask);
        return hit != null;
    }

    private bool IsBreakable(Vector3Int cell)
    {
        // 1) Tilemap primero
        if (breakableMap != null && breakableMap.HasTile(cell)) return true;

        // 2) Fallback por collider (LayerMask)
        Vector2 p = (Vector2)grid.GetCellCenterWorld(cell);
        Collider2D hit = Physics2D.OverlapPoint(p, breakableMask);
        return hit != null;
    }

    private void BreakTile(Vector3Int cell)
    {
        // Si existe como tile en el Tilemap, quítalo y listo
        if (breakableMap != null && breakableMap.HasTile(cell))
        {
            breakableMap.SetTile(cell, null);
            levelManager?.OnBreakableDestroyed(cell); // 👈 AVISAR
            return;
        }

        // Si tus rompibles son GameObjects con colliders (no Tilemap)
        Vector2 p = (Vector2)grid.GetCellCenterWorld(cell);
        Collider2D[] hits = Physics2D.OverlapPointAll(p, breakableMask);
        for (int i = 0; i < hits.Length; i++)
        {
            var go = hits[i].gameObject;
            if (go != null && go != this.gameObject)
            {
                Destroy(go);
            }
        }
        levelManager?.OnBreakableDestroyed(cell);
    }




    // --- Explode usando la MISMA ruta ---
    private void Explode()
    {
        PlayOneShot2D(explosionSfx, explosionVolume);
<<<<<<< HEAD
=======
        if (explosionSfx != null)
            AudioSource.PlayClipAtPoint(explosionSfx, transform.position, 1f);
>>>>>>> f963b82694d3c94ef95ffb5f1fc54b046b09a11a

        DoExplosionAt(cellOrigin);

        var dirs = new Vector3Int[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        foreach (var dir in dirs)
        {
            var cells = GetLineCells(dir);
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];

                // si es rompible, la rompemos y paramos (GetLineCells ya incluye solo la primera)
                if (IsBreakable(cell))
                {
                    BreakTile(cell);

                    // 1) Salida: si era la celda elegida
                    bool spawnedExit = false;
                    if (levelManager != null)
                    {
                        spawnedExit = levelManager.TrySpawnExitAt(cell);
                        levelManager.OnBreakableDestroyed(cell);
                    }

                    // 2) Explosión visual/daño en esta celda PERO SIN limpiar drops aquí
                    DoExplosionAt(cell, clearDrops: false);

                    // 3) Si NO era salida, recién ahora intentamos drop
                    if (!spawnedExit)
                    {
                        TryDrop(cell);
                    }

                    // 4) Detener la propagación
                    break;
                }


                // vacío
                DoExplosionAt(cell);
            }
        }
    }

    private void DoExplosionAt(Vector3Int cell, bool clearDrops = true)
    {
        if (explosionVfx != null) SpawnVFX(explosionVfx, cell, vfxSeconds, 0f);

        // hitbox de daño (como ya lo tenías)
        if (explosionHitboxPrefab != null)
        {
            Vector3 pos = grid.GetCellCenterWorld(cell);
            var hb = Instantiate(explosionHitboxPrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
            var comp = hb.GetComponent<ExplosionHitbox>();
            if (comp != null) comp.lifetime = hitboxLifetime;
            else Destroy(hb, hitboxLifetime);
        }

        // 🧹 limpia drops solo si corresponde
        if (clearDrops)
            ClearPowerUpsAt(cell);
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

    // Devuelve las celdas en una dirección,
    // deteniéndose en el primer bloque destructible o en una pared sólida.
    private List<Vector3Int> GetLineCells(Vector3Int dir)
    {
        var list = new List<Vector3Int>();
        for (int step = 1; step <= range; step++)
        {
            var cell = cellOrigin + dir * step;

            if (IsSolid(cell))
            {
                // pared indestructible: no incluimos esta celda y paramos
                break;
            }

            if (IsBreakable(cell))
            {
                // añadimos la celda destructible y paramos
                list.Add(cell);
                break;
            }

            // vacío: añadimos y seguimos
            list.Add(cell);
        }
        return list;
    }
    private void ClearPowerUpsAt(Vector3Int cell)
    {
        if (powerUpMask == 0) return;
        Vector2 p = (Vector2)grid.GetCellCenterWorld(cell);
        var hits = Physics2D.OverlapPointAll(p, powerUpMask);
        for (int i = 0; i < hits.Length; i++)
        {
            var go = hits[i].gameObject;
            if (go != null && go != this.gameObject)
            {
                Destroy(go);
            }
        }
    }
    private static void PlayOneShot2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        var go = new GameObject("OneShot2D_Audio");
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;   // 2D
        src.volume = volume;
        src.clip = clip;
        src.priority = 128;      // prioridad normal
        src.Play();
        Object.Destroy(go, clip.length);
    }

    private static void PlayOneShot2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        var go = new GameObject("OneShot2D_Audio");
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;   // 2D
        src.volume = volume;
        src.clip = clip;
        src.priority = 128;      // prioridad normal
        src.Play();
        Object.Destroy(go, clip.length);
    }

}
