using UnityEngine;
using UnityEngine.InputSystem;

public class BomberPlayer : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("Referencia a la acción 'Move' del Input System (Vector2).")]
    public InputActionReference moveAction;

    [Tooltip("Referencia a la acción 'Place' del Input System (Button).")]
    public InputActionReference placeAction;

    [Header("Círculo (bomba dummy)")]
    [Tooltip("Prefab del círculo que se instanciará al presionar 'Place'.")]
    public GameObject circlePrefab;

    [Tooltip("Si es > 0, el círculo se destruirá automáticamente tras este tiempo (segundos). Si es 0 o negativo, no se destruye.")]
    public float circleAutoDestroySeconds = 3f;

    // Estado interno
    private Vector2 rawMove;   // valor crudo del stick/teclas
    private Vector2 snapped;   // movimiento limitado a 4 direcciones

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (placeAction != null)
        {
            placeAction.action.Enable();
            placeAction.action.performed += OnPlacePerformed;
        }
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (placeAction != null)
        {
            placeAction.action.performed -= OnPlacePerformed;
            placeAction.action.Disable();
        }
    }

    private void Update()
    {
        // 1) Leer input (Vector2)
        rawMove = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

        // 2) Limitar a 4 direcciones (ejes cardinales)
        if (Mathf.Abs(rawMove.x) > Mathf.Abs(rawMove.y))
        {
            snapped = new Vector2(Mathf.Sign(rawMove.x), 0f);
        }
        else if (Mathf.Abs(rawMove.y) > 0f)
        {
            snapped = new Vector2(0f, Mathf.Sign(rawMove.y));
        }
        else
        {
            snapped = Vector2.zero;
        }

        // 3) Mover
        if (snapped != Vector2.zero)
        {
            transform.Translate(snapped * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    private void OnPlacePerformed(InputAction.CallbackContext ctx)
    {
        if (circlePrefab == null) return;

        Vector3 spawnPos = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), 0f);
        GameObject circle = Instantiate(circlePrefab, spawnPos, Quaternion.identity);

        if (circleAutoDestroySeconds > 0f)
        {
            Destroy(circle, circleAutoDestroySeconds);
        }
    }
}