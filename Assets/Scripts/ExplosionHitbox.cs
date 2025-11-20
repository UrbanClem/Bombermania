using UnityEngine;

public class ExplosionHitbox : MonoBehaviour
{
    public float lifetime = 0.2f; // dura poco, solo el “fogonazo”

    private void Start()
    {
        // Asegurarnos de que no hay componentes visuales
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) Destroy(sr);
        
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) Destroy(meshRenderer);

        if (lifetime > 0) Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Daño al jugador
        if (other.CompareTag("Player"))
        {
            Debug.Log("[ExplosionHitbox] Hit Player");
            var hp = other.GetComponent<PlayerHealth>();
            if (hp != null) hp.TakeDamage(1);
        }

        // Daño a enemigos - LLAMAR AL MÉTODO Die() EN LUGAR DE DESTRUIR DIRECTAMENTE
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("[ExplosionHitbox] Hit Enemy");
            EnemyWalker enemy = other.GetComponent<EnemyWalker>();
            if (enemy != null)
            {
                enemy.Die(); // Llama al método que activa la animación
            }
            else
            {
                // Fallback por si acaso
                Destroy(other.gameObject);
            }
        }
    }
}