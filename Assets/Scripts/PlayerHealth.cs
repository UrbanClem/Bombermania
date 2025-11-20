using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Salud")]
    public int maxHealth = 1;
    
    [Header("Efectos de Muerte")]
    public AudioClip deathSound;
    public float deathSoundVolume = 1f;
    
    private int currentHealth;
    private TopDownShooter.PlayerMovement movement;
    private Animator animator;
    private AudioSource audioSource;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        movement = GetComponent<TopDownShooter.PlayerMovement>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        // Si no hay AudioSource, crear uno
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        if (animator == null)
        {
            Debug.LogWarning("No se encontró Animator en el jugador");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || currentHealth <= 0) return;
        
        currentHealth -= damage;
        Debug.Log($"Daño recibido. Salud: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log("Iniciando animación de muerte...");
        
        // Desactivar control inmediatamente
        if (movement != null)
            movement.EnableControl(false);

        // Reproducir sonido de muerte
        PlayDeathSound();

        // Reproducir animación de muerte
        if (animator != null)
        {
            animator.SetTrigger("Die");
            StartCoroutine(WaitForDeathAnimation());
        }
        else
        {
            // Si no hay animator, reiniciar después de un delay
            Invoke("NotifyGameManager", 1.5f);
        }
    }

    private void PlayDeathSound()
    {
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound, deathSoundVolume);
            Debug.Log("🔊 Reproduciendo sonido de muerte");
        }
        else
        {
            if (deathSound == null)
                Debug.LogWarning("No hay sonido de muerte asignado");
            if (audioSource == null)
                Debug.LogWarning("No hay AudioSource en el jugador");
        }
    }

    private IEnumerator WaitForDeathAnimation()
    {
        // Esperar un frame para asegurar que la animación empezó
        yield return null;
        
        // Esperar a que termine la animación actual
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;
        
        Debug.Log($"Duración de animación de muerte: {animationLength} segundos");
        
        // Esperar la duración de la animación + un pequeño extra
        yield return new WaitForSeconds(animationLength + 0.2f);
        
        // Notificar al GameManager
        NotifyGameManager();
    }

    private void NotifyGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDied();
        }
        else
        {
            // Fallback
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }

    // Se llama automáticamente cuando se reinicia la escena
    private void OnEnable()
    {
        ResetPlayer();
    }

    private void ResetPlayer()
    {
        isDead = false;
        currentHealth = maxHealth;
        
        // Reactivar el control
        if (movement != null)
            movement.EnableControl(true);

        // Asegurar que el Animator esté en estado normal
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }
}