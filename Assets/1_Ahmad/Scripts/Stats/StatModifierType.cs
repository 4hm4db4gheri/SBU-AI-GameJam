public enum StatModifierType
{
    /// <summary>Applied before percentages/multipliers. Example: +5 damage.</summary>
    Flat = 0,
    /// <summary>All additive percentages are summed, then applied once. Example: +10% damage.</summary>
    PercentAdd = 1,
    /// <summary>Applied one-by-one as multiplicative stacking. Example: +10% damage multiplicative.</summary>
    Mult = 2
}

