using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ExitPortal : MonoBehaviour
{
    public AudioClip lockedSfx;
    public AudioClip openSfx;
    public AudioClip victoryMusic; // ✅ NUEVO: Música de victoria
    
    [Header("Render Settings")]
    [SerializeField] private int sortingOrder = -1; // Más bajo que las bombas
    
    [Header("Transition Settings")]
    [SerializeField] private float transitionDelay = 2f; // ✅ NUEVO: Tiempo de delay antes de cambiar escena
    [SerializeField] private string nextSceneName = "Stage2"; // ✅ NUEVO: Nombre de la siguiente escena

    private LevelManager manager;
    private EnemyManager enemyManager; // ✅ NUEVO: Referencia al EnemyManager
    private AudioSource audioSrc;
    private SpriteRenderer spriteRenderer;
    private bool isTransitioning = false; // ✅ NUEVO: Evitar múltiples triggers

    private void Awake()
    {
        manager = FindFirstObjectByType<LevelManager>();
        enemyManager = FindFirstObjectByType<EnemyManager>(); // ✅ NUEVO: Buscar EnemyManager
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        
        // Obtener o agregar SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        // Configurar sorting order
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }

    private void Start()
    {
        // Asegurar el sorting order en Start también
        if (spriteRenderer != null && spriteRenderer.sortingOrder >= 0)
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isTransitioning) return;

        // ✅ MODIFICADO: Verificar si todos los enemigos están muertos
        bool canExit = enemyManager != null && enemyManager.AllEnemiesDead;

        if (canExit)
        {
            if (openSfx) audioSrc.PlayOneShot(openSfx);
            StartCoroutine(TransitionToNextStage(other.gameObject)); // ✅ NUEVO: Iniciar transición
        }
        else
        {
            if (lockedSfx) audioSrc.PlayOneShot(lockedSfx);
            Debug.Log("[ExitPortal] La salida todavía está bloqueada. Enemigos restantes: " + 
                     (enemyManager != null ? enemyManager.GetRemainingEnemies() : "N/A"));
        }
    }

    // ✅ NUEVO: Corrutina para manejar la transición
    private IEnumerator TransitionToNextStage(GameObject player)
    {
        isTransitioning = true;
        
        Debug.Log("[ExitPortal] Iniciando transición a Stage2...");
        
        // 1. Deshabilitar movimiento del jugador usando tu método EnableControl
        TopDownShooter.PlayerMovement playerMovement = player.GetComponent<TopDownShooter.PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.EnableControl(false);
            Debug.Log("[ExitPortal] Movimiento del jugador deshabilitado");
        }
        
        // 2. Reproducir música de victoria si está asignada
        if (victoryMusic != null && audioSrc != null)
        {
            audioSrc.Stop(); // Detener sonido actual
            audioSrc.clip = victoryMusic;
            audioSrc.loop = false;
            audioSrc.Play();
            Debug.Log("[ExitPortal] Reproduciendo música de victoria");
        }
        
        // 3. Opcional: Mostrar algún efecto visual o texto
        ShowTransitionEffect();
        
        // 4. Esperar el delay
        Debug.Log($"[ExitPortal] Esperando {transitionDelay} segundos...");
        yield return new WaitForSeconds(transitionDelay);
        
        // 5. Cargar la siguiente escena
        Debug.Log($"[ExitPortal] Cargando escena: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }

    // ✅ NUEVO: Método para efectos visuales durante la transición
    private void ShowTransitionEffect()
    {
        // Aquí puedes agregar efectos como:
        // - Partículas
        // - Fade de pantalla
        // - Texto de "¡Victoria!"
        Debug.Log("[ExitPortal] Mostrando efecto de transición");
        
        // Ejemplo: Hacer que el portal brille más
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashPortal());
        }
    }

    // ✅ NUEVO: Corrutina para hacer brillar el portal
    private IEnumerator FlashPortal()
    {
        Color originalColor = spriteRenderer.color;
        float flashDuration = 0.2f;
        int flashCount = 3;
        
        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }
    }
}