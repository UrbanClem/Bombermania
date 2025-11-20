using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDownShooter
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Audio")]
        [SerializeField] private AudioClip moveSound;
        [SerializeField] private float soundInterval = 0.5f;
        
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
        [SerializeField] private InputAction placeBombAction;

        // Variables privadas
        private Vector2 movementDirection;
        private Vector2 lastMovementDirection = Vector2.down;
        private Rigidbody2D rb;
        private AudioSource audioSource;
        private AudioSource walkAudioSource;
        private float soundTimer;
        private bool wasMoving = false;
        private GameObject currentCircle;
        private float circleCreationTime;
        private bool canMove = true;
        private bool isMoving = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            audioSource = GetComponent<AudioSource>();
            
            // Crear AudioSource para sonidos de caminar
            walkAudioSource = gameObject.AddComponent<AudioSource>();
            walkAudioSource.playOnAwake = false;
            walkAudioSource.loop = false;
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }

            // Obtener SpriteRenderer si no está asignado
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            placeBombAction.Enable();
        }

        private void OnDisable()
        {
            placeBombAction.Disable();
        }

        private void Update()
        {
            if (placeBombAction.WasPressedThisFrame())
            {
                CreateCircle();
            }
            
            CheckCircleExpiration();
            HandleMovementSound();
            UpdateAnimation();
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

            // Actualizar última dirección SI HAY movimiento
            if (movementDirection.magnitude > 0.1f)
            {
                lastMovementDirection = movementDirection;
                
                // Actualizar parámetros LastMove en el Animator
                playerAnimator.SetFloat("LastMoveX", lastMovementDirection.x);
                playerAnimator.SetFloat("LastMoveY", lastMovementDirection.y);
            }
        }

        private void UpdateAnimation()
        {
            if (!canMove)
            {
                playerAnimator.SetBool("IsMoving", false);
                return;
            }

            isMoving = movementDirection.magnitude > 0.1f;
            
            // Actualizar IsMoving inmediatamente
            playerAnimator.SetBool("IsMoving", isMoving);

            if (isMoving)
            {
                playerAnimator.SetFloat("MoveX", movementDirection.x);
                playerAnimator.SetFloat("MoveY", movementDirection.y);

                // Flip para movimiento
                if (movementDirection.x < 0) 
                {
                    spriteRenderer.flipX = true;
                    // Actualizar LastMoveX para que el idle sepa que fue izquierda
                    playerAnimator.SetFloat("LastMoveX", -1f);
                }
                else if (movementDirection.x > 0) 
                {
                    spriteRenderer.flipX = false;
                    // Actualizar LastMoveX para que el idle sepa que fue derecha
                    playerAnimator.SetFloat("LastMoveX", 1f);
                }
            }
            else
            {
                // Cuando no se está moviendo, asegurar que los parámetros de movimiento sean 0
                playerAnimator.SetFloat("MoveX", 0f);
                playerAnimator.SetFloat("MoveY", 0f);
                
                // Mantener el flip basado en la última dirección
                if (lastMovementDirection.x < -0.1f)
                {
                    spriteRenderer.flipX = true;
                    playerAnimator.SetFloat("LastMoveX", -1f);
                }
                else if (lastMovementDirection.x > 0.1f)
                {
                    spriteRenderer.flipX = false;
                    playerAnimator.SetFloat("LastMoveX", 1f);
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

        // --- SISTEMA DE BOMBAS ORIGINAL (NO MODIFICADO) ---
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

        // Métodos de control externo
        public void EnableControl(bool enable)
        {
            canMove = enable;
            if (!enable)
            {
                rb.linearVelocity = Vector2.zero;
                playerAnimator.SetBool("IsMoving", false);
            }
        }

        public float CurrentSpeed => moveSpeed;

        public void AddSpeed(float amount, float maxSpeed)
        {
            moveSpeed = Mathf.Min(moveSpeed + amount, maxSpeed);
        }
    }
}