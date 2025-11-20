using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [Header("Configuración")]
    public TextMeshProUGUI victoryText;
    public string victoryMessage = "¡Todos los enemigos eliminados!";
    
    private List<GameObject> enemies = new List<GameObject>();
    private bool victoryShown = false;
    
    // ✅ NUEVO: Evento para cuando todos los enemigos mueren
    public System.Action OnAllEnemiesDefeated;

    private void Start()
    {
        // Encontrar todos los enemigos al inicio
        FindAllEnemies();
        
        // Ocultar el texto al inicio
        if (victoryText != null)
        {
            victoryText.gameObject.SetActive(false);
        }
        
        Debug.Log($"Enemigos encontrados: {enemies.Count}");
    }

    private void Update()
    {
        if (!victoryShown && AreAllEnemiesDead())
        {
            ShowVictoryText();
            // ✅ NUEVO: Disparar evento cuando todos los enemigos mueren
            OnAllEnemiesDefeated?.Invoke();
        }
    }

    private void FindAllEnemies()
    {
        // Buscar todos los objetos con tag "Enemy"
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        enemies.Clear();
        enemies.AddRange(enemyObjects);
    }

    private bool AreAllEnemiesDead()
    {
        // Remover enemigos nulos (los que fueron destruidos)
        enemies.RemoveAll(enemy => enemy == null);
        
        // Si no quedan enemigos, todos murieron
        return enemies.Count == 0;
    }

    private void ShowVictoryText()
    {
        victoryShown = true;
        
        if (victoryText != null)
        {
            victoryText.text = victoryMessage;
            victoryText.gameObject.SetActive(true);
            Debug.Log("¡Victoria! Mostrando texto (indefinidamente).");
        }
    }

    // ✅ ESTE MÉTODO YA NO SE LLAMA AUTOMÁTICAMENTE
    private void HideVictoryText()
    {
        if (victoryText != null)
        {
            victoryText.gameObject.SetActive(false);
        }
    }

    // Método para agregar enemigos manualmente (si los creas durante el juego)
    public void AddEnemy(GameObject enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
        }
    }

    // Método para remover enemigos manualmente
    public void RemoveEnemy(GameObject enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
        }
    }

    // Para debug: ver enemigos restantes
    public int GetRemainingEnemies()
    {
        enemies.RemoveAll(enemy => enemy == null);
        return enemies.Count;
    }

    // ✅ NUEVO: Método para ocultar el texto manualmente si lo necesitas
    public void HideVictoryTextManual()
    {
        HideVictoryText();
        victoryShown = false; // Permitir que vuelva a aparecer si es necesario
    }

    // ✅ NUEVO: Método para mostrar el texto manualmente
    public void ShowVictoryTextManual()
    {
        ShowVictoryText();
    }
    
    // ✅ NUEVO: Propiedad para verificar si todos los enemigos están muertos
    public bool AllEnemiesDead => AreAllEnemiesDead();
}