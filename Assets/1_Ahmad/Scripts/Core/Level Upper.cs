using UnityEngine;

[RequireComponent(typeof(StatsComponent))]
public class LevelUpper : MonoBehaviour
{
    private StatsComponent _statsComponent;
    private ExperienceManager _experienceManager;

    private void Awake()
    {
        _statsComponent = GetComponent<StatsComponent>();
    }
    private void OnEnable()
    {
        if (ExperienceManager.TryGetInstance(out var mgr))
        {
            _experienceManager = mgr;
            _experienceManager.LevelUp += OnLevelUp;
        }
        else
        {
            Debug.LogWarning($"{nameof(LevelUpper)} on '{name}' could not find an {nameof(ExperienceManager)} in the scene, so it won't apply level-up upgrades.", this);
        }
    }

    private void OnDisable()
    {
        if (_experienceManager != null)
            _experienceManager.LevelUp -= OnLevelUp;

        _experienceManager = null;
    }
    private void OnLevelUp(int newLevel)
    {
        Debug.Log("Level Up");

        foreach (var entry in _statsComponent.Config.Entries)
        {
            if (entry.Stat != null && entry.Stat.HasDefaultUpgrade)
                _statsComponent.Stats.AddModifier(entry.Stat, entry.Stat.CreateDefaultUpgradeModifier(source: this));
        }
    }
}