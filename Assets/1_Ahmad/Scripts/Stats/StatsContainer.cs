using System;
using System.Collections.Generic;

public sealed class StatsContainer
{
    private readonly Dictionary<string, StatInstance> _statsByKey = new();
    private readonly Dictionary<string, ComputedStat> _computedByKey = new();

    public event Action<StatDefinition, float> StatValueChanged;

    private sealed class ComputedStat
    {
        public StatDefinition Def;
        public Func<StatsContainer, float> Compute;
        public HashSet<string> DependencyKeys;
        public bool IsUpdating;
    }

    public bool Has(StatDefinition def) =>
        def != null && !string.IsNullOrWhiteSpace(def.Key) && _statsByKey.ContainsKey(def.Key);

    public float GetValue(StatDefinition def, float fallback = 0f)
    {
        if (def == null || string.IsNullOrWhiteSpace(def.Key))
            return fallback;

        return GetOrCreate(def).Value;
    }

    public void SetBaseValue(StatDefinition def, float baseValue)
    {
        if (def == null || string.IsNullOrWhiteSpace(def.Key))
            return;

        GetOrCreate(def).SetBaseValue(baseValue);
    }

    public ModifierHandle AddModifier(StatDefinition def, StatModifier modifier)
    {
        if (def == null || string.IsNullOrWhiteSpace(def.Key))
            return default;

        return GetOrCreate(def).AddModifier(modifier);
    }

    public bool RemoveModifier(StatDefinition def, ModifierHandle handle)
    {
        if (def == null || string.IsNullOrWhiteSpace(def.Key))
            return false;

        if (!_statsByKey.TryGetValue(def.Key, out var stat))
            return false;

        return stat.RemoveModifier(handle);
    }

    public int RemoveAllFromSource(object source)
    {
        int total = 0;
        foreach (var kv in _statsByKey)
            total += kv.Value.RemoveAllFromSource(source);
        return total;
    }

    public void ApplyConfig(StatsConfig config)
    {
        if (config == null)
            return;

        foreach (var (stat, baseValue) in config.Enumerate())
        {
            var instance = GetOrCreate(stat);
            instance.SetBaseValue(baseValue);
        }
    }

    /// <summary>
    /// Registers a stat whose BASE value is computed from other stats (but can still receive modifiers).
    /// Useful for "derived" stats without hard-coding them into gameplay code.
    /// </summary>
    public void RegisterComputed(StatDefinition def, Func<StatsContainer, float> compute, params StatDefinition[] dependencies)
    {
        if (def == null || string.IsNullOrWhiteSpace(def.Key) || compute == null)
            return;

        var computed = new ComputedStat
        {
            Def = def,
            Compute = compute,
            DependencyKeys = new HashSet<string>()
        };

        if (dependencies != null)
        {
            foreach (var dep in dependencies)
            {
                if (dep == null || string.IsNullOrWhiteSpace(dep.Key) || dep.Key == def.Key)
                    continue;
                computed.DependencyKeys.Add(dep.Key);
            }
        }

        _computedByKey[def.Key] = computed;

        // Ensure the stat exists, then set initial computed base.
        GetOrCreate(def).SetBaseValue(compute(this));
    }

    private StatInstance GetOrCreate(StatDefinition def)
    {
        if (_statsByKey.TryGetValue(def.Key, out var existing))
            return existing;

        var created = new StatInstance(def, def.DefaultBaseValue);
        created.ValueChanged += newValue =>
        {
            // Notify listeners.
            StatValueChanged?.Invoke(def, newValue);

            // Update any computed stats that depend on this key.
            foreach (var kv in _computedByKey)
            {
                var computed = kv.Value;
                if (computed == null || computed.IsUpdating)
                    continue;

                if (!computed.DependencyKeys.Contains(def.Key))
                    continue;

                computed.IsUpdating = true;
                try
                {
                    GetOrCreate(computed.Def).SetBaseValue(computed.Compute(this));
                }
                finally
                {
                    computed.IsUpdating = false;
                }
            }
        };

        _statsByKey.Add(def.Key, created);
        return created;
    }
}

