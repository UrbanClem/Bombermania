using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBombPlacer : MonoBehaviour
{
    [Header("Input (Input System)")]
    public InputActionReference placeAction;   // Drag: Gameplay/Place (Button)

    [Header("Bomb")]
    public GameObject bombPrefab;
    public float placeCooldown = 0.2f;

    private float nextPlaceTime = 0f;
    private bool useEvent = false;

    private void OnEnable()
    {
        if (placeAction != null)
        {
            placeAction.action.Enable();
            placeAction.action.performed += OnPlacePerformed;
            useEvent = true;
        }
        else
        {
            useEvent = false; // no hay acción asignada -> usamos fallback en Update
        }
    }

    private void OnDisable()
    {
        if (placeAction != null)
        {
            placeAction.action.performed -= OnPlacePerformed;
            placeAction.action.Disable();
        }
    }

    private void Update()
    {
        if (useEvent) return; // ya escuchamos por evento

        // Fallback: si no tienes action enlazada, chequea inputs directo
        bool pressed =
            (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

        if (pressed) TryPlace();
    }

    private void OnPlacePerformed(InputAction.CallbackContext ctx)
    {
        TryPlace();
    }

    private void TryPlace()
    {
        if (Time.time < nextPlaceTime) return;
        nextPlaceTime = Time.time + placeCooldown;

        if (bombPrefab == null) return;

        Instantiate(bombPrefab, new Vector3(transform.position.x, transform.position.y, 0f), Quaternion.identity);
    }
}
