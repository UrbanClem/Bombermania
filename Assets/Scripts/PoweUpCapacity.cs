using UnityEngine;

public class PowerUpCapacity : MonoBehaviour
{
    public int amount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var placer = other.GetComponent<PlayerBombPlacer>();
        if (placer != null) placer.AddBombCapacity(amount);

        Destroy(gameObject);
    }
}
