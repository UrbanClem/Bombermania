using UnityEngine;

public class ExitPortal : MonoBehaviour
{
    public AudioClip lockedSfx;
    public AudioClip openSfx;

    private LevelManager manager;
    private AudioSource audioSrc;

    private void Awake()
    {
        manager = FindFirstObjectByType<LevelManager>();
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (manager != null && manager.CanExit)
        {
            if (openSfx) audioSrc.PlayOneShot(openSfx);
            manager.TryFinishLevel();
        }
        else
        {
            if (lockedSfx) audioSrc.PlayOneShot(lockedSfx);
            Debug.Log("[ExitPortal] La salida todavía está bloqueada.");
        }
    }
}
