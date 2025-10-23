using UnityEngine;
using UnityEngine.Tilemaps;

public class DestructibleTilemap : MonoBehaviour
{
    [Tooltip("Tilemap que contiene los tiles rompibles")]
    public Tilemap breakableTilemap;

    [Tooltip("Tilemap de paredes indestructibles (para detener la explosión)")]
    public Tilemap wallTilemap;

    // Borra el tile rompible en una celda dada (si existe)
    public bool TryBreakAtCell(Vector3Int cell)
    {
        if (breakableTilemap == null) return false;

        var tile = breakableTilemap.GetTile(cell);
        if (tile != null)
        {
            breakableTilemap.SetTile(cell, null);
            return true;
        }
        return false;
    }

    // ¿Hay una pared indestructible en esta celda?
    public bool IsWallAtCell(Vector3Int cell)
    {
        if (wallTilemap == null) return false;
        return wallTilemap.HasTile(cell);
    }

    // Convierte posición del mundo a celda de grid
    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        return breakableTilemap.WorldToCell(worldPos);
    }
}
