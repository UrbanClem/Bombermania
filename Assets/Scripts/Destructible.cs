using UnityEngine;

public class Destructible : MonoBehaviour
{
  [SerializeField] GameObject destroyVfx;

  public void DestroySelf()
  {
    if (destroyVfx)
      Instantiate(destroyVfx, transform.position, Quaternion.identity);

    Destroy(gameObject);
  }
}
