using UnityEngine;
using TopDownShooter;  //  importante para ver PlayerMovement

public class PowerUpSpeed : MonoBehaviour
{
    public float amount = 1.0f;
    public float maxSpeed = 9.0f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var move = other.GetComponent<PlayerMovement>();
        if (move != null)
        {
            move.AddSpeed(amount, maxSpeed);
        }
        else
        {
            Debug.LogWarning("[PowerUpSpeed] PlayerMovement no encontrado en Player.");
        }

        Destroy(gameObject);
    }
}
