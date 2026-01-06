using UnityEngine;

/// <summary>
/// Thin Unity wrapper around the pure C# StatsContainer to keep gameplay code decoupled.
/// </summary>
public sealed class StatsComponent : MonoBehaviour
{
    [SerializeField] private StatsConfig _config;

    private StatsContainer _stats;
    public StatsContainer Stats => _stats;

    private void Awake()
    {
        _stats = new StatsContainer();
        _stats.ApplyConfig(_config);
    }

    public void ApplyConfig(StatsConfig config)
    {
        _config = config;
        _stats ??= new StatsContainer();
        _stats.ApplyConfig(_config);
    }
}

