using UnityEngine;
using System.Collections.Generic;

public class PoolTeleportTester : MonoBehaviour
{
    [Header("Pools")]
    [SerializeField] private ChildPoolTeleporter bigPool;
    [SerializeField] private ChildPoolTeleporter smallPool;

    [Header("Grid")]
    [SerializeField] private float cellSize = 15f;
    [SerializeField] private int activeCellsX = 4;
    [SerializeField] private int activeCellsZ = 4;

    [Header("Target counts per cell")]
    [SerializeField] private int bigPerCell = 2;
    [SerializeField] private int smallPerCell = 2;

    [Header("Performance")]
    [SerializeField] private float refreshInterval = 0.25f;
    [SerializeField] private int maxSpawnAttemptsPerCell = 30;

    // cell -> objects currently assigned to that cell
    private readonly Dictionary<Vector2Int, HashSet<Transform>> bigInCell = new();
    private readonly Dictionary<Vector2Int, HashSet<Transform>> smallInCell = new();

    // object -> cell (so we can remove it from previous cell when teleported)
    private readonly Dictionary<Transform, Vector2Int> objectToCell = new();

    private Vector2Int lastPlayerCell;
    private float timer;

    private void Start()
    {
        lastPlayerCell = WorldToCell(transform.position);
        RefreshWindow(force: true);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < refreshInterval) return;
        timer = 0f;

        Vector2Int currentCell = WorldToCell(transform.position);
        if (currentCell != lastPlayerCell)
        {
            lastPlayerCell = currentCell;
            RefreshWindow(force: false);
        }
        else
        {
            // optional enforcement
            RefreshWindow(force: false);
        }
    }

    private void RefreshWindow(bool force)
    {
        if (bigPool == null || smallPool == null)
        {
            Debug.LogError("[GridCellPopulatorTracked] Pools not assigned.");
            return;
        }

        int w = Mathf.Max(1, activeCellsX);
        int h = Mathf.Max(1, activeCellsZ);
        int halfW = w / 2;
        int halfH = h / 2;

        // Ensure all cells in the window meet counts
        for (int dz = -halfH; dz < -halfH + h; dz++)
        {
            for (int dx = -halfW; dx < -halfW + w; dx++)
            {
                Vector2Int cell = lastPlayerCell + new Vector2Int(dx, dz);
                EnsureCounts(cell);
            }
        }
    }

    private void EnsureCounts(Vector2Int cell)
    {
        PruneDeadRefs(cell);

        int currentBig = GetCount(bigInCell, cell);
        int currentSmall = GetCount(smallInCell, cell);

        int needBig = Mathf.Max(0, bigPerCell - currentBig);
        int needSmall = Mathf.Max(0, smallPerCell - currentSmall);

        if (needBig == 0 && needSmall == 0) return;

        Bounds b = GetCellBounds(cell);

        SpawnMissing(bigPool, needBig, b, isBig: true);
        SpawnMissing(smallPool, needSmall, b, isBig: false);
    }

    private void SpawnMissing(ChildPoolTeleporter pool, int need, Bounds b, bool isBig)
    {
        if (need <= 0) return;

        int spawned = 0;
        int attempts = 0;

        while (spawned < need && attempts < maxSpawnAttemptsPerCell)
        {
            attempts++;

            Vector3 p = RandomPointInBoundsXZ(b);
            if (pool.TryTeleportRandomTo(p.x, p.z, out Transform moved))
            {
                // moved object now belongs to THIS cell
                Vector2Int newCell = WorldToCell(moved.position);
                AssignObjectToCell(moved, newCell, isBig);
                spawned++;
            }
        }
    }

    private void AssignObjectToCell(Transform obj, Vector2Int newCell, bool isBig)
    {
        // Remove from previous cell if it was tracked before
        if (objectToCell.TryGetValue(obj, out Vector2Int oldCell))
        {
            if (bigInCell.TryGetValue(oldCell, out var bSet)) bSet.Remove(obj);
            if (smallInCell.TryGetValue(oldCell, out var sSet)) sSet.Remove(obj);
        }

        objectToCell[obj] = newCell;

        var dict = isBig ? bigInCell : smallInCell;
        if (!dict.TryGetValue(newCell, out var set))
        {
            set = new HashSet<Transform>();
            dict[newCell] = set;
        }
        set.Add(obj);
    }

    private void PruneDeadRefs(Vector2Int cell)
    {
        if (bigInCell.TryGetValue(cell, out var bSet))
            bSet.RemoveWhere(t => t == null || !t.gameObject);

        if (smallInCell.TryGetValue(cell, out var sSet))
            sSet.RemoveWhere(t => t == null || !t.gameObject);
    }

    private int GetCount(Dictionary<Vector2Int, HashSet<Transform>> dict, Vector2Int cell)
    {
        if (!dict.TryGetValue(cell, out var set) || set == null) return 0;
        // Count only active objects (optional; remove this condition if you want to count inactive too)
        int c = 0;
        foreach (var t in set)
            if (t != null && t.gameObject.activeSelf) c++;
        return c;
    }

    private Vector2Int WorldToCell(Vector3 pos)
    {
        int cx = Mathf.FloorToInt(pos.x / cellSize);
        int cz = Mathf.FloorToInt(pos.z / cellSize);
        return new Vector2Int(cx, cz);
    }

    private Bounds GetCellBounds(Vector2Int cell)
    {
        float minX = cell.x * cellSize;
        float minZ = cell.y * cellSize;

        Vector3 center = new Vector3(minX + cellSize * 0.5f, 0f, minZ + cellSize * 0.5f);
        Vector3 size = new Vector3(cellSize, 0f, cellSize);
        return new Bounds(center, size);
    }

    private Vector3 RandomPointInBoundsXZ(Bounds b)
    {
        float x = Random.Range(b.min.x, b.max.x);
        float z = Random.Range(b.min.z, b.max.z);
        return new Vector3(x, 0f, z);
    }
}


