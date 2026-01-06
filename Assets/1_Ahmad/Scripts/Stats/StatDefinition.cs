using UnityEngine;

[CreateAssetMenu(menuName = "DOOGH/Stats/Stat Definition", fileName = "Stat_")]
public sealed class StatDefinition : ScriptableObject
{
    [Tooltip("Unique key used for lookup/serialization. Keep stable once shipped.")]
    public string Key;

    [Tooltip("Default base value when not explicitly set in a config.")]
    public float DefaultBaseValue;

    [Header("Optional Clamping")]
    public bool Clamp;
    public float MinValue;
    public float MaxValue = 999999f;

    [Header("Default Upgrade (Optional)")]
    [Tooltip("If enabled, upgrade systems can use this as the default increment for this stat (so you don't hard-code +10, +0.2, etc).")]
    public bool HasDefaultUpgrade;

    [Tooltip("How the default upgrade should be applied (Flat, PercentAdd, Mult).")]
    public StatModifierType DefaultUpgradeType = StatModifierType.Flat;

    [Tooltip("Upgrade value. Flat: +X. PercentAdd/Mult: +X where 0.1 means +10%.")]
    public float DefaultUpgradeValue = 1f;

    [Tooltip("Optional ordering within the modifier type.")]
    public int DefaultUpgradeOrder;

    public StatModifier CreateDefaultUpgradeModifier(object source = null)
    {
        return new StatModifier(DefaultUpgradeType, DefaultUpgradeValue, DefaultUpgradeOrder, source);
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(Key))
            Key = name;

        if (MaxValue < MinValue)
            MaxValue = MinValue;
    }
}

