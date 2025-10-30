using UnityEngine;

public class PowerUpRange : MonoBehaviour
{
    public int amount = 1;   // cuánto aumenta el rango

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var placer = other.GetComponent<PlayerBombPlacer>();
        if (placer != null)
        {
            placer.AddBombRange(amount);
        }

        Destroy(gameObject);
    }
}
