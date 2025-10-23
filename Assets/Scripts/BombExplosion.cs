using UnityEngine;
using UnityEngine.Tilemaps;

public class BombExplosion : MonoBehaviour
{
    public int range = 3;
    public DestructibleTilemap destructible; // <-- conexión al MapController
    public Tilemap referenceTilemapForWorldToCell; // usa TM_Breakables

    public void Explode()
    {
        Vector3Int centerCell = referenceTilemapForWorldToCell.WorldToCell(transform.position);

        // Direcciones: derecha, izquierda, arriba, abajo
        Vector3Int[] dirs = {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };

        foreach (var dir in dirs)
        {
            for (int i = 1; i <= range; i++)
            {
                Vector3Int cell = centerCell + dir * i;

                // Si hay pared → se detiene
                if (destructible.IsWallAtCell(cell))
                    break;

                // Si hay rompible → lo borra y se detiene
                if (destructible.TryBreakAtCell(cell))
                    break;

                // Si no hay nada, la explosión sigue
            }
        }
    }
}
