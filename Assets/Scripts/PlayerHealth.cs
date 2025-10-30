using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHP = 3;
    public float invulnSeconds = 1.0f;

    [Header("Respawn")]
    public int lives = 3;
    public float respawnDelay = 1.2f;
    public Transform respawnPoint; // arrastra un empty en la escena (o se usa la posición inicial)

    [Header("Blink (parpadeo)")]
    public float blinkTotal = 1.0f;      // duración del parpadeo tras daño (además de invuln)
    public float blinkInterval = 0.1f;

    private int currentHP;
    private float invulnUntil = 0f;
    private Vector3 startPosition;
    private List<SpriteRenderer> renderers;
    private TopDownShooter.PlayerMovement movement; // para bloquear control

    private void Awake()
    {
        currentHP = maxHP;
        startPosition = transform.position;
        movement = GetComponent<TopDownShooter.PlayerMovement>();
        renderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
        if (respawnPoint == null)
        {
            // Si no asignas un punto, usamos la posición inicial
            GameObject rp = new GameObject("RespawnPoint_Auto");
            rp.transform.position = startPosition;
            respawnPoint = rp.transform;
        }
    }

    public void TakeDamage(int amount)
    {
        if (Time.time < invulnUntil) return;

        currentHP -= Mathf.Max(1, amount);
        invulnUntil = Time.time + invulnSeconds;

        // Parpadeo corto de golpe recibido
        StartCoroutine(BlinkRoutine(blinkTotal));

        if (currentHP <= 0)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        // “Muerto”
        lives = Mathf.Max(0, lives - 1);
        if (movement != null) movement.EnableControl(false);

        // pequeñísimo feedback: oculto parcial y bloqueo
        yield return new WaitForSeconds(respawnDelay);

        if (lives <= 0)
        {
            // TODO: Game Over real
            Debug.Log("[PlayerHealth] GAME OVER (placeholder). Reiniciando stats.");
            lives = 3;
        }

        // Reaparecer
        transform.position = respawnPoint.position;
        currentHP = maxHP;

        // invulnerable un rato para no morir instantáneo
        invulnUntil = Time.time + invulnSeconds + 0.5f;
        yield return StartCoroutine(BlinkRoutine(invulnSeconds)); // parpadeo de invuln

        if (movement != null) movement.EnableControl(true);
    }

    private IEnumerator BlinkRoutine(float totalDuration)
    {
        float t = 0f;
        bool visible = true;

        while (t < totalDuration)
        {
            visible = !visible;
            SetRenderersVisible(visible);
            yield return new WaitForSeconds(blinkInterval);
            t += blinkInterval;
        }

        SetRenderersVisible(true);
    }

    private void SetRenderersVisible(bool v)
    {
        if (renderers == null || renderers.Count == 0)
            renderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());

        for (int i = 0; i < renderers.Count; i++)
        {
            var sr = renderers[i];
            if (sr == null) continue;
            var c = sr.color;
            c.a = v ? 1f : 0.3f; // 0.3 = semitransparente
            sr.color = c;
        }
    }
}
