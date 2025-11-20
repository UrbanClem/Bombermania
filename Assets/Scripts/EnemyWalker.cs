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

    [Header("Animations")]
    public Animator enemyAnimator;
    public SpriteRenderer spriteRenderer;

    private Vector3Int currentCell;
    private Vector3Int dir;
    private Rigidbody2D rb;
    private Vector2 lastMovementDirection = Vector2.left;
    private bool isDead = false;
    private Coroutine walkCoroutine;

    private static readonly Vector3Int[] DIRS = {
        Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
    };

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Obtener componentes de animación si no están asignados
        if (enemyAnimator == null) enemyAnimator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
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
        UpdateLastMovementDirection();
        walkCoroutine = StartCoroutine(WalkLoop());
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

    private void UpdateLastMovementDirection()
    {
        // Convertir Vector3Int a Vector2 para el sistema de animación
        lastMovementDirection = new Vector2(dir.x, dir.y);
        
        // Actualizar parámetros del Animator
        if (enemyAnimator != null)
        {
            enemyAnimator.SetFloat("MoveX", lastMovementDirection.x);
            enemyAnimator.SetFloat("MoveY", lastMovementDirection.y);
            enemyAnimator.SetBool("IsMoving", true);
        }

        // Aplicar flip para izquierda/derecha
        if (spriteRenderer != null)
        {
            if (dir.x < 0) spriteRenderer.flipX = true;
            else if (dir.x > 0) spriteRenderer.flipX = false;
        }
    }

    private IEnumerator WalkLoop()
    {
        while (true)
        {
            // Si está muerto, detener el movimiento
            if (isDead) yield break;
            
            dir = PickNextDir();
            
            if (dir == Vector3Int.zero)
            {
                // Si no puede moverse, poner animación de idle
                if (enemyAnimator != null)
                    enemyAnimator.SetBool("IsMoving", false);
                
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            // Actualizar animación con la nueva dirección
            UpdateLastMovementDirection();

            var nextCell = currentCell + dir;
            var nextWorld = grid.GetCellCenterWorld(nextCell);

            // Moverse usando físicas (no atraviesa colliders)
            float t = 0f;
            Vector3 start = transform.position;
            while (t < stepTime)
            {
                // Si muere durante el movimiento, detenerse
                if (isDead) yield break;
                
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
        // Si tu ExplosionHitbox tiene tag/nombre, destruye aquí
        if (other.gameObject.name.Contains("ExplosionHitbox"))
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return; // Evitar múltiples llamadas
        
        isDead = true;
        
        // Detener el movimiento inmediatamente
        if (walkCoroutine != null)
            StopCoroutine(walkCoroutine);
        
        // Detener la física
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        
        // Actualizar parámetro del Animator
        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("IsDead", true);
            enemyAnimator.SetBool("IsMoving", false);
        }

        // Desactivar el collider para evitar más colisiones
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;

        // Esperar a que la animación de muerte se reproduzca antes de destruir
        StartCoroutine(DestroyAfterAnimation());
    }

    private IEnumerator DestroyAfterAnimation()
    {
        // Esperar un tiempo suficiente para que la animación de muerte se reproduzca
        yield return new WaitForSeconds(3f); // Ajusta este tiempo según tu animación
        
        // Ahora destruir el objeto
        Destroy(gameObject);
    }

    // Método público para verificar si está muerto
    public bool IsDead()
    {
        return isDead;
    }
}