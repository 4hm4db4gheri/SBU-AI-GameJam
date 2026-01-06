using System;

/// <summary>
/// A pure-data modifier. Duration/stacking policy are handled by higher-level systems.
/// </summary>
public readonly struct StatModifier
{
    public readonly StatModifierType Type;
    public readonly float Value;
    public readonly int Order;
    public readonly object Source;

    public StatModifier(StatModifierType type, float value, int order = 0, object source = null)
    {
        Type = type;
        Value = value;
        Order = order;
        Source = source;
    }

    public override string ToString() => $"{Type} {Value} (Order={Order}, Source={Source?.GetType().Name ?? "null"})";
}

