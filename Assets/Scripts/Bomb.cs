using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class Bomb : MonoBehaviour
{
    [Header("Drops")]
    public LayerMask powerUpMask;  // ← asigna la capa PowerUp en el Inspector

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
    public float vfxSeconds = 0.25f;

    [Header("Daño")]
    public GameObject explosionHitboxPrefab; // <-- Asignar ExplosionHitbox prefab
    public float hitboxLifetime = 0.2f;

    [Header("Sprites de Explosión Individuales")]
    public Sprite explosionCenter;          // Bomb_Explosion_SpriteSheet_0
    public Sprite explosionHorizontal1;     // Bomb_Explosion_SpriteSheet_1  
    public Sprite explosionHorizontal2;     // Bomb_Explosion_SpriteSheet_2
    public Sprite explosionVertical1;       // Bomb_Explosion_SpriteSheet_3
    public Sprite explosionVertical2;       // Bomb_Explosion_SpriteSheet_4
    public Sprite explosionEndRight;        // Bomb_Explosion_SpriteSheet_5
    public Sprite explosionEndLeft;         // Bomb_Explosion_SpriteSheet_6
    public Sprite explosionEndUp;           // Bomb_Explosion_SpriteSheet_7
    public Sprite explosionEndDown;         // Bomb_Explosion_SpriteSheet_8

    [Header("Drops (power-ups)")]
    [Range(0f, 1f)] public float dropChance = 0.25f; // 25% prob por bloque destruido
    public GameObject[] dropPrefabs; // lista de prefabs posibles (rango, velocidad, capacidad)

    [Header("SFX")]
    public AudioClip explosionSfx;
    private AudioSource _audio;
    [Range(0f, 1f)] public float explosionVolume = 1f;

    [Header("Física de la bomba")]
    [SerializeField] private float tiempoHastaColision = 0.5f;
    [SerializeField] private LayerMask jugadorLayer;

    [HideInInspector] public PlayerBombPlacer owner; // para liberar capacidad

    private Vector3Int cellOrigin;
    private LevelManager levelManager;
    private Sprite[] explosionSprites;
    
    // Variables para el comportamiento de colisión temporal
    private Collider2D bombCollider;
    private bool colisionActivada = false;
    private GameObject jugadorQueColoco;

    private void Awake()
    {
        bombCollider = GetComponent<Collider2D>();
        
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
        // Inicializar array de sprites
        explosionSprites = new Sprite[]
        {
            explosionCenter,      // 0
            explosionHorizontal1, // 1
            explosionHorizontal2, // 2
            explosionVertical1,   // 3
            explosionVertical2,   // 4
            explosionEndRight,    // 5
            explosionEndLeft,     // 6
            explosionEndUp,       // 7
            explosionEndDown      // 8
        };

        // Configuración inicial de colisiones
        if (bombCollider != null)
        {
            // La bomba es atravesable SOLO para el jugador que la colocó
            // Pero sólida para enemigos desde el principio
            bombCollider.isTrigger = true;
        }
        
        if (grid == null)
        {
            Debug.LogError("[Bomb] No se encontró Grid en la escena.");
            return;
        }

        cellOrigin = grid.WorldToCell(transform.position);
        Vector3 center = grid.GetCellCenterWorld(cellOrigin);
        transform.position = new Vector3(center.x, center.y, 0f);

        StartCoroutine(Fuse());
    }

    // Método para configurar qué jugador colocó esta bomba
    public void SetJugadorQueColoco(GameObject jugador)
    {
        jugadorQueColoco = jugador;
    }

    private void ActivarColision()
    {
        colisionActivada = true;
        if (bombCollider != null)
        {
            bombCollider.isTrigger = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si un enemigo entra en la bomba, ignorar la colisión (es trigger)
        // Pero si la bomba ya está solidificada, bloquear al enemigo
        if (other.CompareTag("Enemy") && colisionActivada)
        {
            // La bomba ya está sólida, el enemigo no puede pasar
            // El collider normal se encargará de bloquearlo
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Solo el jugador que colocó la bomba puede activar la solidificación
        if (!colisionActivada && other.gameObject == jugadorQueColoco)
        {
            ActivarColision();
            Debug.Log("Bomba solidificada por jugador");
        }
        
        // Los enemigos ya no activan la solidificación (sólidos desde el principio)
    }

    private IEnumerator Fuse()
    {
        // Fallback: activar colisión después de un tiempo aunque el jugador no salga
        StartCoroutine(ActivarColisionPorTiempo());
        
        yield return new WaitForSeconds(fuseSeconds);
        Explode();
        owner?.OnBombFinished(); // libera capacidad
        Destroy(gameObject);
    }

    private IEnumerator ActivarColisionPorTiempo()
    {
        yield return new WaitForSeconds(tiempoHastaColision);
        if (!colisionActivada)
        {
            ActivarColision();
            Debug.Log("Bomba solidificada por tiempo");
        }
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

    // --- Explode usando sprites reales ---
    private void Explode()
    {
        PlayOneShot2D(explosionSfx, explosionVolume);

        // Crear explosión en el centro
        CreateExplosionSprite(cellOrigin, 0); // Centro es el sprite 0

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
                    CreateExplosionEnd(cell, dir);
                    DoExplosionAt(cell, clearDrops: false);

                    // 3) Si NO era salida, recién ahora intentamos drop
                    if (!spawnedExit)
                    {
                        TryDrop(cell);
                    }

                    // 4) Detener la propagación
                    break;
                }

                // vacío - crear sprite de explosión según la posición
                if (i == cells.Count - 1)
                {
                    // Última celda - crear extremo
                    CreateExplosionEnd(cell, dir);
                }
                else
                {
                    // Celda intermedia - crear segmento
                    CreateExplosionSegment(cell, dir, i);
                }

                DoExplosionAt(cell);
            }
        }
    }

    private void CreateExplosionSprite(Vector3Int cell, int spriteIndex)
    {
        if (explosionSprites == null || spriteIndex < 0 || spriteIndex >= explosionSprites.Length)
        {
            Debug.LogWarning($"[Bomb] Índice de sprite inválido: {spriteIndex}");
            return;
        }

        if (explosionSprites[spriteIndex] == null)
        {
            Debug.LogWarning($"[Bomb] Sprite en índice {spriteIndex} es null!");
            return;
        }

        Vector3 pos = grid.GetCellCenterWorld(cell);
        GameObject explosion = new GameObject($"ExplosionSprite_{spriteIndex}");
        explosion.transform.position = pos;
        
        SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
        sr.sprite = explosionSprites[spriteIndex];
        
        // Configuración de renderizado
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 10;

        // Destruir después del tiempo de VFX
        Destroy(explosion, vfxSeconds);
    }

    private void CreateExplosionSegment(Vector3Int cell, Vector3Int dir, int segmentIndex)
    {
        if (dir.x != 0) // Horizontal
        {
            // Alternar entre las dos versiones de segmentos horizontales
            int spriteIndex = (segmentIndex % 2 == 0) ? 1 : 2; // Sprite 1 o 2 para horizontal
            CreateExplosionSprite(cell, spriteIndex);
        }
        else if (dir.y != 0) // Vertical
        {
            // Alternar entre las dos versiones de segmentos verticales
            int spriteIndex = (segmentIndex % 2 == 0) ? 3 : 4; // Sprite 3 o 4 para vertical
            CreateExplosionSprite(cell, spriteIndex);
        }
    }

    private void CreateExplosionEnd(Vector3Int cell, Vector3Int dir)
    {
        if (dir == Vector3Int.right)
            CreateExplosionSprite(cell, 5); // Extremo derecho
        else if (dir == Vector3Int.left)
            CreateExplosionSprite(cell, 6); // Extremo izquierdo
        else if (dir == Vector3Int.up)
            CreateExplosionSprite(cell, 7); // Extremo superior
        else if (dir == Vector3Int.down)
            CreateExplosionSprite(cell, 8); // Extremo inferior
    }

    private void DoExplosionAt(Vector3Int cell, bool clearDrops = true)
    {
        // hitbox de daño
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
}