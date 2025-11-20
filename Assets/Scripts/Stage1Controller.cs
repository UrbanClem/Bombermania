using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class Stage1Controller : MonoBehaviour
{
    [Header("Configuración")]
    public float waitTime = 3f; // Tiempo de espera en Stage1
    public string nextSceneName = "Level1"; // Escena a cargar después

    [Header("UI Elements (Opcional)")]
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI countdownText;

    private void Start()
    {
        Debug.Log("🎬 Iniciando Stage1 - Transición al nivel...");

        // Configurar texto del stage si existe
        if (stageText != null)
        {
            stageText.text = "STAGE 1";
        }

        // Iniciar la transición automática
        StartCoroutine(TransitionToLevel());
    }

    private IEnumerator TransitionToLevel()
    {
        // Opcional: Mostrar cuenta regresiva
        if (countdownText != null)
        {
            for (int i = (int)waitTime; i > 0; i--)
            {
                countdownText.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }
            countdownText.text = "GO!";
        }
        else
        {
            // Simplemente esperar el tiempo configurado
            yield return new WaitForSeconds(waitTime);
        }

        Debug.Log($"🚀 Cargando {nextSceneName}...");
        
        // Cargar el nivel principal
        SceneManager.LoadScene(nextSceneName);
    }

    
}