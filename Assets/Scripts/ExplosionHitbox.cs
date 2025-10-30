using UnityEngine;

public class ExplosionHitbox : MonoBehaviour
{
    public float lifetime = 0.2f; // dura poco, solo el “fogonazo”

    private void Start()
    {
        if (lifetime > 0) Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Daño al jugador (y luego a enemigos)
        if (other.CompareTag("Player"))
        {
            Debug.Log("[ExplosionHitbox] Hit Player");
            var hp = other.GetComponent<PlayerHealth>();
            if (hp != null) hp.TakeDamage(1);
        }


        // Aquí luego: if (other.CompareTag("Enemy")) { other.GetComponent<EnemyHealth>()?.TakeDamage(1); }
    }
}
