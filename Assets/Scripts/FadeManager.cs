using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;
    
    public Image blackImage; // Arrastra una Image negra desde el Inspector
    public float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowDeathScreen()
    {
        if (blackImage != null)
        {
            StartCoroutine(DeathScreenRoutine());
        }
        else
        {
            // Si no hay imagen, simplemente reiniciar
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private IEnumerator DeathScreenRoutine()
    {
        // Mostrar la imagen negra
        blackImage.color = Color.black;
        blackImage.gameObject.SetActive(true);
        
        // Esperar 2 segundos
        yield return new WaitForSeconds(2f);
        
        // Reiniciar escena
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
        // Ocultar la imagen después de reiniciar
        yield return new WaitForSeconds(0.1f);
        blackImage.gameObject.SetActive(false);
    }
}