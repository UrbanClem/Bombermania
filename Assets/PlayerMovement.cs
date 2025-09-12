using UnityEngine;
using UnityEngine.InputSystem;

namespace TopDownShooter
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        private Vector2 movementDirection;

        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
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
        }
    }
}