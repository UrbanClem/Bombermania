using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement; // para ejemplo de "siguiente nivel"

public class LevelManager : MonoBehaviour
{
    [Header("Tilemaps")]
    public Grid grid;
    public Tilemap breakableMap;

    [Header("Salida")]
    public GameObject exitPrefab;   // Prefab con collider trigger + ExitPortal.cs
    public bool pickRandomOnStart = true;

    private HashSet<Vector3Int> breakableCells = new HashSet<Vector3Int>();
    private Vector3Int exitCell;
    private bool exitCellChosen = false;
    private bool exitSpawned = false;
    private int remainingBreakables = 0;

    public bool CanExit => remainingBreakables <= 0;

    private void Awake()
    {
        if (grid == null) grid = FindFirstObjectByType<Grid>();
        if (breakableMap == null)
        {
            // intenta encontrar por nombre
            var go = GameObject.Find("Walls_Breakable");
            if (go) breakableMap = go.GetComponent<Tilemap>();
        }
    }

    private void Start()
    {
        RecountBreakablesAndPickExit();
    }

    public void RecountBreakablesAndPickExit()
    {
        breakableCells.Clear();

        // Recorre el área de celdas del tilemap
        BoundsInt b = breakableMap.cellBounds;
        for (int x = b.xMin; x < b.xMax; x++)
        {
            for (int y = b.yMin; y < b.yMax; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                if (breakableMap.HasTile(cell))
                    breakableCells.Add(cell);
            }
        }

        remainingBreakables = breakableCells.Count;

        if (pickRandomOnStart && breakableCells.Count > 0)
        {
            int idx = Random.Range(0, breakableCells.Count);
            int i = 0;
            foreach (var c in breakableCells)
            {
                if (i == idx)
                {
                    exitCell = c;
                    exitCellChosen = true;
                    break;
                }
                i++;
            }
        }
    }

    // Llamado por la bomba cuando destruye un tile rompible
    public void OnBreakableDestroyed(Vector3Int cell)
    {
        if (breakableCells.Remove(cell))
            remainingBreakables = Mathf.Max(0, remainingBreakables - 1);

        // Si esta celda era la elegida, spawnear salida
        if (exitCellChosen && !exitSpawned && cell == exitCell)
        {
            SpawnExitAt(cell);
        }

        // Si ya no queda ninguno, puedes desbloquear UI o efectos
        if (remainingBreakables <= 0)
        {
            // Aquí podrías encender una luz en la salida, cambiar color, etc.
            Debug.Log("[LevelManager] ¡Todos los bloques destruidos! La salida está desbloqueada.");
        }
    }

    private void SpawnExitAt(Vector3Int cell)
    {
        if (exitPrefab == null) return;
        Vector3 pos = grid.GetCellCenterWorld(cell);
        Instantiate(exitPrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
        exitSpawned = true;
        Debug.Log("[LevelManager] Salida revelada.");
    }

    // Llamado por ExitPortal.cs cuando el jugador intenta salir
    public void TryFinishLevel()
    {
        if (!CanExit)
        {
            Debug.Log("[LevelManager] Aún quedan bloques por romper. La salida está bloqueada.");
            return;
        }

        Debug.Log("[LevelManager] Nivel completado. (Ejemplo: cargar siguiente escena)");
        // Ejemplo simple: recargar escena o cargar siguiente por índice
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public bool IsExitCell(Vector3Int cell)
    {
        return exitCellChosen && cell == exitCell;
    }

    public bool TrySpawnExitAt(Vector3Int cell)
    {
        if (exitCellChosen && !exitSpawned && cell == exitCell)
        {
            SpawnExitAt(cell);
            return true; // se usó el bloque para salida
        }
        return false; // no era la celda de salida
    }

}
