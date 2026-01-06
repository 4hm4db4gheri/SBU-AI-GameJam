using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class StatInstance
{
    private sealed class ModifierInstance
    {
        public int Id;
        public StatModifier Modifier;
    }

    private readonly StatDefinition _definition;
    private readonly List<ModifierInstance> _modifiers = new();

    private float _baseValue;
    private float _cachedValue;
    private bool _dirty = true;
    private int _nextModifierId = 1;

    public StatDefinition Definition => _definition;
    public float BaseValue => _baseValue;
    public float Value => GetValue();

    public event Action<float> ValueChanged;

    public StatInstance(StatDefinition definition, float baseValue)
    {
        _definition = definition;
        _baseValue = baseValue;
    }

    public void SetBaseValue(float newBaseValue)
    {
        if (Math.Abs(_baseValue - newBaseValue) < 0.000001f)
            return;

        _baseValue = newBaseValue;
        MarkDirtyAndNotify();
    }

    public ModifierHandle AddModifier(StatModifier modifier)
    {
        int id = ++_nextModifierId;
        _modifiers.Add(new ModifierInstance { Id = id, Modifier = modifier });
        _modifiers.Sort(CompareModifierInstances);
        MarkDirtyAndNotify();
        return new ModifierHandle(id);
    }

    public bool RemoveModifier(ModifierHandle handle)
    {
        if (!handle.IsValid)
            return false;

        for (int i = 0; i < _modifiers.Count; i++)
        {
            if (_modifiers[i].Id != handle.Id)
                continue;

            _modifiers.RemoveAt(i);
            MarkDirtyAndNotify();
            return true;
        }

        return false;
    }

    public int RemoveAllFromSource(object source)
    {
        if (source == null)
            return 0;

        int removed = 0;
        for (int i = _modifiers.Count - 1; i >= 0; i--)
        {
            if (!ReferenceEquals(_modifiers[i].Modifier.Source, source))
                continue;

            _modifiers.RemoveAt(i);
            removed++;
        }

        if (removed > 0)
            MarkDirtyAndNotify();

        return removed;
    }

    private float GetValue()
    {
        if (!_dirty)
            return _cachedValue;

        float value = _baseValue;

        // 1) Flat
        float flatSum = 0f;
        for (int i = 0; i < _modifiers.Count; i++)
        {
            var m = _modifiers[i].Modifier;
            if (m.Type == StatModifierType.Flat)
                flatSum += m.Value;
        }
        value += flatSum;

        // 2) PercentAdd (summed once)
        float percentAddSum = 0f;
        for (int i = 0; i < _modifiers.Count; i++)
        {
            var m = _modifiers[i].Modifier;
            if (m.Type == StatModifierType.PercentAdd)
                percentAddSum += m.Value;
        }
        value *= 1f + percentAddSum;

        // 3) Mult (stacked one-by-one)
        for (int i = 0; i < _modifiers.Count; i++)
        {
            var m = _modifiers[i].Modifier;
            if (m.Type == StatModifierType.Mult)
                value *= 1f + m.Value;
        }

        if (_definition != null && _definition.Clamp)
            value = Mathf.Clamp(value, _definition.MinValue, _definition.MaxValue);

        _cachedValue = value;
        _dirty = false;
        return _cachedValue;
    }

    private void MarkDirtyAndNotify()
    {
        float before = _cachedValue;
        _dirty = true;
        float after = GetValue();

        if (Math.Abs(before - after) > 0.000001f)
            ValueChanged?.Invoke(after);
    }

    private static int CompareModifierInstances(ModifierInstance a, ModifierInstance b)
    {
        int typeCompare = a.Modifier.Type.CompareTo(b.Modifier.Type);
        if (typeCompare != 0) return typeCompare;

        int orderCompare = a.Modifier.Order.CompareTo(b.Modifier.Order);
        if (orderCompare != 0) return orderCompare;

        return a.Id.CompareTo(b.Id);
    }
}

