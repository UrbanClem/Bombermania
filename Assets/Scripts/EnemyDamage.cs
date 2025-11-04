using UnityEngine;
using System.Collections;

public class EnemyDamage : MonoBehaviour
{
    [Header("Daño al tocar")]
    public int touchDamage = 1;
    public float damageCooldown = 0.6f;   // tiempo entre golpes
    public bool useTrigger = true;        // si tu collider es Trigger = true

    [Header("Knockback (opcional)")]
    public float knockbackForce = 0f;     // 0 = sin knockback

    private bool _canDamage = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTrigger) return;
        TryDamage(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!useTrigger) return;
        TryDamage(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (useTrigger) return;
        TryDamage(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (useTrigger) return;
        TryDamage(collision.gameObject);
    }

    private void TryDamage(GameObject other)
    {
        if (!_canDamage) return;
        if (!other.CompareTag("Player")) return;

        var hp = other.GetComponent<PlayerHealth>();
        if (hp == null) return;

        hp.TakeDamage(touchDamage);
        _canDamage = false;
        if (knockbackForce > 0f)
        {
            var rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (other.transform.position - transform.position).normalized;
                rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }
        }
        StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(damageCooldown);
        _canDamage = true;
    }
}
