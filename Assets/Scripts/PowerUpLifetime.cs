using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpLifetime : MonoBehaviour
{
    public float lifetime = 10f;       // cuánto dura en el piso
    public float blinkTime = 2f;       // tiempo final parpadeando
    public float blinkInterval = 0.1f;

    private List<SpriteRenderer> srs;

    private void Awake()
    {
        srs = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
        if (srs.Count == 0)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) srs.Add(sr);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(LifeRoutine());
    }

    private IEnumerator LifeRoutine()
    {
        float wait = Mathf.Max(0f, lifetime - blinkTime);
        if (wait > 0f) yield return new WaitForSeconds(wait);

        // parpadeo
        float t = 0f;
        bool vis = true;
        while (t < blinkTime)
        {
            vis = !vis;
            SetVisible(vis);
            yield return new WaitForSeconds(blinkInterval);
            t += blinkInterval;
        }

        Destroy(gameObject);
    }

    private void SetVisible(bool v)
    {
        for (int i = 0; i < srs.Count; i++)
        {
            if (srs[i] == null) continue;
            var c = srs[i].color;
            c.a = v ? 1f : 0.25f;
            srs[i].color = c;
        }
    }
}
