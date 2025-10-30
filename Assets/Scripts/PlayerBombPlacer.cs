using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBombPlacer : MonoBehaviour
{
    [Header("Input (Input System)")]
    public InputActionReference placeAction;

    [Header("Bomb")]
    public GameObject bombPrefab;
    public float placeCooldown = 0.2f;

    [Header("Stats")]
    public int bombRange = 1;     // para el Bomb.range
    public int maxBombRange = 7;

    public int bombCapacity = 1;  // cuántas bombas a la vez
    public int maxBombCapacity = 8;

    private float nextPlaceTime = 0f;
    private bool useEvent = false;
    private int activeBombs = 0;

    private void OnEnable()
    {
        if (placeAction != null)
        {
            placeAction.action.Enable();
            placeAction.action.performed += OnPlacePerformed;
            useEvent = true;
        }
        else useEvent = false;
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

    private void OnPlacePerformed(InputAction.CallbackContext ctx) => TryPlace();

    private void TryPlace()
    {
        if (Time.time < nextPlaceTime) return;
        if (activeBombs >= bombCapacity) return;

        nextPlaceTime = Time.time + placeCooldown;
        if (bombPrefab == null) return;

        var go = Instantiate(bombPrefab, new Vector3(transform.position.x, transform.position.y, 0f), Quaternion.identity);
        var bomb = go.GetComponent<Bomb>();
        if (bomb != null)
        {
            bomb.range = Mathf.Clamp(bombRange, 1, maxBombRange);
            bomb.owner = this; // para liberar capacidad al explotar
        }

        activeBombs++;
    }

    public void OnBombFinished()
    {
        activeBombs = Mathf.Max(0, activeBombs - 1);
    }

    // POWER-UPS
    public void AddBombRange(int amount)
    {
        bombRange = Mathf.Clamp(bombRange + amount, 1, maxBombRange);
    }

    public void AddBombCapacity(int amount)
    {
        bombCapacity = Mathf.Clamp(bombCapacity + amount, 1, maxBombCapacity);
    }
}
