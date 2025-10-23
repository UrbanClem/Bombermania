using UnityEngine;

public class SnapToGridOnStart : MonoBehaviour
{
    [Tooltip("Arrastra aquí el Level_01 → Grid")]
    public Grid grid;

    void Start()
    {
        if (grid == null)
        {
            Debug.LogWarning("[SnapToGridOnStart] Falta asignar el Grid en el Inspector.");
            return;
        }

        // Celda donde está el Player al iniciar
        Vector3Int cell = grid.WorldToCell(transform.position);
        // Centro exacto de esa celda
        Vector3 center = grid.GetCellCenterWorld(cell);
        transform.position = new Vector3(center.x, center.y, transform.position.z);
    }
}

