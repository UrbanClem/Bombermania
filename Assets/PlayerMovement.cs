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
        [SerializeField] private AudioClip moveSound;
        [SerializeField] private float soundCooldown = 0.3f;
        
        [Header("Circle Effect")]
        [SerializeField] private GameObject circlePrefab;
        [SerializeField] private float circleDuration = 3f;
        
        [Header("Explosion Effect")]
        [SerializeField] private GameObject explosionLinePrefab;
        [SerializeField] private float explosionDuration = 1f;
        [SerializeField] private float explosionLength = 5f; // Aumentado de 2f a 5f
        [SerializeField] private float explosionWidth = 0.3f; // Ancho de las líneas
        
        private Rigidbody2D rb;
        private AudioSource audioSource;
        private float lastSoundTime;
        private bool wasMoving = false;
        private GameObject currentCircle;
        private float circleCreationTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        private void Update()
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                CreateCircle();
            }
            
            CheckCircleExpiration();
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
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = movementDirection * moveSpeed;
            HandleMovementSound();
        }

        private void HandleMovementSound()
        {
            bool isMoving = movementDirection.magnitude > 0.1f;
            
            if (isMoving && !wasMoving)
            {
                PlayMoveSound();
            }
            
            wasMoving = isMoving;
        }

        private void PlayMoveSound()
        {
            if (Time.time - lastSoundTime >= soundCooldown && moveSound != null)
            {
                audioSource.PlayOneShot(moveSound);
                lastSoundTime = Time.time;
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
                CreateExplosionEffect(currentCircle.transform.position);
                Destroy(currentCircle);
                currentCircle = null;
            }
        }

        private void CreateExplosionEffect(Vector3 position)
        {
            // Crear las 4 direcciones de la cruz (más grande)
            CreateExplosionLine(position, Vector3.right, explosionLength);    // Derecha
            CreateExplosionLine(position, Vector3.left, explosionLength);     // Izquierda
            CreateExplosionLine(position, Vector3.up, explosionLength);       // Arriba
            CreateExplosionLine(position, Vector3.down, explosionLength);     // Abajo
        }

        private void CreateExplosionLine(Vector3 position, Vector3 direction, float length)
        {
            if (explosionLinePrefab != null)
            {
                GameObject line = Instantiate(explosionLinePrefab, position, Quaternion.identity);
                
                // Orientar la línea según la dirección
                float angle = 0f;
                if (direction == Vector3.up) angle = 90f;
                else if (direction == Vector3.down) angle = 270f;
                else if (direction == Vector3.left) angle = 180f;
                
                line.transform.rotation = Quaternion.Euler(0, 0, angle);
                
                // Escalar la línea (más ancha y larga)
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
            
            // Crear sprite más grande
            int baseSize = 100; // Tamaño base más grande
            int width = (int)(baseSize * explosionWidth);
            int height = (int)(baseSize * length);
            
            spriteRenderer.sprite = CreateLineSprite(width, height);
            
            // Orientar la línea
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
            
            // Rellenar la textura con color sólido
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Opcional: hacer bordes transparentes para mejor apariencia
                    if (x < 2 || x > width - 3 || y < 2 || y > height - 3)
                    {
                        texture.SetPixel(x, y, new Color(1, 1, 0, 0.5f)); // Borde semi-transparente
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.yellow); // Centro sólido
                    }
                }
            }
            
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        }
    }
}