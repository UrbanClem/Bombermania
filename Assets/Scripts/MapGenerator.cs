using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
  [Header("Size (cells)")]
  [Min(3)] public int width = 10;  // impar
  [Min(3)] public int height = 10; // impar

  [Header("Prefabs (1x1 units)")]
  public GameObject floorPrefab;
  public GameObject wallPrefab;
  public GameObject destructibleBoxPrefab;

  [Header("Placement")]
  [Range(0f, 1f)] public float createDensity = 0.55f;
  public bool checkerWalls = true; //pilares en x%2==0 y y%2==0


  [Header("Safe spawn corners (radius = 1)")]
  public bool safeTopLeft = true;
  public bool safeTopRight = true;
  public bool safeBottomLeft = true;
  public bool safeBottomRight = true;

  [Header("Camera")]
  public bool centerMainCamera = true;

  [SerializeField] Transform mapRoot;

  HashSet<Vector2Int> safeCells;

  void Reset()
  {
    if (!mapRoot)
    {
      var root = new GameObject("MapRoot");
      root.transform.SetParent(transform, false);
      mapRoot = root.transform;
    }
  }

  [ContextMenu("Generate")]
  public void Generate()
  {
    if (!floorPrefab || !wallPrefab || !destructibleBoxPrefab)
    {
      Debug.LogError("Assign prefabs before generating the map");
      return;
    }

    Clear();
    safeCells = BuildSafeCells();

    for (int y = 0; y < height; y++)
    {
      for (int x = 0; x < width; x++)
      {
        Instantiate(floorPrefab, CellToWorld(x, y), Quaternion.identity, mapRoot);

        bool IsBorder = x == 0 || y == 0 || x == width - 1 || y == height - 1;

        if (IsBorder)
        {
          Instantiate(wallPrefab, CellToWorld(x, y), Quaternion.identity, mapRoot);
          continue;
        }
        if (checkerWalls && x % 2 == 0 && y % 2 == 0)
        {
          Instantiate(wallPrefab, CellToWorld(x, y), Quaternion.identity, mapRoot);
          continue;
        }

        var cell = new Vector2Int(x, y);
        if (!safeCells.Contains(cell) && Random.value < createDensity)
        {
          Instantiate(destructibleBoxPrefab, CellToWorld(x, y), Quaternion.identity, mapRoot);
        }
      }
    }

    if (centerMainCamera) CenterCamera();
  }

  Vector3 CellToWorld(int x, int y) => new Vector3(x, y, 0f);

  //Vector3 CellToWorld(int x, int y)
  //{
  //  float offsetX = -(width - 1) / 2f;
  //  float offsetY = -(height - 1) / 2f;
  //  return new Vector3(x + offsetX, y + offsetY, 0f);
  //}

  HashSet<Vector2Int> BuildSafeCells()
  {
    var set = new HashSet<Vector2Int>();

    void AddSafeCorner(int cx, int cy)
    {
      var around = new Vector2Int[]
      {
        new(cx, cy),
        new(cx + 1, cy),
        new(cx, cy + 1),
      };

      foreach (var c in around) set.Add(c);
    }
    if (safeBottomRight) AddSafeCorner(1, 1);
    if (safeBottomLeft) AddSafeCorner(width - 2, 1);
    if (safeTopLeft) AddSafeCorner(1, height - 2);
    if (safeTopRight) AddSafeCorner(width - 2, height - 2);

    return set;
  }

  void Clear()
  {
    if (!mapRoot) return;

    for (int i = mapRoot.childCount - 1; i >= 0; i--)
    {
      if (Application.isPlaying)
        Destroy(mapRoot.GetChild(i).gameObject);
      else
        DestroyImmediate(mapRoot.GetChild(i).gameObject);
    }
  }

  void CenterCamera()
  {
    var cam = Camera.main;
    if (!cam) return;
    cam.transform.position = new Vector3((width - 1) / 2f, (height - 1) / 2f, -10f);
  }

  void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.gray;
    for (int y = 0; y < height; y++)
      for (int x = 0; x < width; x++)
        Gizmos.DrawWireCube(CellToWorld(x, y), Vector3.one);
  }
}
