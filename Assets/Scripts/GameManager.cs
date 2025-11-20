using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public int totalLives = 3;
    private int currentLives;
    public string gameOverScene = "GameOver";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentLives = totalLives;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayerDied()
    {
        currentLives--;
        Debug.Log($"Vidas restantes: {currentLives}");
        
        if (currentLives <= 0)
        {
            // Game Over
            currentLives = totalLives; // Reset para la próxima vez
            SceneManager.LoadScene(gameOverScene);
        }
        else
        {
            // Reiniciar el nivel actual
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }
}