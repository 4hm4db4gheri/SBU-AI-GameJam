using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChildPoolTeleporter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player; // drag Player here

    [Header("Rules")]
    [SerializeField] private float minDistanceFromPlayer = 17f;

    [Header("Selection")]
    [SerializeField] private int maxAttempts = 50; // avoid infinite loops

    private readonly List<Transform> pooledChildren = new List<Transform>();

    private void Awake()
    {
        CacheChildren();
    }

    private void OnTransformChildrenChanged()
    {
        CacheChildren();
    }

    private void CacheChildren()
    {
        pooledChildren.Clear();
        for (int i = 0; i < transform.childCount; i++)
            pooledChildren.Add(transform.GetChild(i));
    }

    /// <summary>
    /// Promise-like version (Task). Returns true if teleported, false otherwise.
    /// </summary>
    public Task<bool> TeleportRandomToAsync(float x, float z)
    {
        bool result = TeleportRandomTo(x, z);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Chooses a random eligible child (distance to player > minDistanceFromPlayer),
    /// enables it if disabled, and teleports it to (x, z) WITHOUT changing its Y.
    /// </summary>
    public bool TeleportRandomTo(float x, float z)
    {
        if (player == null)
        {
            Debug.LogError("[ChildPoolTeleporter] Player reference is not assigned.");
            return false;
        }

        if (pooledChildren.Count == 0)
        {
            Debug.LogWarning("[ChildPoolTeleporter] No children under pool parent.");
            return false;
        }

        // Try random picks first
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Transform candidate = pooledChildren[Random.Range(0, pooledChildren.Count)];
            if (candidate == null) continue;

            // IMPORTANT: distance check works even if candidate is inactive
            float dist = Vector3.Distance(candidate.position, player.position);
            if (dist <= minDistanceFromPlayer) continue;

            // Enable if disabled
            if (!candidate.gameObject.activeSelf)
                candidate.gameObject.SetActive(true);

            // Keep Y fixed (do not change it)
            Vector3 pos = candidate.position;
            candidate.position = new Vector3(x, pos.y, z);

            return true;
        }

        // Fallback: deterministic scan to avoid unlucky random attempts
        for (int i = 0; i < pooledChildren.Count; i++)
        {
            Transform candidate = pooledChildren[i];
            if (candidate == null) continue;

            float dist = Vector3.Distance(candidate.position, player.position);
            if (dist <= minDistanceFromPlayer) continue;

            if (!candidate.gameObject.activeSelf)
                candidate.gameObject.SetActive(true);

            Vector3 pos = candidate.position;
            candidate.position = new Vector3(x, pos.y, z);

            return true;
        }

        return false; // nothing eligible
    }

    public bool TryTeleportRandomTo(float x, float z, out Transform teleported)
    {
        teleported = null;

        if (player == null || pooledChildren.Count == 0)
            return false;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Transform candidate = pooledChildren[Random.Range(0, pooledChildren.Count)];
            if (candidate == null) continue;

            float dist = Vector3.Distance(candidate.position, player.position);
            if (dist <= minDistanceFromPlayer) continue;

            if (!candidate.gameObject.activeSelf)
                candidate.gameObject.SetActive(true);

            Vector3 pos = candidate.position;
            candidate.position = new Vector3(x, pos.y, z); // keep Y

            teleported = candidate;
            return true;
        }

        // fallback scan
        for (int i = 0; i < pooledChildren.Count; i++)
        {
            Transform candidate = pooledChildren[i];
            if (candidate == null) continue;

            float dist = Vector3.Distance(candidate.position, player.position);
            if (dist <= minDistanceFromPlayer) continue;

            if (!candidate.gameObject.activeSelf)
                candidate.gameObject.SetActive(true);

            Vector3 pos = candidate.position;
            candidate.position = new Vector3(x, pos.y, z);

            teleported = candidate;
            return true;
        }

        return false;
    }
}
