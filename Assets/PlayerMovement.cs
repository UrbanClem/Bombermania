using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDownShooter
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        private Vector2 movementDirection;

        [Header("Audio")]
        [SerializeField] private AudioClip moveSound; // Arrastra el sonido aquí en el Inspector
        [SerializeField] private float soundCooldown = 0.3f; // Evita que suene demasiado rápido
        
        private Rigidbody2D rb;
        private AudioSource audioSource;
        private float lastSoundTime;
        private bool wasMoving = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            audioSource = GetComponent<AudioSource>();
            
            // Si no hay AudioSource, crear uno
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        private void OnMove(InputValue value)
        {
            Vector2 input = value.Get<Vector2>();
            
            // Reiniciar dirección
            movementDirection = Vector2.zero;
            
            // Solo permitir una dirección a la vez (sin diagonales)
            if (Mathf.Abs(input.y) > 0.1f)
            {
                movementDirection.y = Mathf.Sign(input.y); // Arriba o abajo
                movementDirection.x = 0; // Asegurar que no haya movimiento horizontal
            }
            else if (Mathf.Abs(input.x) > 0.1f)
            {
                movementDirection.x = Mathf.Sign(input.x); // Izquierda o derecha
                movementDirection.y = 0; // Asegurar que no haya movimiento vertical
            }
            
            movementDirection = movementDirection.normalized;
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = movementDirection * moveSpeed;
            
            // Controlar el sonido del movimiento
            HandleMovementSound();
        }

        private void HandleMovementSound()
        {
            bool isMoving = movementDirection.magnitude > 0.1f;
            
            // Reproducir sonido cuando empieza a moverse
            if (isMoving && !wasMoving)
            {
                PlayMoveSound();
            }
            
            wasMoving = isMoving;
        }

        private void PlayMoveSound()
        {
            // Verificar cooldown para no saturar con sonidos
            if (Time.time - lastSoundTime >= soundCooldown && moveSound != null)
            {
                audioSource.PlayOneShot(moveSound);
                lastSoundTime = Time.time;
            }
        }
    }
}