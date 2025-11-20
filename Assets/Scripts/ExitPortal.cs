using UnityEngine;

public class ExitPortal : MonoBehaviour
{
    public AudioClip lockedSfx;
    public AudioClip openSfx;
    
    [Header("Render Settings")]
    [SerializeField] private int sortingOrder = -1; // Más bajo que las bombas

    private LevelManager manager;
    private AudioSource audioSrc;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        manager = FindFirstObjectByType<LevelManager>();
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
        if (!other.CompareTag("Player")) return;

        if (manager != null && manager.CanExit)
        {
            if (openSfx) audioSrc.PlayOneShot(openSfx);
            manager.TryFinishLevel();
        }
        else
        {
            if (lockedSfx) audioSrc.PlayOneShot(lockedSfx);
            Debug.Log("[ExitPortal] La salida todavía está bloqueada.");
        }
    }
}