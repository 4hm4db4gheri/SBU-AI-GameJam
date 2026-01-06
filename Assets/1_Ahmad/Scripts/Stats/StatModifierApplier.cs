using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Practical, indie-friendly way to apply buffs/debuffs/upgrades (flat/%/mult), optionally timed.
/// This stays thin: it delegates all math and stacking to the Stats runtime.
/// </summary>
public sealed class StatModifierApplier : MonoBehaviour
{
    [Serializable]
    public sealed class Entry
    {
        public StatDefinition Stat;
        public StatModifierType Type;
        public float Value;
        public int Order;

        [Tooltip("0 = permanent until removed/disabled. >0 = auto removed after duration.")]
        public float DurationSeconds;
    }

    [SerializeField] private StatsComponent _target;
    [SerializeField] private List<Entry> _entries = new();
    [SerializeField] private bool _removeOnDisable = true;

    private void Awake()
    {
        if (_target == null)
            _target = GetComponent<StatsComponent>();
    }

    private void OnEnable()
    {
        if (_target == null)
            return;

        foreach (var e in _entries)
        {
            if (e == null || e.Stat == null)
                continue;

            _target.Stats.AddModifier(e.Stat, new StatModifier(e.Type, e.Value, e.Order, source: this));

            if (e.DurationSeconds > 0f)
                StartCoroutine(RemoveAfterSeconds(e.DurationSeconds));
        }
    }

    private IEnumerator RemoveAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (_target != null)
            _target.Stats.RemoveAllFromSource(this);
    }

    private void OnDisable()
    {
        if (_removeOnDisable && _target != null)
            _target.Stats.RemoveAllFromSource(this);
    }
}

