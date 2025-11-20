using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 1;
    private int currentHealth;
    private TopDownShooter.PlayerMovement movement;
    private Animator animator;
    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        movement = GetComponent<TopDownShooter.PlayerMovement>();
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("No se encontró Animator en el jugador");
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

        // Reproducir animación de muerte
        if (animator != null)
        {
            animator.SetTrigger("Die");
            
            // Esperar a que termine la animación antes de reiniciar
            StartCoroutine(WaitForDeathAnimation());
        }
        else
        {
            // Si no hay animator, reiniciar después de un delay
            Invoke("NotifyGameManager", 1.5f);
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