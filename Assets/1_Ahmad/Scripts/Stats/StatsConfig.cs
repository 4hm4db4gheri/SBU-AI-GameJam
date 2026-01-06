using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DOOGH/Stats/Stats Config", fileName = "StatsConfig_")]
public sealed class StatsConfig : ScriptableObject
{
    [Serializable]
    public sealed class Entry
    {
        public StatDefinition Stat;
        public float BaseValue;
        public bool UseStatDefault = true;
    }

    public List<Entry> Entries = new();

    public IEnumerable<(StatDefinition stat, float baseValue)> Enumerate()
    {
        foreach (var e in Entries)
        {
            if (e == null || e.Stat == null)
                continue;

            float baseValue = e.UseStatDefault ? e.Stat.DefaultBaseValue : e.BaseValue;
            yield return (e.Stat, baseValue);
        }
    }
}