/*{
    [Header("Pools")]
    [SerializeField] private ChildPoolTeleporter bigObjectPool;
    [SerializeField] private ChildPoolTeleporter smallObjectPool;

    [Header("Square Area (centered on Player)")]
    [SerializeField] private float squareSize = 60f;   // e.g. 60x60
    [SerializeField] private float cellSize = 20f;     // e.g. 20x20 cells

    [Header("Spawn Count Per Cell")]
    [SerializeField] private int bigObjectsPerCell = 3;
    [SerializeField] private int smallObjectsPerCell = 8;

    [Header("Execution")]
    [SerializeField] private bool runOnStart = true;

    [ContextMenu("Run Grid Pool Teleport Test (Big + Small)")]
    public void RunTest()
    {
        if (bigObjectPool == null || smallObjectPool == null)
        {
            Debug.LogError("[PoolTeleportGridTester] One or both pools are not assigned.");
            return;
        }

        int cellsPerSide = Mathf.CeilToInt(squareSize / cellSize);
        float half = squareSize * 0.5f;

        Vector3 center = transform.position;
        float startX = center.x - half;
        float startZ = center.z - half;

        int totalCalls = 0;
        int success = 0;

        for (int gx = 0; gx < cellsPerSide; gx++)
        {
            for (int gz = 0; gz < cellsPerSide; gz++)
            {
                float cellMinX = startX + gx * cellSize;
                float cellMinZ = startZ + gz * cellSize;

                float cellMaxX = Mathf.Min(cellMinX + cellSize, center.x + half);
                float cellMaxZ = Mathf.Min(cellMinZ + cellSize, center.z + half);

                // ---- BIG OBJECTS ----
                for (int i = 0; i < bigObjectsPerCell; i++)
                {
                    float x = Random.Range(cellMinX, cellMaxX);
                    float z = Random.Range(cellMinZ, cellMaxZ);

                    bool ok = bigObjectPool.TeleportRandomTo(x, z);
                    totalCalls++;
                    if (ok) success++;
                }

                // ---- SMALL OBJECTS ----
                for (int i = 0; i < smallObjectsPerCell; i++)
                {
                    float x = Random.Range(cellMinX, cellMaxX);
                    float z = Random.Range(cellMinZ, cellMaxZ);

                    bool ok = smallObjectPool.TeleportRandomTo(x, z);
                    totalCalls++;
                    if (ok) success++;
                }
            }
        }

        Debug.Log(
            $"[PoolTeleportGridTester] Done | Cells={cellsPerSide * cellsPerSide} | " +
            $"Calls={totalCalls} | Success={success} | Fail={totalCalls - success}"
        );
    }

    private void Start()
    {
        if (runOnStart)
            RunTest();
    }*/

