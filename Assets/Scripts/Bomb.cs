using UnityEngine;

public class Bomb : MonoBehaviour
{
  [Header("Timing")]
  public float fuse = 2f;

  [Header("Explosion")]
  [Min(1)] public int range = 3;
  public GameObject explosionVfx; // opcional, un sprite/anim por celda

  [Header("Layers (asigna en el Inspector)")]
  public LayerMask wallMask;   // paredes indestructibles (PF_Wall)
  public LayerMask crateMask;  // cajas destructibles (PF_Crate)

  bool exploded;

  void Start()
  {
    // Asegúrate de poner la bomba centrada en celda
    transform.position = RoundToCell(transform.position);
    Invoke(nameof(Explode), fuse);
  }

  Vector3 RoundToCell(Vector3 p) =>
      new Vector3(Mathf.Round(p.x), Mathf.Round(p.y), 0f);

  void Explode()
  {
    if (exploded) return;
    exploded = true;

    // Centro
    //AffectCell(transform.position);

    // 4 direcciones
    Vector2Int[] dirs = new[]
    {
            new Vector2Int( 1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int( 0, 1),
            new Vector2Int( 0,-1),
        };

    foreach (var d in dirs)
    {
      for (int i = 1; i <= range; i++)
      {
        Vector3 cell = transform.position + new Vector3(d.x, d.y, 0) * i;

        // 1) ¿hay pared indestructible? -> para la propagación
        var wallHit = Physics2D.OverlapPoint(cell, wallMask);
        if (wallHit) break;

        // 2) ¿hay una caja destructible? -> destruye y para
        var crateHit = Physics2D.OverlapPoint(cell, crateMask);
        if (crateHit)
        {
          crateHit.GetComponent<Destructible>()?.DestroySelf();
          SpawnExplosionVfx(cell);
          break;
        }

        // 3) celda vacía alcanzada por la explosión
        SpawnExplosionVfx(cell);
      }
    }

    // VFX del centro
    SpawnExplosionVfx(transform.position);

    // Destruir la bomba (puedes darle un delay si tienes anim)
    Destroy(gameObject);
  }

  void SpawnExplosionVfx(Vector3 pos)
  {
    if (explosionVfx)
      Instantiate(explosionVfx, pos, Quaternion.identity);
  }

  // Llamado externo si otra explosión toca esta bomba (cadena)
  public void TriggerNow()
  {
    if (IsInvoking(nameof(Explode))) CancelInvoke(nameof(Explode));
    Explode();
  }

  // Si quieres que otras explosiones “activen” esta bomba por contacto:
  void OnTriggerEnter2D(Collider2D other)
  {
    // Si tus VFX/área de explosión llevan un tag/layer, puedes detonarla:
    // if (other.CompareTag("Explosion")) TriggerNow();
  }
}
