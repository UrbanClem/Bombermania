using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDownShooter
{
    public class PlayerMovement : MonoBehaviour
    {
        // --- agrega dentro de la clase PlayerMovement ---
        private bool canMove = true;

    
        public void EnableControl(bool enable)
        {
            canMove = enable;
            if (!enable)
            {
                // detén al jugador y el sonido de caminar
                rb.linearVelocity = Vector2.zero;
            }
        }

        // --- pega esto dentro de PlayerMovement (TopDownShooter) ---

        // Getter opcional por si luego quieres leerlo
        public float CurrentSpeed => moveSpeed;

        // Sumar velocidad con tope
        public void AddSpeed(float amount, float maxSpeed)
        {
            moveSpeed = Mathf.Min(moveSpeed + amount, maxSpeed);
            // (Opcional) Debug o feedback
            // Debug.Log($"[PlayerMovement] Nueva velocidad: {moveSpeed}");
        }

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        private Vector2 movementDirection;

        [Header("Audio")]
        [SerializeField] private AudioClip moveSound;
        [SerializeField] private float soundInterval = 0.5f;
        private float soundTimer;
        
        [Header("Circle Effect")]
        [SerializeField] private GameObject circlePrefab;
        [SerializeField] private float circleDuration = 3f;
        
        [Header("Explosion Effect")]
        [SerializeField] private GameObject explosionLinePrefab;
        [SerializeField] private float explosionDuration = 1f;
        [SerializeField] private float explosionLength = 5f;
        [SerializeField] private float explosionWidth = 0.3f;
        
        [Header("Bomb Sounds")]
        [SerializeField] private AudioClip placeBombSound;
        [SerializeField] private AudioClip explosionSound;
        
        [Header("Input")]
        [SerializeField] private InputAction placeBombAction; // Nueva acción de input

        // Variables para animaciones
        private Vector2 lastMovementDirection = Vector2.down; // Dirección por defecto
        
        [Header("Animation")]
        [SerializeField] private Animator playerAnimator;
        
        private Rigidbody2D rb;
        private AudioSource audioSource;
        private AudioSource walkAudioSource;
        private bool wasMoving = false;
        private GameObject currentCircle;
        private float circleCreationTime;
        private bool isMoving = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            audioSource = GetComponent<AudioSource>();
            
            walkAudioSource = gameObject.AddComponent<AudioSource>();
            walkAudioSource.playOnAwake = false;
            walkAudioSource.loop = false;
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        private void OnEnable()
        {
            placeBombAction.Enable(); // Habilitar la acción
        }

        private void OnDisable()
        {
            placeBombAction.Disable(); // Deshabilitar la acción
        }

        private void Update()
        {
            // Detectar tanto tecla E como botón X del gamepad
            if (placeBombAction.WasPressedThisFrame())
            {
                CreateCircle();
            }
            
            CheckCircleExpiration();
            HandleMovementSound();
            UpdateAnimation(); // Llamar a la actualización de animaciones
        }

        private void OnMove(InputValue value)
        {
            Vector2 input = value.Get<Vector2>();
            
            movementDirection = Vector2.zero;
            
            if (Mathf.Abs(input.y) > 0.1f)
            {
                movementDirection.y = Mathf.Sign(input.y);
                movementDirection.x = 0;
            }
            else if (Mathf.Abs(input.x) > 0.1f)
            {
                movementDirection.x = Mathf.Sign(input.x);
                movementDirection.y = 0;
            }
            
            movementDirection = movementDirection.normalized;

            // Actualizar última dirección solo si hay movimiento
            if (movementDirection.magnitude > 0.1f)
            {
                lastMovementDirection = movementDirection;
            }
        }

        private void UpdateAnimation()
        {
            if (!canMove)
            {
                // Si no puede moverse, poner animación idle según la última dirección
                SetIdleAnimation();
                return;
            }

            // Determinar si está moviéndose
            bool isMoving = movementDirection.magnitude > 0.1f;

            if (isMoving)
            {
                SetWalkingAnimation();
            }
            else
            {
                SetIdleAnimation();
            }
        }

        private void SetWalkingAnimation()
        {
            // Determinar dirección principal (priorizar horizontal sobre vertical si hay diagonal)
            if (Mathf.Abs(movementDirection.x) > Mathf.Abs(movementDirection.y))
            {
                // Movimiento horizontal
                if (movementDirection.x > 0)
                {
                    playerAnimator.SetBool("IsWalkingRight", true);
                    playerAnimator.SetBool("IsWalkingLeft", false);
                    playerAnimator.SetBool("IsWalkingUp", false);
                    playerAnimator.SetBool("IsWalkingDown", false);
                }
                else
                {
                    playerAnimator.SetBool("IsWalkingRight", false);
                    playerAnimator.SetBool("IsWalkingLeft", true);
                    playerAnimator.SetBool("IsWalkingUp", false);
                    playerAnimator.SetBool("IsWalkingDown", false);
                }
            }
            else
            {
                // Movimiento vertical
                if (movementDirection.y > 0)
                {
                    playerAnimator.SetBool("IsWalkingRight", false);
                    playerAnimator.SetBool("IsWalkingLeft", false);
                    playerAnimator.SetBool("IsWalkingUp", true);
                    playerAnimator.SetBool("IsWalkingDown", false);
                }
                else
                {
                    playerAnimator.SetBool("IsWalkingRight", false);
                    playerAnimator.SetBool("IsWalkingLeft", false);
                    playerAnimator.SetBool("IsWalkingUp", false);
                    playerAnimator.SetBool("IsWalkingDown", true);
                }
            }
        }

        private void SetIdleAnimation()
        {
            // Usar la última dirección para la animación idle
            if (Mathf.Abs(lastMovementDirection.x) > Mathf.Abs(lastMovementDirection.y))
            {
                // Dirección horizontal predominante
                if (lastMovementDirection.x > 0)
                {
                    playerAnimator.SetBool("IsWalkingRight", false);
                    playerAnimator.SetBool("IsWalkingLeft", false);
                    playerAnimator.SetBool("IsWalkingUp", false);
                    playerAnimator.SetBool("IsWalkingDown", false);
                    // Opcional: tener animaciones idle separadas
                    //playerAnimator.SetBool("IsIdleRight", true);
                    //playerAnimator.SetBool("IsIdleLeft", false);
                    //playerAnimator.SetBool("IsIdleUp", false);
                    //playerAnimator.SetBool("IsIdleDown", false);
                }
                else
                {
                    playerAnimator.SetBool("IsWalkingRight", false);
                    playerAnimator.SetBool("IsWalkingLeft", false);
                    playerAnimator.SetBool("IsWalkingUp", false);
                    playerAnimator.SetBool("IsWalkingDown", false);
                    //playerAnimator.SetBool("IsIdleRight", false);
                    //playerAnimator.SetBool("IsIdleLeft", true);
                    //playerAnimator.SetBool("IsIdleUp", false);
                    //playerAnimator.SetBool("IsIdleDown", false);
                }
            }
            else
            {
                // Dirección vertical predominante
                if (lastMovementDirection.y > 0)
                {
                    playerAnimator.SetBool("IsWalkingRight", false);
                    playerAnimator.SetBool("IsWalkingLeft", false);
                    playerAnimator.SetBool("IsWalkingUp", false);
                    playerAnimator.SetBool("IsWalkingDown", false);
                    playerAnimator.SetBool("IsIdleRight", false);
                    playerAnimator.SetBool("IsIdleLeft", false);
                    playerAnimator.SetBool("IsIdleUp", true);
                    playerAnimator.SetBool("IsIdleDown", false);
                }
                else
                {
                    playerAnimator.SetBool("IsWalkingRight", false);
                    playerAnimator.SetBool("IsWalkingLeft", false);
                    playerAnimator.SetBool("IsWalkingUp", false);
                    playerAnimator.SetBool("IsWalkingDown", false);
                    playerAnimator.SetBool("IsIdleRight", false);
                    playerAnimator.SetBool("IsIdleLeft", false);
                    playerAnimator.SetBool("IsIdleUp", false);
                    playerAnimator.SetBool("IsIdleDown", true);
                }
            }
        }

        private void FixedUpdate()
        {
            if (!canMove)
            {
                rb.linearVelocity = Vector2.zero;
                isMoving = false;
                return;
            }

            rb.linearVelocity = movementDirection * moveSpeed;
            isMoving = movementDirection.magnitude > 0.1f;
        }


        private void HandleMovementSound()
        {
            if (isMoving && !wasMoving)
            {
                StartWalkingSound();
            }
            else if (!isMoving && wasMoving)
            {
                StopWalkingSound();
            }
            
            if (isMoving)
            {
                soundTimer -= Time.deltaTime;
                if (soundTimer <= 0f)
                {
                    PlayWalkSound();
                    soundTimer = soundInterval;
                }
            }
            
            wasMoving = isMoving;
        }

        private void StartWalkingSound()
        {
            PlayWalkSound();
            soundTimer = soundInterval;
        }

        private void StopWalkingSound()
        {
            soundTimer = 0f;
        }

        private void PlayWalkSound()
        {
            if (moveSound != null && walkAudioSource != null)
            {
                walkAudioSource.PlayOneShot(moveSound);
            }
        }

        private void CreateCircle()
        {
            if (currentCircle != null)
            {
                Debug.Log("Ya hay un círculo activo. Espera a que desaparezca.");
                return;
            }
            
            if (circlePrefab != null)
            {
                currentCircle = Instantiate(circlePrefab, transform.position, Quaternion.identity);
                circleCreationTime = Time.time;
                PlayPlaceBombSound();
            }
            else
            {
                Debug.LogWarning("No hay circlePrefab asignado en el Inspector");
            }
        }

        private void CheckCircleExpiration()
        {
            if (currentCircle != null && Time.time - circleCreationTime >= circleDuration)
            {
                PlayExplosionSound();
                CreateExplosionEffect(currentCircle.transform.position);
                Destroy(currentCircle);
                currentCircle = null;
            }
        }

        private void CreateExplosionEffect(Vector3 position)
        {
            CreateExplosionLine(position, Vector3.right, explosionLength);
            CreateExplosionLine(position, Vector3.left, explosionLength);
            CreateExplosionLine(position, Vector3.up, explosionLength);
            CreateExplosionLine(position, Vector3.down, explosionLength);
        }

        private void CreateExplosionLine(Vector3 position, Vector3 direction, float length)
        {
            if (explosionLinePrefab != null)
            {
                GameObject line = Instantiate(explosionLinePrefab, position, Quaternion.identity);
                
                float angle = 0f;
                if (direction == Vector3.up) angle = 90f;
                else if (direction == Vector3.down) angle = 270f;
                else if (direction == Vector3.left) angle = 180f;
                
                line.transform.rotation = Quaternion.Euler(0, 0, angle);
                line.transform.localScale = new Vector3(length, explosionWidth, 1f);
                
                Destroy(line, explosionDuration);
            }
            else
            {
                CreateLineProgrammatically(position, direction, length);
            }
        }

        private void CreateLineProgrammatically(Vector3 position, Vector3 direction, float length)
        {
            GameObject line = new GameObject("ExplosionLine");
            line.transform.position = position;
            
            SpriteRenderer spriteRenderer = line.AddComponent<SpriteRenderer>();
            spriteRenderer.color = Color.yellow;
            
            int baseSize = 100;
            int width = (int)(baseSize * explosionWidth);
            int height = (int)(baseSize * length);
            
            spriteRenderer.sprite = CreateLineSprite(width, height);
            
            float angle = 0f;
            if (direction == Vector3.up) angle = 90f;
            else if (direction == Vector3.down) angle = 270f;
            else if (direction == Vector3.left) angle = 180f;
            
            line.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            Destroy(line, explosionDuration);
        }

        private Sprite CreateLineSprite(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x < 2 || x > width - 3 || y < 2 || y > height - 3)
                    {
                        texture.SetPixel(x, y, new Color(1, 1, 0, 0.5f));
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.yellow);
                    }
                }
            }
            
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        }

        private void PlayPlaceBombSound()
        {
            if (placeBombSound != null)
            {
                audioSource.PlayOneShot(placeBombSound);
            }
        }

        private void PlayExplosionSound()
        {
            if (explosionSound != null)
            {
                audioSource.PlayOneShot(explosionSound);
            }
        }
    }
}