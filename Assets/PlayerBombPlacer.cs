using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBombPlacer : MonoBehaviour
{
    [Header("Input (Input System)")]
    public InputActionReference placeAction;   // Drag: Gameplay/Place (Button)

    [Header("Bomb")]
    public GameObject bombPrefab;
    public float placeCooldown = 0.2f;

    [Header("Stats")]
    public int bombRange = 1;                  // Aumenta con power-ups
    public int maxBombRange = 7;

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
            useEvent = false;
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
        if (useEvent) return;

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

        var go = Instantiate(bombPrefab, new Vector3(transform.position.x, transform.position.y, 0f), Quaternion.identity);

        // Inyectar rango a la Bomb
        var bomb = go.GetComponent<Bomb>();
        if (bomb != null)
        {
            bomb.range = Mathf.Clamp(bombRange, 1, maxBombRange);
        }
    }

    // Llamado por power-ups
    public void AddBombRange(int amount)
    {
        bombRange = Mathf.Clamp(bombRange + amount, 1, maxBombRange);
        // Aquí puedes reproducir un sonido o UI feedback
    }
}
